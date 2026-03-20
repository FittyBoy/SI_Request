<template>
    <div class="rescreen-section" :class="{ dark: isDarkMode }">
        <!-- Header -->
        <div class="header">
            <div class="header-content">
                <div class="title-section">
                    <div class="icon-wrapper">
                        <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none"
                            stroke="currentColor" stroke-width="2">
                            <path d="M9 11l3 3L22 4" />
                            <path d="M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11" />
                        </svg>
                    </div>
                    <div>
                        <h1>Rescreen LOT Check</h1>
                    </div>
                </div>
                <div class="header-right">
                    <!-- Dark Mode Toggle -->
                    <button class="btn-dark-toggle" @click="isDarkMode = !isDarkMode" :title="isDarkMode ? 'Light Mode' : 'Dark Mode'">
                        <svg v-if="isDarkMode" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <circle cx="12" cy="12" r="5"/>
                            <line x1="12" y1="1" x2="12" y2="3"/><line x1="12" y1="21" x2="12" y2="23"/>
                            <line x1="4.22" y1="4.22" x2="5.64" y2="5.64"/><line x1="18.36" y1="18.36" x2="19.78" y2="19.78"/>
                            <line x1="1" y1="12" x2="3" y2="12"/><line x1="21" y1="12" x2="23" y2="12"/>
                            <line x1="4.22" y1="19.78" x2="5.64" y2="18.36"/><line x1="18.36" y1="5.64" x2="19.78" y2="4.22"/>
                        </svg>
                        <svg v-else xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/>
                        </svg>
                    </button>
                    <button class="btn-back" @click="$emit('go-back')">
                        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none"
                            stroke="currentColor" stroke-width="2">
                            <polyline points="15 18 9 12 15 6" />
                        </svg>
                        กลับหน้าหลัก
                    </button>
                </div>
            </div>
        </div>

        <div class="content">
            <!-- Connection Error Alert -->
            <div v-if="connectionError" class="alert error connection-alert">
                <div class="alert-icon">
                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <circle cx="12" cy="12" r="10" /><line x1="12" y1="8" x2="12" y2="12" /><line x1="12" y1="16" x2="12.01" y2="16" />
                    </svg>
                </div>
                <div class="alert-content">
                    <div class="alert-title">⚠️ ไม่สามารถเชื่อมต่อกับ API ได้</div>
                    <div class="alert-message">
                        กรุณาตรวจสอบ:<br>
                        • API Server กำลังทำงานอยู่หรือไม่<br>
                        • URL: <code>{{ apiBaseUrl }}</code><br>
                        • Network connection
                    </div>
                </div>
                <button class="alert-close" @click="connectionError = false">
                    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" />
                    </svg>
                </button>
            </div>

            <!-- Add Rescreen LOT Section -->
            <div class="add-section">
                <div class="section-header">
                    <h2>
                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <path d="M3 7V5a2 2 0 0 1 2-2h2"/><path d="M17 3h2a2 2 0 0 1 2 2v2"/>
                            <path d="M21 17v2a2 2 0 0 1-2 2h-2"/><path d="M7 21H5a2 2 0 0 1-2-2v-2"/>
                            <path d="M7 12h10"/>
                        </svg>
                        Scan Imobile LOT
                    </h2>
                </div>
                <div class="input-section">
                    <div class="scanner-wrapper">
                        <div class="input-group-large">
                            <label for="imobile-lot">Imobile LOT Number</label>
                            <input id="imobile-lot" ref="imobileLotInput" v-model="imobileLot" type="text"
                                placeholder="สแกน หรือ พิมพ์ Imobile LOT"
                                @keyup.enter="addRescreenLot" :disabled="isLoading" />
                        </div>
                        <button class="btn-add" @click="addRescreenLot" :disabled="isLoading || !imobileLot">
                            <svg v-if="!isLoading" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"/>
                                <polyline points="17 21 17 13 7 13 7 21"/><polyline points="7 3 7 8 15 8"/>
                            </svg>
                            <div v-else class="spinner"></div>
                            {{ isLoading ? 'กำลังบันทึก...' : 'เพิ่ม LOT' }}
                        </button>
                    </div>
                    <div v-if="alertMessage" :class="['alert', alertType]">
                        <div class="alert-icon">
                            <svg v-if="alertType === 'success'" xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/>
                            </svg>
                            <svg v-else xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>
                            </svg>
                        </div>
                        <div class="alert-content">
                            <div class="alert-message" v-html="formatMessage(alertMessage)"></div>
                        </div>
                        <button class="alert-close" @click="alertMessage = ''">
                            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                            </svg>
                        </button>
                    </div>
                </div>
            </div>

            <!-- Rescreen LOT List -->
            <div class="list-section">
                <div class="section-header">
                    <h2>
                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
                            <polyline points="14 2 14 8 20 8"/>
                            <line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/>
                        </svg>
                        รายการ Rescreen LOT (วันที่ผลิต)
                    </h2>
                    <div class="header-actions">
                        <div class="filter-group">
                            <label>วันที่ผลิต:</label>
                            <input type="date" v-model="filterDate" @change="loadRescreenLots" />
                        </div>
                        <button class="btn-check-status" @click="refreshAndReload" :disabled="isRefreshingStatus || isLoadingList">
                            <div v-if="isRefreshingStatus" class="spinner spinner-white-sm"></div>
                            <svg v-else xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <path d="M9 11l3 3L22 4"/><path d="M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11"/>
                            </svg>
                            {{ isRefreshingStatus ? 'กำลังเช็ค...' : 'เช็ค Status' }}
                        </button>
                        <button class="btn-refresh" @click="loadRescreenLots" :disabled="isLoadingList">
                            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" :class="{ 'spin-once': isLoadingList }">
                                <polyline points="23 4 23 10 17 10"/><polyline points="1 20 1 14 7 14"/>
                                <path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15"/>
                            </svg>
                            รีเฟรช
                        </button>
                        <div class="auto-refresh-badge">
                            <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>
                            </svg>
                            Auto {{ nextRefreshIn }}s
                        </div>
                    </div>
                </div>

                <!-- ═══════════════════════════════════════════════════
                     FILTER BAR — Search LOT + Select MC
                     ═══════════════════════════════════════════════════ -->
                <div class="filter-bar" v-if="rescreenLots.length > 0 || searchLot || filterMC">
                    <!-- Search LOT -->
                    <div class="filter-bar-item search-item">
                        <div class="filter-bar-icon">
                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
                            </svg>
                        </div>
                        <input
                            v-model="searchLot"
                            type="text"
                            class="filter-bar-input"
                            placeholder="ค้นหา PO LOT / Imobile LOT..."
                            @input="resetHighlight"
                        />
                        <button v-if="searchLot" class="filter-clear-btn" @click="searchLot = ''" title="ล้างการค้นหา">
                            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                                <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                            </svg>
                        </button>
                    </div>

                    <!-- Select MC -->
                    <div class="filter-bar-item select-item">
                        <div class="filter-bar-icon">
                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <rect x="2" y="3" width="20" height="14" rx="2"/><line x1="8" y1="21" x2="16" y2="21"/><line x1="12" y1="17" x2="12" y2="21"/>
                            </svg>
                        </div>
                        <select v-model="filterMC" class="filter-bar-select">
                            <option value="">ทุก MC</option>
                            <option v-for="mc in availableMCs" :key="mc" :value="mc">{{ mc }}</option>
                        </select>
                        <svg class="select-chevron" xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                            <polyline points="6 9 12 15 18 9"/>
                        </svg>
                    </div>

                    <!-- Active filter chips -->
                    <div class="filter-chips" v-if="searchLot || filterMC">
                        <span v-if="searchLot" class="filter-chip">
                            <svg xmlns="http://www.w3.org/2000/svg" width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                                <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
                            </svg>
                            "{{ searchLot }}"
                            <button @click="searchLot = ''">×</button>
                        </span>
                        <span v-if="filterMC" class="filter-chip mc-chip">
                            <svg xmlns="http://www.w3.org/2000/svg" width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <rect x="2" y="3" width="20" height="14" rx="2"/><line x1="8" y1="21" x2="16" y2="21"/>
                            </svg>
                            MC: {{ filterMC }}
                            <button @click="filterMC = ''">×</button>
                        </span>
                        <button class="clear-all-btn" @click="clearAllFilters">
                            ล้างทั้งหมด
                        </button>
                    </div>

                    <!-- Result count -->
                    <div class="filter-result-count" v-if="searchLot || filterMC">
                        <span class="count-highlight">{{ filteredLots.length }}</span> / {{ rescreenLots.length }} รายการ
                    </div>
                </div>
                <!-- ═══════════════════════════════════════════════════ -->

                <!-- Summary Stats (based on filtered data) -->
                <div v-if="rescreenLots.length > 0" class="stats-grid">
                    <div class="stat-card total">
                        <div class="stat-icon">
                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2"/>
                                <rect x="8" y="2" width="8" height="4" rx="1" ry="1"/>
                            </svg>
                        </div>
                        <div class="stat-content">
                            <div class="stat-label">{{ (searchLot || filterMC) ? 'ที่กรอง' : 'ทั้งหมด' }}</div>
                            <div class="stat-value">{{ summary.totalCount }}</div>
                        </div>
                    </div>
                    <div class="stat-card approved">
                        <div class="stat-icon">
                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/>
                            </svg>
                        </div>
                        <div class="stat-content">
                            <div class="stat-label">Approved (OK)</div>
                            <div class="stat-value">{{ summary.approvedCount }}</div>
                        </div>
                    </div>
                    <div class="stat-card pending">
                        <div class="stat-icon">
                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>
                            </svg>
                        </div>
                        <div class="stat-content">
                            <div class="stat-label">Pending</div>
                            <div class="stat-value">{{ summary.pendingCount }}</div>
                        </div>
                    </div>
                </div>

                <!-- Loading State -->
                <div v-if="isLoadingList" class="loading-state">
                    <div class="spinner spinner-amber"></div>
                    <p>กำลังโหลดข้อมูล...</p>
                </div>

                <!-- Empty State -->
                <div v-else-if="rescreenLots.length === 0" class="empty-state">
                    <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>
                    </svg>
                    <h3>ไม่พบข้อมูล</h3>
                    <p>ยังไม่มี Rescreen LOT ในวันที่เลือก</p>
                </div>

                <!-- No filter results -->
                <div v-else-if="filteredLots.length === 0" class="empty-state">
                    <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
                        <line x1="8" y1="11" x2="14" y2="11"/>
                    </svg>
                    <h3>ไม่พบรายการที่ตรงกับการค้นหา</h3>
                    <p>ลองเปลี่ยนคีย์เวิร์ด หรือ <button class="link-btn" @click="clearAllFilters">ล้าง filter</button></p>
                </div>

                <!-- LOT List Table -->
                <div v-else class="table-wrapper">
                    <table class="lot-table">
                        <thead>
                            <tr>
                                <th>PO LOT</th>
                                <th>Imobile LOT</th>
                                <th>MC</th>
                                <th>TH100 Status</th>
                                <th>Final Status</th>
                                <th>Approved Status</th>
                                <th>วันที่เพิ่ม</th>
                                <th>จัดการ</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="lot in filteredLots" :key="lot.id" :class="{ 'row-highlight': isSearchMatch(lot) }">
                                <td>
                                    <span class="lot-badge" v-html="highlightText(lot.poLot, searchLot)"></span>
                                </td>
                                <td>
                                    <span class="imobile-badge" v-html="highlightText(lot.imobileLot, searchLot)"></span>
                                </td>
                                <td>
                                    <span class="mc-badge" v-if="lot.mc">{{ lot.mc }}</span>
                                    <span v-else class="text-muted">-</span>
                                </td>
                                <td>
                                    <span :class="['status-badge-small', getStatusClass(lot.th100Status)]">
                                        {{ lot.th100Status || '-' }}
                                    </span>
                                </td>
                                <td>
                                    <span :class="['status-badge', getStatusClass(lot.finalStatus)]">
                                        {{ lot.finalStatus }}
                                    </span>
                                </td>
                                <td>
                                    <span :class="['approval-badge', getApprovalClass(lot)]">
                                        {{ getApprovalLabel(lot) }}
                                    </span>
                                </td>
                                <td class="date-cell">{{ formatDate(lot.checkDate) }}</td>
                                <td>
                                    <button class="btn-action delete" @click="deleteLot(lot)" title="ลบ">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                            <polyline points="3 6 5 6 21 6"/>
                                            <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/>
                                        </svg>
                                    </button>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick, computed } from 'vue'

