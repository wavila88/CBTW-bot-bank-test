import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { ApiError, NetworkError, type ApiClient } from '../shared/api/client';
import { App } from './App';

function clientThatAnswers(answer: string): ApiClient & { post: ReturnType<typeof vi.fn> } {
  return { post: vi.fn().mockResolvedValue({ answer, sources: [], conversationId: 'c1' }) };
}

function clientThatFails(error: Error): ApiClient {
  return { post: vi.fn().mockRejectedValue(error) };
}

async function ask(question: string) {
  await userEvent.type(screen.getByRole('textbox', { name: 'Your question' }), question);
  await userEvent.click(screen.getByRole('button', { name: 'Send' }));
}

describe('App', () => {
  it('sends the question to POST /Chat and shows the answer', async () => {
    const client = clientThatAnswers('Open Settings and choose Reset password.');
    render(<App client={client} />);

    await ask('How do I reset my password?');

    expect(await screen.findByText('Open Settings and choose Reset password.')).toBeInTheDocument();
    expect(client.post).toHaveBeenCalledWith('/Chat', { message: 'How do I reset my password?' });
  });

  it('does not call the API and says the field is empty when the question is blank', async () => {
    const client = clientThatAnswers('should not show');
    render(<App client={client} />);

    await ask('   ');

    expect(screen.getByRole('alert')).toHaveTextContent('Please type your question.');
    expect(client.post).not.toHaveBeenCalled();
  });

  it('does not call the API when nothing was typed', async () => {
    const client = clientThatAnswers('should not show');
    render(<App client={client} />);

    await userEvent.click(screen.getByRole('button', { name: 'Send' }));

    expect(screen.getByRole('alert')).toHaveTextContent('Please type your question.');
    expect(client.post).not.toHaveBeenCalled();
  });

  it("shows the API's customer message and trace id when it answers an error", async () => {
    render(<App client={clientThatFails(new ApiError(429, 'Please wait a minute and try again.', 'trace-9'))} />);

    await ask('Hello');

    expect(await screen.findByRole('alert')).toHaveTextContent('Please wait a minute and try again.');
    expect(screen.getByRole('alert')).toHaveTextContent('trace-9');
  });

  it('shows a generic failure line when the API cannot be reached', async () => {
    render(<App client={clientThatFails(new NetworkError())} />);

    await ask('Hello');

    expect(await screen.findByRole('alert')).toHaveTextContent('could not be reached');
  });
});
