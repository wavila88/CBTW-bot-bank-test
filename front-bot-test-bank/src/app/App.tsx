import styles from './App.module.css';

export function App() {
  return (
    <header className={styles.header}>
      <h1 className={styles.brand}>Bank Test</h1>
      <div className={styles.divider} />
      <span className={styles.service}>Customer support</span>
    </header>
  );
}
