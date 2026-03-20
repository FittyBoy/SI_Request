<template>
  <div class="zone-dashboard" :class="{ 'dark': isDarkMode }">
    <!-- Header -->
    <div class="header">
      <div class="header-content">
        <div class="title-section">
          <div class="icon-wrapper">
            <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="3" y="3" width="7" height="7"/>
              <rect x="14" y="3" width="7" height="7"/>
              <rect x="14" y="14" width="7" height="7"/>
              <rect x="3" y="14" width="7" height="7"/>
            </svg>
          </div>
          <div>
            <h1>Zone Layout Dashboard</h1>
            <p class="subtitle">Flow-out Check by Production Zone</p>
          </div>
        </div>
        <div class="header-actions">
          <button class="btn-dark" @click="isDarkMode = !isDarkMode" :title="isDarkMode ? 'Light Mode' : 'Dark Mode'">
            <svg v-if="isDarkMode" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="5"/>
              <line x1="12" y1="1" x2="12" y2="3"/>
              <line x1="12" y1="21" x2="12" y2="23"/>
              <line x1="4.22" y1="4.22" x2="5.64" y2="5.64"/>
              <line x1="18.36" y1="18.36" x2="19.78" y2="19.78"/>
              <line x1="1" y1="12" x2="3" y2="12"/>
              <line x1="21" y1="12" x2="23" y2="12"/>
              <line x1="4.22" y1="19.78" x2="5.64" y2="18.36"/>
              <line x1="18.36" y1="5.64" x2="19.78" y2="4.22"/>
            </svg>
            <svg v-else xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/>
            </svg>
          </button>
          <button class="btn-back" @click="handleGoBack">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="19" y1="12" x2="5" y2="12"/>
              <polyline points="12 19 5 12 12 5"/>
            </svg>
            Back
          </button>
          <button class="btn-summary" @click="showSummary = !showSummary">
            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M3 3v18h18"/>
              <path d="M18 17V9"/>
              <path d="M13 17V5"/>
              <path d="M8 17v-3"/>
            </svg>
            {{ showSummary ? 'Hide' : 'Show' }} Summary
          </button>
        </div>
      </div>
    </div>

    <div class="content">
      <!-- Filters -->
      <div class="card filter-card">
        <div class="filter-section">
          <div class="input-group">
            <label>Date</label>
            <input type="date" v-model="selectedDate" @change="onDateChange" class="input-modern"/>
          </div>
          <button class="btn-refresh" @click="loadData" :disabled="isLoading">
            <svg v-if="!isLoading" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M21.5 2v6h-6M2.5 22v-6h6M2 11.5a10 10 0 0 1 18.8-4.3M22 12.5a10 10 0 0 1-18.8 4.2"/>
            </svg>
            <div v-else class="spinner-small"></div>
            Refresh
          </button>
        </div>
      </div>

      <!-- Summary Stats -->
      <div v-if="showSummary" class="stats-overview">
        <div class="stat-card">
          <div class="stat-icon total">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
              <circle cx="12" cy="7" r="4"/>
            </svg>
          </div>
          <div class="stat-content">
            <span class="stat-label">Total MCs</span>
            <span class="stat-value">{{ totalMCs }}</span>
          </div>
        </div>
        <div class="stat-card">
          <div class="stat-icon total">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="2" y="7" width="20" height="14" rx="2"/>
              <path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/>
            </svg>
          </div>
          <div class="stat-content">
            <span class="stat-label">Total LOTs</span>
            <span class="stat-value">{{ totalLots }}</span>
          </div>
        </div>
        <div class="stat-card success">
          <div class="stat-icon ok">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
              <polyline points="22 4 12 14.01 9 11.01"/>
            </svg>
          </div>
          <div class="stat-content">
            <span class="stat-label">OK Status</span>
            <span class="stat-value">{{ okCount }}</span>
          </div>
        </div>
        <div class="stat-card danger">
          <div class="stat-icon ng">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"/>
              <line x1="15" y1="9" x2="9" y2="15"/>
              <line x1="9" y1="9" x2="15" y2="15"/>
            </svg>
          </div>
          <div class="stat-content">
            <span class="stat-label">NG Status</span>
            <span class="stat-value">{{ ngCount }}</span>
          </div>
        </div>
        <div class="stat-card info">
          <div class="stat-icon qty">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="1" y="3" width="15" height="13"/>
              <polygon points="16 8 20 8 23 11 23 16 16 16 16 8"/>
              <circle cx="5.5" cy="18.5" r="2.5"/>
              <circle cx="18.5" cy="18.5" r="2.5"/>
            </svg>
          </div>
          <div class="stat-content">
            <span class="stat-label">Total Quantity</span>
            <span class="stat-value">{{ totalQuantity?.toLocaleString() }}</span>
          </div>
        </div>
        <!-- ✅ REP stat card -->
        <div class="stat-card rep">
          <div class="stat-icon rep-icon">
            <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/>
            </svg>
          </div>
          <div class="stat-content">
            <span class="stat-label">REP LOTs</span>
            <span class="stat-value">{{ repLotCount }}</span>
          </div>
        </div>
      </div>

      <!-- Loading -->
      <div v-if="isLoading" class="loading-container">
        <div class="loading-spinner">
          <div class="spinner"></div>
          <p>Loading zone data...</p>
        </div>
      </div>

      <!-- Zone Layout -->
      <div v-else class="zones-container">
        <!-- Normal zones (Zone A–I) -->
        <div v-for="zone in mainZones" :key="zone.name" class="zone-row">
          <div class="zone-section">
            <div class="zone-header">
              <h2>{{ zone.name }}</h2>
              <span class="zone-count">{{ getZoneMCCount(zone.machines) }} MCs</span>
            </div>
            <div class="zone-machines-horizontal">
              <div v-for="(item, idx) in zone.layout" :key="`${zone.name}-${idx}`" class="machine-wrapper">
                <div v-if="item.type === 'machine'" class="machine-cell" :class="getMachineStatus(item.mc)">
                  <div class="machine-header">
                    <span class="machine-name">{{ item.mc }}</span>
                    <span class="lot-count">{{ getMachineLotCount(item.mc) }}</span>
                  </div>
                  <div class="machine-lots">
                    <div v-for="lot in getMachineLots(item.mc)" :key="lot.id || lot.poLot"
                      class="lot-item" :class="lot.statusTn?.toUpperCase() === 'SCRAP' ? 'scrap' : (lot.checkSt ? 'ok' : 'ng')">
                      <span class="lot-name">{{ lot.poLot }}</span>
                      <span class="lot-badge" :class="lot.statusTn?.toUpperCase() === 'SCRAP' ? 'badge-scrap' : (lot.checkSt ? 'badge-ok' : 'badge-ng')">
                        {{ lot.check }}
                      </span>
                      <span class="lot-qty">{{ (lot.lotQty || 0).toLocaleString() }}</span>
                    </div>
                    <div v-if="getMachineLots(item.mc).length === 0" class="no-lots">No LOTs</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Zone 9B -->
        <div class="zone-row">
          <div class="zone-section">
            <div class="zone-header">
              <h2>{{ zone9B.name }}</h2>
              <span class="zone-count">{{ getZoneMCCount(zone9B.machines) }} MCs</span>
            </div>
            <div class="zone-machines-horizontal">
              <div v-for="item in zone9B.layout" :key="item.mc" class="machine-wrapper">
                <div v-if="item.type === 'machine'" class="machine-cell" :class="getMachineStatus(item.mc)">
                  <div class="machine-header">
                    <span class="machine-name">{{ item.mc }}</span>
                    <span class="lot-count">{{ getMachineLotCount(item.mc) }}</span>
                  </div>
                  <div class="machine-lots">
                    <div v-for="lot in getMachineLots(item.mc)" :key="lot.id || lot.poLot"
                      class="lot-item" :class="lot.statusTn?.toUpperCase() === 'SCRAP' ? 'scrap' : (lot.checkSt ? 'ok' : 'ng')">
                      <span class="lot-name">{{ lot.poLot }}</span>
                      <span class="lot-badge" :class="lot.statusTn?.toUpperCase() === 'SCRAP' ? 'badge-scrap' : (lot.checkSt ? 'badge-ok' : 'badge-ng')">
                        {{ lot.check }}
                      </span>
                      <span class="lot-qty">{{ (lot.lotQty || 0).toLocaleString() }}</span>
                    </div>
                    <div v-if="getMachineLots(item.mc).length === 0" class="no-lots">No LOTs</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- ✅ Zone REP — แถวล่างสุด -->
        <div class="zone-row" v-if="repLots.length > 0 || true">
          <div class="zone-section zone-section-rep">
            <div class="zone-header zone-header-rep">
              <div class="zone-header-left">
                <span class="rep-zone-badge">
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                    <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/>
                  </svg>
                  REP
                </span>
                <h2>REP Product</h2>
                <span class="rep-subtitle">Special Product — No ImobileLot Required</span>
              </div>
              <span class="zone-count rep-count">{{ repLots.length }} LOTs</span>
            </div>

            <!-- เนื้อหา: ถ้าไม่มี lot แสดง empty state -->
            <div v-if="repLots.length === 0" class="rep-empty">
              <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                <circle cx="12" cy="12" r="10"/>
                <line x1="12" y1="8" x2="12" y2="12"/>
                <line x1="12" y1="16" x2="12.01" y2="16"/>
              </svg>
              <p>ไม่มี REP LOT สำหรับวันที่เลือก</p>
            </div>

            <!-- Grid ของ REP lots แบ่งตาม prefix (0403O-REP, 0503O-REP, …) -->
            <div v-else class="rep-groups">
              <div v-for="(group, prefix) in repGrouped" :key="prefix" class="rep-group">
                <div class="rep-group-header">
                  <span class="rep-group-name">{{ prefix }}</span>
                  <span class="rep-group-count">{{ group.length }} lots</span>
                </div>
                <div class="rep-lots-grid">
                  <div v-for="lot in group" :key="lot.id || lot.poLot" class="rep-lot-item">
                    <div class="rep-lot-top">
                      <span class="rep-lot-name">{{ lot.poLot }}</span>
                      <span class="lot-badge badge-ok">OK</span>
                    </div>
                    <div class="rep-lot-bottom">
                      <span class="rep-lot-seq">
                        #{{ getRepSeq(lot.poLot) }}
                      </span>
                      <span class="rep-lot-suffix" v-if="getRepSuffix(lot.poLot)">
                        {{ getRepSuffix(lot.poLot) }}
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useFlowOutApi } from '~/composables/useFlowOutApi'

