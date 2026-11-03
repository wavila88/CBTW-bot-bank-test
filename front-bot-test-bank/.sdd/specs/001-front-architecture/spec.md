# Front project foundation and architecture
Size: complex — a new project with its tooling, a feature-based structure whose rules are enforced by the build, a typed API client and the design tokens every feature will use.

## Goal
A developer clones `front-bot-test-bank`, runs it, and starts building features on a structure that keeps its own rules: feature-based layout, React Context for state, one place that talks to the API, and the colors, type and spacing of the Figma design ready to use. The chat (spec 002) is built on top of this.

## Happy path
- HP-1: Given a fresh clone, when the developer installs the dependencies and starts the app, then a first page opens in the browser, already styled with the design's font and colors.
- HP-2: Given the project, when the developer runs the production build, then it ends with no type or lint errors and produces the static files.
- HP-3: Given the project, when the developer runs the test command, then the example tests run and pass with the same setup every feature will use (rendering a component, replacing the API call).
- HP-4: Given a new feature, when it exposes only its `index.ts`, then other features and `app` can use what it exposes, its Context provider is composed in `app`, and its state is read through a hook.
- HP-5: Given a file imports another feature's internals, or `shared` imports from a feature or from `app`, when the check runs, then it fails with a message naming the rule that was broken.
- HP-6: Given the API base URL is set, when a feature uses the shared client to send JSON to an endpoint, then the request goes to that base URL and the client returns the parsed body, typed.
- HP-7: Given the Figma design, then its colors, type scale, spacing and radii exist as CSS variables in one place, and features use them instead of hardcoded values.

## Sad paths
- SP-1: Given the API base URL is missing or is not a valid URL, when the app starts, then it fails with a clear error that names the variable, instead of calling a wrong address.
- SP-2: Given the API answers an error JSON `{ status, title, detail, traceId }`, when the shared client reads it, then it throws an `ApiError` carrying the status, the customer message (`detail`) and the trace id.
- SP-3: Given the API cannot be reached, or answers something that is not JSON, then the client throws a typed error of kind "network" or "unexpected", never a raw exception, so each feature decides its own message.
- SP-4: Given a feature reads its Context outside its provider, then it fails with a message that names the missing provider.

## Out of scope
- The screens and behavior of the chat and the shell: spec 002.
- Routing (one page for now), login, translations, deployment and CI.
- A state library and a UI kit.
- The CORS change in the backend: a separate backend spec.

## Touches
- New folder `front-bot-test-bank`: `src/app` (entry and providers), `src/features/` (the structure and one tiny example feature that proves the rules), `src/shared/` (`api`, `config`, `ui`, `styles`), the tooling configuration and a `.env.example`.

## Decisions and open questions
- Stack decided by the developer: React with TypeScript, feature-based architecture, React Context for state.
- Tooling is proposed in `.sdd/project.md`: Vite, Vitest with React Testing Library, CSS Modules with CSS variables, a markdown renderer that never renders raw HTML. To confirm before building.
- The dependency rules must fail by themselves (a lint rule on imports), not depend on someone reading `project.md`. To confirm the mechanism.
- Tokens come from the Figma file `gzN0JJqE4heyyz0kKTufXN` (variables if it has them, otherwise the frame's styles). The design is desktop only.
- The API base URL comes from the environment variable `VITE_API_BASE_URL`, with a committed `.env.example` that has names only.
- Package manager and Node version: to decide when the project is created.
