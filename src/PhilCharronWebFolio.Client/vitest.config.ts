// nOUVEL AJOUT, À SUPPRIMER PEUT-ÊTRE, LE ANGULAR PLUGIN NE FONCTIONNAIT PAS

import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['src/test-setup.ts'],
    include: ['src/**/*.{test,spec}.ts'],
  },
});