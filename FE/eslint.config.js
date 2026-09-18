import js from '@eslint/js'
import { defineConfig } from 'eslint/config'
import globals from 'globals'
import react from 'eslint-plugin-react'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import jsxA11y from 'eslint-plugin-jsx-a11y'
import tseslint from 'typescript-eslint'
import eslintConfigPrettier from 'eslint-config-prettier'

const typedTypeScriptConfigs = tseslint.configs.recommendedTypeChecked.map(
  (config) => ({
    ...config,
    files: ['**/*.{ts,tsx}'],
  }),
)

export default defineConfig([
  {
    ignores: [
      'dist/**',
      'coverage/**',
      'node_modules/**',
      'src/api/generated/**',
    ],
  },

  // JavaScript / ESM config files
  {
    files: ['**/*.{js,mjs}'],

    ...js.configs.recommended,

    languageOptions: {
      ecmaVersion: 2022,
      sourceType: 'module',

      globals: {
        ...globals.node,
        ...globals.es2022,
      },
    },
  },

  ...typedTypeScriptConfigs,

  // Frontend TypeScript
  {
    files: ['src/**/*.{ts,tsx}'],

    languageOptions: {
      ecmaVersion: 2022,

      globals: {
        ...globals.browser,
        ...globals.es2022,
      },

      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
      },
    },

    plugins: {
      react,
      'react-hooks': reactHooks,
      'react-refresh': reactRefresh,
      'jsx-a11y': jsxA11y,
    },

    settings: {
      react: {
        version: 'detect',
      },
    },

    rules: {
      ...react.configs.recommended.rules,
      ...react.configs['jsx-runtime'].rules,
      ...reactHooks.configs.recommended.rules,
      ...jsxA11y.configs.recommended.rules,

      'react/prop-types': 'off',

      'react-refresh/only-export-components': [
        'warn',
        {
          allowConstantExport: true,
        },
      ],

      '@typescript-eslint/no-explicit-any': 'warn',

      '@typescript-eslint/no-floating-promises': 'error',
      '@typescript-eslint/no-misused-promises': 'error',
      '@typescript-eslint/await-thenable': 'error',

      '@typescript-eslint/no-unnecessary-type-assertion': 'warn',
    },
  },

  // Node TypeScript configuration files
  {
    files: [
      'vite.config.ts',
      'vitest.config.ts',
      'orval.config.ts',
    ],

    languageOptions: {
      ecmaVersion: 2022,

      globals: {
        ...globals.node,
        ...globals.es2022,
      },

      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
      },
    },
  },

  // Tests
  {
    files: [
      'tests/**/*.{ts,tsx}',
      'src/**/*.test.{ts,tsx}',
      'src/**/*.spec.{ts,tsx}',
    ],

    languageOptions: {
      ecmaVersion: 2022,

      globals: {
        ...globals.browser,
        ...globals.es2022,
      },

      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
      },
    },
  },

  eslintConfigPrettier,
])