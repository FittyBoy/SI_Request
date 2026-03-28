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
/* ===== CSS Variables สำหรับ Light / Dark ===== */
.zone-dashboard {
  --bg-main: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  --bg-header: rgba(255, 255, 255, 0.95);
  --bg-card: white;
  --bg-zone: white;
  --bg-machine: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
  --bg-lot: white;
  --text-primary: #1e293b;
  --text-secondary: #64748b;
  --text-machine: #1e293b;
  --border-default: #e2e8f0;
  --border-zone: #f1f5f9;
  --header-shadow: 0 4px 20px rgba(0,0,0,0.1);
  --card-shadow: 0 8px 32px rgba(0,0,0,0.08);
  --zone-shadow: 0 4px 20px rgba(0,0,0,0.08);
  --input-border: #e2e8f0;
  --lot-count-bg: white;
  --no-lots-color: #94a3b8;
  min-height: 100vh;
  background: var(--bg-main);
  transition: all 0.3s ease;
}

/* ===== Dark Mode Variables ===== */
.zone-dashboard.dark {
  --bg-main: linear-gradient(135deg, #1e1b4b 0%, #312e81 100%);
  --bg-header: rgba(15, 23, 42, 0.97);
  --bg-card: #1e293b;
  --bg-zone: #1e293b;
  --bg-machine: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
  --bg-lot: #0f172a;
  --text-primary: #f1f5f9;
  --text-secondary: #94a3b8;
  --text-machine: #e2e8f0;
  --border-default: #334155;
  --border-zone: #334155;
  --header-shadow: 0 4px 20px rgba(0,0,0,0.4);
  --card-shadow: 0 8px 32px rgba(0,0,0,0.3);
  --zone-shadow: 0 4px 20px rgba(0,0,0,0.3);
  --input-border: #475569;
  --lot-count-bg: #0f172a;
  --no-lots-color: #475569;
}

/* ===== Header ===== */
.header {
  background: var(--bg-header);
  backdrop-filter: blur(10px);
  box-shadow: var(--header-shadow);
  position: sticky;
  top: 0;
  z-index: 100;
  transition: background 0.3s;
}

.header-content {
  max-width: 100%;
  margin: 0 auto;
  padding: 20px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.title-section {
  display: flex;
  align-items: center;
  gap: 16px;
}

.icon-wrapper {
  width: 48px;
  height: 48px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.title-section h1 {
  font-size: 24px;
  font-weight: 700;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  margin: 0;
}

.dark .title-section h1 {
  background: linear-gradient(135deg, #a5b4fc 0%, #c4b5fd 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.subtitle {
  font-size: 13px;
  color: var(--text-secondary);
  margin: 2px 0 0 0;
}

.header-actions {
  display: flex;
  gap: 10px;
  align-items: center;
}

/* ===== Buttons ===== */
.btn-back {
  background: white;
  color: #667eea;
  border: 2px solid #667eea;
  padding: 10px 20px;
  border-radius: 10px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.3s;
}

.dark .btn-back {
  background: #1e293b;
  color: #a5b4fc;
  border-color: #a5b4fc;
}

.btn-back:hover {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  transform: translateY(-2px);
}

.btn-summary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 10px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.3s;
}

.btn-summary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(102, 126, 234, 0.4);
}

.btn-dark {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  border: 2px solid var(--border-default);
  background: var(--bg-card);
  color: var(--text-primary);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s;
}

.btn-dark:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
}

/* ===== Content ===== */
.content {
  max-width: 100%;
  margin: 0 auto;
  padding: 20px 24px;
}

/* ===== Filter Card ===== */
.card {
  background: var(--bg-card);
  border-radius: 16px;
  box-shadow: var(--card-shadow);
  overflow: hidden;
  margin-bottom: 20px;
  transition: background 0.3s;
}

.filter-card {
  padding: 16px 20px;
}

.filter-section {
  display: flex;
  gap: 16px;
  align-items: flex-end;
  flex-wrap: wrap;
}

.input-group {
  flex: 1;
  min-width: 180px;
  max-width: 300px;
}

.input-group label {
  display: block;
  margin-bottom: 6px;
  font-weight: 600;
  color: var(--text-primary);
  font-size: 13px;
}

.input-modern {
  width: 100%;
  padding: 10px 14px;
  border: 2px solid var(--input-border);
  border-radius: 8px;
  font-size: 14px;
  background: var(--bg-card);
  color: var(--text-primary);
  transition: all 0.2s;
  box-sizing: border-box;
}

.input-modern:focus {
  outline: none;
  border-color: #667eea;
}

.btn-refresh {
  display: flex;
  align-items: center;
  gap: 8px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 10px 20px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 600;
  border: none;
  cursor: pointer;
  transition: all 0.3s;
  white-space: nowrap;
}

.btn-refresh:hover:not(:disabled) { transform: translateY(-2px); }
.btn-refresh:disabled { opacity: 0.6; cursor: not-allowed; }

/* ===== Stats ===== */
.stats-overview {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
  margin-bottom: 20px;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 18px;
  background: var(--bg-card);
  border-radius: 14px;
  box-shadow: var(--card-shadow);
  transition: background 0.3s;
}

/* ✅ REP stat card */
.stat-card.rep {
  border: 2px solid #a78bfa;
}
.dark .stat-card.rep {
  border-color: #5b21b6;
}

.stat-icon {
  width: 52px;
  height: 52px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.stat-icon.total   { background: linear-gradient(135deg, #60a5fa, #3b82f6); color: white; }
.stat-icon.ok      { background: linear-gradient(135deg, #86efac, #22c55e); color: white; }
.stat-icon.ng      { background: linear-gradient(135deg, #fca5a5, #ef4444); color: white; }
.stat-icon.qty     { background: linear-gradient(135deg, #c084fc, #a855f7); color: white; }
.stat-icon.rep-icon { background: linear-gradient(135deg, #a78bfa, #7c3aed); color: white; }

.stat-content { display: flex; flex-direction: column; }
.stat-label { font-size: 12px; color: var(--text-secondary); }
.stat-value { font-size: 26px; font-weight: 700; color: var(--text-primary); }

/* ===== Zone ===== */
.zones-container { display: flex; flex-direction: column; gap: 16px; }
.zone-row { width: 100%; }

.zone-section {
  background: var(--bg-zone);
  border-radius: 16px;
  box-shadow: var(--zone-shadow);
  padding: 14px;
  transition: background 0.3s;
}

/* ✅ REP zone section — ขอบม่วง */
.zone-section-rep {
  border: 2px solid #a78bfa;
  background: linear-gradient(135deg, #faf5ff 0%, #f5f3ff 100%);
}
.dark .zone-section-rep {
  border-color: #5b21b6;
  background: linear-gradient(135deg, #1e1b4b 0%, #2e1065 100%);
}

.zone-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  padding-bottom: 10px;
  border-bottom: 2px solid var(--border-zone);
}

/* ✅ REP zone header */
.zone-header-rep {
  border-bottom-color: #ddd6fe;
}
.dark .zone-header-rep {
  border-bottom-color: #4c1d95;
}

.zone-header-left {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.zone-header h2 {
  font-size: 16px;
  font-weight: 700;
  color: var(--text-primary);
  margin: 0;
}

.zone-count {
  font-size: 12px;
  color: var(--text-secondary);
  background: var(--bg-machine);
  padding: 4px 8px;
  border-radius: 6px;
}

/* ✅ REP zone count */
.rep-count {
  background: linear-gradient(135deg, #a78bfa, #7c3aed);
  color: white;
  font-weight: 600;
}

/* ✅ REP zone badge */
.rep-zone-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  background: linear-gradient(135deg, #a78bfa, #7c3aed);
  color: white;
  padding: 3px 10px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.5px;
}

.rep-subtitle {
  font-size: 12px;
  color: #7c3aed;
  font-style: italic;
}
.dark .rep-subtitle {
  color: #c4b5fd;
}

/* ✅ REP empty state */
.rep-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 28px 0;
  color: #a78bfa;
  opacity: 0.6;
}
.rep-empty p {
  margin: 0;
  font-size: 14px;
}

/* ✅ REP groups */
.rep-groups {
  display: flex;
  flex-wrap: wrap;
  gap: 14px;
}

.rep-group {
  min-width: 220px;
  flex: 1 1 220px;
  background: rgba(139, 92, 246, 0.06);
  border: 1px solid #ddd6fe;
  border-radius: 10px;
  padding: 10px 12px;
}
.dark .rep-group {
  background: rgba(139, 92, 246, 0.1);
  border-color: #4c1d95;
}

.rep-group-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
  padding-bottom: 6px;
  border-bottom: 1px dashed #ddd6fe;
}
.dark .rep-group-header {
  border-bottom-color: #4c1d95;
}

.rep-group-name {
  font-size: 13px;
  font-weight: 700;
  color: #6d28d9;
  font-family: 'Courier New', monospace;
}
.dark .rep-group-name {
  color: #c4b5fd;
}

.rep-group-count {
  font-size: 11px;
  color: #7c3aed;
  background: #ede9fe;
  padding: 2px 6px;
  border-radius: 4px;
  font-weight: 600;
}
.dark .rep-group-count {
  background: #3b0764;
  color: #c4b5fd;
}

/* ✅ REP lots grid */
.rep-lots-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.rep-lot-item {
  background: white;
  border: 1px solid #ddd6fe;
  border-left: 3px solid #7c3aed;
  border-radius: 6px;
  padding: 6px 10px;
  min-width: 140px;
  flex: 1 1 140px;
  transition: transform 0.15s;
}
.rep-lot-item:hover {
  transform: translateY(-1px);
  box-shadow: 0 2px 8px rgba(124, 58, 237, 0.15);
}
.dark .rep-lot-item {
  background: #1e1b4b;
  border-color: #4c1d95;
}

.rep-lot-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 6px;
  margin-bottom: 4px;
}

.rep-lot-name {
  font-family: 'Courier New', monospace;
  font-size: 11px;
  font-weight: 700;
  color: var(--text-primary);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.rep-lot-bottom {
  display: flex;
  align-items: center;
  gap: 6px;
}

.rep-lot-seq {
  font-size: 11px;
  color: #7c3aed;
  font-weight: 600;
}
.dark .rep-lot-seq {
  color: #c4b5fd;
}

.rep-lot-suffix {
  font-size: 10px;
  background: #ede9fe;
  color: #5b21b6;
  padding: 1px 5px;
  border-radius: 3px;
  font-weight: 600;
}
.dark .rep-lot-suffix {
  background: #3b0764;
  color: #e9d5ff;
}

/* ===== Machine Card ===== */
.zone-machines-horizontal {
  display: flex;
  gap: 10px;
  overflow-x: auto;
  padding-bottom: 6px;
}

.zone-machines-horizontal::-webkit-scrollbar { height: 6px; }
.zone-machines-horizontal::-webkit-scrollbar-track { background: var(--border-zone); border-radius: 4px; }
.zone-machines-horizontal::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 4px; }
.dark .zone-machines-horizontal::-webkit-scrollbar-thumb { background: #475569; }

.machine-wrapper { flex: 0 0 auto; width: 260px; }

.machine-cell {
  background: var(--bg-machine);
  border: 2px solid var(--border-default);
  border-radius: 10px;
  padding: 10px;
  transition: all 0.3s;
  height: 100%;
  min-height: 120px;
}

.machine-cell:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
}

.machine-cell.all-ok  { border-color: #86efac; }
.dark .machine-cell.all-ok  { border-color: #166534; background: linear-gradient(135deg, #052e16 0%, #14532d 100%); }

.machine-cell.has-ng  { border-color: #fca5a5; }
.dark .machine-cell.has-ng  { border-color: #991b1b; background: linear-gradient(135deg, #2d0000 0%, #450a0a 100%); }

.machine-cell.no-data { border-color: var(--border-default); opacity: 0.5; }

.machine-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
  padding-bottom: 6px;
  border-bottom: 1px solid var(--border-default);
}

.machine-name { font-size: 14px; font-weight: 700; color: var(--text-machine); }

.lot-count {
  font-size: 11px;
  color: var(--text-secondary);
  background: var(--lot-count-bg);
  padding: 2px 6px;
  border-radius: 4px;
}

.machine-lots {
  display: flex;
  flex-direction: column;
  gap: 5px;
  max-height: 480px;
  overflow-y: auto;
}

.machine-lots::-webkit-scrollbar { width: 4px; }
.machine-lots::-webkit-scrollbar-track { background: transparent; }
.machine-lots::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 4px; }
.dark .machine-lots::-webkit-scrollbar-thumb { background: #475569; }

/* ===== LOT Item ===== */
.lot-item {
  display: grid;
  grid-template-columns: 1fr auto auto;
  gap: 6px;
  align-items: center;
  padding: 6px 8px;
  background: var(--bg-lot);
  border-radius: 6px;
  font-size: 12px;
  transition: all 0.2s;
  white-space: nowrap;
}

.lot-item:hover { transform: scale(1.01); }
.lot-item.ok    { border-left: 3px solid #22c55e; }
.lot-item.ng    { border-left: 3px solid #ef4444; }
.lot-item.scrap { border-left: 3px solid #f59e0b; }

.lot-name {
  font-weight: 600;
  color: var(--text-primary);
  font-family: 'Courier New', monospace;
  overflow: hidden;
  text-overflow: ellipsis;
}

.lot-badge {
  padding: 2px 7px;
  border-radius: 4px;
  font-weight: 700;
  font-size: 10px;
  white-space: nowrap;
}

.badge-ok    { background: #dcfce7; color: #166534; }
.badge-ng    { background: #fee2e2; color: #991b1b; }
.badge-scrap { background: #fef3c7; color: #92400e; }

.dark .badge-ok    { background: #14532d; color: #86efac; }
.dark .badge-ng    { background: #450a0a; color: #fca5a5; }
.dark .badge-scrap { background: #431407; color: #fdba74; }

.lot-qty { color: var(--text-secondary); font-weight: 600; white-space: nowrap; }

.no-lots {
  text-align: center;
  color: var(--no-lots-color);
  font-size: 11px;
  padding: 8px;
  font-style: italic;
}

/* ===== Loading ===== */
.loading-container {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 300px;
}

.loading-spinner { text-align: center; color: white; }

.spinner {
  width: 48px;
  height: 48px;
  margin: 0 auto 16px;
  border: 4px solid rgba(255,255,255,0.2);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

.spinner-small {
  width: 18px;
  height: 18px;
  border: 2px solid rgba(255,255,255,0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin { to { transform: rotate(360deg); } }

/* ===== Responsive ===== */
@media (max-width: 1400px) { .machine-wrapper { width: 230px; } }
@media (max-width: 1024px) { .machine-wrapper { width: 200px; } }

@media (max-width: 768px) {
  .header-content { flex-direction: column; gap: 12px; align-items: stretch; }
  .header-actions { flex-wrap: wrap; justify-content: flex-end; }
  .content { padding: 12px; }
  .machine-wrapper { width: 180px; }
  .stats-overview { grid-template-columns: 1fr 1fr; }
  .input-group { max-width: 100%; }
  .rep-lot-item { min-width: 120px; }
  .rep-group { min-width: 100%; }
}

/* ── Page-load & micro-interaction animations ── */
@keyframes fadeUp {
    from { opacity: 0; transform: translateY(14px); }
    to   { opacity: 1; transform: translateY(0); }
}
@keyframes scaleIn {
    from { opacity: 0; transform: scale(0.92); }
    to   { opacity: 1; transform: scale(1); }
}
@keyframes slideRight {
    from { opacity: 0; transform: translateX(-16px); }
    to   { opacity: 1; transform: translateX(0); }
}
@keyframes slideDown {
    from { opacity: 0; transform: translateY(-10px); }
    to   { opacity: 1; transform: translateY(0); }
}
@keyframes countUp {
    from { opacity: 0; transform: translateY(8px) scale(0.85); }
    to   { opacity: 1; transform: translateY(0) scale(1); }
}

/* Entry animations */
.header         { animation: fadeUp 0.35s cubic-bezier(0.22,1,0.36,1) both; }
.input-group    { animation: fadeUp 0.4s cubic-bezier(0.22,1,0.36,1) 0.05s both; }
.stats-overview { animation: fadeUp 0.4s cubic-bezier(0.22,1,0.36,1) 0.1s both; }
.content        { animation: fadeUp 0.4s cubic-bezier(0.22,1,0.36,1) 0.12s both; }

/* Stat cards stagger */
.stats-overview > *:nth-child(1) { animation: scaleIn 0.35s cubic-bezier(0.22,1,0.36,1) 0.1s both; }
.stats-overview > *:nth-child(2) { animation: scaleIn 0.35s cubic-bezier(0.22,1,0.36,1) 0.15s both; }
.stats-overview > *:nth-child(3) { animation: scaleIn 0.35s cubic-bezier(0.22,1,0.36,1) 0.2s both; }
.stats-overview > *:nth-child(4) { animation: scaleIn 0.35s cubic-bezier(0.22,1,0.36,1) 0.25s both; }

/* Rep group items slide in */
.rep-group     { animation: slideRight 0.3s cubic-bezier(0.22,1,0.36,1) both; }
.rep-group:nth-child(1) { animation-delay: 0.1s; }
.rep-group:nth-child(2) { animation-delay: 0.15s; }
.rep-group:nth-child(3) { animation-delay: 0.2s; }
.rep-group:nth-child(4) { animation-delay: 0.25s; }

/* Rep lot items appear */
.rep-lot-item  { animation: scaleIn 0.25s cubic-bezier(0.22,1,0.36,1) both; }

/* Machine column slide down */
.machine-wrapper { animation: slideDown 0.35s cubic-bezier(0.22,1,0.36,1) 0.08s both; }

/* Button hover lift */
.header-actions button:hover,
.header-actions a:hover {
    transform: translateY(-2px);
    transition: transform 0.18s cubic-bezier(0.22,1,0.36,1),
                box-shadow 0.18s ease !important;
}
</style>