# Tasks: Guide search API and error contract
Spec: ./spec.md
Test command: `dotnet test Tests/Tests.csproj`

Phases 1 to 3c are this spec. The chat work starts at Phase 4 in `002-rag-customer-bot-chat/tasks.md`; phase numbers continue across the two specs so references stay stable.

## Phase 1 — Remove the Register sample and add the test project (covers Decisions: Register removal, Tests)
Test: `Tests/` (xUnit + Moq, references Domain, Application, Infra, Api) builds and runs one placeholder test; no `Register` type remains in any project.
Manual check: `dotnet build Api/Api.csproj` succeeds, `/swagger` shows no `/Register` endpoint, and `CLAUDE.md`, `README.md` and `.sdd/project.md` no longer describe `Register`.

- [x] 1.1 Failing test — result: `FAILED: RegisterSample_IsRemoved (Domain, Application, Infra, Api) — Assert.Empty() Failure: Collection was not empty` (4 failed, 1 passed)
- [x] 1.2 Implementation — files: deleted `Domain/Models/Register.cs`, `Domain/Repositories/IRegisterRepository.cs`, `Application/UseCases/IRegisterUseCase.cs`, `Application/UseCases/RegisterUseCase.cs`, `Application/DTO/RegisterDTO.cs`, `Infra/Publisher/RegisterRepository.cs`, `Api/DTO/Register.cs`, `Api/Controllers/RegisterController.cs` (and their now-empty folders); added `Tests/Tests.csproj`, `Tests/ProjectStructureTests.cs`; `back-bot-test-bank.sln` (+Tests); updated `CLAUDE.md`, `.sdd/project.md`
- [x] 1.3 Passing test — result: `Passed! - Failed: 0, Passed: 5, Total: 5`; `dotnet build back-bot-test-bank.sln` 0 warnings, 0 errors
- [x] 1.4 Developer approval — approved after a successful manual check; at the developer's request `Tests/ProjectStructureTests.cs` was then deleted (the `Tests/` project stays for the next phases)

## Phase 2 — Search the guides in Pinecone (covers HP-1, retrieval part)
Result: `IVector` (Domain port) and its Pinecone adapter in `Infra.Vector.Pinne/` turn a question into the top-K guide fragments (id, score, guide id, section, text, best first). A Development-only endpoint `GET /Search?query=...&topK=5` shows them, so the search can be tried from Swagger. Pinecone settings come from environment variables (`PineconeOptions`), validated at startup.
Test: `Tests/Infra/PineconeVectorTests.cs` (stubbed `HttpMessageHandler`) asserts the behavior that has decisions: results best first, guide metadata mapped, a match without metadata tolerated, the question embedded as `input_type=query`, and the index host looked up only once. Also verified manually against the real index.
Manual check: set `Pinecone__ApiKey` (or put it in `Api/appsettings.Local.json`), run the API, and in Swagger call `GET /Search` with `my card is declined even though I have funds`: results come best first, with `KB-01` among them, and the same question in Spanish also works. An empty `query` returns 400.

- [x] 2.1 Failing test — result: `FAILED: 5 of 5 — KeyNotFoundException at PineconeVector.cs:36` (a match without `metadata` crashes the mapping)
- [x] 2.2 Implementation — files: `Domain/Models/VectorChunk.cs`, `Domain/Ports/IVector.cs`, `Infra.Vector.Pinne/PineconeOptions.cs`, `Infra.Vector.Pinne/PineconeVector.cs` (missing metadata now defaults to empty), `Application/UseCases/ISearchGuidesUseCase.cs`, `Application/UseCases/SearchGuidesUseCase.cs`, `Api/Controllers/SearchController.cs`, `Api/DTO/SearchResultResponse.cs`, `Api/Program.cs` (DI), `Api/Api.csproj` (project references); `Api/Program.cs` loads `appsettings.Local.json`; tests `Tests/Infra/StubHandler.cs`, `Tests/Infra/PineconeVectorTests.cs`
- [x] 2.3 Passing test — result: `Passed! - Failed: 0, Passed: 14, Total: 14`; `dotnet build back-bot-test-bank.sln` 0 warnings, 0 errors; manual: `GET /Search` returns `KB-01` fragments best first, empty `query` returns 400
- [x] 2.4 Developer approval — approved after the manual check; naming settled as `Infra.Vector.Pinne`, `IVector`, `VectorChunk`, `PineconeVector`

## Phase 3a — Error contract and the global handler (covers SP-1, SP-2 contract)
Result: Domain defines `DomainException` with an error code from an enum (`InvalidRequest`, `Unreachable`, `Timeout`, `Rejected`, `RateLimited`, `Unavailable`, `NotFound`, `Malformed`), with no text and no HTTP. `Api` owns one table that maps each code to an HTTP status and a customer message; a global handler applies it and logs the real exception with a trace id. Controllers do not catch errors. Messages are generic (no stack trace, no inner text, no keys, no URLs), also in Development.

| Code | HTTP | Customer message |
|---|---|---|
| `InvalidRequest` | 400 | "The question is empty." |
| `Unreachable`, `Unavailable`, `Rejected`, `RateLimited`, `NotFound` | 503 | "The search service is unavailable. Please try again." |
| `Timeout` | 504 | "The search service took too long. Please try again." |
| `Malformed` | 502 | "The search service returned an unexpected response." |
| any other | 500 | "Something went wrong." |

