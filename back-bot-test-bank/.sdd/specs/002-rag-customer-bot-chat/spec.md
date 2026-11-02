# RAG customer service bot: chat
Size: complex — new chat endpoint, conversation state, language handling and a second connector (the chat model). Builds on spec 001, which provides the guide search and the error contract.

## Goal
A customer of "Bank Test" sends a message to the API and gets an answer in the language they wrote (English first), based only on the bank's guides, with citations, and the conversation keeps its context. Behavior is ported from the working reference `C:\Nortal\AIStudy\ragCustomerBot`, but exposed as a Web API and built with the project's hexagonal rules.

## Happy path
- HP-1 (generation part): Given guides that cover the question, when the customer sends a message to `POST /Chat`, then the API returns an answer built only from the retrieved fragments, with citations (e.g. `[KB-01]`) and the sources used (id and score). The search itself is spec 001 HP-1.
- HP-2 (priority): Given the customer writes in English, when the guides are in Spanish, then the answer is fully in natural English, with no Spanish sentences, keeping only app menu/button names untranslated.
- HP-2b: Given the customer writes in another language (e.g. Spanish, Portuguese), then the answer is written in that language.
- HP-3: Given a `conversationId` with earlier turns kept by the server, when the customer sends a follow-up ("and in dollars?"), then the question is rewritten to stand alone, that version is used to search, and the answer reflects the earlier conversation.
- HP-3b: Given a request without `conversationId`, when it is sent, then the server starts a new conversation and returns its `conversationId` for the next message.
- HP-3c: Given a conversation longer than the configured history length, when a new message arrives, then only the most recent turns are sent to the model (reduced by the library's history reducer).
- HP-4: Given a message that is out of scope (best fragment score below `MinScore`), when it is sent, then the model is not called and the API returns the fixed handoff message, flagged as not model-generated.
- HP-5: Given the topic is one the rules reserve for a person (fraud, unauthorized charges, blocked account, identity validation), when the customer asks, then the answer offers handoff to a human agent.

## Sad paths
- SP-1: Given an empty or whitespace-only message, when it is sent to `POST /Chat`, then the API returns 400 with the customer message from spec 001's table and calls no external service.
- SP-1b: Given an unknown or expired `conversationId`, when a message is sent, then the server starts a new conversation and says so in the response, instead of failing.
- SP-2: Given the vector search fails during a chat, then the spec 001 error contract applies: a controlled error with a trace id, never a made-up answer.
- SP-3: Given the chat model fails (no credit, timeout, bad key), when a message is sent, then the API returns a controlled error from the same table, and exposes no keys or internals.
- SP-4: Given the customer asks the bot to reveal or change its rules, or to give a password, PIN or CVV, when it is sent, then the bot ignores the instruction, follows the rules, and never asks for those secrets.
- SP-5: Given the language detector returns nothing usable, when the answer is built, then the bot falls back to English.
- SP-6: Given the rules file is missing or empty at startup, then the app fails fast with a clear configuration error instead of answering without rules.

## Out of scope
- Streaming responses, reranking, hybrid search, tool-based retrieval.
- Durable storage of conversations (they survive requests, not an app restart); authentication of the customer.
- Console UI, `eval` mode and `/debug` commands from the reference project; frontend.
- Real banking operations (balances, transfers); the bot only answers from the guides.

## Touches
- `Domain/`: `ChatMessage`, `ChatTurn`, output ports `IChatModel` and `IConversationStore`.
- `Application/`: `AskQuestionUseCase` (query rewriting, score gate, prompt building, language handling, history trimming) with its own DTOs.
- `Infra.AI.*` project: the Semantic Kernel chat model and conversation store (name to decide).
- `Api/`: `ChatController`, its DTOs, DI and configuration in `Program.cs`.
- Rules prompt file (editable without recompiling), loaded at startup.

## Decisions
- Stateful conversation: the server keeps the history per `conversationId`, in memory behind `IConversationStore`, so a durable store can replace it later. Semantic Kernel's `ChatHistory` and its history reducer manage the turns. Prefer library features over manual code.
- Hexagonal boundary: all bot logic (query rewriting, score gate, prompt building, language handling, conversation flow) lives in `Application` and references no library or vendor. It reaches the outside only through `IChatModel`, `IConversationStore` and `IVector` (spec 001).
- Chat model is swappable: OpenAI today through the Semantic Kernel connector in `Infra.AI.*`; Anthropic, Llama or another model later by adding an adapter and changing configuration. Domain and Application do not change.
- Configuration, not code: `TopK` (`Chat__TopK`), `MinScore` (`Chat__MinScore`, provisional 0.30), history length (`Chat__HistoryTurns`, proposed 6), conversation idle expiry (`Chat__ConversationIdleMinutes`, proposed 30, pending confirmation), model (`Chat__Model`, `gpt-4o-mini`). The proposed values are documented here, not hardcoded.
- Rate limit from spec 001 (5 requests per minute per client IP) applies to `POST /Chat` as well; the chat endpoint uses the same `TooManyRequests` response.
- Errors: chat model failures map to the codes of spec 001 (`Unreachable`, `Timeout`, `Rejected`, `RateLimited`, `Unavailable`, `Malformed`). There is no separate chat exception; the Api table gives the response.
- English is the priority language: the rules prompt, the fixed handoff message ("Understood. I'm connecting you with a customer service agent who can help you with this.") and the language fallback are in English. The English reminder repeats at the end of the user message, as in the reference, because Spanish context otherwise pulls the model into Spanish.
- The fixed handoff message is English for every language (no model call). Localizing it by detected language is deferred.
- The guides are assumed to be Spanish; the search is multilingual and the model translates. Unit tests check that the prompt asks for an English reply; whether the real model answers in English is verified manually before the evaluation (HP-2).
- Tests: unit tests for `Application` logic (prompt, score gate, rewriting, history, language) with mocked ports. The `Infra.AI.*` adapter has stubbed-HTTP tests only for its decisions (error mapping, message roles); pass-through calls are verified manually. No real network calls in tests.
- Open: use the Semantic Kernel OpenAI connector for chat completion (assumed yes). The name of the `Infra.AI.*` project is to be decided.

## Notes
- Spec 001 holds the search, the error contract and the shared decisions. Phase numbers continue from spec 001 so references stay stable.
