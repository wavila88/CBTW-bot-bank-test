import type { ApiClient } from '../../../shared/api/client';
import { useSendMessage } from '../hooks/useSendMessage';
import styles from './Chat.module.css';
import { ChatHeader } from './ChatHeader';
import { Composer } from './Composer';
import { MessageList } from './MessageList';

/** The chat with the assistant: its header, the conversation and where the customer writes. */
export function Chat({ client }: { client: ApiClient }) {
  const { state, send } = useSendMessage(client);

  return (
    <div className={styles.chat}>
      <ChatHeader />
      <MessageList messages={state.messages} failure={state.failure} />
      <Composer onSend={send} />
    </div>
  );
}
