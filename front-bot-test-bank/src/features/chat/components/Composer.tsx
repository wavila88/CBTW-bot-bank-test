import { ArrowUp, Headset, LockKeyhole, Mic } from 'lucide-react';
import { useRef, useState, type FormEvent, type KeyboardEvent } from 'react';
import styles from './Composer.module.css';

const suggestions = ['To Bank Test', 'To another bank', 'Speak with an advisor'];

/**
 * Where the customer writes: three suggested buttons (they fill the field, they do not send), the message field
 * (Enter sends, Shift+Enter adds a line), the disabled microphone, the send button and the notes under it.
 */
export function Composer({ onSend }: { onSend: (text: string) => Promise<boolean> }) {
  const [text, setText] = useState('');
  const field = useRef<HTMLTextAreaElement>(null);

  async function submit() {
    // A blank message is kept in the field; a sent one clears it.
    if (await onSend(text)) setText('');
  }

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    void submit();
  }

  function handleKeyDown(event: KeyboardEvent<HTMLTextAreaElement>) {
    if (event.key === 'Enter' && !event.shiftKey && !event.nativeEvent.isComposing) {
      event.preventDefault();
      void submit();
    }
  }

  function suggest(suggestion: string) {
    setText(suggestion);
    field.current?.focus();
  }

  return (
    <div className={styles.composer}>
      <div className={styles.suggestions}>
        {suggestions.map((suggestion) => (
          <button key={suggestion} type="button" className={styles.chip} onClick={() => suggest(suggestion)}>
            {suggestion === 'Speak with an advisor' && <Headset size={16} aria-hidden />}
            <span>{suggestion}</span>
          </button>
        ))}
      </div>
      <form className={styles.field} onSubmit={handleSubmit}>
        <textarea
          ref={field}
          className={styles.input}
          aria-label="Your question"
          placeholder="Type your question here..."
          rows={1}
          value={text}
          onChange={(event) => setText(event.target.value)}
          onKeyDown={handleKeyDown}
        />
        <button type="button" className={styles.mic} disabled aria-label="Voice input (not available)">
          <Mic size={20} aria-hidden />
        </button>
        <span className={styles.divider} />
        <button type="submit" className={styles.send} aria-label="Send">
          <ArrowUp size={20} aria-hidden />
        </button>
      </form>
      <div className={styles.notes}>
        <span className={styles.privacy}>
          <LockKeyhole size={12} aria-hidden />
          <span>We will never ask for passwords or security codes.</span>
        </span>
        <span>Virtual assistant · May make mistakes</span>
      </div>
    </div>
  );
}
