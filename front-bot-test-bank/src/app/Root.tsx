import { createApiClient } from '../shared/api/client';
import { ConfigError, readConfig } from '../shared/config/config';
import { App } from './App';
import { AppProviders } from './AppProviders';
import { ConfigErrorPage } from './ConfigErrorPage';

/** Checks the settings before anything starts: if one is wrong, the page says which, instead of calling a wrong address. */
export function Root({ env }: { env: Record<string, unknown> }) {
  let client;
  try {
    client = createApiClient({ baseUrl: readConfig(env).apiBaseUrl });
  } catch (error) {
    if (error instanceof ConfigError) return <ConfigErrorPage message={error.message} />;
    throw error;
  }
  return (
    <AppProviders>
      <App client={client} />
    </AppProviders>
  );
}
