# Bank Test chat experience
Size: complex — a new React project, a full chat screen with conversation state, API integration and error states, built from a Figma design.

## Goal
A Bank Test customer opens the support page and talks to the virtual assistant in a chat that follows the Figma design: they write a question, read a well formatted answer, keep the conversation going, and start a new inquiry when they want. The front uses the `POST /Chat` API of `back-bot-test-bank`.

## Happy path
- HP-1: Given the customer opens the page, when it loads, then the design's shell shows (header with Bank Test, "Customer support", "Official and secure channel" and the "Guest · General inquiry" profile; the side menu; the "Bank Test Assistant · Online · Available 24/7" header; the help center column) and the chat shows today's date and the assistant's welcome message with its time.
- HP-2: Given an open chat, when the customer types in "Type your question here..." and sends (Enter, or the blue arrow button; Shift+Enter adds a line), then the message appears on the right as "You · time" with its checks, a typing indicator shows while the API answers, and the answer appears on the left with the assistant avatar and "Virtual assistant · time".
- HP-3: Given an answer with numbered steps, bold text, code-style menu names or citations like `[KB-05]`, when it is shown, then the steps render as the design's numbered list, bold and code styles render, and any HTML or script in the text is shown as plain text, never run.
- HP-4: Given the first answer returned a `conversationId`, when the customer sends a follow-up such as "and in dollars?", then the front sends that id and the answer continues the same topic.
- HP-5: Given an open conversation, when the customer clicks "New inquiry" (header button or the "+" of "Current conversation"), then the messages clear, the welcome message shows again, and the next message starts a new conversation without the old id.
- HP-6: Given the customer sent the first message, when the answer arrives, then "Current conversation" in the side menu lists it with a title taken from that first message and its date and time.
- HP-7: Given the customer writes in English, Spanish or another language, then the reply is shown as the API returns it.
- HP-8: Given an assistant answer, then "Was this helpful?" with thumbs up and thumbs down shows under it, and the customer's choice is highlighted; it is kept only on screen.

## Sad paths
- SP-1: Given an empty or whitespace-only message, when the customer tries to send it, then nothing is sent and the page says the field is empty ("Please type your question."). Checked only by the front, with unit tests (decided by the developer).
- SP-2: Given an answer is pending, when the customer tries to send another message, then sending is disabled until the answer or an error arrives, so a message is never sent twice.
- SP-3: Given the API answers 500, 502, 503 or 504, then an error message with the API's `detail` shows in the conversation, with a "Try again" action that resends the same message; the customer's message stays.
- SP-4: Given the API answers 429, then the API's message ("You are sending requests too fast...") shows in the conversation and the customer can send again.
- SP-5: Given the API cannot be reached (no connection or the server is down), then a message "We could not reach the assistant. Check your connection and try again." shows with a "Try again" action.
- SP-6: Given the customer reloads the page, then the conversation is gone (it lives only in memory) and the welcome state shows.

## Out of scope
- Login and profile: "Guest · General inquiry" is fixed text.
- My inquiries, FAQs, Contact channels, "Go to Bank Test", the help center topics, "View all questions", "How to protect yourself", "View contact channels" and the legal links: shown as in the design, static.
- The quick replies ("To Bank Test", "To another bank", "Speak with an advisor") and the privacy strip inside the sample answer: the API returns neither, and the front does not invent bot content.
- Voice input (the microphone is shown disabled), saving the feedback, a list of past conversations (the API has no endpoint), streaming, attachments, and a mobile layout (the design is desktop only).

## Touches
- Inside the project created by spec 001: a `chat` feature (messages, composer, conversation state, the call to `POST /Chat`, feedback) and a `shell` feature (header, side menu, help center column). They use that spec's shared API client, configuration and design tokens.
- Backend `back-bot-test-bank/Api`: allow the front's origin (CORS), set through an environment variable. A small change that is not in any backend spec yet.

## Decisions and open questions
- Builds on spec 001 (`001-front-architecture`): the project, its structure and rules, the shared API client and the design tokens must exist first.
- Stack decided by the developer: React with TypeScript, feature-based architecture, React Context for state. The details are in the front's `.sdd/project.md`.
- Design read from Figma (file `gzN0JJqE4heyyz0kKTufXN`, frame `2:679` "Bank Test · Asistente de atención", 1440 × 1000, desktop). It has no loading, error or empty states: the typing indicator and the error messages use the design's style by default.
- The API also returns `sources` (id and score). The design has no place for them. Default: show only the `[KB-xx]` citations inside the answer and do not list the sources.
- The conversation title: the design shows "Transfer inquiry" and the API gives no title. Default: the first message, shortened.
- The welcome message, "Online · Available 24/7" and the other labels are fixed texts from the design, not from the API. Times use the customer's local time. The checks on the customer's message: one while sent, two once answered.
- Open, to confirm: the legal links read "Privacidad" and "Accesibilidad" in Spanish inside an English design. Default: English ("Privacy", "Accessibility").
- Open, to confirm: the thumbs only change the screen. Saving them needs a backend endpoint that does not exist.
- Open, to confirm: quick replies need the backend to return suggestions in a structured way. Out of scope until then.
- The browser cannot call the API until the backend allows the front's origin; that backend change must be done before the front can be tried end to end.
