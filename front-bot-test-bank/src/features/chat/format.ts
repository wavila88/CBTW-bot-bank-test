const time = new Intl.DateTimeFormat('en-US', { hour: '2-digit', minute: '2-digit', hourCycle: 'h23' });
const day = new Intl.DateTimeFormat('en-US', { month: 'long', day: 'numeric' });

/** "10:24", in the customer's local time. */
export function formatTime(date: Date): string {
  return time.format(date);
}

/** "Today, November 3". */
export function formatTodayLabel(date: Date): string {
  return `Today, ${day.format(date)}`;
}
