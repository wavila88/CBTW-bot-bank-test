import basicSsl from '@vitejs/plugin-basic-ssl';
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vitest/config';

export default defineConfig({
  // The front is served over HTTPS (self-signed certificate) so it matches the API's HTTPS address.
  plugins: [react(), basicSsl()],
  test: {
    include: ['src/**/*.test.{ts,tsx}'],
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    // Vitest returns CSS as an empty string unless told otherwise; the tokens test reads tokens.css.
    css: { include: [/tokens\.css/] },
  },
});
