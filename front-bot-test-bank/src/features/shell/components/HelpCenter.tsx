import {
  ArrowLeftRight,
  ArrowRight,
  ArrowUpRight,
  ChevronRight,
  CreditCard,
  Headset,
  ShieldCheck,
  Smartphone,
  Wallet,
} from 'lucide-react';
import type { ReactNode } from 'react';
import styles from './HelpCenter.module.css';

interface Topic {
  title: string;
  description: string;
  icon: ReactNode;
}

const topics: Topic[] = [
  { title: 'Transfers', description: 'Times, statuses, and receipts', icon: <ArrowLeftRight size={18} aria-hidden /> },
  { title: 'Cards', description: 'Activation, limits, and blocks', icon: <CreditCard size={18} aria-hidden /> },
  { title: 'Accounts and transactions', description: 'Inquiries and documentation', icon: <Wallet size={18} aria-hidden /> },
  { title: 'Digital banking', description: 'Access and app usage', icon: <Smartphone size={18} aria-hidden /> },
];

/** The help center column of the design. Everything in it is shown as in the design and does nothing. */
export function HelpCenter() {
  return (
    <aside className={styles.help} aria-label="Help center">
      <div className={styles.content}>
        <section className={styles.topics}>
          <h2 className={styles.title}>Your help center</h2>
          <p className={styles.subtitle}>Resolve your questions easily.</p>
          <ul className={styles.list}>
            {topics.map((topic) => (
              <li key={topic.title} className={styles.item}>
                <button type="button" className={styles.topic}>
                  <span className={styles.tile}>{topic.icon}</span>
                  <span className={styles.topicText}>
                    <span className={styles.topicTitle}>{topic.title}</span>
                    <span className={styles.topicDescription}>{topic.description}</span>
                  </span>
                  <ChevronRight className={styles.chevron} size={15} aria-hidden />
                </button>
              </li>
            ))}
          </ul>
          <button type="button" className={styles.link}>
            <span>View all questions</span>
            <ArrowRight size={14} aria-hidden />
          </button>
        </section>
        <section className={styles.security}>
          <div className={styles.cardHeader}>
            <ShieldCheck size={21} aria-hidden />
            <h3 className={styles.securityTitle}>Your security first</h3>
          </div>
          <p className={styles.securityText}>
            Bank Test will never ask for your passwords, PIN, or codes via chat. If you receive a suspicious message, do not
            reply or open its links.
          </p>
          <button type="button" className={`${styles.link} ${styles.securityLink}`}>
            <span>How to protect yourself</span>
            <ArrowUpRight size={14} aria-hidden />
          </button>
        </section>
        <section className={styles.advisor}>
          <div className={styles.advisorHeader}>
            <Headset size={20} aria-hidden />
            <h3 className={styles.advisorTitle}>Need an advisor?</h3>
          </div>
          <div className={styles.hours}>
            <p>Personalized support</p>
            <p>Monday to Friday, 8:00 AM to 8:00 PM.</p>
          </div>
          <button type="button" className={styles.contact}>
            <span>View contact channels</span>
            <ArrowUpRight size={14} aria-hidden />
          </button>
        </section>
      </div>
      <div className={styles.legal}>
        <div className={styles.rule} />
        <p className={styles.legalText}>This chat provides general guidance. It does not allow banking transactions.</p>
        <div className={styles.legalLinks}>
          <button type="button">Privacy</button>
          <button type="button">Accessibility</button>
        </div>
      </div>
    </aside>
  );
}