interface LotData {
  id?: string
  poLot: string
  imobileLot?: string | null
  mcNo: string
  status?: string
  statusTn?: string
  lotQty?: number | null
  check?: string
  checkSt?: boolean
  checkDate?: string
}

interface ZoneLayout {
  name: string
  columns: number
  machines: string[]
  layout: Array<{ type: string; mc?: string }>
}

const emit = defineEmits<{ (e: 'goBack'): void }>()
const api = useFlowOutApi()

const selectedDate = ref<string>('')
const isLoading = ref<boolean>(false)
const showSummary = ref<boolean>(true)
const isDarkMode = ref<boolean>(false)
const allLotData = ref<LotData[]>([])

// Zone Configuration
const mainZones = ref<ZoneLayout[]>([
  { name: 'Zone A', columns: 1, machines: ['637','638','639','640','641'],
    layout: [{ type:'machine',mc:'637'},{ type:'machine',mc:'638'},{ type:'machine',mc:'639'},{ type:'machine',mc:'640'},{ type:'machine',mc:'641'}] },
  { name: 'Zone B', columns: 1, machines: ['642','617','616','615','614'],
    layout: [{ type:'machine',mc:'642'},{ type:'machine',mc:'617'},{ type:'machine',mc:'616'},{ type:'machine',mc:'615'},{ type:'machine',mc:'614'}] },
  { name: 'Zone C', columns: 1, machines: ['613','612','618','611','610'],
    layout: [{ type:'machine',mc:'613'},{ type:'machine',mc:'612'},{ type:'machine',mc:'618'},{ type:'machine',mc:'611'},{ type:'machine',mc:'610'}] },
  { name: 'Zone D', columns: 1, machines: ['609','608','607','606','605'],
    layout: [{ type:'machine',mc:'609'},{ type:'machine',mc:'608'},{ type:'machine',mc:'607'},{ type:'machine',mc:'606'},{ type:'machine',mc:'605'}] },
  { name: 'Zone E', columns: 1, machines: ['604','603','602','601','647'],
    layout: [{ type:'machine',mc:'604'},{ type:'machine',mc:'603'},{ type:'machine',mc:'602'},{ type:'machine',mc:'601'},{ type:'machine',mc:'647'}] },
  { name: 'Zone F', columns: 1, machines: ['623','624','622','621','646'],
    layout: [{ type:'machine',mc:'623'},{ type:'machine',mc:'624'},{ type:'machine',mc:'622'},{ type:'machine',mc:'621'},{ type:'machine',mc:'646'}] },
  { name: 'Zone G', columns: 1, machines: ['620','619','628','627','625'],
    layout: [{ type:'machine',mc:'620'},{ type:'machine',mc:'619'},{ type:'machine',mc:'628'},{ type:'machine',mc:'627'},{ type:'machine',mc:'625'}] },
  { name: 'Zone H', columns: 1, machines: ['631','630','629','636','635'],
    layout: [{ type:'machine',mc:'631'},{ type:'machine',mc:'630'},{ type:'machine',mc:'629'},{ type:'machine',mc:'636'},{ type:'machine',mc:'635'}] },
  { name: 'Zone I', columns: 1, machines: ['633','632','643','644','645'],
    layout: [{ type:'machine',mc:'633'},{ type:'machine',mc:'632'},{ type:'machine',mc:'643'},{ type:'machine',mc:'644'},{ type:'machine',mc:'645'}] },
])

