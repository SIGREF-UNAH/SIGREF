import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')

  return {
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

    server: {
      host: true,
      port: Number(env.PORT ?? 5173),

      proxy: {
        '/api': {
          target: env.VITE_API_URL || 'http://localhost:5226',
          changeOrigin: true,
          secure: false,
        },
      },

      watch: {
        usePolling: true,
        interval: 1000,
      },

      hmr: {
        clientPort: 5173,
      },
    },
  }
})
