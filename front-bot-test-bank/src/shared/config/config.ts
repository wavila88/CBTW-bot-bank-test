const API_BASE_URL = 'VITE_API_BASE_URL';

export class ConfigError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'ConfigError';
  }
}

export interface AppConfig {
  /** Address of the API, without a trailing slash. */
  apiBaseUrl: string;
}

/** Reads and validates the settings that come from the environment. It fails with a message that names the variable. */
export function readConfig(env: Record<string, unknown>): AppConfig {
  const raw = env[API_BASE_URL];
  if (typeof raw !== 'string' || raw.trim() === '') {
    throw new ConfigError(
      `${API_BASE_URL} is missing. Set it to the address of the API, for example http://localhost:5082.`,
    );
  }

  let url: URL;
  try {
    url = new URL(raw.trim());
  } catch {
    throw new ConfigError(`${API_BASE_URL} is not a valid address: "${raw}".`);
  }
  if (url.protocol !== 'http:' && url.protocol !== 'https:') {
    throw new ConfigError(`${API_BASE_URL} must start with http:// or https://, but it is "${raw}".`);
  }

  return { apiBaseUrl: url.toString().replace(/\/+$/, '') };
}
