# Tasks: RAG customer service bot API
Spec: ./spec.md
Test command: `dotnet test Tests/Tests.csproj` (the test project is created in Phase 1; project.md had none)

## Phase 1 — Remove the Register sample and add the test project (covers Decisions: Register removal, Tests)
Test: `Tests/` (xUnit + Moq, references Domain, Application, Infra, Api) builds and runs one placeholder test; no `Register` type remains in any project.
Manual check: `dotnet build Api/Api.csproj` succeeds, `/swagger` shows no `/Register` endpoint, and `CLAUDE.md`, `README.md` and `.sdd/project.md` no longer describe `Register`.

- [x] 1.1 Failing test — result: `FAILED: RegisterSample_IsRemoved (Domain, Application, Infra, Api) — Assert.Empty() Failure: Collection was not empty` (4 failed, 1 passed)
- [x] 1.2 Implementation — files: deleted `Domain/Models/Register.cs`, `Domain/Repositories/IRegisterRepository.cs`, `Application/UseCases/IRegisterUseCase.cs`, `Application/UseCases/RegisterUseCase.cs`, `Application/DTO/RegisterDTO.cs`, `Infra/Publisher/RegisterRepository.cs`, `Api/DTO/Register.cs`, `Api/Controllers/RegisterController.cs` (and their now-empty folders); added `Tests/Tests.csproj`, `Tests/ProjectStructureTests.cs`; `back-bot-test-bank.sln` (+Tests); updated `CLAUDE.md`, `.sdd/project.md`
- [x] 1.3 Passing test — result: `Passed! - Failed: 0, Passed: 5, Total: 5`; `dotnet build back-bot-test-bank.sln` 0 warnings, 0 errors
- [x] 1.4 Developer approval — approved after a successful manual check; at the developer's request `Tests/ProjectStructureTests.cs` was then deleted (the `Tests/` project stays for the next phases)

## Phase 2 — Answer a question from the guides (covers HP-1)
Test: `Tests/Application/AskQuestionUseCaseTests.cs` asserts that, with mocked `IRetriever`/`IChatModel`, the answer comes from the model, the prompt holds the system rules plus the retrieved fragments with their ids, and the response lists the sources (id and score); `Tests/Infra/` asserts that the Pinecone and chat adapters send and parse the expected HTTP calls (stubbed `HttpMessageHandler`); `Tests/Api/ChatControllerTests.cs` asserts that `POST /Chat` maps request and response.
Manual check: run the API, send an English question about a blocked card from Swagger to `POST /Chat`, and get an answer with `[KB-xx]` citations and sources.

- [ ] 2.1 Failing test — result: ________
- [ ] 2.2 Implementation — files: ________
- [ ] 2.3 Passing test — result: ________
- [ ] 2.4 Developer approval

## Phase 3 — Reply in the customer's language, English first (covers HP-2, HP-2b)
Test: `AskQuestionUseCaseTests` asserts that the detected language is named in the last user message, together with the language reminder at the end, for English and for Spanish questions.
Manual check: ask the same question in English and in Spanish; the English answer has no Spanish sentences, only app menu names stay in Spanish.

- [ ] 3.1 Failing test — result: ________
- [ ] 3.2 Implementation — files: ________
- [ ] 3.3 Passing test — result: ________
- [ ] 3.4 Developer approval

## Phase 4 — Keep the conversation and understand follow-ups (covers HP-3b, HP-3)
Test: `AskQuestionUseCaseTests` asserts that a request without `conversationId` returns a new id, and that with an existing id the follow-up is rewritten (search uses the rewritten text) and earlier turns are sent as user/assistant messages; `Tests/Infra/` asserts that the Semantic Kernel conversation store saves and returns turns by id.
Manual check: ask about card payments abroad, then send "and in dollars?" with the returned `conversationId`; the answer follows the earlier topic.

- [ ] 4.1 Failing test — result: ________
- [ ] 4.2 Implementation — files: ________
- [ ] 4.3 Passing test — result: ________
- [ ] 4.4 Developer approval

