# Project: back-bot-test-bank
Sources read: CLAUDE.md, README.md, *.csproj (Api, Application, Domain, Infra), .sln, Api/Program.cs, Api/Properties/launchSettings.json, Api/Controllers/RegisterController.cs, Application/UseCases/RegisterUseCase.cs, Domain/Models/Register.cs, Infra/Repository/RegisterPublisher.cs, Domain/Repositories/IRegisterRepository.cs
Last scan: 2026-11-01

## Stack
- C# on .NET 8 (`net8.0`), nullable + implicit usings enabled in all projects (← Api/Api.csproj, Domain/Domain.csproj)
- ASP.NET Core Web API with controllers (← Api/Api.csproj `Microsoft.NET.Sdk.Web`, Api/Program.cs `AddControllers`)
- Swashbuckle.AspNetCore 6.6.2 for Swagger (← Api/Api.csproj)
- No other libraries; no mapper, DB or queue client packages (← all *.csproj)

## Architecture
- Pattern: Hexagonal / Ports & Adapters (← README.md, CLAUDE.md, Domain ports + Infra adapters)
- Layers:
  - `Domain/` → entity (`Models/Register`) and output port (`Repositories/IRegisterRepository`)
  - `Application/` → use cases (`UseCases/IRegisterUseCase`, `RegisterUseCase`) and `DTO/RegisterDTO`
  - `Infra/` → output adapters implementing Domain ports (DB save)
  - `Api/` → input adapter (`Controllers/RegisterController`, `DTO/Register`)
- Dependency rules:
  - Domain references nothing (← Domain/Domain.csproj)
  - Application → Domain; Infra → Domain (← Application/Application.csproj, Infra/Infra.csproj)
  - Api has no project references yet, so it does not reach Application/Infra (← Api/Api.csproj)
- Publisher/queue port was removed; the use case saves through `IRegisterRepository` only (← Application/UseCases/RegisterUseCase.cs)

## Conventions
- Constructor injection of ports into use cases (← Application/UseCases/RegisterUseCase.cs)
- Three parallel shapes (`Api.DTO.Register`, `Application.DTO.RegisterDTO`, `Domain.Models.Register`), mapped manually in the use case, no mapper library (← RegisterUseCase.cs, CLAUDE.md)
- Block-scoped namespaces matching folder (except Infra, see below); synchronous port methods (`void SaveRegister`) (← Domain/Repositories/IRegisterRepository.cs)
- Unimplemented adapters/controllers `throw new NotImplementedException()` (← RegisterController.cs, Infra/Publisher/RegisterRepository.cs)
- Controllers: `[ApiController]`, `[Route("[controller]")]`, `ILogger<T>` injected (← Api/Controllers/RegisterController.cs)
- Error handling and logging conventions: only the stub `ILogger` exists; nothing else established (← RegisterController.cs)
- Gotcha: `RegisterRepository` sits in `Infra/Publisher/` with namespace `Infra.Publisher` (leftover name) (← Infra/Publisher/RegisterRepository.cs)
- Port interface comment references nonexistent `Infra.Messaging` (← Domain/Repositories/IRegisterRepository.cs)

## Frontend
- N/A (API only; Swagger UI in Development) (← Api/Program.cs)

## Tests
- Frameworks: none (← no test project, CLAUDE.md "No tests exist yet")
- Location and naming: none
- Command: `not detected`

## Run the app
- `dotnet build Api/Api.csproj` (the `.sln` lists no projects, so build per project) (← CLAUDE.md, back-bot-test-bank.sln)
- `dotnet run --project Api --launch-profile http` → http://localhost:5082/swagger (← Api/Properties/launchSettings.json)
- `dotnet run --project Api --launch-profile https` → https://localhost:7283/swagger (← Api/Properties/launchSettings.json)
- Program.cs registers no DI for use cases or adapters yet (← Api/Program.cs)

## Not detected / questions
- Test command and framework: none exist. Which should we use (xUnit + Moq / FluentAssertions?) and where should the test project go (e.g. `Tests/`)? TDD needs this.
- Should `Api` get project references to Application/Infra and DI wiring in `Program.cs` as part of upcoming work?
- Should `Infra/Publisher/RegisterRepository.cs` be moved to `Infra/Repository` with namespace `Infra.Repository` now that the publisher is gone?
- Error-handling/logging conventions: none established beyond stubs.