const emit = defineEmits(['go-back'])
const config = useRuntimeConfig()
const apiBaseUrl = computed(() => config.public.apiBase || 'http://localhost:24678')

// Guard against updating state after unmount
const isMounted = ref(true)

const imobileLot = ref('')
const alertMessage = ref('')
const alertType = ref<'success' | 'error'>('success')
const filterDate = ref('')
const imobileLotInput = ref<HTMLInputElement | null>(null)
const connectionError = ref(false)
const isLoading = ref(false)
const isLoadingList = ref(false)
const isDeleting = ref(false)
const isDarkMode = ref(false)
const rescreenLots = ref<any[]>([])

// ── Filter: Search LOT + MC ───────────────────────────────────
const searchLot = ref('')
const filterMC = ref('')

/** รวบรวม MC ที่ไม่ซ้ำกันจากข้อมูลทั้งหมด */
const availableMCs = computed<string[]>(() => {
    const mcs = rescreenLots.value
        .map((r: any) => r.mc)
        .filter(Boolean)
    return [...new Set(mcs)].sort()
})

/** ข้อมูลที่ผ่านทั้ง search และ MC filter */
const filteredLots = computed(() => {
    let data = rescreenLots.value

    if (filterMC.value) {
        data = data.filter((r: any) => r.mc === filterMC.value)
    }

    if (searchLot.value.trim()) {
        const q = searchLot.value.trim().toLowerCase()
        data = data.filter((r: any) =>
            (r.poLot || '').toLowerCase().includes(q) ||
            (r.imobileLot || '').toLowerCase().includes(q)
        )
    }

    return data
})

