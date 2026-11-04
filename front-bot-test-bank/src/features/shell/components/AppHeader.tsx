import { ChevronDown, Landmark, ShieldCheck, UserRound } from 'lucide-react';
import styles from './AppHeader.module.css';

/** The institutional header of the design: the brand and service on the left, the secure channel and the guest profile on the right. */
export function AppHeader() {
  return (
    <header className={styles.header}>
      <div className={styles.brandAndService}>
        <div className={styles.brand}>
          <span className={styles.emblem}>
            <Landmark size={24} aria-hidden />
          </span>
          <h1 className={styles.name}>Bank Test</h1>
        </div>
        <div className={styles.divider} />
        <span className={styles.service}>Customer support</span>
      </div>
      <div className={styles.access}>
        <div className={styles.secure}>
          <ShieldCheck size={17} aria-hidden />
          <span>Official and secure channel</span>
        </div>
        <div className={styles.divider} />
        <div className={styles.profile}>
          <span className={styles.avatar}>
            <UserRound size={18} aria-hidden />
          </span>
          <div className={styles.profileText}>
            <span className={styles.guest}>Guest</span>
            <span className={styles.kind}>General inquiry</span>
          </div>
          <ChevronDown className={styles.chevron} size={16} aria-hidden />
        </div>
      </div>
    </header>
  );
}
