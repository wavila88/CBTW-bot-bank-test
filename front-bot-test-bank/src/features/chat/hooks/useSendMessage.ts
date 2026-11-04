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

/** Sends a question to `POST /Chat` and keeps the conversation state. Returns whether the message was sent (a blank one is not). */
export function useSendMessage(client: ApiClient) {
  const { state, dispatch } = useChatState();

  async function send(text: string): Promise<boolean> {
    const question = text.trim();
    if (question === '') {
      dispatch({ type: 'blankRejected' });
      return false;
    }

    dispatch({ type: 'messageSent', message: newMessage('customer', question) });
    try {
      const body = state.conversationId ? { message: question, conversationId: state.conversationId } : { message: question };
      const result = await client.post<ChatAnswer>('/Chat', body);
      dispatch({
        type: 'answerReceived',
        message: newMessage('assistant', result.answer),
        conversationId: result.conversationId,
      });
    } catch (error) {
      dispatch({ type: 'requestFailed', failure: failureMessage(error) });
    }
    return true;
  }

  return { state, send };
}