const isSearchMatch = (lot: any) => searchLot.value.trim().length > 0

const resetHighlight = () => { /* reactive—no-op; just triggers watcher */ }

const clearAllFilters = () => {
    searchLot.value = ''
    filterMC.value = ''
}

/** Highlight matched text in badges */
const highlightText = (text: string, query: string): string => {
    if (!query || !text) return text || ''
    const escaped = query.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
    return text.replace(new RegExp(`(${escaped})`, 'gi'), '<mark class="search-highlight">$1</mark>')
}
// ──────────────────────────────────────────────────────────────

// ── Auto-refresh ──────────────────────────────────────────────
const REFRESH_EVERY = 30
const nextRefreshIn = ref(REFRESH_EVERY)
let refreshTimer: ReturnType<typeof setInterval> | null = null
let countdownTimer: ReturnType<typeof setInterval> | null = null

const startAutoRefresh = () => {
    stopAutoRefresh()
    nextRefreshIn.value = REFRESH_EVERY

    countdownTimer = setInterval(() => {
        nextRefreshIn.value = Math.max(0, nextRefreshIn.value - 1)
    }, 1000)

    refreshTimer = setInterval(async () => {
        try {
            await $fetch('/api/RescreenCheck/refresh-rescreen-status', {
                baseURL: apiBaseUrl.value,
                method: 'POST',
                params: { date: filterDate.value }
            })
        } catch (_) { /* silent fail */ }
        await loadRescreenLots()
        nextRefreshIn.value = REFRESH_EVERY
    }, REFRESH_EVERY * 1000)
}

