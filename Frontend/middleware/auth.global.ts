/**
 * Global Auth Middleware
 * - ถ้าไม่มี token → redirect ไป /login?redirect=<intended-url>
 * - ถ้ามี token อยู่แล้ว → ผ่านได้เลย
 * - หน้า /login และ / ไม่ต้องตรวจสอบ
 */
export default defineNuxtRouteMiddleware((to) => {
  // หน้าที่ไม่ต้องการ auth
  const publicPages = ['/login', '/']
  if (publicPages.includes(to.path)) return

  const token = useCookie('token')

  if (!token.value) {
    // เก็บ full path (รวม query string) เพื่อ redirect กลับหลัง login
    const intendedUrl = to.fullPath
    return navigateTo(`/login?redirect=${encodeURIComponent(intendedUrl)}`, { replace: true })
  }
})
