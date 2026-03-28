<script setup lang="ts">
import ScatterChart from "@/components/ScatterChart.vue";

/**
 * Deep-link query params ที่รองรับ:
 *   ?lot=AZ2NJ088411A0          → pre-filter โดย LotId
 *   ?size=76x76x0.2+Sample+10+Pcs → pre-filter โดย Product Size
 *   ?monitor=1                  → เปิด Monitor Dialog ของ lot นั้นทันที (ต้องใช้คู่กับ lot)
 *
 * ตัวอย่าง link ใน email:
 *   http://your-server/polishing/pol-page?lot=AZ2NJ088411A0&size=76x76x0.2%20Sample%2010%20Pcs&monitor=1
 */

const route = useRoute()

const initialLot  = computed(() => (route.query.lot  as string) || null)
const initialSize = computed(() => (route.query.size as string) || null)
const autoMonitor = computed(() => route.query.monitor === '1')
</script>

<template>
  <v-card>
    <ScatterChart
      :initial-lot="initialLot"
      :initial-size="initialSize"
      :auto-open-monitor="autoMonitor"
    />
  </v-card>
</template>
