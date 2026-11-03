# Tasks: Front project foundation and architecture
Spec: ./spec.md
Test command: `npm test` (Vitest, run from `front-bot-test-bank`; the script and the package manager are fixed in Phase 1)

Each phase ends with something the developer can run. Tests only where they protect a decision; no real calls to the API in tests. The chat work is spec 002, which starts after Phase 6.

## Phase 1 — The app starts and tests run (covers HP-1, HP-2, HP-3)
Result: the project exists (Vite, React, TypeScript, Vitest with React Testing Library), with the folders `src/app`, `src/features` and `src/shared`, a first page that shows "Bank Test", a `.gitignore` (`node_modules`, `dist`, `.env.local`), a `CLAUDE.md` that points to `.sdd/project.md`, and the real run and test commands written in `.sdd/project.md`. The package manager and the Node version are decided here.
Test: `src/app/App.test.tsx` renders the app and asserts that the first page shows "Bank Test"; it proves the test setup renders a component.
Manual check: `npm install`, `npm run dev` and open the page; `npm run build` ends without errors; `npm test` passes.

- [x] 1.1 Failing test — result: `FAIL src/app/App.test.tsx — Error: Failed to resolve import "./App" from "src/app/App.test.tsx"` (the tooling was installed first: Vite 8, React 19, Vitest 5, TypeScript 7)
- [x] 1.2 Implementation — files: `package.json`, `tsconfig.json`, `vite.config.ts`, `index.html`, `src/main.tsx`, `src/app/App.tsx`, `src/test/setup.ts`, `src/features/.gitkeep`, `src/shared/.gitkeep`, `.gitignore`, `CLAUDE.md`; test `src/app/App.test.tsx`; `.sdd/project.md` (stack, tests and run commands). Decided here: npm as the package manager, Node 20 or newer
- [x] 1.3 Passing test — result: `Test Files 1 passed (1), Tests 1 passed (1)`; `npm run build` ends without errors (`✓ 15 modules transformed`, `dist/index.html` and one bundle); the dev server answers with the page (`<title>Bank Test · Customer support</title>` and `<div id="root">`)
- [x] 1.4 Developer approval — approved by the developer

## Phase 2 — The design's tokens (covers HP-1, HP-7)
Result: the colors, type scale, spacing and radii of the Figma frame `2:679` exist as CSS variables in one file in `src/shared/styles`, with the design's font loaded, and the first page uses them. Features will use these variables and no hardcoded values.
Test: `src/shared/styles/tokens.test.ts` asserts that the key tokens (primary blue, surfaces, text colors, border, radius, font family) have the values read from the Figma file, so a stray edit is noticed. The rest of the values are checked by eye against the design.
Manual check: open the page next to the Figma frame; the font and the main colors match.

- [ ] 2.1 Failing test — result: ________
- [ ] 2.2 Implementation — files: ________
- [ ] 2.3 Passing test — result: ________
- [ ] 2.4 Developer approval

## Phase 3 — The API address comes from the environment (covers SP-1)
Result: `src/shared/config` reads `VITE_API_BASE_URL`, validates it, and exposes it. If it is missing or not a valid URL, the page does not start the app: it shows an error that names the variable. A committed `.env.example` lists the name with no value, and `.env.local` stays out of git.
Test: `src/shared/config/config.test.ts` asserts that a missing value and an invalid URL fail with a message naming `VITE_API_BASE_URL`, and that a valid URL is accepted.
Manual check: start the app without the variable and read the error on the page; create `.env.local` with the address of the backend and the app loads.

- [ ] 3.1 Failing test — result: ________
- [ ] 3.2 Implementation — files: ________
- [ ] 3.3 Passing test — result: ________
- [ ] 3.4 Developer approval

## Phase 4 — One client talks to the API (covers HP-6, SP-2, SP-3)
Result: `src/shared/api` has a client that sends JSON to the configured base URL and returns the parsed body, typed. An error JSON `{ status, title, detail, traceId }` becomes an `ApiError` with the status, the customer message and the trace id. A connection failure or an answer that is not JSON becomes a typed error of kind "network" or "unexpected", never a raw exception. A dev script `npm run api:check` uses the same client to call `POST /Chat` from the terminal, so the contract can be tried without the browser (the backend still needs its CORS change for that).
Test: `src/shared/api/client.test.ts` asserts, with the call to the network replaced, that a success returns the body, that an error JSON becomes an `ApiError` with its three fields, and that a network failure and a non-JSON answer become the two typed errors.
Manual check: with the backend running, `npm run api:check` prints the answer and the conversation id; with the backend stopped it prints a "network" error; with a wrong message it prints the API's customer message and trace id.

- [ ] 4.1 Failing test — result: ________
- [ ] 4.2 Implementation — files: ________
- [ ] 4.3 Passing test — result: ________
- [ ] 4.4 Developer approval

## Phase 5 — The Context pattern every feature uses (covers HP-4, SP-4)
Result: `src/shared/context` has a helper that creates, for a feature, its Context provider with a reducer and the hook that reads it. Reading the hook outside its provider fails with a message that names the provider. Providers are composed in `src/app`. The rule for what a feature exposes (only its `index.ts`) is written in `.sdd/project.md` and enforced in Phase 6.
Test: `src/shared/context/createFeatureContext.test.tsx` asserts that the hook returns the state and the dispatch inside the provider, that dispatching an action updates the state, and that reading it outside the provider throws a message naming the provider.
Manual check: review the helper and run the tests; the first feature of spec 002 uses it. Nothing visible changes in the page.

- [ ] 5.1 Failing test — result: ________
- [ ] 5.2 Implementation — files: ________
- [ ] 5.3 Passing test — result: ________
- [ ] 5.4 Developer approval

## Phase 6 — The dependency rules are enforced (covers HP-5, HP-2)
Result: a lint rule on imports makes the build fail when a feature imports another feature's internals, or when `shared` imports from a feature or from `app`; the message names the rule. `npm run build` now runs the type check, the lint and the bundle, so a violation stops it.
Test: `eslint.rules.test.ts` runs the lint on small code samples and asserts that each forbidden import fails naming its rule and that the allowed imports (a feature through its `index.ts`, `app` importing features and `shared`) pass.
Manual check: add a forbidden import in a scratch file and `npm run build` fails naming the rule; remove it and the build passes.

- [ ] 6.1 Failing test — result: ________
- [ ] 6.2 Implementation — files: ________
- [ ] 6.3 Passing test — result: ________
- [ ] 6.4 Developer approval
