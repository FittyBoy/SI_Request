<script setup lang="ts">
import avatar1 from '@images/avatars/avatar-1.png'
import Swal from 'sweetalert2';
// ใช้ Router สำหรับเปลี่ยนเส้นทาง
const router = useRouter();
// ดึงค่า Cookie
const tokenCookie = useCookie('token');
const userIdCookie = useCookie('userId');
const userNameCookie = useCookie('userName');
const roleCookie = useCookie('role');

const logout = () => {
  Swal.fire({
    title: 'Logout',
    text: 'Are you sure you want to log out?',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Yes, Logout',
    cancelButtonText: 'Cancel',
  }).then((result) => {
    if (result.isConfirmed) {
      // ล้างค่า Cookie
      tokenCookie.value = null;
      userIdCookie.value = null;
      userNameCookie.value = null;
      roleCookie.value = null;

      Swal.fire({
        title: 'Logged Out',
        text: 'You have successfully logged out.',
        icon: 'success',
        confirmButtonText: 'OK',
      });

      // กลับไปหน้า Login
      router.push('/login');
    }
  });
};

</script>

<template>
  <VBadge dot location="bottom right" offset-x="3" offset-y="3" bordered color="success">
    <VAvatar class="cursor-pointer" color="primary" variant="tonal">
      <VImg :src="avatar1" />

      <!-- SECTION Menu -->
      <VMenu activator="parent" width="230" location="bottom end" offset="14px">
        <VList>
          <!-- 👉 User Avatar & Name -->
          <VListItem>
            <template #prepend>
              <VListItemAction start>
                <VBadge dot location="bottom right" offset-x="3" offset-y="3" color="success">
                  <VAvatar color="primary" variant="tonal">
                    <VImg :src="avatar1" />
                  </VAvatar>
                </VBadge>
              </VListItemAction>
            </template>

            <VListItemTitle class="font-weight-semibold">
              {{userNameCookie}}
            </VListItemTitle>
            <VListItemSubtitle>{{roleCookie}}</VListItemSubtitle>
          </VListItem>

          <VDivider class="my-2" />

          <!-- 👉 Logout -->
          <VListItem @click="logout">
            <template #prepend>
              <VIcon class="me-2" icon="tabler-logout" size="22" />
            </template>

            <VListItemTitle>Logout</VListItemTitle>
          </VListItem>
        </VList>
      </VMenu>
      <!-- !SECTION -->
    </VAvatar>
  </VBadge>
</template>
