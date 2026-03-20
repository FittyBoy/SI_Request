<script setup lang="ts">
import { useGenerateImageVariant } from '@core/composable/useGenerateImageVariant'
import authV2LoginIllustrationBorderedDark from '@images/pages/auth-v2-login-illustration-bordered-dark.png'
import authV2LoginIllustrationBorderedLight from '@images/pages/auth-v2-login-illustration-bordered-light.png'
import authV2LoginIllustrationDark from '@images/pages/auth-v2-login-illustration-dark.png'
import authV2LoginIllustrationLight from '@images/pages/auth-v2-login-illustration-light.png'
import authV2MaskDark from '@images/pages/misc-mask-dark.png'
import authV2MaskLight from '@images/pages/misc-mask-light.png'
import { VNodeRenderer } from '@layouts/components/VNodeRenderer'
import { themeConfig } from '@themeConfig'
import { useRouter, useRuntimeConfig } from '#imports'
import Swal from 'sweetalert2'
import 'sweetalert2/src/sweetalert2.scss'

definePageMeta({ layout: 'blank' })

// 🔧 Types - ปรับให้ตรงกับ API Response จริง
interface LoginRequest {
  UserId: string
  UserPassword: string
}

interface LoginResponse {
  Token: string           // ← API ส่งมาเป็น Token (T ใหญ่)
  SectionId: string
  SectionName: string
  User: {
    Id: string
    UserId: string
    UserName: string
    RoleName: string
  }
}

// 🍪 ใช้ Cookie แทน localStorage
const token = useCookie('token', { maxAge: 3600 * 24 }) // 24 ชั่วโมง
const userId = useCookie('userId', { maxAge: 3600 * 24 })
const userName = useCookie('userName', { maxAge: 3600 * 24 })
const role = useCookie('roleName', { maxAge: 3600 * 24 })
const sectionId = useCookie('sectionId', { maxAge: 3600 * 24 })
const sectionName = useCookie('sectionName', { maxAge: 3600 * 24 })

const router = useRouter()
const config = useRuntimeConfig()

// 📝 Form Data - ปรับให้ตรงกับ API
const form = ref<LoginRequest>({
  UserId: '',
  UserPassword: ''
})

const isPasswordVisible = ref(false)
const isLoading = ref(false)

// 🎨 Theme Images
const authThemeImg = useGenerateImageVariant(
  authV2LoginIllustrationLight,
  authV2LoginIllustrationDark,
  authV2LoginIllustrationBorderedLight,
  authV2LoginIllustrationBorderedDark,
  true
)
const authThemeMask = useGenerateImageVariant(authV2MaskLight, authV2MaskDark)

// 🚀 ฟังก์ชัน Login - แก้ไขให้ครบถ้วน
const login = async () => {
  // ✅ Validation
  if (!form.value.UserId?.trim() || !form.value.UserPassword?.trim()) {
    return Swal.fire({
      title: 'ข้อมูลไม่ครบถ้วน',
      text: 'กรุณากรอก User ID และ Password',
      icon: 'warning',
      confirmButtonText: 'ตกลง'
    });
  }

  isLoading.value = true;

  try {
    // 🔗 เรียก API
    const { data, error, status } = await useFetch<LoginResponse>('/api/User/login', {
      method: 'POST',
      body: {
        UserId: form.value.UserId.trim(),
        UserPassword: form.value.UserPassword.trim()
      },
      baseURL: config.public.apiBase,
      headers: {
        'Content-Type': 'application/json',
      },
    });


    // ✅ ตรวจสอบ HTTP Status
    if (status.value === 'error' || error.value) {
      let errorMessage = 'เกิดข้อผิดพลาดในการเข้าสู่ระบบ'
      
      if (error.value?.status === 401) {
        errorMessage = 'User ID หรือ Password ไม่ถูกต้อง'
      } else if (error.value?.status === 400) {
        errorMessage = 'ข้อมูลที่ส่งไม่ถูกต้อง'
      } else if (error.value?.status === 500) {
        errorMessage = 'เซิร์ฟเวอร์ขัดข้อง กรุณาลองใหม่อีกครั้ง'
      } else if (error.value?.data?.Message) {
        errorMessage = error.value.data.Message
      } else if (error.value?.data?.message) {
        errorMessage = error.value.data.message
      }
      
      throw new Error(errorMessage)
    }

    // ✅ ตรวจสอบ Response Data
    if (!data.value) {
      throw new Error('ไม่ได้รับข้อมูลตอบกลับจากเซิร์ฟเวอร์')
    }

    // ✅ ตรวจสอบ Token - ใช้ field name ที่ถูกต้อง
    const receivedToken = data.value.Token || data.value.token
    if (!receivedToken) {
      console.error('❌ Token not found in response:', data.value)
      throw new Error('ไม่ได้รับ Token จากเซิร์ฟเวอร์')
    }


    // 🍪 เก็บข้อมูลลง Cookie
    token.value = receivedToken
    userId.value = data.value.User?.Id || ''
    userName.value = data.value.User?.UserName || ''
    role.value = data.value.User?.RoleName || ''
    sectionId.value = data.value.SectionId || ''
    sectionName.value = data.value.SectionName || ''

    // 🎉 แสดงข้อความสำเร็จ
    await Swal.fire({
      title: 'เข้าสู่ระบบสำเร็จ!',
      text: `ยินดีต้อนรับ ${userName.value}`,
      icon: 'success',
      confirmButtonText: 'ตกลง',
      timer: 2000,
      timerProgressBar: true
    })

    // 🔄 Redirect ไปหน้าหลัก
    await router.push('/request/ina-page')

  } catch (error: any) {
    console.error('❌ Login Failed:', error)

    // 🚨 แสดงข้อความผิดพลาด
    await Swal.fire({
      title: 'เข้าสู่ระบบไม่สำเร็จ!',
      text: error.message || 'กรุณาตรวจสอบ User ID และ Password แล้วลองใหม่อีกครั้ง',
      icon: 'error',
      confirmButtonText: 'ลองใหม่',
      confirmButtonColor: '#d33'
    })

    // ล้างข้อมูลฟอร์ม
    form.value.UserPassword = ''
  } finally {
    isLoading.value = false
  }
}