Only the 400 and 500 rows get their own text. The 503 row uses one text for all five codes. The 503 `Rejected` and `NotFound` cases exist so the real cause goes to the log, not the client.

Test: `Tests/Api/ApiExceptionHandlerTests.cs` asserts, per code, the status and message from the table, that the body has no exception text or stack trace, that it carries a trace id, and that the exception is logged. `Tests/Application/SearchGuidesUseCaseTests.cs` asserts that an empty question raises `DomainException` with code `InvalidRequest`.
Manual check: `GET /Search?query=` returns 400 with the customer message; a forced 500 returns the generic message with a trace id; the console shows the same trace id with the full stack trace.

- [x] 3a.1 Failing test — result: `CS0103 The name 'ErrorCode' does not exist`, `CS0246 ... 'ApiExceptionHandler' could not be found`, `CS0234 ... 'Errors' does not exist in 'Api'` (build error, tests do not run); `SearchGuidesUseCaseTests` still expects `ArgumentException`
- [x] 3a.2 Implementation — files: `Domain/Exceptions/ErrorCode.cs` (new), `Domain/Exceptions/DomainException.cs` (new), `Domain/Exceptions/VectorUnavailableException.cs` (now a `DomainException` with code `Unavailable`), `Application/UseCases/SearchGuidesUseCase.cs` and `ISearchGuidesUseCase.cs` (throw `DomainException(InvalidRequest)`), `Api/Errors/ApiExceptionHandler.cs` (new: the code → status/message table and the logging), `Api/Controllers/SearchController.cs` (no `try/catch`; `query` is `string?` so the use case decides), `Api/Program.cs` (`AddProblemDetails`, `AddExceptionHandler`, `UseExceptionHandler`), `Tests/Tests.csproj` (`FrameworkReference` Microsoft.AspNetCore.App)
- [x] 3a.3 Passing test — result: `Passed! - Failed: 0, Passed: 25, Total: 25`; `dotnet build back-bot-test-bank.sln` 0 warnings, 0 errors. Manual: `GET /Search?query=%20` → 400 `{"status":400,"title":"Request failed","detail":"The question is empty.","traceId":"0HNPOMC5RDOEU:00000001"}`; the console shows `Request failed with InvalidRequest, trace 0HNPOMC5RDOEU:00000001`. A forced 500 is covered by `TryHandleAsync_UsesTheGenericMessageForAnyOtherException`
- [x] 3a.4 Developer approval — approved by the developer

## Phase 3c — Rate limit: 5 requests per minute per client (covers SP-3)
Result: the API rejects the 6th request from the same client IP within one minute with 429 and the customer message "You are sending requests too fast. Please wait a minute and try again.". The rejection happens before any controller or service runs, so Pinecone is not called. The limiter is one global limiter in the pipeline (`UseRateLimiter`), not an attribute on each controller, so new endpoints are covered automatically. Values come from `RateLimit__PermitLimit` and `RateLimit__WindowSeconds`, validated at startup like the Pinecone settings. The table gets a row for `TooManyRequests` (429), so this response has the same format as every other error.

| Code | HTTP | Customer message |
|---|---|---|
| `TooManyRequests` | 429 | "You are sending requests too fast. Please wait a minute and try again." |

Test: `Tests/Api/ApiExceptionHandlerTests.cs` gets a row for `TooManyRequests` (429 and the message). `Tests/Api/RateLimitOptionsTests.cs` asserts that startup validation names a missing `RateLimit__PermitLimit` or `RateLimit__WindowSeconds`, and rejects zero or negative values. The counting itself belongs to the framework and is verified manually.
Manual check: run the API and send 6 requests to `GET /Search?query=hello` within a minute: the first 5 get a search result (or 503 if Pinecone is not configured); the 6th gets 429 with the customer message. A minute later a request works again.

- [x] 3c.1 Failing test — result: `CS0117 'ErrorCode' does not contain a definition for 'TooManyRequests'` (ApiExceptionHandlerTests) and `CS0234 The type or namespace name 'RateLimiting' does not exist in the namespace 'Api'` (RateLimitOptionsTests); build error, tests do not run
- [x] 3c.2 Implementation — files: `Domain/Exceptions/ErrorCode.cs` (`TooManyRequests`), `Api/RateLimiting/RateLimitOptions.cs` (new, validated at startup), `Api/Errors/ApiExceptionHandler.cs` (429 row), `Api/Program.cs` (`AddRateLimiter` as a global per-IP fixed-window limiter, `UseRateLimiter` after `UseExceptionHandler`)
- [x] 3c.3 Passing test — result: `Passed! - Failed: 0, Passed: 29, Total: 29`; `dotnet build back-bot-test-bank.sln` 0 warnings, 0 errors. Manual, starting the API with only `Api/appsettings.Local.json` (`RateLimit` section, 5 per 60 seconds): requests 1 to 5 pass the limiter and the 6th returns 429 `{"status":429,"title":"Request failed","detail":"You are sending requests too fast. Please wait a minute and try again.","traceId":"..."}`. The `Retry-After` header was dropped from the phase: the exception handler clears response headers, and the message already tells the client to wait a minute.
- [x] 3c.4 Developer approval — approved by the developer
