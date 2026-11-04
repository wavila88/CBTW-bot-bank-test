import { BotMessageSquare, Ellipsis } from 'lucide-react';
import styles from './ChatHeader.module.css';

/** The assistant's header: who it is and that it is online. */
export function ChatHeader() {
  return (
    <div className={styles.header}>
      <div className={styles.assistant}>
        <span className={styles.identity}>
          <BotMessageSquare size={23} aria-hidden />
        </span>
        <div className={styles.status}>
          <h2 className={styles.name}>Bank Test Assistant</h2>
          <div className={styles.availability}>
            <span className={styles.online} />
            <span>Online · Available 24/7</span>
          </div>
        </div>
      </div>
      <button type="button" className={styles.more} aria-label="More options">
        <Ellipsis size={20} aria-hidden />
      </button>
    </div>
  );
}