// เรียกทดสอบ connection เมื่อ component mount (เฉพาะ development)
onMounted(() => {
})
</script>

<template>
  <NuxtLink to="/">
    <div class="auth-logo d-flex align-center gap-x-3">
      <VNodeRenderer :nodes="themeConfig.app.logo" />
      <h1 class="auth-title">{{ themeConfig.app.title }}</h1>
    </div>
  </NuxtLink>

  <VRow no-gutters class="auth-wrapper bg-surface">
    <VCol md="8" class="d-none d-md-flex">
      <div class="position-relative bg-background w-100 me-0">
        <div class="d-flex align-center justify-center w-100 h-100" style="padding-inline: 6.25rem;">
          <VImg max-width="613" :src="authThemeImg" class="auth-illustration mt-16 mb-2" />
        </div>
        <img class="auth-footer-mask" :src="authThemeMask" alt="auth-footer-mask" height="280" width="100">
      </div>
    </VCol>

    <VCol cols="12" md="4" class="auth-card-v2 d-flex align-center justify-center">
      <VCard flat :max-width="500" class="mt-12 mt-sm-0 pa-4">
        <VCardText>
          <h4 class="text-h4 mb-1">
            ยินดีต้อนรับสู่ <span class="text-capitalize"> {{ themeConfig.app.title }} </span>! 👋🏻
          </h4>
          <p class="mb-0">กรุณาเข้าสู่ระบบเพื่อเริ่มใช้งาน</p>
        </VCardText>

        <VCardText>
          <VForm @submit.prevent="login">
            <VRow>
              <!-- User ID -->
              <VCol cols="12">
                <AppTextField 
                  v-model="form.UserId" 
                  autofocus 
                  label="User ID" 
                  placeholder="กรอก User ID ของคุณ"
                  :disabled="isLoading"
                  required
                />
              </VCol>

              <!-- Password -->
              <VCol cols="12">
                <AppTextField 
                  v-model="form.UserPassword"
                  label="Password"
                  placeholder="············"
                  :type="isPasswordVisible ? 'text' : 'password'"
                  :append-inner-icon="isPasswordVisible ? 'tabler-eye-off' : 'tabler-eye'"
                  :disabled="isLoading"
                  required
                  @click:append-inner="isPasswordVisible = !isPasswordVisible" 
                  @keyup.enter="login"
                />
              </VCol>

              <!-- Login Button -->
              <VCol cols="12">
                <VBtn 
                  block 
                  type="submit" 
                  :loading="isLoading"
                  :disabled="!form.UserId || !form.UserPassword"
                  color="primary"
                  size="large"
                >
                  <span v-if="!isLoading">เข้าสู่ระบบ</span>
                  <span v-else>กำลังเข้าสู่ระบบ...</span>
                </VBtn>
              </VCol>
            </VRow>
          </VForm>
        </VCardText>
      </VCard>
    </VCol>
  </VRow>
</template>

<style lang="scss">
@use "@core/scss/template/pages/page-auth.scss";
.auth-card-v2 {
  .v-card { box-shadow: var(--el-4) !important; border-radius: var(--rr-xl) !important; }
  .v-btn { border-radius: var(--rr-md) !important; font-weight: 700 !important; text-transform: none !important; }
  .v-text-field .v-field { border-radius: var(--rr-md) !important; }
}
</style>