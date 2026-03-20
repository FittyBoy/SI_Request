<template>
  <div class="flow-out-container">
    <!-- หน้าหลัก: Main Form -->
    <MainForm 
      v-if="currentPage === 'main'" 
      @go-to-summary="currentPage = 'summary'"
      @go-to-rescreen="currentPage = 'rescreen'"
    />
    
    <!-- หน้าสรุป: Summary -->
    <Summary 
      v-else-if="currentPage === 'summary'" 
      @go-back="currentPage = 'main'"
    />
    
    <!-- หน้า Rescreen Check -->
    <RescreenCheck
      v-else-if="currentPage === 'rescreen'"
      @go-back="currentPage = 'main'"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useFlowOutStore } from '~/stores/flowout'

// Import components
import MainForm from '~/components/FlowOut/MainForm.vue'
import Summary from '~/components/FlowOut/Summary.vue'
import RescreenCheck from '@/components/FlowOut/RescreenCheck.vue'

// Page state
const currentPage = ref<'main' | 'summary' | 'rescreen'>('main')
const store = useFlowOutStore()

// Initialize store
onMounted(() => {
  store.initialize()
})
</script>

<style scoped>
.flow-out-container {
  min-height: 100vh;
  background: var(--color-bg);
}
</style>