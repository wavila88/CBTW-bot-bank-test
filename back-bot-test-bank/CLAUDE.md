# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Hexagonal Architecture (Ports & Adapters) in C# on .NET 8 (`net8.0`, nullable + implicit usings enabled). The project is being turned into a RAG customer service bot API for "Bank Test" (see `.sdd/specs/001-rag-customer-bot-back/`). The original `Register` sample has been removed, so the layers are currently empty.

## Commands

- Build: `dotnet build back-bot-test-bank.sln`
- Test: `dotnet test Tests/Tests.csproj` (xUnit + Moq, unit tests only, no real network calls)
- Run: `dotnet run --project Api --launch-profile http` (http://localhost:5082) or `--launch-profile https` (https://localhost:7283). Swagger UI is served at `/swagger` in Development only.

## Architecture

Dependency direction: `Application → Domain` and `Infra → Domain`. `Domain` references nothing. `Api` currently has **no** project references, so it does not yet reach Application/Infra. `Tests` references all four.

- **Domain/**: entities and output ports (interfaces).
- **Application/**: use cases. References only `Domain`, never an external package.
- **Infra/**: output adapters implementing the Domain ports, one folder per adapter.
- **Api/**: input adapter (controllers, DTOs, DI wiring in `Program.cs`).

Each layer has its own shape for the data it handles; mapping between layers is manual, with no mapper library.
