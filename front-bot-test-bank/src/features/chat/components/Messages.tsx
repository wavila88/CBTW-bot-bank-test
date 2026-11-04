import { BotMessageSquare, Check, CheckCheck } from 'lucide-react';
import type { ReactNode } from 'react';
import { formatTime } from '../format';
import styles from './Messages.module.css';

/** A message of the assistant: its avatar, the bubble with the tail, and "Virtual assistant · time" under it. */
export function AssistantMessage({
  time,
  variant,
  children,
}: {
  time: Date;
  variant: 'welcome' | 'answer';
  children: ReactNode;
}) {
  return (
    <div className={styles.assistantRow}>
      <span className={styles.avatar}>
        <BotMessageSquare size={16.5} aria-hidden />
      </span>
      <div className={variant === 'welcome' ? styles.welcomeColumn : styles.answerColumn}>
        <div className={variant === 'welcome' ? styles.welcomeBubble : styles.answerBubble}>{children}</div>
        <p className={styles.meta}>Virtual assistant · {formatTime(time)}</p>
      </div>
    </div>
  );
}

/** The assistant is writing: its avatar and three dots in a bubble, while the answer is pending. */
export function TypingMessage() {
  return (
    <div className={styles.assistantRow}>
      <span className={styles.avatar}>
        <BotMessageSquare size={16.5} aria-hidden />
      </span>
      <div className={styles.typingBubble} role="status" aria-label="The assistant is typing">
        <span className={styles.dot} />
        <span className={styles.dot} />
        <span className={styles.dot} />
      </div>
    </div>
  );
}

/** A message of the customer: the blue bubble on the right and "You · time" with its check under it (one check while it waits, two once answered). */
export function CustomerMessage({ text, time, answered }: { text: string; time: Date; answered: boolean }) {
  return (
    <div className={styles.customerRow}>
      <div className={styles.customerBubble}>{text}</div>
      <p className={styles.customerMeta}>
        <span>You · {formatTime(time)}</span>
        <span role="img" aria-label={answered ? 'Answered' : 'Sent'} className={styles.checks}>
          {answered ? <CheckCheck size={13} aria-hidden /> : <Check size={13} aria-hidden />}
        </span>
      </p>
    </div>
  );
}
