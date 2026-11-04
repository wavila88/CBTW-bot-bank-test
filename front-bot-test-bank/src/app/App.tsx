import { useState, type FormEvent } from 'react';
import { ApiError, NetworkError, type ApiClient } from '../shared/api/client';
import styles from './App.module.css';

interface ChatAnswer {
  answer: string;
}

function failureMessage(error: unknown): string {
  if (error instanceof ApiError) return `${error.message} (reference: ${error.traceId})`;
  if (error instanceof NetworkError) return 'The service could not be reached. Please try again.';
  return 'The service answered something unexpected. Please try again.';
}

/** A plain box to try the API from the browser. Spec 002 replaces it with the designed chat. */
export function App({ client }: { client: ApiClient }) {
  const [question, setQuestion] = useState('');
  const [answer, setAnswer] = useState<string | null>(null);
  const [failure, setFailure] = useState<string | null>(null);
  const [sending, setSending] = useState(false);

  async function send(event: FormEvent) {
    event.preventDefault();
    setAnswer(null);
    setFailure(null);
    // A blank question never reaches the API: the front says so and sends nothing.
    if (question.trim() === '') {
      setFailure('Please type your question.');
      return;
    }
    setSending(true);
    try {
      const result = await client.post<ChatAnswer>('/Chat', { message: question });
      setAnswer(result.answer);
    } catch (error) {
      setFailure(failureMessage(error));
    } finally {
      setSending(false);
    }
  }

  return (
    <>
      <header className={styles.header}>
        <h1 className={styles.brand}>Bank Test</h1>
        <div className={styles.divider} />
        <span className={styles.service}>Customer support</span>
      </header>
      <main className={styles.main}>
        <form className={styles.form} onSubmit={send}>
          <label className={styles.label} htmlFor="question">
            Your question
          </label>
          <textarea
            id="question"
            className={styles.input}
            rows={3}
            value={question}
            onChange={(event) => setQuestion(event.target.value)}
          />
          <button className={styles.button} type="submit" disabled={sending}>
            Send
          </button>
        </form>
        {answer !== null && <p className={styles.answer}>{answer}</p>}
        {failure !== null && (
          <p className={styles.failure} role="alert">
            {failure}
          </p>
        )}
      </main>
    </>
  );
}