const stopAutoRefresh = () => {
    if (refreshTimer)   { clearInterval(refreshTimer);   refreshTimer = null }
    if (countdownTimer) { clearInterval(countdownTimer); countdownTimer = null }
}

const isRefreshingStatus = ref(false)
const refreshAndReload = async () => {
    isRefreshingStatus.value = true
    try {
        await $fetch('/api/RescreenCheck/refresh-rescreen-status', {
            baseURL: apiBaseUrl.value,
            method: 'POST',
            params: { date: filterDate.value }
        })
    } catch { /* silent fail */ } finally {
        isRefreshingStatus.value = false
    }
    await loadRescreenLots()
    nextRefreshIn.value = REFRESH_EVERY
}
// ─────────────────────────────────────────────────────────────

/** Summary คำนวณจาก filteredLots เพื่อให้ตรงกับที่แสดง */
const summary = computed(() => {
    const data = (searchLot.value || filterMC.value) ? filteredLots.value : rescreenLots.value
    return {
        totalCount: data.length,
        approvedCount: data.filter((r: any) => r.isApproved).length,
        pendingCount: data.filter((r: any) => !r.isApproved).length
    }
})

const getApprovalLabel = (lot: any): string => {
    const source: string = lot.approvedSource || 'Pending'
    if (lot.isApproved) {
        if (source === 'TH100 Confirm') return '✓ TH100 Confirm'
        return '✓ Approved'
    }
    if (source === 'TH100 Confirm') return '⏸ TH100 Confirm'
    return '⏸ Pending'
}

