import { defineConfig } from 'vite'
import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    tailwindcss(),
    react()
  ],
  build: {
    // Ignorar errores de TypeScript en producción
    rollupOptions: {
      onwarn(warning, warn) {
        // Suprimir ciertos warnings
        if (warning.code === 'UNUSED_EXTERNAL_IMPORT') return
        warn(warning)
      }
    }
  },
  server: {
    host: '0.0.0.0',
    port: parseInt(process.env.PORT ?? "5173"),
    proxy: {
      '/api': {
        target: process.env.VITE_API_URL || 'http://localhost:5226',
        changeOrigin: true,
        secure: false
      }
    },
    watch: {
      usePolling: true,
      interval: 1000
    },
    hmr: {
      clientPort: 5173
    }
  }
})
