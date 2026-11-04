import { describe, expect, it } from 'vitest';
import { chatReducer, initialChatState, type ChatMessage } from './chatReducer';

const at = new Date(2026, 10, 3, 10, 24);

function message(author: ChatMessage['author'], text: string): ChatMessage {
  return { id: `${author}-${text}`, author, text, sentAt: at };
}

describe('chatReducer', () => {
  it('adds the customer message, marks the answer as pending and clears an earlier failure', () => {
    const failed = { ...initialChatState, failure: 'old failure' };

    const next = chatReducer(failed, { type: 'messageSent', message: message('customer', 'Hello') });

    expect(next.messages).toEqual([message('customer', 'Hello')]);
    expect(next.pending).toBe(true);
    expect(next.failure).toBeNull();
  });

  it('adds the answer, stops the pending state and keeps the conversation id', () => {
    const waiting = chatReducer(initialChatState, { type: 'messageSent', message: message('customer', 'Hello') });

    const next = chatReducer(waiting, {
      type: 'answerReceived',
      message: message('assistant', 'Hi'),
      conversationId: 'c1',
    });

    expect(next.messages.map((m) => m.author)).toEqual(['customer', 'assistant']);
    expect(next.pending).toBe(false);
    expect(next.conversationId).toBe('c1');
  });

  it('keeps the messages and the conversation id when the request fails, and stores the failure', () => {
    const waiting = chatReducer(
      { ...initialChatState, conversationId: 'c1' },
      { type: 'messageSent', message: message('customer', 'Hello') },
    );

    const next = chatReducer(waiting, { type: 'requestFailed', failure: 'The service could not be reached.' });

    expect(next.messages).toHaveLength(1);
    expect(next.pending).toBe(false);
    expect(next.conversationId).toBe('c1');
    expect(next.failure).toBe('The service could not be reached.');
  });

  it('asks for a question when the message is blank, without adding a message or waiting for an answer', () => {
    const next = chatReducer(initialChatState, { type: 'blankRejected' });

    expect(next.failure).toBe('Please type your question.');
    expect(next.messages).toEqual([]);
    expect(next.pending).toBe(false);
  });
});