const getApprovalClass = (lot: any): string => {
    const source: string = lot.approvedSource || 'Pending'
    if (lot.isApproved) {
        if (source === 'TH100 Confirm') return 'th100-confirmed'
        return 'approved'
    }
    if (source === 'TH100 Confirm') return 'th100-pending'
    return 'pending'
}

const loadRescreenLots = async () => {
    if (!filterDate.value || !isMounted.value) return
    isLoadingList.value = true
    nextRefreshIn.value = REFRESH_EVERY
    connectionError.value = false
    try {
        const response: any = await $fetch('/api/RescreenCheck/get-rescreen-records', {
            baseURL: apiBaseUrl.value, method: 'GET', params: { date: filterDate.value }
        })
        if (!isMounted.value) return
        rescreenLots.value = response?.success ? (response.data || []) : []
    } catch (error: any) {
        if (!isMounted.value) return
        if (error.message?.includes('fetch failed') || error.cause?.code === 'ECONNREFUSED') {
            connectionError.value = true
            showAlert('ไม่สามารถเชื่อมต่อกับ API Server ได้', 'error')
        } else {
            showAlert('เกิดข้อผิดพลาดในการโหลดข้อมูล', 'error')
        }
        rescreenLots.value = []
    } finally {
        if (isMounted.value) isLoadingList.value = false
    }
}

const addRescreenLot = async () => {
    if (!imobileLot.value.trim()) { showAlert('กรุณาระบุ Imobile LOT', 'error'); return }
    alertMessage.value = ''
    isLoading.value = true
    connectionError.value = false
    try {
        const response: any = await $fetch('/api/RescreenCheck/quick-add-rescreen', {
            baseURL: apiBaseUrl.value, method: 'POST',
            body: { imobileLot: imobileLot.value, checkedBy: 'System' }
        })
        if (response?.success) {
            showAlert(response.message, 'success')
            imobileLot.value = ''
            await loadRescreenLots()
            await nextTick()
            imobileLotInput.value?.focus()
        } else {
            showAlert(response?.message || 'เกิดข้อผิดพลาด', 'error')
        }
    } catch (error: any) {
        if (error.message?.includes('fetch failed') || error.cause?.code === 'ECONNREFUSED') {
            connectionError.value = true
            showAlert('ไม่สามารถเชื่อมต่อกับ API Server ได้', 'error')
        } else {
            showAlert(error?.data?.message || error?.message || 'เกิดข้อผิดพลาด', 'error')
        }
    } finally {
        isLoading.value = false
    }
}

const deleteLot = async (lot: any) => {
    if (!confirm(`ต้องการลบ LOT: ${lot.poLot}\nImobile LOT: ${lot.imobileLot} ?`)) return
    isDeleting.value = true
    try {
        const response: any = await $fetch(`/api/RescreenCheck/delete-rescreen-record/${lot.id}`, {
            baseURL: apiBaseUrl.value, method: 'DELETE'
        })
        if (response?.success) { showAlert(response.message, 'success'); await loadRescreenLots() }
        else showAlert(response?.message || 'เกิดข้อผิดพลาด', 'error')
    } catch (error: any) {
        showAlert(error?.data?.message || 'เกิดข้อผิดพลาดในการลบ', 'error')
    } finally {
        isDeleting.value = false
    }
}

