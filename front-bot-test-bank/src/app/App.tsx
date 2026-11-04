import { Chat } from '../features/chat';
import { AppHeader, HelpCenter, SideMenu } from '../features/shell';
import type { ApiClient } from '../shared/api/client';
import styles from './App.module.css';

/** The page of the design: the header on top and, below it, the side menu, the chat and the help center. */
export function App({ client }: { client: ApiClient }) {
  return (
    <div className={styles.page}>
      <AppHeader />
      <div className={styles.body}>
        <SideMenu />
        <main className={styles.main}>
          <Chat client={client} />
        </main>
        <HelpCenter />
      </div>
    </div>
  );
}
