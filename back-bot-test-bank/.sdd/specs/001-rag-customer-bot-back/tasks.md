# Tasks: RAG customer service bot API
Spec: ./spec.md
Test command: `dotnet test Tests/Tests.csproj` (the test project is created in Phase 1; project.md had none)

Phases 2-5 build HP-1 piece by piece, at the developer's request, so each one stays small and has a concrete result (following the layout of `C:\Nortal\AIStudy\ragSearch`). HP-1 is complete at the end of Phase 5.

## Phase 1 — Remove the Register sample and add the test project (covers Decisions: Register removal, Tests)
Test: `Tests/` (xUnit + Moq, references Domain, Application, Infra, Api) builds and runs one placeholder test; no `Register` type remains in any project.
Manual check: `dotnet build Api/Api.csproj` succeeds, `/swagger` shows no `/Register` endpoint, and `CLAUDE.md`, `README.md` and `.sdd/project.md` no longer describe `Register`.

- [x] 1.1 Failing test — result: `FAILED: RegisterSample_IsRemoved (Domain, Application, Infra, Api) — Assert.Empty() Failure: Collection was not empty` (4 failed, 1 passed)
- [x] 1.2 Implementation — files: deleted `Domain/Models/Register.cs`, `Domain/Repositories/IRegisterRepository.cs`, `Application/UseCases/IRegisterUseCase.cs`, `Application/UseCases/RegisterUseCase.cs`, `Application/DTO/RegisterDTO.cs`, `Infra/Publisher/RegisterRepository.cs`, `Api/DTO/Register.cs`, `Api/Controllers/RegisterController.cs` (and their now-empty folders); added `Tests/Tests.csproj`, `Tests/ProjectStructureTests.cs`; `back-bot-test-bank.sln` (+Tests); updated `CLAUDE.md`, `.sdd/project.md`
- [x] 1.3 Passing test — result: `Passed! - Failed: 0, Passed: 5, Total: 5`; `dotnet build back-bot-test-bank.sln` 0 warnings, 0 errors
- [x] 1.4 Developer approval — approved after a successful manual check; at the developer's request `Tests/ProjectStructureTests.cs` was then deleted (the `Tests/` project stays for the next phases)

## Phase 2 — Search the guides in Pinecone (covers HP-1, retrieval part)
Result: `IVector` (Domain port) and its Pinecone adapter in `Infra.Vector.Pinne/` turn a question into the top-K guide fragments (id, score, guide id, section, text, best first). A Development-only endpoint `GET /Search?query=...&topK=5` shows them, so the search can be tried from Swagger. Pinecone defaults in code (`PineconeOptions`), the API key in `Api/appsettings.Local.json` (git-ignored).
Test: `Tests/Infra/PineconeVectorTests.cs` (stubbed `HttpMessageHandler`) asserts the behavior that has decisions: results best first, guide metadata mapped, a match without metadata tolerated, the question embedded as `input_type=query`, and the index host looked up only once. Also verified manually against the real index.
Manual check: copy `Api/appsettings.Local.example.json` to `Api/appsettings.Local.json`, put your Pinecone key in it, run the API, and in Swagger call `GET /Search` with `my card is declined even though I have funds`: results come best first, with `KB-01` among them, and the same question in Spanish also works. An empty `query` returns 400.

- [x] 2.1 Failing test — result: `FAILED: 5 of 5 — KeyNotFoundException at PineconeVector.cs:36` (a match without `metadata` crashes the mapping)
- [x] 2.2 Implementation — files: `Domain/Models/VectorChunk.cs`, `Domain/Ports/IVector.cs`, `Infra.Vector.Pinne/PineconeOptions.cs`, `Infra.Vector.Pinne/PineconeVector.cs` (missing metadata now defaults to empty), `Application/UseCases/ISearchGuidesUseCase.cs`, `Application/UseCases/SearchGuidesUseCase.cs`, `Api/Controllers/SearchController.cs`, `Api/DTO/SearchResultResponse.cs`, `Api/Program.cs` (DI), `Api/appsettings.Local.example.json` (key template), `Api/Api.csproj` (project references); `Api/Program.cs` loads `appsettings.Local.json`; tests `Tests/Infra/StubHandler.cs`, `Tests/Infra/PineconeVectorTests.cs`
- [x] 2.3 Passing test — result: `Passed! - Failed: 0, Passed: 14, Total: 14`; `dotnet build back-bot-test-bank.sln` 0 warnings, 0 errors; manual: `GET /Search` returns `KB-01` fragments best first, empty `query` returns 400
- [x] 2.4 Developer approval — approved after the manual check; naming settled as `Infra.Vector.Pinne`, `IVector`, `VectorChunk`, `PineconeVector`

