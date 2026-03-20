import { fileURLToPath } from 'node:url';

import vuetify from 'vite-plugin-vuetify';
import svgLoader from 'vite-svg-loader';

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  app: {
    head: {
      titleTemplate: 'OE-Production',
      title: 'OE',

      link: [{
        rel: 'icon',
        type: 'image/x-icon',
        href: '/favicon.ico',
      }],
    },
  },

  devtools: {
    enabled: true,
  },

  css: [
    '@/assets/css/main.css',
    '@core/scss/template/index.scss',
    '@styles/styles.scss',
    '@/plugins/iconify/icons.css',
    'vuetify/styles',
    '@mdi/font/css/materialdesignicons.css',
  ],

  /*
    ❗ Please read the docs before updating runtimeConfig
    https://nuxt.com/docs/guide/going-further/runtime-config
  */
  runtimeConfig: {
    // Private keys are only available on the server
    AUTH_ORIGIN: process.env.AUTH_ORIGIN,
    AUTH_SECRET: process.env.AUTH_SECRET,

    // Public keys that are exposed to the client.
    public: {
      // ✅ ใช้ environment variable สำหรับ API Base URL
      apiBase: process.env.API_BASE_URL || 'http://172.18.106.100:9011',
      apiTimeout: 600000, // 10 minutes timeout สำหรับ PDF processing

      // ✅ เพิ่ม config สำหรับ development
      isDevelopment: process.env.NODE_ENV === 'development',

      // ✅ เพิ่ม API endpoints configuration
      endpoints: {
        login: '/api/User/login',
        compare: '/api/SI25011/compare',
        health: '/api/SI25011/health',
        uploadProgress: '/api/SI25011/upload-progress'
      }
    },
  },

  components: {
    dirs: [{
      path: '@/@core/components',
      pathPrefix: false,
    }, {
      path: '~/components/global',
      global: true,
    }, {
      path: '~/components',
      pathPrefix: false,
    }],
  },

  plugins: [
    '@/plugins/vuetify/index.ts',
    '@/plugins/iconify/index.ts'
  ],

  imports: {
    dirs: ['./@core/utils', './@core/composable/', './plugins/*/composables/*'],
    presets: [],
  },

  hooks: {
    'vite:extendConfig': (config) => {
      config.plugins!.push(
        vuetify({
          autoImport: true,
        })
      )
    },
  },

  experimental: {
    typedPages: true,
  },

  typescript: {
    tsConfig: {
      compilerOptions: {
        paths: {
          '@/*': ['../*'],
          '@themeConfig': ['../themeConfig.ts'],
          '@layouts/*': ['../@layouts/*'],
          '@layouts': ['../@layouts'],
          '@core/*': ['../@core/*'],
          '@core': ['../@core'],
          '@images/*': ['../assets/images/*'],
          '@styles/*': ['../assets/styles/*'],
          '@validators': ['../@core/utils/validators'],
          '@db/*': ['../server/fake-db/*'],
          '@api-utils/*': ['../server/utils/*'],
        },
      },
    },
  },

  nitro: {
    esbuild: {
      options: {
        target: 'esnext'
      }
    }
  },

  // ℹ️ Disable source maps until this is resolved: https://github.com/vuetifyjs/vuetify-loader/issues/290
  sourcemap: {
    server: false,
    client: false,
  },

  vue: {
    compilerOptions: {
      isCustomElement: tag => tag === 'swiper-container' || tag === 'swiper-slide',
    },
  },

  vite: {
    define: { 'process.env': {} },

    resolve: {
      alias: {
        '@': fileURLToPath(new URL('.', import.meta.url)),
        '@themeConfig': fileURLToPath(new URL('./themeConfig.ts', import.meta.url)),
        '@core': fileURLToPath(new URL('./@core', import.meta.url)),
        '@layouts': fileURLToPath(new URL('./@layouts', import.meta.url)),
        '@images': fileURLToPath(new URL('./assets/images/', import.meta.url)),
        '@styles': fileURLToPath(new URL('./assets/styles/', import.meta.url)),
        '@configured-variables': fileURLToPath(new URL('./assets/styles/variables/_template.scss', import.meta.url)),
        '@db': fileURLToPath(new URL('./server/fake-db/', import.meta.url)),
        '@api-utils': fileURLToPath(new URL('./server/utils/', import.meta.url)),
      },
    },

    build: {
      chunkSizeWarningLimit: 5000,
      sourcemap: false,
    },

    optimizeDeps: {
      exclude: ['vuetify'],
      entries: [
        './**/*.vue',
      ],
    },

    plugins: [
      svgLoader(),
      vuetify({
        styles: {
          configFile: 'assets/styles/variables/_vuetify.scss',
        },
      }),
      null,
    ],
  },

  build: {
    transpile: ['vuetify'],
  },

  modules: ['@vueuse/nuxt', '@nuxtjs/device', '@pinia/nuxt'],
  ssr: false,
})