const showAlert = (message: string, type: 'success' | 'error') => {
    alertMessage.value = message
    alertType.value = type
    setTimeout(() => { alertMessage.value = '' }, 5000)
}

const formatMessage = (message: string) => message.replace(/\n/g, '<br>')

const formatDate = (dateString: string) => {
    if (!dateString) return '-'
    return new Date(dateString).toLocaleDateString('th-TH', {
        year: 'numeric', month: 'short', day: 'numeric',
        hour: '2-digit', minute: '2-digit', timeZone: 'Asia/Bangkok'
    })
}

const getStatusClass = (status: string) => {
    if (!status) return 'default'
    const lower = status.toLowerCase()
    if (lower === 'ok') return 'ok'
    if (lower === 'hold') return 'hold'
    if (lower === 'scrap') return 'scrap'
    if (lower === 'rescreen') return 'rescreen'
    return 'default'
}

onMounted(async () => {
    if (window.matchMedia?.('(prefers-color-scheme: dark)').matches) isDarkMode.value = true
    const today = new Date()
    filterDate.value = today.toISOString().split('T')[0]
    await loadRescreenLots()
    await nextTick()
    imobileLotInput.value?.focus()
    startAutoRefresh()
})

onUnmounted(() => {
    isMounted.value = false
    stopAutoRefresh()
})
</script>

