import styles from './ConfigErrorPage.module.css';

export function ConfigErrorPage({ message }: { message: string }) {
  return (
    <main className={styles.page}>
      <h1 className={styles.title}>Configuration error</h1>
      <p role="alert" className={styles.message}>
        {message}
      </p>
      <p className={styles.hint}>
        Copy <code>.env.example</code> to <code>.env.local</code>, fill in the value and restart the app.
      </p>
    </main>
  );
}
