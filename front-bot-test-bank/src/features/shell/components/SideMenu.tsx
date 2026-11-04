import { ArrowUpRight, ClipboardList, CircleHelp, ExternalLink, Headset, MessagesSquare, Plus } from 'lucide-react';
import type { ReactNode } from 'react';
import styles from './SideMenu.module.css';

interface Option {
  label: string;
  icon: ReactNode;
  current?: boolean;
}

const options: Option[] = [
  { label: 'Virtual assistant', icon: <MessagesSquare size={19} aria-hidden />, current: true },
  { label: 'My inquiries', icon: <ClipboardList size={19} aria-hidden /> },
  { label: 'FAQs', icon: <CircleHelp size={19} aria-hidden /> },
  { label: 'Contact channels', icon: <Headset size={19} aria-hidden /> },
];

/** The navy side menu of the design. Only "Virtual assistant" is the current page; the other options are shown as in the design and do nothing. */
export function SideMenu() {
  return (
    <nav className={styles.menu} aria-label="Support menu">
      <div className={styles.top}>
        <div className={styles.help}>
          <p className={styles.label}>WE ARE HERE TO HELP</p>
          <ul className={styles.options}>
            {options.map((option) => (
              <li key={option.label}>
                <button
                  type="button"
                  className={option.current ? `${styles.option} ${styles.current}` : styles.option}
                  aria-current={option.current ? 'page' : undefined}
                >
                  {option.icon}
                  <span>{option.label}</span>
                  {option.current && <span className={styles.dot} />}
                </button>
              </li>
            ))}
          </ul>
        </div>
        <div className={styles.divider} />
        <div className={styles.conversations}>
          <div className={styles.conversationsHeader}>
            <p className={`${styles.label} ${styles.wide}`}>CURRENT CONVERSATION</p>
            <button type="button" className={styles.add} aria-label="New inquiry">
              <Plus size={15} aria-hidden />
            </button>
          </div>
        </div>
      </div>
      <div className={styles.bottom}>
        <div className={styles.card}>
          <Headset size={24} aria-hidden />
          <p className={styles.cardTitle}>We are also nearby.</p>
          <p className={styles.cardText}>If you need it, an advisor can help you.</p>
          <button type="button" className={styles.cardLink}>
            <span>View contact channels</span>
            <ArrowUpRight size={15} aria-hidden />
          </button>
        </div>
        <button type="button" className={styles.back}>
          <ExternalLink size={16} aria-hidden />
          <span>Go to Bank Test</span>
        </button>
        <p className={styles.footer}>© 2026 Bank Test</p>
      </div>
    </nav>
  );
}