<style scoped>
/* === Page === */
.rescreen-section {
  min-height: 100vh;
  background: linear-gradient(160deg, #fffbeb 0%, #fef3c7 40%, #fff7ed 100%);
  transition: background var(--t-slow) var(--ease);
}

.rescreen-section.dark {
  background: linear-gradient(160deg, #1a1200 0%, #2d1f00 40%, #1f1000 100%);
}

/* === Header === */
.rescreen-header {
  background: rgba(255,255,255,0.95);
  backdrop-filter: blur(12px);
  border-bottom: 1px solid rgba(251,191,36,0.3);
  padding: var(--s-4) var(--s-6);
  display: flex; align-items: center; justify-content: space-between;
  position: sticky; top: 0; z-index: 50;
  box-shadow: 0 2px 12px rgba(251,191,36,0.15);
}
.rescreen-section.dark .rescreen-header {
  background: rgba(26,18,0,0.97);
  border-bottom-color: rgba(251,191,36,0.15);
}

.header-title {
  font-size: var(--fs-xl); font-weight: 800; letter-spacing: -0.03em;
  background: linear-gradient(135deg, #d97706 0%, #f59e0b 100%);
  -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text;
}

/* === Scan Card === */
.scan-card {
  background: rgba(255,255,255,0.97);
  border-radius: var(--r-xl);
  border: 1.5px solid rgba(251,191,36,0.4);
  box-shadow: 0 4px 24px rgba(251,191,36,0.15), var(--shadow-sm);
  padding: var(--s-6);
  margin-bottom: var(--s-6);
}
.rescreen-section.dark .scan-card {
  background: rgba(30,22,0,0.95);
  border-color: rgba(251,191,36,0.2);
}

.scan-label { font-size: var(--fs-sm); font-weight: 700; color: var(--text-2); text-transform: uppercase; letter-spacing: 0.06em; margin-bottom: var(--s-2); }

.scan-input {
  font-size: var(--fs-md) !important; padding: var(--s-4) var(--s-5) !important;
  border: 2px solid rgba(251,191,36,0.5) !important;
  border-radius: var(--r-lg) !important;
  background: var(--surface) !important;
  color: var(--text-1) !important;
  font-family: var(--font-mono) !important;
  width: 100%; outline: none;
  transition: border-color var(--t-fast) var(--ease), box-shadow var(--t-fast) var(--ease);
}
.scan-input:focus {
  border-color: #f59e0b !important;
  box-shadow: 0 0 0 4px rgba(251,191,36,0.2) !important;
}

.btn-add-lot {
  padding: var(--s-4) var(--s-6);
  background: linear-gradient(135deg, #d97706 0%, #f59e0b 100%);
  color: #fff; border: none; border-radius: var(--r-lg);
  font-size: var(--fs-sm); font-weight: 700; font-family: var(--font);
  cursor: pointer; white-space: nowrap; flex-shrink: 0;
  box-shadow: 0 4px 14px rgba(217,119,6,0.4);
  transition: all var(--t-fast) var(--ease);
  display: flex; align-items: center; gap: var(--s-2);
}
.btn-add-lot:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 6px 20px rgba(217,119,6,0.5); }
.btn-add-lot:disabled { opacity: 0.45; cursor: not-allowed; }

/* === Filter Bar === */
.filter-bar {
  background: rgba(255,251,235,0.9);
  border-radius: var(--r-lg);
  border: 1px solid rgba(251,191,36,0.35);
  padding: var(--s-4) var(--s-5);
  display: flex; align-items: center; gap: var(--s-3); flex-wrap: wrap;
  margin-bottom: var(--s-4);
}
.rescreen-section.dark .filter-bar { background: rgba(30,22,0,0.8); border-color: rgba(251,191,36,0.15); }

/* === Stat Cards === */
.stat-card-rescreen {
  background: rgba(255,255,255,0.97);
  border-radius: var(--r-lg);
  border: 1.5px solid rgba(251,191,36,0.2);
  padding: var(--s-5);
  text-align: center;
  box-shadow: var(--shadow-sm);
  transition: transform var(--t-mid) var(--ease), box-shadow var(--t-mid) var(--ease);
}
.stat-card-rescreen:hover { transform: translateY(-3px); box-shadow: var(--shadow-md); }
.rescreen-section.dark .stat-card-rescreen { background: rgba(30,22,0,0.95); }

.stat-number { font-size: var(--fs-3xl); font-weight: 800; letter-spacing: -0.04em; color: var(--text-1); }
.stat-number-ok { color: var(--success); }
.stat-number-pending { color: #d97706; }
.stat-label-r { font-size: var(--fs-xs); color: var(--text-3); font-weight: 600; text-transform: uppercase; letter-spacing: 0.06em; margin-top: var(--s-1); }

/* === Table === */
.rescreen-table-wrapper {
  background: rgba(255,255,255,0.97);
  border-radius: var(--r-xl);
  border: 1px solid rgba(251,191,36,0.25);
  overflow: hidden; box-shadow: var(--shadow-md);
}
.rescreen-section.dark .rescreen-table-wrapper { background: rgba(26,18,0,0.95); }

.rescreen-table { width: 100%; border-collapse: collapse; font-size: var(--fs-sm); }
.rescreen-table th {
  background: rgba(251,191,36,0.12);
  padding: var(--s-3) var(--s-4);
  text-align: left; font-weight: 700; font-size: var(--fs-xs);
  text-transform: uppercase; letter-spacing: 0.06em;
  color: #92400e; border-bottom: 2px solid rgba(251,191,36,0.25);
}
.rescreen-table td { padding: 14px var(--s-4); border-bottom: 1px solid rgba(251,191,36,0.1); color: var(--text-1); vertical-align: middle; }
.rescreen-table tbody tr:hover td { background: rgba(251,191,36,0.06); }

/* === Badges === */
.badge-polot {
  background: var(--brand-light); color: var(--brand);
  padding: 3px var(--s-2); border-radius: var(--r-full);
  font-size: var(--fs-xs); font-weight: 700; font-family: var(--font-mono);
}
.badge-imobile {
  background: rgba(251,191,36,0.15); color: #92400e;
  padding: 3px var(--s-2); border-radius: var(--r-full);
  font-size: var(--fs-xs); font-weight: 700; font-family: var(--font-mono);
}
.badge-mc {
  background: var(--info-bg); color: var(--info);
  padding: 3px var(--s-2); border-radius: var(--r-full);
  font-size: var(--fs-xs); font-weight: 700;
}
.badge-ok       { background: var(--success-bg); color: var(--success); padding: 3px var(--s-2); border-radius: var(--r-full); font-size: var(--fs-xs); font-weight: 700; }
.badge-hold     { background: var(--error-bg);   color: var(--error);   padding: 3px var(--s-2); border-radius: var(--r-full); font-size: var(--fs-xs); font-weight: 700; }
.badge-approved { background: var(--success-bg); color: var(--success); padding: 3px var(--s-2); border-radius: var(--r-full); font-size: var(--fs-xs); font-weight: 600; display: inline-flex; align-items: center; gap: 3px; }
.badge-pending  { background: rgba(251,191,36,0.2); color: #92400e; padding: 3px var(--s-2); border-radius: var(--r-full); font-size: var(--fs-xs); font-weight: 700; }

/* === Buttons === */
.btn-refresh {
  padding: var(--s-2) var(--s-4); background: transparent;
  border: 1.5px solid rgba(251,191,36,0.5); border-radius: var(--r-md);
  color: #92400e; font-size: var(--fs-sm); font-weight: 600; cursor: pointer;
  font-family: var(--font); transition: all var(--t-fast) var(--ease);
  display: inline-flex; align-items: center; gap: var(--s-2);
}
.btn-refresh:hover { background: rgba(251,191,36,0.15); border-color: #f59e0b; }

.btn-check-status {
  padding: var(--s-2) var(--s-4);
  background: linear-gradient(135deg, var(--success) 0%, #388e3c 100%);
  color: #fff; border: none; border-radius: var(--r-md);
  font-size: var(--fs-sm); font-weight: 600; cursor: pointer; font-family: var(--font);
  display: inline-flex; align-items: center; gap: var(--s-2);
  transition: all var(--t-fast) var(--ease);
}
.btn-check-status:hover { filter: brightness(0.92); transform: translateY(-1px); }

.btn-back {
  padding: var(--s-2) var(--s-4);
  background: transparent;
  border: 1.5px solid rgba(251,191,36,0.5);
  border-radius: var(--r-md); color: #92400e;
  font-size: var(--fs-sm); font-weight: 600; cursor: pointer; font-family: var(--font);
  display: inline-flex; align-items: center; gap: var(--s-2);
  transition: all var(--t-fast) var(--ease);
}
.btn-back:hover { background: rgba(251,191,36,0.1); }

/* === Alert === */
.alert-message {
  padding: var(--s-3) var(--s-4); border-radius: var(--r-md); font-size: var(--fs-sm); font-weight: 600;
  display: flex; align-items: center; gap: var(--s-2); margin-bottom: var(--s-4);
  animation: slideIn 0.25s var(--ease);
}
.alert-success-r { background: var(--success-bg); color: var(--success); border: 1px solid var(--success-border); }
.alert-error-r   { background: var(--error-bg);   color: var(--error);   border: 1px solid var(--error-border); }

/* === Dark Toggle === */
.dark-toggle {
  width: 40px; height: 22px;
  background: rgba(251,191,36,0.3);
  border-radius: var(--r-full); cursor: pointer; position: relative;
  border: 1px solid rgba(251,191,36,0.4); transition: background var(--t-fast) var(--ease);
}
.dark-toggle.active { background: #f59e0b; }
.dark-toggle-knob {
  position: absolute; top: 2px; left: 2px;
  width: 16px; height: 16px; background: #fff;
  border-radius: 50%; transition: transform var(--t-fast) var(--ease);
  box-shadow: 0 1px 3px rgba(0,0,0,0.2);
}
.dark-toggle.active .dark-toggle-knob { transform: translateX(18px); }

/* === Animations === */
@keyframes slideIn { from { opacity: 0; transform: translateY(-6px); } to { opacity: 1; transform: translateY(0); } }
@keyframes fadeInUp { from { opacity: 0; transform: translateY(12px); } to { opacity: 1; transform: translateY(0); } }

/* === Responsive === */
@media (max-width: 768px) {
  .rescreen-header { padding: var(--s-3) var(--s-4); }
  .scan-card { padding: var(--s-4); }
  .filter-bar { gap: var(--s-2); }
}
</style>