const zone9B = ref<ZoneLayout>({
  name: 'Zone 9B', columns: 1, machines: ['917','918','921','920','904','905'],
  layout: [{ type:'machine',mc:'917'},{ type:'machine',mc:'918'},{ type:'machine',mc:'921'},{ type:'machine',mc:'920'},{ type:'machine',mc:'904'},{ type:'machine',mc:'905'}]
})

// ✅ filteredLotData ไม่รวม REP (แสดงแยก)
const filteredLotData = computed(() =>
  allLotData.value.filter(lot => lot.mcNo?.toUpperCase() !== 'REP')
)

// ✅ REP lots แยกออกมา
const repLots = computed(() =>
  allLotData.value
    .filter(lot => lot.mcNo?.toUpperCase() === 'REP')
    .sort((a, b) => {
      const partsA = a.poLot?.split('-') ?? []
      const partsB = b.poLot?.split('-') ?? []
      const prefixA = `${partsA[0]}-${partsA[1]}`
      const prefixB = `${partsB[0]}-${partsB[1]}`
      if (prefixA !== prefixB) return prefixA.localeCompare(prefixB)
      const seqA = parseInt(partsA[2] ?? '0', 10)
      const seqB = parseInt(partsB[2] ?? '0', 10)
      return seqA - seqB
    })
)

