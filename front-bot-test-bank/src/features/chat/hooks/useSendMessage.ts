import type { ApiClient } from '../../../shared/api/client';
import { useChatState } from '../context/chatContext';
import type { ChatMessage } from '../context/chatReducer';
import { failureMessage } from '../failureMessage';

interface ChatAnswer {
  answer: string;
  conversationId: string;
}

let lastId = 0;

function newMessage(author: ChatMessage['author'], text: string): ChatMessage {
  lastId += 1;
  return { id: `message-${lastId}`, author, text, sentAt: new Date() };
}

/**
 * Sends a question to `POST /Chat` and keeps the conversation state. `send` answers at once whether the message was
 * accepted (a blank one, or one sent while an answer is pending, is not), so the caller can clear its field right
 * away; the answer arrives later through the state.
 */
export function useSendMessage(client: ApiClient) {
  const { state, dispatch } = useChatState();

  async function ask(question: string, conversationId: string | null) {
    try {
      const body = conversationId ? { message: question, conversationId } : { message: question };
      const result = await client.post<ChatAnswer>('/Chat', body);
      dispatch({
        type: 'answerReceived',
        message: newMessage('assistant', result.answer),
        conversationId: result.conversationId,
      });
    } catch (error) {
      dispatch({ type: 'requestFailed', failure: failureMessage(error) });
    }
  }

  function send(text: string): boolean {
    // One question at a time: while an answer is pending, nothing else is sent.
    if (state.pending) return false;

    const question = text.trim();
    if (question === '') {
      dispatch({ type: 'blankRejected' });
      return false;
    }

    dispatch({ type: 'messageSent', message: newMessage('customer', question) });
    void ask(question, state.conversationId);
    return true;
  }

  return { state, send };
}
