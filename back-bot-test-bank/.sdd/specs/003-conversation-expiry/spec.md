# Conversation expiry
Size: complex — a new Domain entity (`Conversation`) that owns the expiry rule, changes to the `IConversationStore` port, the use case and the store, and one configuration value.

## Goal
A conversation that nobody uses for 10 minutes is cleared by the server, so memory does not grow forever and old context does not leak into a much later chat. The rule is a business rule, so it lives in Domain.

## Happy path
- HP-1: Given a conversation with turns, when the customer sends another message within 10 minutes of the last activity, then the context is kept and the idle time starts again.
- HP-2: Given a conversation idle for more than 10 minutes, when the cleanup runs, then the server removes it without any request and frees its memory.

## Sad paths
- SP-1: Given a `conversationId` whose conversation is idle past 10 minutes (cleaned or not yet), when a message is sent, then it is treated as unknown (no turns) and a new conversation starts, as in spec 002 SP-1b.
- SP-2: Given `Chat__ConversationIdleMinutes` is missing or not a positive number, when the app starts, then it fails fast with a message naming the setting.

## Out of scope
- Telling the customer the conversation was reset (spec 002 Phase 12 owns the response flag).
- A cap on turns or on the number of conversations; durable storage.

## Touches
- `Domain/`: new entity `Conversation` (id, turns, last activity) with the rule `IsExpired(now, idle)` and `AddTurn(turn, now)`; `IConversationStore` returns and saves `Conversation`, and can remove expired ones.
- `Application/`: `AskQuestionUseCase` asks the entity (`IsExpired`, `IsFollowUp`) instead of deciding itself; it gets the current time from `TimeProvider`.
- `Infra.AI.SemanticKernel`: `SemanticKernelConversationStore` keeps `Conversation` and removes the ones the Domain rule marks as expired (physical cleanup only, no rule of its own).
- `Api/`: reads and validates `Chat__ConversationIdleMinutes`, wires `TimeProvider` and the periodic cleanup in `Program.cs`.

## Decisions and open questions
- Priority: this is the next change to implement on the back end, before the pending phases of spec 002 (8 to 14). Spec 002 Phase 12 keeps only the "new conversation" flag in the response.
- 10 minutes of inactivity, set by the developer (replaces the 15 and the 30 proposed earlier). The value comes from `Chat__ConversationIdleMinutes`, never from code (project.md rule); Domain receives it as a `TimeSpan`.
- Rule in Domain, decided by the developer: Domain decides when a conversation is expired; Application orchestrates; Infra only deletes what the rule marks. Domain gets the time as a parameter (`now`), so it holds no clock and references nothing.
- Idle time counts from the last turn added or read (sliding expiry).
- Cleanup runs every `Chat__ConversationIdleMinutes` as well (no second setting); an expired conversation found before the cleanup runs is already treated as unknown (SP-1), so the timing of the cleanup never changes what the customer sees.
- Tests use a fake `TimeProvider`, never real waiting. Domain rule tests need no mocks.