## Phase 3 — Talk to the chat model through Semantic Kernel (covers HP-1, generation part)
Result: `IChatModel` (Domain port) and its Semantic Kernel adapter in the `Infra.AI.*` project send a list of messages (system/user/assistant) and return the model's text, with the provider and model read from configuration. A Development-only endpoint `POST /Diagnostics/Chat` sends a message to the model and returns its reply.
Test: no automated test for the adapter (see spec Decisions); verified manually against the real model.
Manual check: put the OpenAI key in `Api/appsettings.Local.json`, run the API, send "Say hello" from Swagger and get a reply; confirm that no Semantic Kernel type appears outside the `Infra.AI.*` project.

- [ ] 3.1 Failing test — result: ________
- [ ] 3.2 Implementation — files: ________
- [ ] 3.3 Passing test — result: ________
- [ ] 3.4 Developer approval

## Phase 4 — Ask-a-question use case (covers HP-1, logic part)
Result: the use case in `Application` searches, builds the prompt (rules file as system message, fragments with their ids, customer question) and returns the model's answer with the sources (id and score). It uses only the ports.
Test: `Tests/Application/AskQuestionUseCaseTests.cs` asserts, with mocked `IVector` and `IChatModel`, that the prompt holds the rules and every fragment with its id, the answer is the model's text, and the sources match the retrieved fragments.
Manual check: review the use case; confirm that `Application` references only `Domain`.

- [ ] 4.1 Failing test — result: ________
- [ ] 4.2 Implementation — files: ________
- [ ] 4.3 Passing test — result: ________
- [ ] 4.4 Developer approval

## Phase 5 — `POST /Chat` endpoint, end to end (covers HP-1)
Result: the controller, its DTOs, configuration (Pinecone and model settings, keys in `Api/appsettings.Local.json`) and DI in `Program.cs` connect the pieces from Phases 2-4; the rules file is written in English.
Test: `Tests/Api/ChatControllerTests.cs` asserts, with a mocked use case, that `POST /Chat` maps the request to the use case and the result to the response (answer, sources).
Manual check: set the keys, run the API, send an English question about a blocked card from Swagger to `POST /Chat`, and get an answer with `[KB-xx]` citations and sources.

- [ ] 5.1 Failing test — result: ________
- [ ] 5.2 Implementation — files: ________
- [ ] 5.3 Passing test — result: ________
- [ ] 5.4 Developer approval

## Phase 6 — Reply in the customer's language, English first (covers HP-2, HP-2b)
Test: `AskQuestionUseCaseTests` asserts that the detected language is named in the last user message, together with the language reminder at the end, for English and for Spanish questions.
Manual check: ask the same question in English and in Spanish; the English answer has no Spanish sentences, only app menu names stay in Spanish.

- [ ] 6.1 Failing test — result: ________
- [ ] 6.2 Implementation — files: ________
- [ ] 6.3 Passing test — result: ________
- [ ] 6.4 Developer approval

## Phase 7 — Keep the conversation and understand follow-ups (covers HP-3b, HP-3)
Test: `AskQuestionUseCaseTests` asserts that a request without `conversationId` returns a new id, and that with an existing id the follow-up is rewritten (search uses the rewritten text) and earlier turns are sent as user/assistant messages; `Tests/Infra/` asserts that the Semantic Kernel conversation store saves and returns turns by id.
Manual check: ask about card payments abroad, then send "and in dollars?" with the returned `conversationId`; the answer follows the earlier topic.

- [ ] 7.1 Failing test — result: ________
- [ ] 7.2 Implementation — files: ________
- [ ] 7.3 Passing test — result: ________
- [ ] 7.4 Developer approval