// ✅ จัดกลุ่ม REP lots ด้วย prefix (เช่น "0403O-REP")
const repGrouped = computed(() => {
  const groups: Record<string, LotData[]> = {}
  repLots.value.forEach(lot => {
    const parts = lot.poLot?.split('-') ?? []
    const prefix = `${parts[0]}-${parts[1]}`
    if (!groups[prefix]) groups[prefix] = []
    groups[prefix].push(lot)
  })
  return groups
})

const repLotCount = computed(() => repLots.value.length)

// ✅ helpers สำหรับ REP lot display
const getRepSeq = (poLot: string): string => {
  const parts = poLot?.split('-') ?? []
  return parts[2] ?? ''
}
const getRepSuffix = (poLot: string): string => {
  const parts = poLot?.split('-') ?? []
  return parts[3] ?? ''
}

const totalMCs = computed(() => {
  const allMachines = [...mainZones.value.flatMap(z => z.machines), ...zone9B.value.machines]
  const uniqueMCs = new Set(filteredLotData.value.map(lot => lot.mcNo))
  return allMachines.filter(mc => uniqueMCs.has(mc)).length
})

const totalLots = computed(() => filteredLotData.value.length)
const okCount = computed(() => filteredLotData.value.filter(lot => lot.checkSt).length)
const ngCount = computed(() => filteredLotData.value.filter(lot => !lot.checkSt && lot.statusTn?.toUpperCase() !== 'SCRAP').length)
const totalQuantity = computed(() => filteredLotData.value.reduce((sum, lot) => sum + (lot.lotQty || 0), 0))

