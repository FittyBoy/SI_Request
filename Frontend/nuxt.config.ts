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
    // ✅ ปิด devtools ใน dev — ลด overhead ได้มาก
    enabled: false,
  },

  css: [
    '@/assets/css/main.css',
    '@core/scss/template/index.scss',
    '@styles/styles.scss',
    '@/plugins/iconify/icons.css',
    'vuetify/styles',
    '@mdi/font/css/materialdesignicons.css',
  ],

  runtimeConfig: {
    // 🔒 Private keys — server-side only (จาก .env.development / .env.production)
    AUTH_ORIGIN: process.env.AUTH_ORIGIN,
    AUTH_SECRET: process.env.AUTH_SECRET,

    // 🌐 Public keys — expose ให้ client อ่านได้
    public: {
      apiBase: process.env.API_BASE_URL || 'http://localhost:9011',
      apiTimeout: 600000,
      isDevelopment: process.env.NODE_ENV === 'development',
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
    // ✅ เปิด payload extraction ลด hydration overhead
    payloadExtraction: false,
  },

  typescript: {
    // ✅ ปิด typecheck ตอน dev — ให้ IDE ทำแทน เร็วขึ้นมาก
    typeCheck: false,
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
      // ✅ แก้จาก exclude → include: ให้ Vite pre-bundle vuetify ตั้งแต่ต้น
      //    (exclude คือสาเหตุหลักที่ warmup ช้า 62 วิ)
      include: [
        'vuetify',
        'vuetify/components',
        'vuetify/directives',
        'pinia',
        'axios',
        'date-fns',
        'uuid',
        'jwt-decode',
      ],
      // ✅ ลบ entries glob './**/*.vue' ออก — scan ทุก vue file ทำให้ช้ามาก
    },

    // ✅ warmup เฉพาะ pages หลัก แทนการ scan glob ทั้งหมด
    server: {
      warmup: {
        clientFiles: [
          './pages/index.vue',
          './layouts/*.vue',
          './components/FlowOut/MainForm.vue',
        ],
      },
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