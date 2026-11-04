import { defineConfig } from '@playwright/test';

// Values come from the environment (or .env.local); nothing about addresses is written here.
// .env.local is read next to this file, not from the current folder, so the VS Code extension
// finds it when the workspace is opened at the repository root.
try {
  process.loadEnvFile(new URL('.env.local', import.meta.url));
} catch {
  // No .env.local: the variables must already be in the environment.
}

function required(name: string): string {
  const value = process.env[name]?.trim();
  if (!value) throw new Error(`${name} is missing. See .env.example.`);
  return value;
}

const baseURL = required('E2E_BASE_URL');
required('VITE_API_BASE_URL');
// A second API instance, started here with its vector search pointed at a host where nothing listens,
// so it answers a real error. Only the error test sends its request there (see e2e/ask-a-question.spec.ts).
const brokenApiURL = required('E2E_BROKEN_API_URL');

export default defineConfig({
  testDir: 'e2e',
  fullyParallel: false,
  workers: 1,
  reporter: 'list',
  use: { baseURL, channel: 'msedge', ignoreHTTPSErrors: true },
  webServer: [
    {
      command: `npm run dev -- --port ${new URL(baseURL).port} --strictPort`,
      url: baseURL,
      // Locally, a front already running at E2E_BASE_URL (yours or the one the VS Code extension left up) is reused,
      // instead of failing with "port already used". In CI a fresh one is always started.
      reuseExistingServer: !process.env.CI,
      ignoreHTTPSErrors: true,
    },
    {
      // --no-build: the developer's own API may be running and locking the build output; the last build is used.
      // The API reads the rest of its settings as usual (environment variables or Api/appsettings.Local.json).
      command: `dotnet run --no-build --no-launch-profile --urls ${brokenApiURL}`,
      cwd: '../back-bot-test-bank/Api',
      url: `${brokenApiURL}/swagger/index.html`,
      env: {
        ASPNETCORE_ENVIRONMENT: 'Development',
        Pinecone__Host: 'http://127.0.0.1:9',
        Cors__AllowedOrigin: baseURL,
      },
      reuseExistingServer: !process.env.CI,
      ignoreHTTPSErrors: true,
      timeout: 120_000,
    },
  ],
});
