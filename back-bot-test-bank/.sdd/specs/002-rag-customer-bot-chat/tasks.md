# Tasks: RAG customer service bot — chat
Spec: ./spec.md
Test command: `dotnet test Tests/Tests.csproj`

Continues from `001-guide-search/tasks.md` (phases 1 to 3c, the guide search and the error contract, are done or pending there). Phase numbers continue from spec 001 so references stay stable. Each phase has a concrete result and ends with something the developer can run.

## Phase 4 — Talk to the chat model through Semantic Kernel (covers HP-1, generation part)
Result: `IChatModel` (Domain port) and its Semantic Kernel adapter in the `Infra.AI.SemanticKernel` project send a list of messages (system/user/assistant) and return the model's text, with the provider and model read from configuration (`Chat__Model`). A Development-only endpoint `POST /Diagnostics/Chat` checked it from Swagger; it was removed in Phase 5, once `POST /Chat` existed.
Test: `Tests/Infra/OpenAIChatOptionsTests.cs` (startup validation names `OpenAI__ApiKey` and `Chat__Model` when missing), `Tests/Infra/ChatHistoryMapperTests.cs` (the domain messages keep their roles and order when mapped to Semantic Kernel's history). The call to the real model is verified manually.
Manual check: set `OpenAI__ApiKey` (or put it in `Api/appsettings.Local.json`), run the API, send "Say hello" from Swagger and get a reply; confirm that no Semantic Kernel type appears outside the `Infra.AI.SemanticKernel` project.

- [x] 4.1 Failing test — result: `CS0234 The type or namespace name 'AI' does not exist in the namespace 'Infra'` (OpenAIChatOptionsTests, ChatHistoryMapperTests); build error, tests do not run. The empty project `Infra.AI.SemanticKernel` (Semantic Kernel 1.81) was created first so the tests can reference it
- [x] 4.2 Implementation — files: (the diagnostic endpoint, `TalkToModelUseCase`, their DTOs and tests named below were removed in Phase 5) `Domain/Models/ChatRole.cs`, `Domain/Models/ChatMessage.cs`, `Domain/Ports/IChatModel.cs` (new); `Infra.AI.SemanticKernel/` (new project: `OpenAIChatOptions.cs`, `ChatHistoryMapper.cs`, `SemanticKernelChatModel.cs`, `ServiceCollectionExtensions.cs`); `Application/UseCases/ITalkToModelUseCase.cs` and `TalkToModelUseCase.cs` (new); `Api/Controllers/DiagnosticsController.cs`, `Api/DTO/TalkRequest.cs`, `Api/DTO/TalkResponse.cs` (new); `Api/Program.cs` (options, `AddSemanticKernelChatModel`, use case), `Api/Api.csproj`, `Tests/Tests.csproj`, `back-bot-test-bank.sln` (new project); `Api/appsettings.Local.json` (`Chat:Model`, not committed); tests `Tests/Infra/OpenAIChatOptionsTests.cs`, `Tests/Infra/ChatHistoryMapperTests.cs`, `Tests/Application/TalkToModelUseCaseTests.cs`
- [x] 4.3 Passing test — result: `Passed! - Failed: 0, Passed: 35, Total: 35`; `dotnet build back-bot-test-bank.sln` 0 warnings, 0 errors. Manual, without an OpenAI key: startup stops with `Missing settings: OpenAI__ApiKey`. With a fake key: `POST /Diagnostics/Chat` with an empty message gives 400; with "Say hello" the call reaches OpenAI (the log shows `HTTP 401 invalid_api_key`) and the client gets the generic 500. Real model, checked by the developer: three messages through `POST /Diagnostics/Chat` (unauthorized charges, how to block a card, a thank-you) all got replies. Each call is independent, with no history and no guides, so the third reply had no context; that is expected until phases 5, 6 and 8
- [x] 4.4 Developer approval — approved by the developer after the real-model check

## Phase 5 — Ask a question end to end: `POST /Chat` (covers HP-1)
Result, first half (done): the use case in `Application` searches, builds the prompt (rules as the system message, fragments with their ids, customer question) and returns the model's answer with the sources (id and score). It uses only the ports.
Result, second half: `POST /Chat` exposes it. `ChatController` and its DTOs, the rules file `Api/prompts/system.en.md` (in English, path from `Chat__RulesPath`, read at startup) and the DI in `Program.cs`. The file holds only the basic rules (role, answer from the context, reply language, citations); the handoff topics and the safety rules are Phase 10. Errors go through the spec 001 handler, not the controller. This phase merges the former Phases 5 and 6 so it can be tried end to end from Swagger.
Test: `Tests/Application/AskQuestionUseCaseTests.cs` asserts, with mocked `IVector` and `IChatModel`, that the prompt holds the rules and every fragment with its id, the answer is the model's text, and the sources match the retrieved fragments. `Tests/Api/ChatControllerTests.cs` asserts, with a mocked use case, that `POST /Chat` maps the request to the use case and the result to the response (answer, sources).
Manual check: run the API with the Pinecone and OpenAI keys and `Chat__RulesPath` set, send an English question about a blocked card from Swagger to `POST /Chat`, and get an answer with `[KB-xx]` citations and the sources. Confirm that `Application` references only `Domain`.

- [x] 5.1 Failing test — result: `CS0246 The type or namespace name 'AskQuestionUseCase' could not be found` in `Tests/Application/AskQuestionUseCaseTests.cs`; build error, tests do not run
- [x] 5.2 Implementation — files: `Application/UseCases/IAskQuestionUseCase.cs`, `Application/UseCases/AskQuestionUseCase.cs`, `Application/Prompts/PromptBuilder.cs`, `Application/DTO/AnswerDTO.cs`, `Application/DTO/SourceDTO.cs` (all new); test `Tests/Application/AskQuestionUseCaseTests.cs`. The rules text and `topK` arrive through the constructor, so `Application` reads no file and no configuration. Not registered in `Program.cs` yet: that is the second half of this phase
- [x] 5.3 Passing test — result: `Passed! - Failed: 0, Passed: 39, Total: 39`; `dotnet build back-bot-test-bank.sln` 0 warnings, 0 errors. Manual check: `Application/Application.csproj` has a single reference, `Domain`, and no package
- [x] 5.4 Failing test (endpoint) — result: `CS0246 The type or namespace name 'ChatController' could not be found` in `Tests/Api/ChatControllerTests.cs`; build error, tests do not run
- [x] 5.5 Implementation (endpoint) — files: `Api/Controllers/ChatController.cs`, `Api/DTO/ChatRequest.cs`, `Api/DTO/ChatResponse.cs`, `Api/DTO/SourceResponse.cs`, `Api/prompts/system.en.md` (the rules, in English), `Api/Program.cs` (reads `Chat:RulesPath` and the rules file, registers `IAskQuestionUseCase`), `Api/Api.csproj` (copies `prompts/*.md`), `Api/appsettings.Local.json` (`Chat:RulesPath`, not committed); test `Tests/Api/ChatControllerTests.cs`. At the developer's request, 6 low-value tests were also removed (the three "passes when set" validations, the duplicate rate-limit test, "best match first" and "host looked up once"). Also removed the Phase 4 diagnostic endpoint (`DiagnosticsController`, `TalkToModelUseCase`, their DTOs and tests)
- [x] 5.6 Passing test (endpoint) — result: `Passed! - Failed: 0, Passed: 32, Total: 32`; `dotnet build back-bot-test-bank.sln` 0 warnings, 0 errors. Manual `POST /Chat`: the API starts and the request reaches Pinecone, which answers 401 (the Pinecone key in `appsettings.Local.json` looks cut: it lacks its last character), so the client gets the 503 customer message with a trace id and the log shows `answered 401`. OPEN: the real answer with `[KB-xx]` citations still needs a valid Pinecone key
- [x] 5.7 Developer approval — approved by the developer after a real answer from POST /Chat; the reply language (it answered in English to a Spanish question) is Phase 6

## Phase 6 — Reply in the customer's language, English first (covers HP-2, HP-2b, SP-5)
Result: the bot answers in the language of the question with a single model call. `PromptBuilder` ends the user message with an explicit language reminder, next to the question: identify the language of the question, write the whole reply in it even though the context is in Spanish, and reply in English when the language cannot be told. The rules alone did not hold against a prompt in English around guides in Spanish. A first design asked the model for the language in a separate call; the developer asked to drop it because it doubled the model calls and tokens of every question. The reminder also gives the English fallback of SP-5, so the former Phase 14 is gone.
Test: `Tests/Application/AskQuestionUseCaseTests.cs` asserts that a question costs exactly one model call and that the user message ends with the language reminder.
Manual check: ask in English, in Spanish and with very short phrases; an English question gets an English reply and a Spanish question a Spanish reply, and only app menu names stay in Spanish.

- [x] 6.1 Failing test — result: `CS0117 'PromptBuilder' does not contain a definition for 'LanguageReminder'` and `CS0246/CS0103 'LanguageDetector' could not be found` (the first, two-call version of the tests); build error, tests do not run
- [x] 6.2 Implementation — files: `Application/Prompts/PromptBuilder.cs` (`LanguageReminder` at the end of the user message), `Application/UseCases/AskQuestionUseCase.cs` (one model call); `Api/Program.cs` (the environment variables are added again after `appsettings.Local.json`, so they win as `project.md` says; found while testing); test `Tests/Application/AskQuestionUseCaseTests.cs` (adapted). A `LanguageDetector` and its tests were built first and then removed
- [x] 6.3 Passing test — result: `Passed! - Failed: 0, Passed: 33, Total: 33`; `dotnet build back-bot-test-bank.sln` 0 warnings, 0 errors; `Application` references only `Domain`. Manual with the real services, one call per question: English got English ("My card was lost, how do I block it?", "Help me to block my card", "What is my daily limit?", "ok, thanks", "ok"); Spanish got Spanish ("Perdi mi tarjeta, como la bloqueo?", "gracias", "hola"). Two misses: "Quiero hablar con un agente" was answered in English, and a Portuguese question without accents ("Como bloqueio meu cartao perdido?") was answered in Spanish
- [x] 6.4 Developer approval — approved by the developer, accepting the single-call design and the two language misses (Phase 10 can reinforce the Spanish case)

## Phase 7 — Keep the conversation and understand follow-ups (covers HP-3b, HP-3)
Test: `AskQuestionUseCaseTests` asserts that a request without `conversationId` returns a new id, and that with an existing id the follow-up is rewritten (search uses the rewritten text) and earlier turns are sent as user/assistant messages; `Tests/Infra/` asserts that the Semantic Kernel conversation store saves and returns turns by id.
Manual check: ask about card payments abroad, then send "and in dollars?" with the returned `conversationId`; the answer follows the earlier topic.

- [ ] 7.1 Failing test — result: ________
- [ ] 7.2 Implementation — files: ________
- [ ] 7.3 Passing test — result: ________
- [ ] 7.4 Developer approval

## Phase 8 — Send only the recent turns (covers HP-3c)
Test: `Tests/Infra/` asserts that, after more than `Chat__HistoryTurns` turns, the conversation store returns only the latest turns, reduced with the Semantic Kernel history reducer.
Manual check: set `Chat__HistoryTurns` to 2, have a 4-turn conversation, and see (in logs) that only the last 2 turns reach the model.

- [ ] 8.1 Failing test — result: ________
- [ ] 8.2 Implementation — files: ________
- [ ] 8.3 Passing test — result: ________
- [ ] 8.4 Developer approval

## Phase 9 — Hand off when no guide matches (covers HP-4)
Test: `AskQuestionUseCaseTests` asserts that when every fragment scores below `Chat__MinScore`, the chat model is never called and the response is the fixed English handoff message with `usedModel: false`.
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

## Phase 11 — Reject empty messages on the chat (covers SP-1)
Test: `ChatControllerTests` and `AskQuestionUseCaseTests` assert that an empty or whitespace message raises `DomainException` with code `InvalidRequest`, that no port is called, and that the response is the 400 from the spec 001 table.
Manual check: send `"message": "  "` from Swagger to `POST /Chat` and get a 400 with the customer message and a trace id.

- [ ] 11.1 Failing test — result: ________
- [ ] 11.2 Implementation — files: ________
- [ ] 11.3 Passing test — result: ________
- [ ] 11.4 Developer approval

## Phase 12 — Unknown or expired conversation (covers SP-1b)
Test: `AskQuestionUseCaseTests` asserts that an unknown `conversationId` starts a new conversation and the response flags it; `Tests/Infra/` asserts that a conversation idle past `Chat__ConversationIdleMinutes` is treated as unknown.
Manual check: send a made-up `conversationId` and get an answer with a new id and the "new conversation" flag.
Note: the idle expiry value (proposed 30 minutes) is pending developer confirmation; it comes from configuration, not code.

- [ ] 12.1 Failing test — result: ________
- [ ] 12.2 Implementation — files: ________
- [ ] 12.3 Passing test — result: ________
- [ ] 12.4 Developer approval

## Phase 13 — Chat model failures (covers SP-3)
Result: the `Infra.AI.SemanticKernel` adapter translates each failure of the chat model into a spec 001 code, the same way `Infra.Vector.Pinne` does for Pinecone: network errors → `Unreachable`, timeouts → `Timeout`, 401/403 → `Rejected`, 429 → `RateLimited`, 5xx → `Unavailable`, response without the expected shape → `Malformed`. The spec 001 handler gives the customer the table's message and status, with no keys or internals. No new exception type.
Test: `Tests/Infra/` asserts, with a stubbed `HttpMessageHandler`, the mapping for each row (status, timeout and network failure) and that the inner exception keeps the original status for the log.
Manual check: run the API with a wrong OpenAI key, ask a question, and get a 503 with the customer message and a trace id; the console shows `Rejected` with `401`.

- [ ] 13.1 Failing test — result: ________
- [ ] 13.2 Implementation — files: ________
- [ ] 13.3 Passing test — result: ________
- [ ] 13.4 Developer approval

## Phase 14 — Fail fast without rules (covers SP-6)
Test: `Tests/Api/` asserts that startup with a missing or empty rules file throws a clear configuration error.
Manual check: rename the rules file, run the API, and see it stop at startup with a message naming the missing file.

- [ ] 14.1 Failing test — result: ________
- [ ] 14.2 Implementation — files: ________
- [ ] 14.3 Passing test — result: ________
- [ ] 14.4 Developer approval
