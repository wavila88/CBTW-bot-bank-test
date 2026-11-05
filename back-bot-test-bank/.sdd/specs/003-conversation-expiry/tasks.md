# Tasks: Conversation expiry
Spec: ./spec.md
Test command: `dotnet test Tests/Tests.csproj`

Phase numbers start at 1 in each spec (a phase is named with its spec, e.g. "spec 003 phase 1"). This spec is the developer's next priority: build it before Phase 8 of spec 002.

## Architecture rules for every phase
Check these at each approval (4th task of the phase), besides the tests.
- **Domain** (`Domain/`): `Conversation` owns the expiry rule and the follow-up rule. It references nothing: no `TimeProvider`, no clock, no package, no configuration. It receives `now` and the idle `TimeSpan` as parameters.
- **Application** (`Application/`): `AskQuestionUseCase` only orchestrates ports and asks the entity (`IsFollowUp`, `IsExpired`). It holds no expiry rule of its own. It gets the time from `TimeProvider` (a .NET base type, not an external package) and references only `Domain`.
- **Infra** (`Infra.AI.SemanticKernel/`): `SemanticKernelConversationStore` stores and physically removes. It decides nothing about when a conversation is expired: it asks `Conversation.IsExpired`. No Semantic Kernel type leaves this project.
- **Api** (`Api/`): composition root. It reads and validates `Chat__ConversationIdleMinutes` (no default in code), wires `TimeProvider` and the cleanup. No business rule here.
- **Dependency direction** unchanged: `Application → Domain`, `Infra.* → Domain`, `Api → everything`. A new `using` that breaks this is a failed phase.
- **Manual mapping** between layers, no mapper library. Tests: xUnit + Moq, fake `TimeProvider`, no real waiting, no real network.

## Phase 1 — A conversation idle for 10 minutes is forgotten, and the rule lives in Domain (covers HP-1, SP-1, SP-2)
Result: the business rule lives in Domain: `Conversation.ShouldForget(found, now, idle)` says a known conversation idle for more than the idle time must be forgotten. `IConversationStore` keeps its turn methods and gains `FindAsync` and `RemoveAsync`; the store records the last activity when a turn is added. `AskQuestionUseCase` only asks Domain and, when it says to forget, removes the conversation so the question starts a new one. The idle time (10 minutes) comes from the environment variable `Chat__ConversationIdleMinutes`, read and validated in `Api/Program.cs` (missing or not positive stops the app naming it) and passed to Domain as a parameter; Domain reads no configuration. Minimum change, only two new tests.
Test: `Tests/Domain/ConversationTests.cs` (no mocks: not forgotten up to the idle time, forgotten after it, an unknown conversation is not forgotten). `Tests/Application/AskQuestionUseCaseTests.cs` (a conversation idle past the idle time starts new: no earlier turns in the prompt; fake `TimeProvider`). The startup validation (SP-2) is checked by hand.
Manual check: set `Chat__ConversationIdleMinutes` to 1, ask a question, ask a follow-up inside the minute (context kept), wait over a minute and send the same `conversationId`: the answer no longer follows the earlier topic. Then unset the variable and see the startup error naming it.

- [x] 1.1 Failing test — result: FAILED: build errors, `Conversation` and `FindAsync`/`RemoveAsync` do not exist yet
- [x] 1.2 Implementation — files: `Domain/Models/Conversation.cs` (new), `Domain/Ports/IConversationStore.cs`, `Application/UseCases/AskQuestionUseCase.cs`, `Infra.AI.SemanticKernel/SemanticKernelConversationStore.cs`, `Api/Program.cs`, `Api/appsettings.Local.json` (git-ignored, 1 minute for the manual test)
- [x] 1.3 Passing test — result: Passed! 57 passed, 0 failed (`dotnet test Tests/Tests.csproj`)
- [x] 1.4 Developer approval (including the architecture rules above)

## Phase 2 — Clean idle conversations without a request (covers HP-2)
Result: a periodic cleanup removes the conversations that `Conversation.IsExpired` marks, so memory is freed with no traffic. The port gets one method to remove expired conversations (the rule stays in Domain; Infra only deletes). The cleanup runs in `Api` as a hosted background service, every `Chat__ConversationIdleMinutes`, and it is the only new piece in `Api`.
Test: `Tests/Infra/SemanticKernelConversationStoreTests.cs` (the removal deletes only the conversations the rule marks as expired and keeps the active ones, with a fake `TimeProvider`). `Tests/Api/` (one cleanup pass calls the removal with the idle time; the loop itself is not tested with real waiting).
Manual check: set `Chat__ConversationIdleMinutes` to 1, create a conversation, wait about two minutes without any request, and confirm in a log line (count of removed conversations, no content) that it was cleaned.

- [ ] 2.1 Failing test — result: ________
- [ ] 2.2 Implementation — files: ________
- [ ] 2.3 Passing test — result: ________
- [ ] 2.4 Developer approval (including the architecture rules above)
