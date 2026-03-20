<script setup lang="ts">
definePageMeta({ layout: false })

const token = useCookie('token')
if (token.value) {
  await navigateTo('/request/ina-page', { replace: true })
} else {
  await navigateTo('/login', { replace: true })
}
</script>

<template>
  <div />
</template>
