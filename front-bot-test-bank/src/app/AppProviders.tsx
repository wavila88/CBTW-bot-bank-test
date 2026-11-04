import type { ReactNode } from 'react';
import { ChatProvider } from '../features/chat';

/**
 * The one place where the features' providers are composed (created with `shared/context/createFeatureContext`).
 * Each feature's provider is imported from its `index.ts` and wraps the children here.
 */
export function AppProviders({ children }: { children: ReactNode }) {
  return <ChatProvider>{children}</ChatProvider>;
}
