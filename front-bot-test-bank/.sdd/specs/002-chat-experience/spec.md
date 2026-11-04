# Bank Test chat experience
Size: complex — a full chat screen with conversation state and API integration, built from a Figma design.

## Goal
A Bank Test customer opens the support page and talks to the virtual assistant in a chat that follows the Figma design: they write a question, read the answer and keep the conversation going. A customer has one conversation. The front uses the `POST /Chat` API of `back-bot-test-bank`.

## Happy path
- HP-1: Given the customer opens the page, when it loads, then the design's shell shows (header with Bank Test, "Customer support", "Official and secure channel" and the "Guest · General inquiry" profile; the side menu; the "Bank Test Assistant · Online · Available 24/7" header; the help center column) and the chat shows today's date and the assistant's welcome message with its time.
- HP-2: Given an open chat, when the customer types in "Type your question here..." and sends (Enter, or the blue arrow button; Shift+Enter adds a line), then the message appears on the right as "You · time" with its checks, a typing indicator shows while the API answers, and the answer appears on the left with the assistant avatar and "Virtual assistant · time".
- HP-4: Given the first answer returned a `conversationId`, when the customer sends a follow-up such as "and in dollars?", then the front sends that id and the answer continues the same topic.
- HP-7: Given the customer writes in English, Spanish or another language, then the reply is shown as the API returns it.
- HP-9 (visual fidelity): Given the page open at 1440 × 1000 with a conversation like the design's sample (welcome, one customer message, one answer), when it is placed next to the Figma frame, then the layout, colors, type, spacing, radii and icons match the section "Visual design" below, with no area missing and nothing invented.

