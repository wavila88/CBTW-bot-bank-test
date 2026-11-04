import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { ApiError, NetworkError, type ApiClient } from '../../../shared/api/client';
import { ChatProvider } from '../context/chatContext';
import { Chat } from './Chat';

type FakeClient = ApiClient & { post: ReturnType<typeof vi.fn> };

function clientThatAnswers(answer: string, conversationId = 'c1'): FakeClient {
  return { post: vi.fn().mockResolvedValue({ answer, sources: [], conversationId }) };
}

function clientThatFails(error: Error): FakeClient {
  return { post: vi.fn().mockRejectedValue(error) };
}

function renderChat(client: ApiClient) {
  render(
    <ChatProvider>
      <Chat client={client} />
    </ChatProvider>,
  );
}

const field = () => screen.getByRole('textbox', { name: 'Your question' });

async function ask(question: string) {
  await userEvent.type(field(), question);
  await userEvent.click(screen.getByRole('button', { name: 'Send' }));
}

describe('Chat', () => {
  it('sends the question to POST /Chat and shows the answer', async () => {
    const client = clientThatAnswers('Open Settings and choose Reset password.');
    renderChat(client);

    await ask('How do I reset my password?');

    expect(await screen.findByText('Open Settings and choose Reset password.')).toBeInTheDocument();
    expect(client.post).toHaveBeenCalledWith('/Chat', { message: 'How do I reset my password?' });
  });

  it('sends with Enter and adds a line with Shift+Enter', async () => {
    const client = clientThatAnswers('Done.');
    renderChat(client);

    await userEvent.type(field(), 'first line{Shift>}{Enter}{/Shift}second line');
    expect(client.post).not.toHaveBeenCalled();
    expect(field()).toHaveValue('first line\nsecond line');

    await userEvent.type(field(), '{Enter}');
    expect(client.post).toHaveBeenCalledTimes(1);
    expect(client.post).toHaveBeenCalledWith('/Chat', { message: 'first line\nsecond line' });
  });

  it('does not call the API and says the field is empty when the question is blank', async () => {
    const client = clientThatAnswers('should not show');
    renderChat(client);

    await ask('   ');

    expect(screen.getByRole('alert')).toHaveTextContent('Please type your question.');
    expect(client.post).not.toHaveBeenCalled();
  });

  it('does not call the API when nothing was typed', async () => {
    const client = clientThatAnswers('should not show');
    renderChat(client);

    await userEvent.click(screen.getByRole('button', { name: 'Send' }));

    expect(screen.getByRole('alert')).toHaveTextContent('Please type your question.');
    expect(client.post).not.toHaveBeenCalled();
  });

  it("shows the API's customer message and trace id when it answers an error", async () => {
    renderChat(clientThatFails(new ApiError(429, 'Please wait a minute and try again.', 'trace-9')));

    await ask('Hello');

    expect(await screen.findByRole('alert')).toHaveTextContent('Please wait a minute and try again.');
    expect(screen.getByRole('alert')).toHaveTextContent('trace-9');
  });

  it('shows a generic failure line when the API cannot be reached', async () => {
    renderChat(clientThatFails(new NetworkError()));

    await ask('Hello');

    expect(await screen.findByRole('alert')).toHaveTextContent('could not be reached');
  });

  it('sends the conversation id of the first answer with the next message', async () => {
    const client = clientThatAnswers('Answer.', 'c-77');
    renderChat(client);

    await ask('first question');
    await screen.findByText('Answer.');
    await ask('and in dollars?');

    expect(client.post).toHaveBeenNthCalledWith(1, '/Chat', { message: 'first question' });
    expect(client.post).toHaveBeenNthCalledWith(2, '/Chat', { message: 'and in dollars?', conversationId: 'c-77' });
  });

  it('puts the text of a suggested button in the field without sending it', async () => {
    const client = clientThatAnswers('should not show');
    renderChat(client);

    await userEvent.click(screen.getByRole('button', { name: 'To another bank' }));

    expect(field()).toHaveValue('To another bank');
    expect(client.post).not.toHaveBeenCalled();
  });
});
