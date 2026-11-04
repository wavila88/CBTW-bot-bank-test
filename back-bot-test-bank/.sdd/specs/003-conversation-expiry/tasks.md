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

## Phase 1 — A conversation idle for 15 minutes is forgotten, and the rule lives in Domain (covers HP-1, SP-1, SP-2)
Result: the new entity `Conversation` (Domain) holds the id, the turns and the last activity, and decides `IsFollowUp` and `IsExpired(now, idle)`. `IConversationStore` returns and saves `Conversation`. `AskQuestionUseCase` asks the entity instead of testing `history.Count`; an expired conversation is treated as unknown and a new one starts; a message within the idle time keeps the context and restarts the idle time. `Chat__ConversationIdleMinutes` (15) is read and validated at startup, and a missing or non-positive value stops the app naming the setting.
Test: `Tests/Domain/ConversationTests.cs` (no mocks: not expired at exactly the idle time, expired after it, adding a turn restarts the idle time, `IsFollowUp` false when empty). `Tests/Application/AskQuestionUseCaseTests.cs` (a conversation inside the idle time keeps its context; an expired one starts new and is not rewritten as a follow-up; fake `TimeProvider`). `Tests/Infra/SemanticKernelConversationStoreTests.cs` (the store returns the saved `Conversation` and keeps ids apart). `Tests/Api/` (startup validation names `Chat__ConversationIdleMinutes` when missing, zero or negative).
Manual check: set `Chat__ConversationIdleMinutes` to 1, ask a question, ask a follow-up inside the minute (context kept), wait over a minute and send the same `conversationId`: the answer no longer follows the earlier topic. Then unset the variable and see the startup error naming it.

- [ ] 1.1 Failing test — result: ________
- [ ] 1.2 Implementation — files: ________
- [ ] 1.3 Passing test — result: ________
- [ ] 1.4 Developer approval (including the architecture rules above)

## Phase 2 — Clean idle conversations without a request (covers HP-2)
Result: a periodic cleanup removes the conversations that `Conversation.IsExpired` marks, so memory is freed with no traffic. The port gets one method to remove expired conversations (the rule stays in Domain; Infra only deletes). The cleanup runs in `Api` as a hosted background service, every `Chat__ConversationIdleMinutes`, and it is the only new piece in `Api`.
Test: `Tests/Infra/SemanticKernelConversationStoreTests.cs` (the removal deletes only the conversations the rule marks as expired and keeps the active ones, with a fake `TimeProvider`). `Tests/Api/` (one cleanup pass calls the removal with the idle time; the loop itself is not tested with real waiting).
Manual check: set `Chat__ConversationIdleMinutes` to 1, create a conversation, wait about two minutes without any request, and confirm in a log line (count of removed conversations, no content) that it was cleaned.

- [ ] 2.1 Failing test — result: ________
- [ ] 2.2 Implementation — files: ________
- [ ] 2.3 Passing test — result: ________
- [ ] 2.4 Developer approval (including the architecture rules above)
