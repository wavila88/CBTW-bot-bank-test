export interface ChatMessage {
  id: string;
  author: 'customer' | 'assistant';
  text: string;
  sentAt: Date;
}

export interface ChatState {
  messages: ChatMessage[];
  /** The id the first answer returned; every following message sends it back so the API continues the conversation. */
  conversationId: string | null;
  pending: boolean;
  /** What the page tells the customer when something goes wrong (or when the question is blank). */
  failure: string | null;
}

export type ChatAction =
  | { type: 'messageSent'; message: ChatMessage }
  | { type: 'answerReceived'; message: ChatMessage; conversationId: string }
  | { type: 'requestFailed'; failure: string }
  | { type: 'blankRejected' };

export const initialChatState: ChatState = {
  messages: [],
  conversationId: null,
  pending: false,
  failure: null,
};

export function chatReducer(state: ChatState, action: ChatAction): ChatState {
  switch (action.type) {
    case 'messageSent':
      return { ...state, messages: [...state.messages, action.message], pending: true, failure: null };
    case 'answerReceived':
      return {
        ...state,
        messages: [...state.messages, action.message],
        conversationId: action.conversationId,
        pending: false,
      };
    case 'requestFailed':
      return { ...state, pending: false, failure: action.failure };
    case 'blankRejected':
      return { ...state, failure: 'Please type your question.' };
  }
}
