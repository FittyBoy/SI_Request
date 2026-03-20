# SI_Request — OE-Production System

ระบบบริหารจัดการการผลิตและควบคุมคุณภาพ (Production & QA Management System) สำหรับฝ่าย OE  
พัฒนาด้วย **Nuxt 3 + Vuetify 3** (Frontend) และ **ASP.NET / Node.js** (Backend)

---

## 📁 โครงสร้าง Repository

```
SI_Request/
├── SI24004/        # Frontend — Nuxt 3 + Vuetify 3
└── Backend/        # Backend — (เตรียมไว้สำหรับ API Server)
```

---

## 🖥️ Frontend — SI24004

### Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | Nuxt 3.10.3 |
| UI Library | Vuetify 3.5.2 |
| State Management | Pinia 2.1.7 |
| Language | TypeScript 5.3.3 |
| HTTP Client | Axios |
| Chart | ApexCharts, Chart.js, ECharts |
| Auth | Cookie-based JWT (via `useCookie`) |
| Package Manager | pnpm 8.6.2 |

---

### ⚙️ Environment Variables

สร้างไฟล์ `.env` ที่ root ของ `SI24004/` โดย copy จาก `.env.example`:

```env
# API Base URL (ค่า default: http://172.18.106.100:9011)
API_BASE_URL=http://your-api-server:port

# Auth
AUTH_ORIGIN=http://localhost:3000
AUTH_SECRET=your-secret-key
```

> ⚠️ ไฟล์ `.env` ถูก gitignore แล้ว — **อย่า commit ลง repo**

---

### 🚀 Getting Started

```bash
# เข้าไปยัง frontend directory
cd SI24004

# ติดตั้ง dependencies
pnpm install

# รัน development server (http://localhost:3000)
pnpm dev

# Build สำหรับ production
pnpm build

# Generate static site
pnpm generate
```

> ⚠️ Build ต้องการ RAM ค่อนข้างมาก — ใช้ `node --max-old-space-size=4096` อยู่แล้วใน script

---

### 📄 หน้าต่างๆ ในระบบ (Pages)

| Route | ไฟล์ | คำอธิบาย |
|-------|------|----------|
| `/` | `index.vue` | หน้าหลัก (Dashboard) |
| `/login` | `login.vue` | หน้า Login (JWT Cookie) |
| `/check-po` | `check-po.vue` | ตรวจสอบ Purchase Order |
| `/material-receive` | `material-receive.vue` | รับวัตถุดิบ |
| `/qa-page` | `qa-page.vue` | หน้า QA ตรวจสอบคุณภาพ |
| `/qa-page-chart` | `qa-page-chart.vue` | QA Charts & Analytics |
| `/ina-page` | `ina-page.vue` | INA Process |
| `/pol-page` | `pol-page.vue` | POL Process |
| `/lapping_mat` | `lapping_mat.vue` | Lapping Material |
| `/registerdac` | `registerdac.vue` | ลงทะเบียน DAC |
| `/substance-master` | `substance-master.vue` | จัดการ Substance Master |
| `/svhc-master` | `svhc-master.vue` | จัดการ SVHC Master |

---

### 🔐 Authentication

- ใช้ **Cookie-based JWT** (ไม่ใช้ localStorage)
- Cookie มีอายุ **24 ชั่วโมง**
- Cookie ที่ถูก set หลัง login: `token`, `userId`, `userName`, `roleName`, `sectionId`, `sectionName`
- API login endpoint: `POST /api/User/login`

---

### 🔌 API Endpoints (Default)

Base URL: `http://172.18.106.100:9011` (กำหนดผ่าน `API_BASE_URL`)

```
POST   /api/User/login              — เข้าสู่ระบบ
GET    /api/SI25011/health          — Health check
POST   /api/SI25011/compare         — เปรียบเทียบไฟล์
GET    /api/SI25011/upload-progress — ติดตาม progress
```

---

### 📦 โครงสร้าง Frontend

```
SI24004/
├── pages/           # Nuxt routes (แต่ละหน้า)
├── components/      # Reusable Vue components
│   ├── dialogs/     # Dialog components (Add/Edit/Confirm)
│   └── ...
├── views/           # Page-level view components
├── stores/          # Pinia stores (state management)
├── composables/     # Composable functions
├── plugins/         # Nuxt plugins (Vuetify, Iconify)
├── @core/           # Core utilities และ components
├── @layouts/        # Layout components
├── assets/          # Static assets, SCSS styles
├── server/          # Nitro server (SSR disabled)
├── types/           # TypeScript type definitions
├── utils/           # Utility functions
├── navigation/      # Navigation menu config
├── nuxt.config.ts   # Nuxt configuration
├── themeConfig.ts   # Theme configuration
└── package.json
```

---

### 🗒️ Notes

- **SSR ปิดอยู่** (`ssr: false`) — รันเป็น SPA
- Source maps ถูกปิดทั้ง server และ client เพื่อลดขนาด build
- Chunk size warning limit: 5000 kB
- รองรับ dark/light mode ผ่าน Vuetify theme
- i18n พร้อมใช้งานผ่าน `vue-i18n`

---

## 🔧 Backend

> โฟลเดอร์ `Backend/` ถูกเตรียมไว้สำหรับ API Server  
> ปัจจุบัน API ถูก serve ที่ `http://172.18.106.100:9011`

---

## 📋 Requirements

- Node.js >= 18.x
- pnpm >= 8.6.2
- RAM >= 4 GB (สำหรับ build)

---

## 👥 Contributors

- **FittyBoy** — Lead Developer

---

*Last updated: March 2026*
