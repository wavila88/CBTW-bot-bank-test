import react from '@vitejs/plugin-react';
import { defineConfig } from 'vitest/config';

export default defineConfig({
  plugins: [react()],
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    // Vitest returns CSS as an empty string unless told otherwise; the tokens test reads tokens.css.
    css: { include: [/tokens\.css/] },
  },
});
