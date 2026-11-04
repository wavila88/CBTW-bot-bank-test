# Tasks: Bank Test chat experience
Spec: ./spec.md
Test command: `npm test` (`vitest run`, from `front-bot-test-bank`). End to end: `npm run test:e2e` (one real call to the API).

Phase numbers start at 1 in each spec (a phase is named with its spec, e.g. "spec 002 phase 2"). Order decided by the developer: the look first (phases 1 and 2), then the behavior on top of that screen (phases 3 to 5). Every phase leaves the page working: a customer can still ask a question and read the answer, and `e2e/ask-a-question.spec.ts` keeps passing (the field keeps the accessible name "Your question" and the button "Send").

## How every phase is checked
- **The look** is compared by the developer with `design/figma-frame-2-679.png`, next to a Playwright screenshot of the page at 1440 × 1000 (the same way as spec 001 phase 2). Take the values from the real design with `get_design_context` (load the `figma-design-to-code` skill first); new values go to `src/shared/styles/tokens.css`, never hardcoded in a feature. HP-9 is checked in each visual phase for its area, and for the whole page in Phase 5.
- **Tests** only where there is something to assert: the texts and roles that must exist, state logic, and rendering of answers. The look itself is not unit-tested.
- **Architecture** (checked at each approval): the features are `shell` and `chat` in `src/features/`, each exposing only its `index.ts` and keeping its state with `createFeatureContext`; `app` composes them and passes data between them as props (a feature never imports another feature's internals); `shared` imports nothing from `features` or `app`; the HTTP calls go only through the shared client.

## Phase 1 — The page structure as in the mock-up: header, side menu and help center (covers HP-1, HP-9, shell part)
Result: the page has the design's grid below an 80 px header: side menu (232 px, navy), chat column and help center (320 px). Three static areas, built together because they have no state or logic:
- **Header:** the emblem with the `landmark` icon, "Bank Test", the divider, "Customer support", and on the right "Official and secure channel" with `shield-check`, a divider, the round avatar with `user-round`, "Guest" over "General inquiry" and `chevron-down`.
- **Side menu:** "WE ARE HERE TO HELP" with the four options ("Virtual assistant" active, with the lighter block and the dot; "My inquiries", "FAQs", "Contact channels", static and doing nothing), the divider, "CURRENT CONVERSATION" with its "+", the card "We are also nearby." with its link "View contact channels", "Go to Bank Test" and "© 2026 Bank Test". The active inquiry of its list shows later (Phase 5).
- **Help center:** "Your help center" and its line, the four topic rows (Transfers, Cards, Accounts and transactions, Digital banking, each with its icon tile, description and `chevron-right`), "View all questions" with its arrow, the "Your security first" card with "How to protect yourself", "Need an advisor?" with the hours and the outlined "View contact channels" button, and the legal text with "Privacy" and "Accessibility" (English, see the spec).

The icon library `lucide-react` is added (the design's icons are lucide); to confirm before installing. The plain question box stays in the chat column until Phase 2. Each area is its own component in `src/features/shell/components/` (`AppHeader`, `SideMenu`, `HelpCenter`), so they can be reviewed and adjusted one by one inside this phase.
Test: `AppHeader.test.tsx` asserts the brand (the page's heading), "Customer support", "Official and secure channel", "Guest" and "General inquiry". `SideMenu.test.tsx` asserts the four options, that "Virtual assistant" is the current one (`aria-current`), the "CURRENT CONVERSATION" section with its add button, the card texts and the footer. `HelpCenter.test.tsx` asserts the title, the four topics with their descriptions, the two cards, the button and the legal text with its two links. `src/app/App.test.tsx` asserts the three regions (navigation, main chat area, complementary help center).
Manual check: run the front and compare the whole page, area by area (header, left column, right column), with the frame at 1440 × 1000; adjust with the developer until it matches.

- [ ] 1.1 Failing test — result: ________
- [ ] 1.2 Implementation — files: ________
- [ ] 1.3 Passing test — result: ________
- [ ] 1.4 Developer approval

## Phase 2 — Ask in the real chat and read the answer in bubbles (covers HP-1, HP-2, HP-4, HP-7, HP-9, SP-1, SP-3, SP-6)
Result: the plain box is replaced by the designed chat. Header: the blue assistant icon, "Bank Test Assistant", the green dot with "Online · Available 24/7", the "+ New inquiry" button (does nothing yet) and `ellipsis`. History: today's date separator and the assistant's welcome message with "Virtual assistant · time". Composer: the three static chips (a chip puts its text in the field and does not send it), the field with "Type your question here...", the disabled `mic`, the blue arrow button (Enter or the button sends, Shift+Enter adds a line), and the notes under it. Sending shows the customer's bubble on the right ("You · time") and the answer in an assistant bubble on the left, as plain text for now, in whatever language the API returns. A blank message sends nothing and says "Please type your question." An API error or an unreachable API keeps showing the notice spec 001 phase 4 already built (the API message with its reference, or "could not be reached"), and the customer can send again. The first answer's `conversationId` is kept in the chat state and sent with every following message; the API does the understanding of follow-ups such as "and in dollars?", the front only carries the id. The conversation lives only in memory (a reload shows the welcome state). This is the first use of `createFeatureContext`: the `chat` feature has its provider (messages, pending flag), composed in `AppProviders`.
Test: `src/features/chat/` tests assert the welcome message when there are no messages, that sending adds the customer's message and then the answer (client replaced), that Enter sends and Shift+Enter does not, that a blank message calls nothing and shows the notice, that an API error and a network failure show the existing notice and leave sending enabled, that a chip fills the field without sending, that the first request has no `conversationId` and the next ones send the one the API returned, and the reducer's actions. `e2e/ask-a-question.spec.ts` keeps passing.
Manual check: ask about card payments abroad, then "and in dollars?", and see the answer follow the topic; ask one question in Spanish; compare the whole chat column with the frame (without the sample answer's steps).

- [ ] 2.1 Failing test — result: ________
- [ ] 2.2 Implementation — files: ________
- [ ] 2.3 Passing test — result: ________
- [ ] 2.4 Developer approval

## Phase 3 — While the answer is pending (covers HP-2, SP-2)
Result: after sending, a typing indicator (three dots inside an assistant bubble) shows until the answer arrives; the send button and Enter are disabled meanwhile, so a message is never sent twice; the customer's message shows one check while sent and two once answered.
Test: `src/features/chat/` tests assert the indicator while the client call is pending, that a second send does nothing, that the checks go from one to two when the answer arrives, and that the indicator disappears.
Manual check: ask a question and see the dots and the disabled button; press Enter twice quickly and see a single message.

- [ ] 3.1 Failing test — result: ________
- [ ] 3.2 Implementation — files: ________
- [ ] 3.3 Passing test — result: ________
- [ ] 3.4 Developer approval

## Phase 4 — Well formatted answers (covers HP-3)
Result: the answer text renders as the design: numbered steps as the blue-circled list, bold text, code-style menu names and citations like `[KB-05]`, line breaks kept. HTML or script in the text is shown as plain text and never runs. A markdown renderer that never renders raw HTML is added (to confirm the library before installing).
Test: `src/features/chat/components/AnswerText.test.tsx` asserts that numbered steps become a list, bold and code styles render, the citation stays visible, and `<script>` and `<img onerror=...>` appear as text with no element created.
Manual check: ask a question whose answer has steps (for example "How do I reset my password?") and compare the answer bubble with the frame.

- [ ] 4.1 Failing test — result: ________
- [ ] 4.2 Implementation — files: ________
- [ ] 4.3 Passing test — result: ________
- [ ] 4.4 Developer approval

## Phase 5 — New inquiry, the current conversation in the menu and the final look check (covers HP-5, HP-6, HP-9 as a whole)
Result: "New inquiry" (the header button or the "+" of "Current conversation") clears the messages, shows the welcome message again and forgets the old id, so the next message starts a new conversation. Once the first answer arrives, "Current conversation" in the side menu lists it with the first message (shortened) as its title and "Today · time". `app` passes the title to the side menu as a prop; `shell` does not import `chat`.
Test: `src/features/chat/` tests assert that new inquiry empties the messages and the id, and that the next send has no id; `src/features/shell/components/SideMenu.test.tsx` asserts the title and date shown when given a conversation and nothing when not.
Manual check: hold a conversation, press "New inquiry", and see the welcome message and the menu empty; ask again and see the new title; then compare the whole page with `design/figma-frame-2-679.png` (HP-9 as a whole) and adjust with the developer until it matches.

- [ ] 5.1 Failing test — result: ________
- [ ] 5.2 Implementation — files: ________
- [ ] 5.3 Passing test — result: ________
- [ ] 5.4 Developer approval