const getZoneMCCount = (machines: string[]): number => {
  const uniqueMCs = new Set(filteredLotData.value.map(lot => lot.mcNo))
  return machines.filter(mc => uniqueMCs.has(mc)).length
}

const getMachineStatus = (mc: string): string => {
  const lots = getMachineLots(mc)
  if (lots.length === 0) return 'no-data'
  const hasNG = lots.some(lot => !lot.checkSt && lot.statusTn?.toUpperCase() !== 'SCRAP')
  return hasNG ? 'has-ng' : 'all-ok'
}

const getMachineLotCount = (mc: string): number =>
  filteredLotData.value.filter(lot => lot.mcNo === mc).length

const getMachineLots = (mc: string): LotData[] => {
  return filteredLotData.value
    .filter(lot => lot.mcNo === mc)
    .sort((a, b) => {
      const partsA = a.poLot?.split('-') ?? []
      const partsB = b.poLot?.split('-') ?? []
      const prefixA = partsA[0] ?? ''
      const prefixB = partsB[0] ?? ''
      if (prefixA !== prefixB) return prefixA.localeCompare(prefixB)
      const noPoA = parseInt(partsA[2]?.match(/^(\d+)/)?.[1] ?? '0', 10)
      const noPoB = parseInt(partsB[2]?.match(/^(\d+)/)?.[1] ?? '0', 10)
      if (noPoA !== noPoB) return noPoA - noPoB
      const suffixA = partsA[3] ?? ''
      const suffixB = partsB[3] ?? ''
      return suffixA.localeCompare(suffixB)
    })
}

const handleGoBack = (): void => emit('goBack')
const onDateChange = async (): Promise<void> => { await loadData() }

const loadData = async (): Promise<void> => {
  isLoading.value = true
  try {
    const mcListResponse: any = await api.getMcList(selectedDate.value || undefined)
    if (!mcListResponse?.success || !mcListResponse?.data?.length) {
      allLotData.value = []
      return
    }
    const results = await Promise.all(
      mcListResponse.data.map((mcNo: string) =>
        api.getLotsByMc(mcNo, selectedDate.value || undefined)
      )
    )
    const combined: LotData[] = []
    results.forEach((res: any) => {
      if (res?.success && Array.isArray(res?.data)) {
        combined.push(...res.data)
      }
    })
    allLotData.value = combined
  } catch (err) {
    console.error('❌ Error loading data:', err)
    allLotData.value = []
  } finally {
    isLoading.value = false
  }
}

onMounted(async () => {
  if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
    isDarkMode.value = true
  }
  const today = new Date()
  selectedDate.value = today.toISOString().split('T')[0]
  await loadData()
})
</script>

