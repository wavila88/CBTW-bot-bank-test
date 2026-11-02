using Application.UseCases;
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

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<Api.Errors.ApiExceptionHandler>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

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