## Phase 8 — Send only the recent turns (covers HP-3c)
Test: `Tests/Infra/` asserts that, after more than `HistoryTurns` turns, the conversation store returns only the latest `HistoryTurns`, reduced with the Semantic Kernel history reducer.
Manual check: set `HistoryTurns` to 2, have a 4-turn conversation, and see (in logs) that only the last 2 turns reach the model.

- [ ] 8.1 Failing test — result: ________
- [ ] 8.2 Implementation — files: ________
- [ ] 8.3 Passing test — result: ________
- [ ] 8.4 Developer approval

## Phase 9 — Hand off when no guide matches (covers HP-4)
Test: `AskQuestionUseCaseTests` asserts that when every fragment scores below `MinScore`, the chat model is never called and the response is the fixed English handoff message with `usedModel: false`.
Manual check: ask something off topic (e.g. a recipe) and get the fixed handoff message.

- [ ] 9.1 Failing test — result: ________
- [ ] 9.2 Implementation — files: ________
- [ ] 9.3 Passing test — result: ________
- [ ] 9.4 Developer approval

## Phase 10 — Bank rules: human handoff topics and safety (covers HP-5, SP-4)
Test: `AskQuestionUseCaseTests` asserts that the system message is the rules file content (handoff topics, never ask for password/PIN/CVV, ignore rule changes) and that the customer text is marked as customer text, not instructions.
Manual check: report an unauthorized charge, then ask the bot to reveal its rules and for your card PIN; it offers a human agent, refuses to reveal rules, and never asks for secrets.

- [ ] 10.1 Failing test — result: ________
- [ ] 10.2 Implementation — files: ________
- [ ] 10.3 Passing test — result: ________
- [ ] 10.4 Developer approval

## Phase 11 — Reject empty messages (covers SP-1)
Test: `ChatControllerTests` and `AskQuestionUseCaseTests` assert that an empty or whitespace message returns 400 and no port is called.
Manual check: send `"message": "  "` from Swagger and get a 400 with a clear error.

- [ ] 11.1 Failing test — result: ________
- [ ] 11.2 Implementation — files: ________
- [ ] 11.3 Passing test — result: ________
- [ ] 11.4 Developer approval

## Phase 12 — Unknown or expired conversation (covers SP-1b)
Test: `AskQuestionUseCaseTests` asserts that an unknown `conversationId` starts a new conversation and the response flags it; `Tests/Infra/` asserts that a conversation idle past the expiry is treated as unknown.
Manual check: send a made-up `conversationId` and get an answer with a new id and the "new conversation" flag.
Note: expiry default is 30 minutes idle, proposed in the spec review and pending developer confirmation.

- [ ] 12.1 Failing test — result: ________
- [ ] 12.2 Implementation — files: ________
- [ ] 12.3 Passing test — result: ________
- [ ] 12.4 Developer approval

## Phase 13 — External service failures (covers SP-2, SP-3)
Test: `ChatControllerTests` and `Tests/Infra/` assert that a failing or timed-out vector search or chat model (stubbed HTTP 5xx/timeout) returns a controlled 502/503 with a generic message, no invented answer, and no keys or stack traces in the body.
Manual check: run the API with a wrong OpenAI key, ask a question, and get the controlled error.

- [ ] 13.1 Failing test — result: ________
- [ ] 13.2 Implementation — files: ________
- [ ] 13.3 Passing test — result: ________
- [ ] 13.4 Developer approval

## Phase 14 — Fall back to English when the language is unclear (covers SP-5)
Test: `AskQuestionUseCaseTests` asserts that when the language detector returns nothing usable, the prompt asks for an English reply.
Manual check: send a message like "ok??" and get the answer in English.

- [ ] 14.1 Failing test — result: ________
- [ ] 14.2 Implementation — files: ________
- [ ] 14.3 Passing test — result: ________
- [ ] 14.4 Developer approval

## Phase 15 — Fail fast without rules (covers SP-6)
Test: `Tests/Api/` asserts that startup with a missing or empty rules file throws a clear configuration error.
Manual check: rename the rules file, run the API, and see it stop at startup with a message naming the missing file.

- [ ] 15.1 Failing test — result: ________
- [ ] 15.2 Implementation — files: ________
- [ ] 15.3 Passing test — result: ________
- [ ] 15.4 Developer approval