<style scoped>
/* === Page === */
.zone-dashboard {
  min-height: 100vh;
  background: linear-gradient(160deg, #e8eaf6 0%, #f3f4fd 40%, #ede7f6 100%);
  transition: background var(--t-slow) var(--ease);
}
.zone-dashboard.dark {
  background: linear-gradient(160deg, #0d0e1a 0%, #0f1024 40%, #0c0d18 100%);
}

/* === Header === */
.zone-header {
  background: rgba(255,255,255,0.96);
  backdrop-filter: blur(16px);
  border-bottom: 1px solid rgba(92,107,192,0.2);
  padding: var(--s-3) var(--s-6);
  display: flex; align-items: center; justify-content: space-between;
  position: sticky; top: 0; z-index: 50;
  box-shadow: 0 2px 16px rgba(92,107,192,0.08);
}
.zone-dashboard.dark .zone-header {
  background: rgba(13,14,26,0.97);
  border-bottom-color: rgba(92,107,192,0.1);
}

.zone-header-title {
  font-size: var(--fs-lg); font-weight: 800; letter-spacing: -0.03em;
  background: linear-gradient(135deg, var(--brand) 0%, #7c4dff 100%);
  -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text;
}

/* === Summary Bar === */
.summary-bar {
  display: flex; gap: var(--s-3); flex-wrap: wrap;
  padding: var(--s-4) var(--s-6);
  background: rgba(255,255,255,0.7);
  border-bottom: 1px solid rgba(92,107,192,0.1);
  backdrop-filter: blur(8px);
}
.zone-dashboard.dark .summary-bar { background: rgba(18,20,42,0.7); }

.summary-chip {
  display: flex; align-items: center; gap: var(--s-2);
  padding: var(--s-2) var(--s-4);
  border-radius: var(--r-full);
  border: 1px solid var(--border);
  background: var(--surface);
  font-size: var(--fs-sm); font-weight: 700;
  transition: all var(--t-fast) var(--ease);
}
.summary-chip:hover { border-color: var(--brand); background: var(--brand-xlight); }

.chip-icon {
  width: 28px; height: 28px; border-radius: var(--r-md);
  display: flex; align-items: center; justify-content: center; font-size: 14px;
}
.chip-value { font-size: var(--fs-md); font-weight: 800; color: var(--text-1); }
.chip-label { font-size: var(--fs-xs); color: var(--text-3); }

/* === Date Picker === */
.date-input {
  border: 1.5px solid var(--border); border-radius: var(--r-md);
  padding: var(--s-2) var(--s-3); font-family: var(--font);
  font-size: var(--fs-sm); color: var(--text-1); background: var(--surface);
  outline: none;
  transition: border-color var(--t-fast) var(--ease);
}
.date-input:focus { border-color: var(--brand); box-shadow: 0 0 0 3px rgba(92,107,192,0.15); }

/* === Zone Card === */
.zone-card {
  background: rgba(255,255,255,0.97);
  border-radius: var(--r-xl); border: 1px solid var(--border);
  box-shadow: var(--shadow-md);
  overflow: hidden; margin-bottom: var(--s-5);
  transition: box-shadow var(--t-mid) var(--ease);
}
.zone-dashboard.dark .zone-card { background: rgba(18,20,42,0.97); border-color: rgba(92,107,192,0.1); }

.zone-header-bar {
  display: flex; align-items: center; justify-content: space-between;
  padding: var(--s-4) var(--s-5);
  background: linear-gradient(135deg, rgba(92,107,192,0.08) 0%, rgba(124,77,255,0.05) 100%);
  border-bottom: 1px solid rgba(92,107,192,0.1);
}
.zone-name { font-size: var(--fs-md); font-weight: 800; color: var(--brand); }
.zone-count-badge {
  background: var(--brand-light); color: var(--brand);
  padding: 3px var(--s-2); border-radius: var(--r-full);
  font-size: var(--fs-xs); font-weight: 700;
  border: 1px solid rgba(92,107,192,0.2);
}

/* === Machine Card === */
.machine-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: var(--s-4); padding: var(--s-5); }

.machine-card {
  background: var(--surface-2); border-radius: var(--r-lg);
  border: 1px solid var(--border-light);
  overflow: hidden;
  transition: all var(--t-mid) var(--ease);
}
.machine-card:hover { border-color: var(--brand); box-shadow: var(--shadow-md); transform: translateY(-2px); }
.zone-dashboard.dark .machine-card { background: rgba(15,17,32,0.8); border-color: rgba(92,107,192,0.08); }

.machine-header {
  padding: var(--s-3) var(--s-4);
  background: linear-gradient(135deg, var(--brand) 0%, var(--brand-dark) 100%);
  display: flex; align-items: center; justify-content: space-between;
}
.machine-no { font-size: var(--fs-lg); font-weight: 800; color: #fff; font-family: var(--font-mono); }
.machine-lot-count { background: rgba(255,255,255,0.2); color: #fff; padding: 2px var(--s-2); border-radius: var(--r-full); font-size: var(--fs-xs); font-weight: 700; }

/* === LOT Items === */
.lot-list { padding: var(--s-2); }
.lot-item {
  display: flex; align-items: center; justify-content: space-between;
  padding: var(--s-2) var(--s-3); border-radius: var(--r-md);
  margin-bottom: 3px; font-size: var(--fs-xs);
  transition: background var(--t-fast) var(--ease);
}
.lot-item:hover { background: var(--brand-xlight); }
.lot-item:last-child { margin-bottom: 0; }

.lot-id { font-family: var(--font-mono); font-weight: 600; color: var(--text-1); font-size: var(--fs-xs); }
.lot-status-ok   { color: var(--success); font-weight: 700; font-size: var(--fs-xs); }
.lot-status-ng   { color: var(--error);   font-weight: 700; font-size: var(--fs-xs); }
.lot-status-hold { color: var(--warning); font-weight: 700; font-size: var(--fs-xs); }
.lot-qty { font-size: var(--fs-xs); color: var(--text-3); font-weight: 600; }

/* === Buttons === */
.btn-refresh-z {
  padding: var(--s-2) var(--s-4);
  background: linear-gradient(135deg, var(--brand) 0%, var(--brand-dark) 100%);
  color: #fff; border: none; border-radius: var(--r-md);
  font-size: var(--fs-sm); font-weight: 600; cursor: pointer; font-family: var(--font);
  transition: all var(--t-fast) var(--ease);
  display: inline-flex; align-items: center; gap: var(--s-2);
  box-shadow: 0 2px 10px rgba(92,107,192,0.3);
}
.btn-refresh-z:hover { filter: brightness(0.92); transform: translateY(-1px); }

.btn-back-z {
  padding: var(--s-2) var(--s-4);
  background: transparent; border: 1.5px solid rgba(92,107,192,0.3);
  border-radius: var(--r-md); color: var(--brand);
  font-size: var(--fs-sm); font-weight: 600; cursor: pointer; font-family: var(--font);
  display: inline-flex; align-items: center; gap: var(--s-2);
  transition: all var(--t-fast) var(--ease);
}
.btn-back-z:hover { background: var(--brand-light); border-color: var(--brand); }

.btn-toggle-summary {
  padding: var(--s-2) var(--s-4);
  background: var(--surface-2); border: 1.5px solid var(--border);
  border-radius: var(--r-md); color: var(--text-1);
  font-size: var(--fs-sm); font-weight: 600; cursor: pointer; font-family: var(--font);
  transition: all var(--t-fast) var(--ease);
}
.btn-toggle-summary:hover { background: var(--brand-light); border-color: var(--brand); color: var(--brand); }

/* === Empty State === */
.no-data-zone {
  text-align: center; padding: var(--s-12) 0; color: var(--text-3);
}

/* === Animations === */
@keyframes fadeInUp { from { opacity: 0; transform: translateY(12px); } to { opacity: 1; transform: translateY(0); } }

.zone-card { animation: fadeInUp 0.35s var(--ease) both; }

/* === Responsive === */
@media (max-width: 768px) {
  .machine-grid { grid-template-columns: 1fr 1fr; gap: var(--s-3); }
  .summary-bar { padding: var(--s-3) var(--s-4); }
}
@media (max-width: 480px) {
  .machine-grid { grid-template-columns: 1fr; }
}
</style>