## Sad paths
- SP-1: Given an empty or whitespace-only message, when the customer tries to send it, then nothing is sent and the page says the field is empty ("Please type your question."). Checked only by the front, with unit tests (decided by the developer).
- SP-2: Given an answer is pending, when the customer tries to send another message, then sending is disabled until the answer or an error arrives, so a message is never sent twice.
- SP-3: Given the API answers an error or cannot be reached, then the page keeps showing the notice it already has from spec 001 phase 4 (the API's message with its reference, or "The service could not be reached. Please try again."), the customer's message stays, and sending is enabled again. No new error design.
- SP-6: Given the customer reloads the page, then the conversation is gone (it lives only in memory) and the welcome state shows.

## Visual design (what "matches the mock-up" means)
Reference: Figma file `gzN0JJqE4heyyz0kKTufXN`, frame `2:679`, 1440 × 1000, desktop only. A copy of the frame is kept in `design/figma-frame-2-679.png` next to this spec, so the comparison does not depend on opening Figma. Implement each area from the real design (`get_design_context`), take values into `src/shared/styles/tokens.css` (new tokens are added there, never hardcoded in a feature), and use the design's icons (lucide: `landmark`, `shield-check`, `user-round`, `chevron-down`, `messages-square`, `clipboard-list`, `circle-help`, `headset`, `external-link`, `arrow-up-right`, `bot-message-square`, `ellipsis`, `check-check`, `mic`, `arrow-up`, `lock-keyhole`, `arrow-left-right`, `credit-card`, `wallet`, `smartphone`, `chevron-right`, `arrow-right`).

Where it is today (spec 001): a header with only the brand and "Customer support", and a plain question box. Everything below is missing except the brand and the tokens.

**Page grid** (below the 80 px header, 920 px high): side menu 232 px, chat 888 px, help center 320 px. The side menu is navy; the chat and the help center are light.

| Area | What the design shows |
|---|---|
| Header (80 px, white, bottom border) | Left: emblem 38 × 38 (navy rounded square with the `landmark` icon), "Bank Test", a 1 × 28 divider, "Customer support". Right: `shield-check` + "Official and secure channel", a divider, a 36 px round avatar (`user-round`), "Guest" over "General inquiry", `chevron-down`. |
| Side menu (navy, padding 18) | Label "WE ARE HERE TO HELP" (uppercase, wide letter spacing); four options of 196 × 46 with a 19 px icon: "Virtual assistant" (active: lighter navy block and a 5 px dot), "My inquiries", "FAQs", "Contact channels". A divider. The label "CURRENT CONVERSATION" (shown as a label only). At the bottom: a card "We are also nearby." (`headset`, text, link "View contact channels" with `arrow-up-right`), then "Go to Bank Test" (`external-link`) and "© 2026 Bank Test". |
| Chat header (88 px) | A 42 px blue rounded square with `bot-message-square`; "Bank Test Assistant"; a 6 px green dot and "Online · Available 24/7". Right: `ellipsis`. A bottom border. |
| Message history (padding 32, scrolls) | A centered date separator "Today, November 3" between two thin lines. Assistant message: a 30 px blue round avatar and a white bubble with a border and the small corner next to the avatar (`--radius-tail`), at most 600 px wide, with a greeting in bold and text; under it "Virtual assistant · time" in small muted text. Customer message: right aligned, a blue bubble with white text (at most 387 px), under it "You · time" and the `check-check` icon in blue. Assistant answer (632 px): a white bubble with the text the API returns, shown as it comes, and under it "Virtual assistant · time". |
| Composer (166 px, white, top border) | Above the field, three static chips of 34 px (white, bordered; all white with a border, the last one with `headset`): "To Bank Test", "To another bank", "Speak with an advisor". Then the message field (824 × 62, border, radius) with the placeholder "Type your question here...", a `mic` icon (shown disabled), a divider and a 38 px blue send button with `arrow-up`. Under it, left `lock-keyhole` + "We will never ask for passwords or security codes.", right "Virtual assistant · May make mistakes". |
| Help center (320 px, padding 24) | "Your help center" and "Resolve your questions easily."; four topic rows of 64 px with a 34 px tinted icon tile, title, description and `chevron-right`: Transfers, Cards, Accounts and transactions, Digital banking; the link "View all questions" with `arrow-right`. A tinted card "Your security first" (`shield-check`, text, link "How to protect yourself" with `arrow-up-right`). "Need an advisor?" (`headset`), "Personalized support" and the hours, and the outlined button "View contact channels". At the bottom: a divider, the legal text "This chat provides general guidance. It does not allow banking transactions." and the links "Privacy" and "Accessibility". |

**States the design does not draw** (decided here so they are not invented later): the typing indicator is three dots inside an assistant bubble; the disabled send button is the same blue at lower opacity; hover and focus use the design's blue (`--color-primary`) with a visible focus ring.

**How it is checked**
- Phase by phase, the developer compares the running page at 1440 × 1000 with `design/figma-frame-2-679.png` (a Playwright screenshot of the page, as done in spec 001 phase 2 for the header).
- A unit test per component only where there is logic (the conversation state and the sending rules); the look is verified by comparison, not by tests.

## Out of scope
- Login and profile: "Guest · General inquiry" is fixed text.
- My inquiries, FAQs, Contact channels, "Go to Bank Test", the help center topics, "View all questions", "How to protect yourself", "View contact channels" and the legal links: shown as in the design, static.
- Quick replies that come from the API (the API returns none), and the privacy strip and the closing question inside the sample answer: they are bot content, and the front does not invent it. The three chips above the field are drawn as static buttons (see the decisions below).
- The "Was this helpful?" thumbs under the answer, a designed error bubble with a "Try again" action, and the formatting of the answer (numbered list, bold, code styles and citations): not built in this spec. The front shows the text the API returns as it comes; if the API later writes markup, the front can render it in another spec.
- More than one conversation: there is no "New inquiry" button (neither in the chat header nor in the side menu) and no list of inquiries, because a customer has one conversation. The label "CURRENT CONVERSATION" stays as the design draws it. The sample answer's strip and question, the thumbs, the error bubble, the answer formatting and the "New inquiry" buttons are the deliberate differences from the frame.
- Voice input (the microphone is shown disabled), a list of past conversations (the API has no endpoint), streaming, attachments, and a mobile layout (the design is desktop only).

## Touches
- Inside the project created by spec 001: a `chat` feature (messages, composer, conversation state, the call to `POST /Chat`) and a `shell` feature (header, side menu, help center column). They use that spec's shared API client, configuration and design tokens.

## Decisions and open questions
- Builds on spec 001 (`001-front-architecture`): the project, its structure and rules, the shared API client and the design tokens must exist first.
- Stack decided by the developer: React with TypeScript, feature-based architecture, React Context for state. The details are in the front's `.sdd/project.md`.
- Design read from Figma (file `gzN0JJqE4heyyz0kKTufXN`, frame `2:679` "Bank Test · Asistente de atención", 1440 × 1000, desktop). It has no loading state: the typing indicator uses the design's style by default.
- The API also returns `sources` (id and score). The design has no place for them. Default: do not list the sources; any `[KB-xx]` citation the API writes in the answer shows as text.
- The welcome message, "Online · Available 24/7" and the other labels are fixed texts from the design, not from the API. Times use the customer's local time. The checks on the customer's message: one while sent, two once answered.
- Open, to confirm: the legal links read "Privacidad" and "Accesibilidad" in Spanish inside an English design. Default: English ("Privacy", "Accessibility").
- Decided by the developer: the three chips above the field ("To Bank Test", "To another bank", "Speak with an advisor", the last with the `headset` icon) are drawn as static buttons, to match the mock-up. Clicking one puts its text in the field and does not send it; the customer sends it. They are fixed texts of the front, not bot content. The suggestions coming from the API in a structured way stay out of scope until the backend returns them.
- Order of work (decided by the developer): the look comes first. The first phases of this spec build the screen to match the mock-up with the visual areas above (shell, then chat, then help center), compared against `design/figma-frame-2-679.png`; the conversation behavior (HP-4, SP-2) follows on top of that screen.
