# Project: front-bot-test-bank

> **IMPORTANT — mandatory for every change.** Read this whole file before editing code. Configuration values (the API URL, anything environment-specific) come from environment variables, never hardcoded. The feature-based dependency rules apply. A change that breaks them is not done.

Sources read: `package.json`, `tsconfig.json`, `vite.config.ts`, `src/` (the project of spec 001 phase 1). The decisions come from the developer; the backend contract comes from `back-bot-test-bank` (`Api/Controllers/ChatController.cs`, `Api/Errors/ApiExceptionHandler.cs`, `Api/Program.cs`).
Last scan: 2026-11-03, by hand after phase 1 of spec 001. Run `/sdd:scan` again when the project has more code.

## Design
- Figma file `gzN0JJqE4heyyz0kKTufXN`, frame `2:679` "Bank Test · Asistente de atención" (1440 × 1000, desktop only), read through the Figma MCP (`claude.ai Figma`). Layout: a header of 80 px, a side menu of 232 px, the chat of 888 px and the help center column of 320 px.
- Implement from the real design with the Figma tools (`get_design_context`, loading the `figma-design-to-code` skill first), not from a screenshot. Take colors, spacing and type into the CSS variables of `tokens.css`; features use those variables and never hardcode a value.
- The Figma file defines no variables (`get_variable_defs` returns nothing), so the tokens were read from the values of the frame. If the designer adds variables later, update `tokens.css` from them.

## Stack
- React 19 with TypeScript 7, strict mode (`package.json`, `tsconfig.json`)
- Architecture: feature-based (decided by the developer)
- State: React Context, no external state library (decided by the developer)
- Build tool: Vite 8 (`vite.config.ts`)
- Tests: Vitest 5 with React Testing Library and `@testing-library/jest-dom`, in a `jsdom` environment (`vite.config.ts`, `src/test/setup.ts`)
- Package manager: npm; Node 20 or newer (`engines` in `package.json`; developed on Node 24)
- Styling: CSS Modules (`*.module.css`) that use the design's tokens as CSS variables from `src/shared/styles/tokens.css`; global rules in `src/shared/styles/global.css`; no UI kit (spec 001 phase 2)
- Font: Inter, self-hosted through `@fontsource-variable/inter`, so no request goes to a third party (`src/main.tsx`)

## Planned decisions (proposed defaults, not in code yet)
- Answer rendering: a markdown renderer that never renders raw HTML (the API answers carry numbered lists, bold text and code-style menu names).
- The dependency rules below are enforced by a lint rule on imports, so a violation fails the check by itself and nobody has to remember this file.

## Architecture (feature-based)
- `src/app/`: the entry, the providers and the page that composes the features.
- `src/features/<feature>/`: one folder per feature (`chat`, `shell`), each with its own `components/`, `context/`, `api/`, `hooks/` and `types/`, and an `index.ts` that exposes only its public API.
- `src/shared/`: code used by more than one feature: the HTTP client, UI building blocks, utilities, configuration.
- Dependency rules:
  - `app` can import features and shared.
  - A feature can import `shared`, never another feature's internals; it talks to other features only through their `index.ts`.
  - `shared` imports nothing from `features` or `app`.
- State: one React Context per feature, with a reducer (`useReducer`) for its state and actions. Providers are composed in `app/`. No global store.

## Backend contract (back-bot-test-bank)
- `POST /Chat`, body `{ "message": string, "conversationId"?: string }`. Answer `{ "answer": string, "sources": [{ "id": string, "score": number }], "conversationId": string }`. The first message has no `conversationId`; the answer returns one, and every follow-up must send it back.
- The answer is plain text with line breaks, numbered steps (`1. ...`), bold text, code-style menu names and guide citations such as `[KB-05]`. It comes in the language the customer wrote.
- Errors come as JSON `{ "status", "title", "detail", "traceId" }`, where `detail` is a message already meant for the customer: 400 "The question is empty.", 429 "You are sending requests too fast. Please wait a minute and try again.", 502, 503, 504 and 500 with their own generic message. Show `detail`; never show a stack trace or invent a message for these.
- Rate limit: 5 requests per minute per client IP, shared by all endpoints.
- CORS: the backend does not allow browser calls from another origin yet. It must allow the front's origin, set through an environment variable, before the two projects can talk from a browser.

## Configuration
- The API base URL comes from an environment variable (`VITE_API_BASE_URL` with Vite). Never hardcode a URL.
- `.env.local` is git-ignored; a committed `.env.example` lists the names only, with no values.

## Tests
- Frameworks: Vitest with React Testing Library; no real calls to the API. Automated tests only where they add value (state logic, error handling, rendering of answers, the dependency rules).
- Location and naming: next to the code, `<Subject>.test.ts` or `.test.tsx` (e.g. `src/app/App.test.tsx`); helpers in `src/test/`.
- Command: `npm test` (`vitest run`; `npm run test:watch` while developing). Run from the project folder.
- Vitest returns CSS as an empty text unless told otherwise; `vite.config.ts` lets it read `tokens.css` (`test.css.include`) for the tokens test.

## Run the app
- `npm install`, then `npm run dev`: the address is printed in the terminal (Vite's default is http://localhost:5173).
- `npm run build`: type check (`tsc --noEmit`) and production bundle in `dist/`. Lint joins this command in spec 001 phase 6.
- `npm run preview`: serves the production bundle.

## Not detected / questions
- Styling approach, markdown renderer and the lint rule for the dependency rules are proposals above; none is installed yet.
