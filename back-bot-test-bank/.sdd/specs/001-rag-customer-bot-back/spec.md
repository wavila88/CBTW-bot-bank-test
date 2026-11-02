# RAG customer service bot API
Size: complex — new endpoint, new entities, two external integrations (vector search + chat model) and new ports/adapters across all layers.

## Goal
A customer of "Bank Test" sends a message to the API and gets an answer in the same language they wrote, based only on the bank's knowledge-base guides (RAG). Behavior is ported from the working reference `C:\Nortal\AIStudy\ragCustomerBot`, but exposed as a Web API and built with the project's hexagonal rules.

## Happy path
- HP-1: Given the knowledge base has a guide that covers the question, when the customer sends a message, then the API returns an answer built only from the retrieved guide fragments, with citations (e.g. `[KB-01]`) and the sources used (id and score).
- HP-2 (priority): Given the customer writes in English (the language the evaluation will use), when the guides are in Spanish, then the answer is fully in natural English, with no Spanish sentences, keeping only app menu/button names untranslated.
- HP-2b: Given the customer writes in another language (e.g. Spanish, Portuguese), then the answer is written in that language.
- HP-3: Given a `conversationId` with earlier turns kept by the server, when the customer sends a follow-up ("and in dollars?"), then the question is rewritten to stand alone, that version is used to search, and the answer reflects the earlier conversation.
- HP-3b: Given a request without `conversationId`, when it is sent, then the server starts a new conversation and returns its `conversationId` for the next message.
- HP-3c: Given a conversation longer than `HistoryTurns`, when a new message arrives, then only the most recent turns are sent to the model (reduced by the library's history reducer).
- HP-4: Given a message that is out of scope (best fragment score below `MinScore`), when it is sent, then the model is not called and the API returns the fixed "handing you to an agent" message, flagged as not model-generated.
- HP-5: Given the topic is one the rules reserve for a person (fraud, unauthorized charges, blocked account, identity validation), when the customer asks, then the answer offers handoff to a human agent.

## Sad paths
- SP-1b: Given an unknown or expired `conversationId`, when a message is sent, then the server starts a new conversation (and says so in the response) instead of failing.
- SP-1: Given an empty or whitespace-only message, when it is sent, then the API returns 400 with a clear error and calls no external service.
- SP-2: Given the vector search service is down or times out, when a message is sent, then the API returns a controlled error (502/503), never a made-up answer.
- SP-3: Given the chat model service fails (no credit, timeout, bad key), when a message is sent, then the API returns a controlled error and exposes no keys or internals.
- SP-4: Given the customer asks the bot to reveal or change its rules, or to give a password, PIN or CVV, when it is sent, then the bot ignores the instruction, follows the rules, and never asks for those secrets.
- SP-5: Given the language detector returns nothing usable, when the answer is built, then the bot falls back to English.
- SP-6: Given the system rules file is missing or empty at startup, then the app fails fast with a clear configuration error instead of answering without rules.

## Out of scope
- Ingesting or loading guides into the vector index (separate phase, `ragIngest`).
- Streaming responses, reranking, hybrid search, tool-based retrieval.
- Durable storage of conversations (they survive requests, not an app restart); authentication of the customer.
- Console UI, `eval` mode and `/debug` commands from the reference project; frontend.
- Real banking operations (balances, transfers); the bot only answers from the guides.

## Touches
- `Domain/`: models (`ChatMessage`, `ChatTurn`, `VectorChunk`, answer) and output ports (`IVector`, `IChatModel`).
- `Application/`: use case (ask a question): query rewriting, score gate, prompt building, language detection, history trimming; its own DTOs.
- Infra adapter projects, one per connector: `Infra.Vector.Pinne` (Pinecone) and an `Infra.AI.*` project (chat model and conversation store with Semantic Kernel, name to decide), plus options classes.
- `Api/`: new controller (e.g. `POST /Chat`), request/response DTOs, DI wiring in `Program.cs`, configuration and secrets, project references to Application and the `Infra.*` projects.
- Rules prompt file (editable without recompiling), loaded at startup.

## Decisions
- Stateful conversation, decided by the developer: the server keeps the history per `conversationId`. State is managed with Microsoft Semantic Kernel (`ChatHistory` plus its history reducer, `HistoryTurns` default 6) instead of hand-written history code. Prefer library features over manual code wherever the library already provides them (chat completion service, history reduction).
- Hexagonal boundary (decided by the developer): all bot logic (query rewriting, score gate, prompt building, language handling, conversation flow) lives in `Application` and is free of any library or vendor. It reaches the outside only through interfaces (`IChatModel`, `IVector`, `IConversationStore`). Semantic Kernel is one possible adapter behind those interfaces, not a requirement; new adapters can be added at any time.
- Adapter layout (decided: pure hexagon): the ports are our own interfaces in `Domain`. Each connector is its own project named `Infra.<Kind>.<Name>` (decided by the developer, replacing the earlier plan of folders inside one `Infra` project): the Pinecone adapter is `Infra.Vector.Pinne`, and Semantic Kernel will live only in its own `Infra.AI.*` project (name to decide). `Application` references only `Domain` and no external package.
- Conversation storage: in-memory, behind the port, so a durable store can replace it later. Semantic Kernel does not persist `ChatHistory` itself.
- Open: use Semantic Kernel's OpenAI chat connector for chat completion (assumed yes). For retrieval, keep the reference's Pinecone integrated-embedding HTTP adapter unless the SK Pinecone connector supports `llama-text-embed-v2`; verify during implementation.
- The chat model and the vector search are both replaceable ports, not fixed vendors. `IChatModel` can be backed by OpenAI today and by Anthropic (Claude), Llama or any other model later, by swapping the Infra adapter and configuration (provider, model, endpoint, key) with no change to Domain or Application. The same holds for `IVector` (Pinecone today, another vector store later). Neither Domain nor Application may name a vendor.
- Initial configuration, as in the reference: OpenAI chat (`gpt-4o-mini`) and Pinecone (`customer-service-kb`, namespace `kb`, `llama-text-embed-v2`, `TopK` 5, `MinScore` 0.30 provisional). Confirmed: Bank Test reuses the same index and namespace, which already hold the 8 guides loaded by `ragIngest`; nothing new to ingest.
- The provider is selected from configuration; adding a new one means adding one adapter (Semantic Kernel connector when available) and a config value.
- API keys live in `Api/appsettings.Local.json`, which git ignores. The committed template `appsettings.Local.example.json` has empty values. No key goes in any other committed file.
- English is the priority language: the rules prompt, the fixed no-answer message ("Understood. I'm connecting you with a customer service agent who can help you with this.") and the language fallback are all in English. The English-language reminder is repeated at the end of the user message, as in the reference, because Spanish context otherwise pulls the model into Spanish.
- The fixed no-answer message is English for every language (no model call). Localizing it by detected language is deferred.
- The guides in the index are assumed to be Spanish; the search is multilingual and the model translates. Unit tests check that the prompt sent to the model asks for an English reply; whether the real model actually answers in English is verified manually before the evaluation (HP-2).
- Tests (decided): automated tests only where they add real value: logic with decisions, in `Application` (prompt building, score gate, rewriting, history, language) and the `Api` mapping and status codes, with xUnit + Moq and mocked ports (`IChatModel`, `IVector`, `IConversationStore`). No real network calls. `Infra` adapters get stubbed-HTTP tests only for behavior that has decisions (ordering, tolerating missing metadata, query vs document embedding, caching the index host). Plain pass-through calls are not tested that way; they are verified manually against the real services.
- Every phase must be runnable: each one ends with something the developer exercises from Swagger. Diagnostic endpoints (e.g. `GET /Search` for the Pinecone search) are allowed for that, exist only in the Development environment (they return 404 elsewhere), and may be kept for calibrating `MinScore`.
- The existing `Register` sample is removed entirely, as the first phase of the work: `Domain/Models/Register.cs`, `Domain/Repositories/IRegisterRepository.cs`, `Application/UseCases/IRegisterUseCase.cs`, `Application/UseCases/RegisterUseCase.cs`, `Application/DTO/RegisterDTO.cs`, `Infra/Publisher/RegisterRepository.cs` (and the leftover `Infra/Publisher/` folder), `Api/DTO/Register.cs`, `Api/Controllers/RegisterController.cs`. The four projects must still build after the removal. `CLAUDE.md`, `README.md` and `.sdd/project.md` are updated to stop describing `Register`.
