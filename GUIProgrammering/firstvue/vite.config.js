import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-vue';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [plugin()],
  server: {
    port: 57736,
    proxy: {
      '/api': {
        target: 'https://localhost:7032',
        changeOrigin: true,
        secure: false
      }
    }
  }
})
