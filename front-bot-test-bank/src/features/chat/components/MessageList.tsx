import { useEffect, useRef, useState } from 'react';
import type { ChatMessage } from '../context/chatReducer';
import { formatTodayLabel } from '../format';
import { AssistantMessage, CustomerMessage, TypingMessage } from './Messages';
import styles from './MessageList.module.css';

/** The conversation: today's date, the welcome message, then the messages in order, and a notice when something went wrong. */
export function MessageList({
  messages,
  pending,
  failure,
}: {
  messages: ChatMessage[];
  pending: boolean;
  failure: string | null;
}) {
  const [openedAt] = useState(() => new Date());
  const list = useRef<HTMLDivElement>(null);

  // Keep the newest message in view.
  useEffect(() => {
    if (list.current) list.current.scrollTop = list.current.scrollHeight;
  }, [messages, pending, failure]);

  return (
    <div className={styles.list} ref={list}>
      <div className={styles.date}>
        <span className={styles.line} />
        <span>{formatTodayLabel(openedAt)}</span>
        <span className={styles.line} />
      </div>
      <AssistantMessage time={openedAt} variant="welcome">
        <p className={styles.greeting}>Hello, welcome to Bank Test.</p>
        <p className={styles.paragraph}>
          I&rsquo;m your virtual assistant. I can help with transfers, cards, and other services. How can I help you today?
        </p>
      </AssistantMessage>
      {messages.map((message, index) =>
        message.author === 'customer' ? (
          // A customer message is answered once an assistant message follows it.
          <CustomerMessage
            key={message.id}
            text={message.text}
            time={message.sentAt}
            answered={messages[index + 1]?.author === 'assistant'}
          />
        ) : (
          <AssistantMessage key={message.id} time={message.sentAt} variant="answer">
            <p className={styles.paragraph}>{message.text}</p>
          </AssistantMessage>
        ),
      )}
      {pending && <TypingMessage />}
      {failure !== null && (
        <p className={styles.failure} role="alert">
          {failure}
        </p>
      )}
    </div>
  );
}
