import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import { defineConfig } from 'vitest/config'

export default defineConfig({
  resolve: {
    alias: {
      '@endpoints': `${process.cwd()}/src/api/generated/endpoints`,
      '@models': `${process.cwd()}/src/api/generated/schemas/models`,
      '@types': `${process.cwd()}/src/api/generated/schemas/types`,
    },
  },

  plugins: [
    react(),
    tailwindcss(),
  ],

  test: {
    environment: 'jsdom',

    globals: true,

    setupFiles: [
      './tests/setup.ts',
    ],

    include: [
      'src/**/*.test.{ts,tsx}',
      'src/**/*.spec.{ts,tsx}',
      'tests/**/*.test.{ts,tsx}',
      'tests/**/*.spec.{ts,tsx}',
    ],

    coverage: {
      provider: 'v8',

      reporter: [
        'text',
        'html',
        'lcov',
      ],

      exclude: [
        'src/api/**',
        '**/*.d.ts',
        '**/*.config.{js,ts,mjs}',
        '**/index.ts',
      ],
    },
  },
})
