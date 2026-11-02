# Project: back-bot-test-bank

> **IMPORTANT — mandatory for every change.** Read this whole file before editing code. Configuration values come from environment variables (no hardcoded defaults in code). The hexagonal dependency rules apply. Tests follow the conventions below. A change that breaks any of them is not done.
Sources read: CLAUDE.md, README.md, *.csproj (Api, Application, Domain, Infra.Vector.Pinne, Tests), back-bot-test-bank.sln, Api/Program.cs, Api/Properties/launchSettings.json
Last scan: 2026-11-01 (updated after removing the `Register` sample, spec 001 phase 1)

## Stack
- C# on .NET 8 (`net8.0`), nullable + implicit usings enabled in all projects (← Api/Api.csproj, Domain/Domain.csproj)
- ASP.NET Core Web API with controllers (← Api/Api.csproj `Microsoft.NET.Sdk.Web`, Api/Program.cs `AddControllers`)
- Swashbuckle.AspNetCore 6.6.2 for Swagger (← Api/Api.csproj)
- No other libraries in the production projects yet (← all *.csproj)

## Planned decisions (from specs, not yet in code)
- Pure hexagon: `Application` references only `Domain`, never an external package (no Semantic Kernel, no vendor SDK). All bot logic lives in `Application` and calls out through our own ports in `Domain` (← .sdd/specs/001-rag-customer-bot-back/spec.md)
- Microsoft Semantic Kernel is the chosen library for conversation state (`ChatHistory` + history reducer) and chat completion. Prefer library features over manual code, but only inside adapters: one project per adapter: `Infra.Vector.Pinne` for Pinecone and an `Infra.AI.*` project for Semantic Kernel (name to decide) (← same spec)
- Model and vector store are swappable by configuration: a new provider means a new adapter project (`Infra.<Kind>.<Name>`) plus a config value (← same spec)
- Conversations are kept server-side by `conversationId`, in memory behind a port (← same spec)
- English is the priority reply language; the default and fallback is English (← same spec)

## Architecture
- Pattern: Hexagonal / Ports & Adapters (← README.md, CLAUDE.md)
- Layers:
  - `Domain/` → entities and output ports (interfaces)
  - `Application/` → use cases and their DTOs
  - `Infra.<Kind>.<Name>/` (e.g. `Infra.Vector.Pinne`) → one project per output adapter, implementing Domain ports
  - `Api/` → input adapter (controllers, DTOs, DI wiring)
- Dependency rules:
  - Domain references nothing (← Domain/Domain.csproj)
  - Application → Domain; each `Infra.*` adapter → Domain (← Application/Application.csproj, Infra.Vector.Pinne/Infra.Vector.Pinne.csproj)
  - Api → Application, Domain and every `Infra.*` project; it is the composition root that wires the adapters in `Api/Program.cs` (← Api/Api.csproj)
  - Tests → Domain, Application, every `Infra.*` project, Api (← Tests/Tests.csproj)

## Conventions
Carried over from the removed `Register` sample; follow them in new code.
- Constructor injection of ports into use cases
- One shape per layer (Api DTO, Application DTO, Domain model), mapped manually, no mapper library
- Block-scoped namespaces matching the folder (e.g. `namespace Tests { ... }`)
- Controllers: `[ApiController]`, `[Route("[controller]")]`, `ILogger<T>` injected
- Error handling and logging conventions: none established yet

## Configuration
- Environment-specific values (keys, index name, namespace, embedding model, dimension, TopK, host) come from environment variables, e.g. `Pinecone__ApiKey`, `Pinecone__IndexName`, `Chat__TopK`. Never hardcode them in code or commit them in JSON.
- `Api/appsettings.Local.json` is an optional, git-ignored local fallback; environment variables win. The committed template `appsettings.Local.example.json` holds placeholders only.
- Code holds no defaults for these values. Startup validation names each missing setting (`PineconeOptions.Validate()`, `Chat:TopK` in `Api/Program.cs`).
- Only protocol constants stay in code (Pinecone `ControlPlaneUrl`, `ApiVersion`).

## Frontend
- N/A (API only; Swagger UI in Development) (← Api/Program.cs)

## Tests
- Frameworks: xUnit 2.9 + Moq 4.20, unit tests only, no real network calls (← Tests/Tests.csproj, spec 001 Decisions)
- What gets automated tests: logic with decisions in `Application` and the `Api` mapping/status codes, with mocked ports. `Infra` adapters get stubbed-HTTP tests only for behavior with decisions (ordering, missing metadata, query embedding, host caching); pass-through calls are verified manually through Development-only diagnostic endpoints (e.g. `GET /Search`) (← spec 001 Decisions)
- Location and naming: `Tests/`, `<Subject>Tests.cs`, methods `Subject_Condition` (e.g. `AskQuestion_ReturnsModelAnswer`); the project has no tests yet (← Tests/Tests.csproj)
- Command: `dotnet test Tests/Tests.csproj`

## Run the app
- `dotnet build back-bot-test-bank.sln` (the solution lists Api, Application, Domain, Infra.Vector.Pinne, Tests) (← back-bot-test-bank.sln)
- `dotnet run --project Api --launch-profile http` → http://localhost:5082/swagger (← Api/Properties/launchSettings.json)
- `dotnet run --project Api --launch-profile https` → https://localhost:7283/swagger (← Api/Properties/launchSettings.json)
- Diagnostic: `GET /Search?query=...&topK=5` (Development only) returns the guide fragments Pinecone finds (← Api/Controllers/SearchController.cs)

## Not detected / questions
- Error-handling/logging conventions: none established yet (spec 001 SP-2/SP-3 will set them).
