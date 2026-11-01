# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Template/example of Hexagonal Architecture (Ports & Adapters) in C# on .NET 8 (`net8.0`, nullable + implicit usings enabled). The domain is a "Register" (a person registering seats). Adapters are still stubs that `throw new NotImplementedException()`.

## Commands

No tests exist yet. The `.sln` file is empty (it lists no projects), so build per project rather than via the solution:

- Build: `dotnet build Api/Api.csproj` (builds project references transitively)
- Run: `dotnet run --project Api --launch-profile http` (http://localhost:5082) or `--launch-profile https` (https://localhost:7283). Swagger UI is served at `/swagger` in Development only.

## Architecture

Dependency direction: `Application → Domain` and `Infra → Domain`. `Domain` references nothing. `Api` currently has **no** project references, so it does not yet reach Application/Infra.

- **Domain/**: entity (`Models/Register`) and the output port `Repositories/IRegisterRepository` (persistence).
- **Application/**: use cases. `RegisterUseCase` maps `RegisterDTO` to the domain `Register` and calls `IRegisterRepository.SaveRegister`.
- **Infra/**: output adapters implementing the Domain ports (DB save).
- **Api/**: input adapter. `RegisterController` (`POST /Register`) takes `Api.DTO.Register`. It is not yet wired to `IRegisterUseCase`, and `Program.cs` has no DI registrations for use cases or adapters.

Three parallel DTO/model shapes exist (`Api.DTO.Register`, `Application.DTO.RegisterDTO`, `Domain.Models.Register`); mapping between layers is manual, with no mapper library. `RegisterDTO` also carries `QuantityOfPeopleConected`, which is not mapped into the domain model.

## Gotchas

- In Infra, `RegisterRepository` lives in `Infra/Publisher/` with namespace `Infra.Publisher` (a leftover name from the removed publisher). Fix it deliberately if you touch it.
- The comment in `IRegisterRepository` points to a wrong adapter location (`Infra.Messaging`), which does not exist.
