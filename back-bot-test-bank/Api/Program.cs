using System.Threading.RateLimiting;
using Api.RateLimiting;
using Application.UseCases;
using Domain.Exceptions;
using Domain.Ports;
using Infra.AI.SemanticKernel;
using Infra.Vector.Pinne;

var builder = WebApplication.CreateBuilder(args);

// Settings come from environment variables (Pinecone__ApiKey, Chat__TopK, ...). appsettings.Local.json is an
// optional git-ignored fallback; environment variables win. Read once at startup.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);
// A source added later wins, so the environment variables are added again after the local file.
builder.Configuration.AddEnvironmentVariables();

var pinecone = builder.Configuration.GetSection("Pinecone").Get<PineconeOptions>() ?? new PineconeOptions();
pinecone.Validate();
var topK = builder.Configuration.GetValue<int?>("Chat:TopK")
    ?? throw new InvalidOperationException("Missing setting Chat__TopK.");

// The rules for the chat model live in a file, so they can be edited without recompiling.
var rulesPath = builder.Configuration["Chat:RulesPath"]
    ?? throw new InvalidOperationException("Missing setting Chat__RulesPath.");
var rules = File.ReadAllText(Path.Combine(builder.Environment.ContentRootPath, rulesPath));

// Adapters (Infra) behind the Domain ports.
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IVector>(sp =>
    new PineconeVector(sp.GetRequiredService<IHttpClientFactory>().CreateClient(), pinecone));

// The chat model reads the key from OpenAI__ApiKey and the model name from Chat__Model; AddSemanticKernelChatModel validates both.
var openAi = new OpenAIChatOptions
{
    ApiKey = builder.Configuration["OpenAI:ApiKey"] ?? "",
    Model = builder.Configuration["Chat:Model"] ?? ""
};
builder.Services.AddSemanticKernelChatModel(openAi);
builder.Services.AddSemanticKernelConversationStore();

// Use cases (Application)
builder.Services.AddScoped<ISearchGuidesUseCase>(sp =>
    new SearchGuidesUseCase(sp.GetRequiredService<IVector>(), topK));
builder.Services.AddScoped<IAskQuestionUseCase>(sp =>
    new AskQuestionUseCase(
        sp.GetRequiredService<IVector>(), sp.GetRequiredService<IChatModel>(), sp.GetRequiredService<IConversationStore>(), rules, topK));

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

// CORS: only the front's origin may call the API from a browser. It comes from Cors__AllowedOrigin.
var cors = builder.Configuration.GetSection("Cors").Get<Api.Cors.CorsOptions>() ?? new Api.Cors.CorsOptions();
var corsPolicy = cors.ToPolicy();
builder.Services.AddCors(options => options.AddDefaultPolicy(corsPolicy));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<Api.Errors.ApiExceptionHandler>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();
// Before the rate limiter, so even a rejected request carries the CORS header and the browser can read its message.
app.UseCors();
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
