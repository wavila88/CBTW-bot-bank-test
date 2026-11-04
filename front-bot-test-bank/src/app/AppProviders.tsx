import type { ReactNode } from 'react';

/**
 * The one place where the features' providers are composed (created with `shared/context/createFeatureContext`).
 * Each feature's provider is imported from its `index.ts` and wraps the children here. None yet: spec 002 adds the chat.
 */
export function AppProviders({ children }: { children: ReactNode }) {
  return <>{children}</>;
}