## Phase 5 — Send only the recent turns (covers HP-3c)
Test: `Tests/Infra/` asserts that, after more than `HistoryTurns` turns, the conversation store returns only the latest `HistoryTurns`, reduced with the Semantic Kernel history reducer.
Manual check: set `HistoryTurns` to 2, have a 4-turn conversation, and see (in logs) that only the last 2 turns reach the model.

- [ ] 5.1 Failing test — result: ________
- [ ] 5.2 Implementation — files: ________
- [ ] 5.3 Passing test — result: ________
- [ ] 5.4 Developer approval

## Phase 6 — Hand off when no guide matches (covers HP-4)
Test: `AskQuestionUseCaseTests` asserts that when every fragment scores below `MinScore`, the chat model is never called and the response is the fixed English handoff message with `usedModel: false`.
Manual check: ask something off topic (e.g. a recipe) and get the fixed handoff message.

- [ ] 6.1 Failing test — result: ________
- [ ] 6.2 Implementation — files: ________
- [ ] 6.3 Passing test — result: ________
- [ ] 6.4 Developer approval

## Phase 7 — Bank rules: human handoff topics and safety (covers HP-5, SP-4)
Test: `AskQuestionUseCaseTests` asserts that the system message is the rules file content (handoff topics, never ask for password/PIN/CVV, ignore rule changes) and that the customer text is marked as customer text, not instructions.
Manual check: report an unauthorized charge, then ask the bot to reveal its rules and for your card PIN; it offers a human agent, refuses to reveal rules, and never asks for secrets.

- [ ] 7.1 Failing test — result: ________
- [ ] 7.2 Implementation — files: ________
- [ ] 7.3 Passing test — result: ________
- [ ] 7.4 Developer approval

## Phase 8 — Reject empty messages (covers SP-1)
Test: `ChatControllerTests` and `AskQuestionUseCaseTests` assert that an empty or whitespace message returns 400 and no port is called.
Manual check: send `"message": "  "` from Swagger and get a 400 with a clear error.

- [ ] 8.1 Failing test — result: ________
- [ ] 8.2 Implementation — files: ________
- [ ] 8.3 Passing test — result: ________
- [ ] 8.4 Developer approval

## Phase 9 — Unknown or expired conversation (covers SP-1b)
Test: `AskQuestionUseCaseTests` asserts that an unknown `conversationId` starts a new conversation and the response flags it; `Tests/Infra/` asserts that a conversation idle past the expiry is treated as unknown.
Manual check: send a made-up `conversationId` and get an answer with a new id and the "new conversation" flag.
Note: expiry default is 30 minutes idle, proposed in the spec review and pending developer confirmation.

- [ ] 9.1 Failing test — result: ________
- [ ] 9.2 Implementation — files: ________
- [ ] 9.3 Passing test — result: ________
- [ ] 9.4 Developer approval

## Phase 10 — External service failures (covers SP-2, SP-3)
Test: `ChatControllerTests` and `Tests/Infra/` assert that a failing or timed-out vector search or chat model (stubbed HTTP 5xx/timeout) returns a controlled 502/503 with a generic message, no invented answer, and no keys or stack traces in the body.
Manual check: run the API with a wrong OpenAI key, ask a question, and get the controlled error.

- [ ] 10.1 Failing test — result: ________
- [ ] 10.2 Implementation — files: ________
- [ ] 10.3 Passing test — result: ________
- [ ] 10.4 Developer approval

## Phase 11 — Fall back to English when the language is unclear (covers SP-5)
Test: `AskQuestionUseCaseTests` asserts that when the language detector returns nothing usable, the prompt asks for an English reply.
Manual check: send a message like "ok??" and get the answer in English.

- [ ] 11.1 Failing test — result: ________
- [ ] 11.2 Implementation — files: ________
- [ ] 11.3 Passing test — result: ________
- [ ] 11.4 Developer approval

## Phase 12 — Fail fast without rules (covers SP-6)
Test: `Tests/Api/` asserts that startup with a missing or empty rules file throws a clear configuration error.
Manual check: rename the rules file, run the API, and see it stop at startup with a message naming the missing file.

- [ ] 12.1 Failing test — result: ________
- [ ] 12.2 Implementation — files: ________
- [ ] 12.3 Passing test — result: ________
- [ ] 12.4 Developer approval
