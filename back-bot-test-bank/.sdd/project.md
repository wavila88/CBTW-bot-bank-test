# Project: back-bot-test-bank
Sources read: CLAUDE.md, README.md, *.csproj (Api, Application, Domain, Infra, Tests), back-bot-test-bank.sln, Api/Program.cs, Api/Properties/launchSettings.json
Last scan: 2026-11-01 (updated after removing the `Register` sample, spec 001 phase 1)

## Stack
- C# on .NET 8 (`net8.0`), nullable + implicit usings enabled in all projects (← Api/Api.csproj, Domain/Domain.csproj)
- ASP.NET Core Web API with controllers (← Api/Api.csproj `Microsoft.NET.Sdk.Web`, Api/Program.cs `AddControllers`)
- Swashbuckle.AspNetCore 6.6.2 for Swagger (← Api/Api.csproj)
- No other libraries in the production projects yet (← all *.csproj)

## Planned decisions (from specs, not yet in code)
- Pure hexagon: `Application` references only `Domain`, never an external package (no Semantic Kernel, no vendor SDK). All bot logic lives in `Application` and calls out through our own ports in `Domain` (← .sdd/specs/001-rag-customer-bot-back/spec.md)
- Microsoft Semantic Kernel is the chosen library for conversation state (`ChatHistory` + history reducer) and chat completion. Prefer library features over manual code, but only inside adapters: `Infra/AI/` for Semantic Kernel, `Infra/Retrieval/` for Pinecone. No extra adapter projects (← same spec)
- Model and vector store are swappable by configuration: a new provider means a new adapter folder in `Infra` plus a config value (← same spec)
- Conversations are kept server-side by `conversationId`, in memory behind a port (← same spec)
- English is the priority reply language; the default and fallback is English (← same spec)

## Architecture
- Pattern: Hexagonal / Ports & Adapters (← README.md, CLAUDE.md)
- Layers (currently empty after the `Register` sample was removed):
  - `Domain/` → entities and output ports (interfaces)
  - `Application/` → use cases and their DTOs
  - `Infra/` → output adapters implementing Domain ports, one folder per adapter
  - `Api/` → input adapter (controllers, DTOs, DI wiring)
- Dependency rules:
  - Domain references nothing (← Domain/Domain.csproj)
  - Application → Domain; Infra → Domain (← Application/Application.csproj, Infra/Infra.csproj)
  - Api has no project references yet, so it does not reach Application/Infra (← Api/Api.csproj)
  - Tests → Domain, Application, Infra, Api (← Tests/Tests.csproj)

## Conventions
Carried over from the removed `Register` sample; follow them in new code.
- Constructor injection of ports into use cases
- One shape per layer (Api DTO, Application DTO, Domain model), mapped manually, no mapper library
- Block-scoped namespaces matching the folder (e.g. `namespace Tests { ... }`)
- Controllers: `[ApiController]`, `[Route("[controller]")]`, `ILogger<T>` injected
- Error handling and logging conventions: none established yet

## Frontend
- N/A (API only; Swagger UI in Development) (← Api/Program.cs)

## Tests
- Frameworks: xUnit 2.9 + Moq 4.20, unit tests only, no real network calls (← Tests/Tests.csproj, spec 001 Decisions)
- Location and naming: `Tests/`, `<Subject>Tests.cs`, methods `Subject_Condition` (e.g. `AskQuestion_ReturnsModelAnswer`); the project has no tests yet (← Tests/Tests.csproj)
- Command: `dotnet test Tests/Tests.csproj`

## Run the app
- `dotnet build back-bot-test-bank.sln` (the solution lists Api, Application, Domain, Infra, Tests) (← back-bot-test-bank.sln)
- `dotnet run --project Api --launch-profile http` → http://localhost:5082/swagger (← Api/Properties/launchSettings.json)
- `dotnet run --project Api --launch-profile https` → https://localhost:7283/swagger (← Api/Properties/launchSettings.json)
- Program.cs registers no DI for use cases or adapters yet (← Api/Program.cs)

## Not detected / questions
- Error-handling/logging conventions: none established yet (spec 001 SP-2/SP-3 will set them).
