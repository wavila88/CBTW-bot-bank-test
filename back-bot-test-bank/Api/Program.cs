using System.Threading.RateLimiting;
using Api.RateLimiting;
using Application.UseCases;
using Domain.Exceptions;
using Domain.Ports;
using Infra.Vector.Pinne;

var builder = WebApplication.CreateBuilder(args);

// Settings come from environment variables (Pinecone__ApiKey, Chat__TopK, ...). appsettings.Local.json is an
// optional git-ignored fallback; environment variables win. Read once at startup.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);

var pinecone = builder.Configuration.GetSection("Pinecone").Get<PineconeOptions>() ?? new PineconeOptions();
pinecone.Validate();
var topK = builder.Configuration.GetValue<int?>("Chat:TopK")
    ?? throw new InvalidOperationException("Missing setting Chat__TopK.");

// Adapters (Infra) behind the Domain ports.
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IVector>(sp =>
    new PineconeVector(sp.GetRequiredService<IHttpClientFactory>().CreateClient(), pinecone));

// Use cases (Application)
builder.Services.AddScoped<ISearchGuidesUseCase>(sp =>
    new SearchGuidesUseCase(sp.GetRequiredService<IVector>(), topK));

// Add services to the container.

// Rate limit: one global limiter for every endpoint, counted per client IP. Settings come from configuration.
var rateLimit = builder.Configuration.GetSection("RateLimit").Get<RateLimitOptions>() ?? new RateLimitOptions();
rateLimit.Validate();
builder.Services.AddRateLimiter(limiter =>
{
    limiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimit.PermitLimit,
                Window = TimeSpan.FromSeconds(rateLimit.WindowSeconds),
                QueueLimit = 0
            }));
    // The rejection becomes a DomainException so the global handler answers with the same format as every other error.
    limiter.OnRejected = (_, _) => throw new DomainException(ErrorCode.TooManyRequests, "Client exceeded the request limit.");
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<Api.Errors.ApiExceptionHandler>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();
app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
