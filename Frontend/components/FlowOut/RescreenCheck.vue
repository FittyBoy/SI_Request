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
/* ===== CSS Variables ===== */
.rescreen-section {
    --bg-main: linear-gradient(135deg, #fbbf24 0%, #f59e0b 100%);
    --bg-header: rgba(255, 255, 255, 0.95);
    --bg-card: white;
    --bg-section-header: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
    --bg-input: white;
    --bg-table-head: #f8fafc;
    --bg-table-row-hover: #f8fafc;
    --bg-stat-total: linear-gradient(135deg, #dbeafe 0%, #bfdbfe 100%);
    --bg-stat-approved: linear-gradient(135deg, #dcfce7 0%, #bbf7d0 100%);
    --bg-stat-pending: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
    --text-primary: #1e293b;
    --text-secondary: #64748b;
    --text-section-h: #92400e;
    --text-filter-label: #92400e;
    --border-default: #e2e8f0;
    --border-section: #fcd34d;
    --border-table: #f1f5f9;
    --border-input: #fcd34d;
    --header-shadow: 0 4px 20px rgba(0,0,0,0.1);
    --card-shadow: 0 8px 32px rgba(0,0,0,0.08);
    /* filter bar */
    --bg-filter-bar: #fffbeb;
    --border-filter-bar: #fde68a;
    --bg-filter-input: #ffffff;
    --bg-chip: #fef3c7;
    --text-chip: #92400e;
    --border-chip: #fcd34d;
    min-height: 100vh;
    background: var(--bg-main);
    transition: all 0.3s;
}

/* ===== Dark Mode ===== */
.rescreen-section.dark {
    --bg-main: linear-gradient(135deg, #78350f 0%, #92400e 100%);
    --bg-header: rgba(15, 23, 42, 0.97);
    --bg-card: #1e293b;
    --bg-section-header: linear-gradient(135deg, #1c1400 0%, #2d1f00 100%);
    --bg-input: #0f172a;
    --bg-table-head: #0f172a;
    --bg-table-row-hover: #0f172a;
    --bg-stat-total: linear-gradient(135deg, #1e3a5f 0%, #1e40af 100%);
    --bg-stat-approved: linear-gradient(135deg, #052e16 0%, #14532d 100%);
    --bg-stat-pending: linear-gradient(135deg, #1c1400 0%, #422006 100%);
    --text-primary: #f1f5f9;
    --text-secondary: #94a3b8;
    --text-section-h: #fde68a;
    --text-filter-label: #fde68a;
    --border-default: #334155;
    --border-section: #78350f;
    --border-table: #1e293b;
    --border-input: #78350f;
    --header-shadow: 0 4px 20px rgba(0,0,0,0.4);
    --card-shadow: 0 8px 32px rgba(0,0,0,0.3);
    /* filter bar dark */
    --bg-filter-bar: #1a1000;
    --border-filter-bar: #78350f;
    --bg-filter-input: #0f172a;
    --bg-chip: #291a00;
    --text-chip: #fde68a;
    --border-chip: #78350f;
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
    max-width: 1400px;
    margin: 0 auto;
    padding: 20px 32px;
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
    background: linear-gradient(135deg, #fbbf24 0%, #f59e0b 100%);
    border-radius: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
    color: white;
    box-shadow: 0 4px 12px rgba(251,191,36,0.3);
}

.title-section h1 {
    font-size: 26px;
    font-weight: 700;
    background: linear-gradient(135deg, #fbbf24 0%, #f59e0b 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    margin: 0;
}

.dark .title-section h1 {
    background: linear-gradient(135deg, #fde68a 0%, #fbbf24 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
}

.header-right {
    display: flex;
    align-items: center;
    gap: 10px;
}

.btn-dark-toggle {
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
.btn-dark-toggle:hover { transform: translateY(-2px); }

.btn-back {
    background: var(--bg-card);
    color: #f59e0b;
    border: 2px solid #f59e0b;
    padding: 10px 20px;
    border-radius: 12px;
    cursor: pointer;
    font-size: 14px;
    font-weight: 600;
    display: flex;
    align-items: center;
    gap: 8px;
    transition: all 0.3s;
}
.btn-back:hover { background: #f59e0b; color: white; transform: translateY(-2px); }

/* ===== Content ===== */
.content {
    max-width: 1400px;
    margin: 0 auto;
    padding: 28px 32px;
}

.connection-alert { margin-bottom: 20px; }

.add-section,
.list-section {
    background: var(--bg-card);
    border-radius: 16px;
    box-shadow: var(--card-shadow);
    overflow: hidden;
    margin-bottom: 20px;
    transition: background 0.3s;
}

.section-header {
    padding: 20px 28px;
    background: var(--bg-section-header);
    border-bottom: 1px solid var(--border-section);
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.section-header h2 {
    font-size: 18px;
    font-weight: 600;
    color: var(--text-section-h);
    display: flex;
    align-items: center;
    gap: 10px;
    margin: 0;
}

.header-actions {
    display: flex;
    gap: 12px;
    align-items: center;
}

.filter-group {
    display: flex;
    align-items: center;
    gap: 8px;
}

.filter-group label {
    font-size: 13px;
    font-weight: 500;
    color: var(--text-filter-label);
}

.filter-group input[type="date"] {
    padding: 8px 12px;
    border: 2px solid var(--border-input);
    border-radius: 8px;
    font-size: 13px;
    background: var(--bg-input);
    color: var(--text-primary);
    transition: all 0.3s;
}

.btn-check-status {
    background: linear-gradient(135deg, #22c55e 0%, #16a34a 100%);
    color: white;
    border: none;
    padding: 8px 14px;
    border-radius: 8px;
    cursor: pointer;
    font-size: 13px;
    font-weight: 600;
    display: flex;
    align-items: center;
    gap: 6px;
    transition: all 0.3s;
}
.btn-check-status:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 4px 12px rgba(34,197,94,0.35); }
.btn-check-status:disabled { opacity: 0.5; cursor: not-allowed; }

.btn-refresh {
    background: var(--bg-card);
    color: #f59e0b;
    border: 2px solid #f59e0b;
    padding: 8px 14px;
    border-radius: 8px;
    cursor: pointer;
    font-size: 13px;
    font-weight: 600;
    display: flex;
    align-items: center;
    gap: 8px;
    transition: all 0.3s;
}
.btn-refresh:hover:not(:disabled) { background: #f59e0b; color: white; }
.btn-refresh:disabled { opacity: 0.5; cursor: not-allowed; }

.auto-refresh-badge {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    padding: 6px 10px;
    border-radius: 8px;
    font-size: 12px;
    font-weight: 600;
    background: #f0fdf4;
    color: #16a34a;
    border: 1.5px solid #86efac;
    white-space: nowrap;
}
.dark .auto-refresh-badge {
    background: #052e16;
    color: #86efac;
    border-color: #166534;
}

@keyframes spin-once { to { transform: rotate(360deg); } }
.spin-once { animation: spin-once 0.6s linear; }

/* ═══════════════════════════════════════════════════════
   FILTER BAR
   ═══════════════════════════════════════════════════════ */
.filter-bar {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 14px 28px;
    background: var(--bg-filter-bar);
    border-bottom: 1px solid var(--border-filter-bar);
    flex-wrap: wrap;
    animation: slideDown 0.25s ease-out;
}

@keyframes slideDown {
    from { opacity: 0; transform: translateY(-6px); }
    to   { opacity: 1; transform: translateY(0); }
}

.filter-bar-item {
    display: flex;
    align-items: center;
    gap: 0;
    border: 2px solid var(--border-input);
    border-radius: 10px;
    background: var(--bg-filter-input);
    overflow: hidden;
    transition: border-color 0.2s, box-shadow 0.2s;
}
.filter-bar-item:focus-within {
    border-color: #f59e0b;
    box-shadow: 0 0 0 3px rgba(251, 191, 36, 0.15);
}

.filter-bar-icon {
    padding: 0 10px;
    display: flex;
    align-items: center;
    color: var(--text-secondary);
    flex-shrink: 0;
}

/* Search input */
.search-item { flex: 1; min-width: 220px; max-width: 380px; }

.filter-bar-input {
    flex: 1;
    border: none;
    outline: none;
    padding: 9px 8px 9px 0;
    font-size: 13px;
    background: transparent;
    color: var(--text-primary);
    min-width: 0;
}
.filter-bar-input::placeholder { color: var(--text-secondary); }

.filter-clear-btn {
    padding: 0 10px;
    background: none;
    border: none;
    cursor: pointer;
    color: var(--text-secondary);
    display: flex;
    align-items: center;
    transition: color 0.2s;
}
.filter-clear-btn:hover { color: #ef4444; }

/* Select MC */
.select-item { position: relative; min-width: 160px; }

.filter-bar-select {
    flex: 1;
    border: none;
    outline: none;
    padding: 9px 32px 9px 0;
    font-size: 13px;
    background: transparent;
    color: var(--text-primary);
    appearance: none;
    -webkit-appearance: none;
    cursor: pointer;
    min-width: 0;
}

.select-chevron {
    position: absolute;
    right: 10px;
    top: 50%;
    transform: translateY(-50%);
    pointer-events: none;
    color: var(--text-secondary);
    flex-shrink: 0;
}

/* Filter chips */
.filter-chips {
    display: flex;
    align-items: center;
    gap: 8px;
    flex-wrap: wrap;
}

.filter-chip {
    display: inline-flex;
    align-items: center;
    gap: 5px;
    padding: 5px 10px;
    border-radius: 20px;
    font-size: 12px;
    font-weight: 600;
    background: var(--bg-chip);
    color: var(--text-chip);
    border: 1.5px solid var(--border-chip);
    white-space: nowrap;
}
.filter-chip button {
    background: none;
    border: none;
    cursor: pointer;
    color: inherit;
    font-size: 14px;
    line-height: 1;
    padding: 0 0 0 2px;
    opacity: 0.6;
    transition: opacity 0.15s;
}
.filter-chip button:hover { opacity: 1; }

.filter-chip.mc-chip {
    background: #ede9fe;
    color: #6d28d9;
    border-color: #c4b5fd;
}
.dark .filter-chip.mc-chip {
    background: #2e1065;
    color: #c4b5fd;
    border-color: #5b21b6;
}

.clear-all-btn {
    background: none;
    border: 1.5px solid var(--border-default);
    border-radius: 20px;
    padding: 4px 12px;
    font-size: 12px;
    color: var(--text-secondary);
    cursor: pointer;
    transition: all 0.2s;
}
.clear-all-btn:hover { border-color: #ef4444; color: #ef4444; }

.filter-result-count {
    margin-left: auto;
    font-size: 12px;
    color: var(--text-secondary);
    white-space: nowrap;
}
.count-highlight {
    font-size: 15px;
    font-weight: 700;
    color: #f59e0b;
}
/* ═══════════════════════════════════════════════════════ */

.input-section { padding: 28px; }

.scanner-wrapper {
    display: grid;
    grid-template-columns: 1fr auto;
    gap: 16px;
    margin-bottom: 20px;
}

.input-group-large {
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.input-group-large label {
    font-size: 15px;
    font-weight: 600;
    color: var(--text-primary);
}

.input-group-large input {
    padding: 14px 18px;
    border: 2px solid var(--border-default);
    border-radius: 12px;
    font-size: 16px;
    background: var(--bg-input);
    color: var(--text-primary);
    transition: all 0.3s;
}
.input-group-large input:focus { outline: none; border-color: #fbbf24; box-shadow: 0 0 0 3px rgba(251,191,36,0.1); }
.input-group-large input:disabled { opacity: 0.5; cursor: not-allowed; }

.btn-add {
    background: linear-gradient(135deg, #fbbf24 0%, #f59e0b 100%);
    color: white;
    border: none;
    padding: 14px 28px;
    border-radius: 12px;
    cursor: pointer;
    font-size: 16px;
    font-weight: 600;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    transition: all 0.3s;
    align-self: flex-end;
    min-width: 160px;
}
.btn-add:hover:not(:disabled) { transform: translateY(-2px); box-shadow: 0 8px 20px rgba(251,191,36,0.4); }
.btn-add:disabled { opacity: 0.5; cursor: not-allowed; }

/* Alerts */
.alert {
    padding: 14px;
    border-radius: 12px;
    display: flex;
    align-items: flex-start;
    gap: 10px;
    animation: slideIn 0.3s ease-out;
}
.alert.success { background: #dcfce7; border: 2px solid #86efac; color: #166534; }
.alert.error   { background: #fee2e2; border: 2px solid #fca5a5; color: #991b1b; }

.dark .alert.success { background: #052e16; border-color: #166534; color: #86efac; }
.dark .alert.error   { background: #450a0a; border-color: #991b1b; color: #fca5a5; }

.alert-title { font-weight: 700; font-size: 15px; margin-bottom: 6px; }
.alert-content { flex: 1; }
.alert-icon { flex-shrink: 0; }
.alert-message { font-size: 13px; line-height: 1.6; }
.alert-message code { background: rgba(0,0,0,0.1); padding: 2px 6px; border-radius: 4px; font-family: monospace; }
.alert-close { background: none; border: none; cursor: pointer; padding: 4px; color: inherit; opacity: 0.5; }
.alert-close:hover { opacity: 1; }

/* Stats */
.stats-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 16px;
    padding: 24px 28px;
}

.stat-card {
    padding: 18px;
    border-radius: 12px;
    display: flex;
    align-items: center;
    gap: 14px;
    border: 2px solid transparent;
}
.stat-card.total    { background: var(--bg-stat-total);    border-color: #93c5fd; }
.stat-card.approved { background: var(--bg-stat-approved); border-color: #86efac; }
.stat-card.pending  { background: var(--bg-stat-pending);  border-color: #fcd34d; }

.dark .stat-card.total    { border-color: #1e40af; }
.dark .stat-card.approved { border-color: #166534; }
.dark .stat-card.pending  { border-color: #78350f; }

.stat-icon {
    width: 44px;
    height: 44px;
    border-radius: 10px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: rgba(255,255,255,0.5);
}
.dark .stat-icon { background: rgba(255,255,255,0.1); }
.stat-card.total .stat-icon    { color: #3b82f6; }
.stat-card.approved .stat-icon { color: #22c55e; }
.stat-card.pending .stat-icon  { color: #f59e0b; }

.stat-label { font-size: 12px; font-weight: 500; color: var(--text-secondary); margin-bottom: 2px; }
.stat-value { font-size: 28px; font-weight: 700; color: var(--text-primary); }

/* Loading / Empty */
.loading-state { padding: 56px 28px; text-align: center; }
.loading-state p { margin-top: 14px; color: var(--text-secondary); }
.empty-state { padding: 56px 28px; text-align: center; color: var(--text-secondary); }
.empty-state svg { margin: 0 auto 14px; }
.empty-state h3 { margin: 0 0 8px 0; color: var(--text-primary); }
.empty-state p { margin: 0; }

.link-btn {
    background: none;
    border: none;
    color: #f59e0b;
    font-weight: 600;
    cursor: pointer;
    text-decoration: underline;
    font-size: inherit;
    padding: 0;
}

/* Table */
.table-wrapper { padding: 28px; overflow-x: auto; }
.lot-table { width: 100%; border-collapse: collapse; }
.lot-table thead { background: var(--bg-table-head); }
.lot-table th {
    padding: 14px;
    text-align: left;
    font-size: 13px;
    font-weight: 600;
    color: var(--text-secondary);
    border-bottom: 2px solid var(--border-default);
}
.lot-table td { padding: 14px; border-bottom: 1px solid var(--border-table); color: var(--text-primary); }
.lot-table tbody tr:hover { background: var(--bg-table-row-hover); }
.lot-table tbody tr.row-highlight { background: rgba(251, 191, 36, 0.06); }
.dark .lot-table tbody tr.row-highlight { background: rgba(251, 191, 36, 0.04); }
.date-cell { font-size: 12px; color: var(--text-secondary); white-space: nowrap; }
.text-muted { font-size: 13px; color: var(--text-secondary); }

/* Badges */
.lot-badge    { display: inline-block; padding: 5px 10px; border-radius: 6px; font-size: 13px; font-weight: 600; background: #dbeafe; color: #1e40af; }
.imobile-badge { display: inline-block; padding: 5px 10px; border-radius: 6px; font-size: 13px; font-weight: 600; background: #f3e8ff; color: #7c3aed; }
.mc-badge { display: inline-block; padding: 5px 10px; border-radius: 6px; font-size: 12px; font-weight: 600; background: #ecfdf5; color: #065f46; border: 1px solid #6ee7b7; }

.dark .lot-badge    { background: #1e3a5f; color: #93c5fd; }
.dark .imobile-badge { background: #2e1065; color: #c4b5fd; }
.dark .mc-badge { background: #022c22; color: #6ee7b7; border-color: #065f46; }

/* Search highlight */
:deep(.search-highlight) {
    background: #fef08a;
    color: #713f12;
    border-radius: 2px;
    padding: 0 1px;
    font-weight: 700;
}
.dark :deep(.search-highlight) {
    background: #854d0e;
    color: #fef08a;
}

.status-badge, .status-badge-small {
    display: inline-block;
    padding: 5px 10px;
    border-radius: 6px;
    font-size: 12px;
    font-weight: 600;
    text-transform: uppercase;
}
.status-badge-small { padding: 3px 7px; font-size: 10px; }

.status-badge.ok,    .status-badge-small.ok    { background: #dcfce7; color: #166534; }
.status-badge.hold,  .status-badge-small.hold  { background: #fef3c7; color: #92400e; }
.status-badge.scrap, .status-badge-small.scrap { background: #fee2e2; color: #991b1b; }
.status-badge.rescreen, .status-badge-small.rescreen { background: #dbeafe; color: #1e40af; }
.status-badge.default, .status-badge-small.default   { background: #f1f5f9; color: #64748b; }

.dark .status-badge.ok,    .dark .status-badge-small.ok    { background: #052e16; color: #86efac; }
.dark .status-badge.hold,  .dark .status-badge-small.hold  { background: #1c1400; color: #fde68a; }
.dark .status-badge.scrap, .dark .status-badge-small.scrap { background: #450a0a; color: #fca5a5; }
.dark .status-badge.rescreen, .dark .status-badge-small.rescreen { background: #1e3a5f; color: #93c5fd; }
.dark .status-badge.default, .dark .status-badge-small.default   { background: #1e293b; color: #94a3b8; }

/* Approval Badge */
.approval-badge {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    padding: 5px 10px;
    border-radius: 6px;
    font-size: 12px;
    font-weight: 600;
    white-space: nowrap;
}
.approval-badge.th100-confirmed { background: #dcfce7; color: #166534; }
.dark .approval-badge.th100-confirmed { background: #052e16; color: #86efac; }
.approval-badge.approved { background: #dbeafe; color: #1e40af; }
.dark .approval-badge.approved { background: #1e3a5f; color: #93c5fd; }
.approval-badge.th100-pending { background: #ffedd5; color: #9a3412; }
.dark .approval-badge.th100-pending { background: #431407; color: #fdba74; }
.approval-badge.pending { background: #f1f5f9; color: #475569; }
.dark .approval-badge.pending { background: #1e293b; color: #94a3b8; }

/* Delete button */
.btn-action {
    padding: 7px;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    transition: all 0.3s;
    display: flex;
    align-items: center;
    justify-content: center;
}
.btn-action.delete { background: #fee2e2; color: #991b1b; }
.btn-action.delete:hover { background: #ef4444; color: white; }
.dark .btn-action.delete { background: #450a0a; color: #fca5a5; }

/* Spinners */
.spinner {
    width: 20px; height: 20px;
    border: 3px solid rgba(255,255,255,0.3);
    border-top-color: white;
    border-radius: 50%;
    animation: spin 0.8s linear infinite;
}
.spinner-amber {
    width: 36px; height: 36px;
    border: 4px solid #fde68a;
    border-top-color: #f59e0b;
    border-radius: 50%;
    margin: 0 auto;
    animation: spin 0.8s linear infinite;
}
.spinner-white-sm {
    width: 16px; height: 16px;
    border: 2px solid rgba(255,255,255,0.4);
    border-top-color: white;
    border-radius: 50%;
    animation: spin 0.8s linear infinite;
}

@keyframes spin { to { transform: rotate(360deg); } }
@keyframes slideIn { from { opacity: 0; transform: translateY(-8px); } to { opacity: 1; transform: translateY(0); } }

@media (max-width: 1024px) {
    .stats-grid { grid-template-columns: 1fr; }
    .scanner-wrapper { grid-template-columns: 1fr; }
}
@media (max-width: 768px) {
    .header-content { flex-direction: column; gap: 12px; }
    .content { padding: 16px; }
    .section-header { flex-direction: column; align-items: flex-start; gap: 12px; }
    .header-actions { width: 100%; flex-direction: column; }
    .filter-group { width: 100%; }
    .filter-group input[type="date"] { flex: 1; }
    .filter-bar { flex-direction: column; align-items: stretch; }
    .search-item { max-width: 100%; }
    .filter-result-count { margin-left: 0; }
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
@keyframes pulseRing {
    0%   { box-shadow: 0 0 0 0 rgba(251,191,36,0.5); }
    70%  { box-shadow: 0 0 0 10px rgba(251,191,36,0); }
    100% { box-shadow: 0 0 0 0 rgba(251,191,36,0); }
}
@keyframes countUp {
    from { opacity: 0; transform: translateY(6px) scale(0.85); }
    to   { opacity: 1; transform: translateY(0) scale(1); }
}

/* Entry animations */
.header          { animation: fadeUp 0.35s cubic-bezier(0.22,1,0.36,1) both; }
.scanner-wrapper { animation: fadeUp 0.4s cubic-bezier(0.22,1,0.36,1) 0.05s both; }
.stats-grid      { animation: fadeUp 0.4s cubic-bezier(0.22,1,0.36,1) 0.1s both; }
.content         { animation: fadeUp 0.4s cubic-bezier(0.22,1,0.36,1) 0.15s both; }

/* Stat cards stagger */
.stat-card:nth-child(1) { animation: scaleIn 0.35s cubic-bezier(0.22,1,0.36,1) 0.12s both; }
.stat-card:nth-child(2) { animation: scaleIn 0.35s cubic-bezier(0.22,1,0.36,1) 0.18s both; }
.stat-card:nth-child(3) { animation: scaleIn 0.35s cubic-bezier(0.22,1,0.36,1) 0.24s both; }

/* Stat numbers pop */
.stat-value { animation: countUp 0.4s cubic-bezier(0.22,1,0.36,1) 0.3s both; }

/* Pending stat pulse to draw attention */
.stat-card.pending { animation: scaleIn 0.35s cubic-bezier(0.22,1,0.36,1) 0.24s both,
                                pulseRing 2.5s ease-out 1s infinite; }

/* Table rows slide in */
.lot-row { animation: slideRight 0.28s cubic-bezier(0.22,1,0.36,1) both; }

/* Button hover lift */
.btn-check-status:hover,
.btn-refresh:hover,
.btn-add:hover {
    transform: translateY(-2px);
    transition: transform 0.18s cubic-bezier(0.22,1,0.36,1),
                box-shadow 0.18s ease !important;
}
</style>