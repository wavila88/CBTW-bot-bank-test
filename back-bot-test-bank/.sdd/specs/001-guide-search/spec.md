# Guide search API and error contract
Size: complex — new endpoint, a vector-search port and adapter, and the error contract for the whole API.

## Goal
Developers, and later the chat bot in spec 002, can search the bank's guides in Pinecone and get the most similar fragments, best first. Every failure reaches the caller as a controlled, generic response with a trace id; the real cause stays in the server log. Chat and conversation work are in spec 002.

## Happy path
- HP-1 (retrieval part): Given the knowledge base has guides on a topic, when a question is sent to `GET /Search`, then the API returns up to `topK` guide fragments with id, score, guide id, section and text, best first. Works for English and Spanish questions.

## Sad paths
- SP-1: Given an empty or whitespace-only question, when it is sent, then the API returns 400 with the customer message from the table and does not call the search service.
- SP-2: Given the vector search service fails (unreachable, timeout, rejected key, rate limit, bad response), when a question is sent, then the API returns a controlled error with a generic message and a trace id, from the table, never an invented result. The real cause is only in the server log.
- SP-3: Given a client sends more than 5 requests within one minute, when the 6th request arrives, then the API returns 429 with the customer message from the table and a `Retry-After` header, and calls no service. The client can send again once the minute has passed.

## Out of scope
- The chat model, answers, citations, conversation history, language detection, handoff and the rules prompt: spec 002.
- Loading guides into the index (`ragIngest`).
- Score threshold (`MinScore`) and reranking; hybrid search.
- Authentication of callers.
- Rate limiting shared across several server instances; the limit is per process.

## Touches
- `Domain/`: `IVector` port, `VectorChunk`, `ErrorCode`, `DomainException`, `VectorUnavailableException`.
- `Application/`: `SearchGuidesUseCase`, `ISearchGuidesUseCase` and `GuideFragmentDTO`.
- `Infra.Vector.Pinne/`: the Pinecone adapter and its options.
- `Api/`: `SearchController` (Development only), `ApiExceptionHandler` with the code → HTTP table, DI and configuration in `Program.cs`.
- `Tests/`: unit tests for the use case, the adapter's decisions and the handler.

## Decisions
- Hexagonal boundary: `Application` references only `Domain` and no external package. `Api` is the composition root. Each connector is its own project named `Infra.<Kind>.<Name>`, so the Pinecone adapter is `Infra.Vector.Pinne`.
- Configuration, not code: keys, index name, namespace, embedding model, dimension, `TopK` and the optional index `Host` come from environment variables (`Pinecone__ApiKey`, `Chat__TopK`, ...). `Api/appsettings.Local.json` is an optional git-ignored fallback, and environment variables win. The code holds no defaults for these values; startup validation names each missing setting. Only protocol constants (Pinecone `ControlPlaneUrl`, `ApiVersion`) stay in code.
- Pinecone: index `customer-service-kb`, namespace `kb`, model `llama-text-embed-v2`, 1024 dimensions. Confirmed: the bank reuses the index and the 8 guides already loaded by `ragIngest`. The optional `Host` skips the index lookup; without it, the host is looked up once.
- Error handling: `DomainException` carries a code from `ErrorCode` (`InvalidRequest`, `Unreachable`, `Timeout`, `Rejected`, `RateLimited`, `Unavailable`, `NotFound`, `Malformed`, `TooManyRequests`), with no text and no HTTP. Infra translates vendor failures into codes. `Api` owns one table: `InvalidRequest` → 400; `Timeout` → 504; `Malformed` → 502; `TooManyRequests` → 429; the other codes → 503; anything unexpected → 500. One global handler logs the real exception with the trace id. No stack trace, inner message, key or URL reaches the caller, in Development too. Controllers do not catch errors.
- Rate limit (decided by the developer: more than 5 requests in less than a minute is an error): a fixed window of 5 requests per minute per client IP, with `RateLimit__PermitLimit` = 5 and `RateLimit__WindowSeconds` = 60 from configuration, never defaults in code. The built-in ASP.NET Core rate limiter enforces it, with no extra package, before any controller or service runs. Its rejection uses the same table under the new code `TooManyRequests` (429), kept apart from `RateLimited`, which is Pinecone's own 429 and maps to 503. It is one global limiter in the request pipeline, not an attribute on each controller, so every endpoint, present and future, is covered, Development diagnostics included.
- Diagnostic endpoints are allowed for checking each phase from Swagger. They exist only in the Development environment and return 404 elsewhere.
- Tests: automated tests only where they add value. `Application` logic and the `Api` handler are tested with mocked ports. `Infra` adapters get stubbed-HTTP tests only for behavior with decisions (ordering, missing metadata, query embedding, host caching, error mapping); pass-through calls are verified manually against the real service. No real network calls in tests.
- The `Register` sample is removed (spec 001 phase 1, done). The four original projects must keep building.
- Each phase ends with something the developer can run from Swagger or the console.
