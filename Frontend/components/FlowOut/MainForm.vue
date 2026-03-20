<template>
    <div class="main-section" :class="{ 'dark': isDark }">
        <button class="theme-toggle" @click="toggleDark" :title="isDark ? 'Switch to Light Mode' : 'Switch to Dark Mode'">
            <svg v-if="isDark" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
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

        <div class="header">
            <div class="header-content">
                <div class="title-section">
                    <div class="icon-wrapper">
                        <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <path d="M9 11l3 3L22 4" />
                            <path d="M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11" />
                        </svg>
                    </div>
                    <div>
                        <h1>Flow-out Prevention System</h1>
                        <p class="subtitle">ระบบป้องกัน LOT ซ้ำและงานข้าม LOT</p>
                    </div>
                </div>
                <div class="header-actions">
                    <button class="btn-rescreen" @click="handleGoToRescreen">
                        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <path d="M9 11l3 3L22 4" />
                            <path d="M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11" />
                        </svg>
                        Rescreen Check
                    </button>
                    <button class="btn-summarise" @click="handleGoToSummary">
                        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                            <rect x="3" y="3" width="7" height="7" />
                            <rect x="14" y="3" width="7" height="7" />
                            <rect x="14" y="14" width="7" height="7" />
                            <rect x="3" y="14" width="7" height="7" />
                        </svg>
                        Summarise
                    </button>
                </div>
            </div>

            <div class="content">
                <div class="layout-grid">
                    <!-- LEFT -->
                    <div class="card data-section">
                        <div class="mc-header">
                            <div class="mc-info">
                                <span class="mc-label">Machine</span>
                                <div class="mc-select-wrapper">
                                    <select v-model="selectedMc" @change="handleMcChange" class="mc-select">
                                        <option value="" disabled>เลือก MC</option>
                                        <option v-for="mc in mcList" :key="mc" :value="mc">{{ mc }}</option>
                                    </select>
                                </div>
                            </div>
                            <div class="date-info">
                                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
                                    <line x1="16" y1="2" x2="16" y2="6" />
                                    <line x1="8" y1="2" x2="8" y2="6" />
                                    <line x1="3" y1="10" x2="21" y2="10" />
                                </svg>
                                {{ currentDate }}
                            </div>
                        </div>

                        <!-- Color Legend -->
                        <div class="color-legend">
                            <div class="legend-item">
                                <span class="legend-dot" style="background:#22c55e"></span><span>OK</span>
                            </div>
                            <div class="legend-item">
                                <span class="legend-dot" style="background:#ef4444"></span><span>NG / SCRAP</span>
                            </div>
                            <div class="legend-item">
                                <span class="legend-dot" style="background:#f97316"></span><span>Waiting Rescreen</span>
                            </div>
                            <div class="legend-item">
                                <span class="legend-dot" style="background:#eab308"></span><span>Rescreen Pending</span>
                            </div>
                            <div class="legend-item">
                                <span class="legend-dot" style="background:#3b82f6"></span><span>HOLD</span>
                            </div>
                            <div class="legend-item">
                                <span class="legend-dot" style="background:#8b5cf6"></span><span>REP Product</span>
                            </div>
                        </div>

                        <div class="table-container">
                            <table class="lot-table">
                                <thead>
                                    <tr>
                                        <th width="10%">LOT</th>
                                        <th width="30%">PO LOT</th>
                                        <th width="20%">STATUS TN</th>
                                        <th width="20%">CHECK</th>
                                        <th width="20%">QTY</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr v-for="(item, index) in lotRecords"
                                        :key="item.id || (item.poLot + index)"
                                        :class="[
                                            'data-row',
                                            item.rowColor === 'waiting_rescreen' ? 'row-waiting-rescreen' : '',
                                            item.rowColor === 'rescreen_pending'  ? 'row-rescreen-pending'  : '',
                                            item.rowColor === 'scrap'             ? 'row-skipped-scrap'     : '',
                                            item.rowColor === 'hold'              ? 'row-skipped-hold'      : '',
                                            isRepRecord(item)                     ? 'row-rep-product'       : '',
                                        ]">
                                        <td>
                                            <span :class="[
                                                'lot-number',
                                                item.rowColor === 'waiting_rescreen' ? 'lot-color-orange' : '',
                                                item.rowColor === 'rescreen_pending'  ? 'lot-color-yellow' : '',
                                                item.rowColor === 'scrap'             ? 'lot-color-red'    : '',
                                                item.rowColor === 'hold'              ? 'lot-color-blue'   : '',
                                                isRepRecord(item)                     ? 'lot-color-purple'  : '',
                                                item.rowColor === 'default' && item.check !== 'OK' && !isRepRecord(item) ? 'lot-ng' : '',
                                            ]">
                                                {{ String(index + 1).padStart(2, '0') }}
                                            </span>
                                        </td>
                                        <td class="text-left">
                                            <div class="po-lot-cell">
                                                {{ item.poLot }}
                                                <span v-if="isRepRecord(item)" class="rep-badge">REP</span>
                                            </div>
                                        </td>
                                        <td>
                                            <span :class="['status-badge', isRepRecord(item) ? 'status-badge-rep' : getStatusBadgeClass(item.statusTn)]">
                                                {{ isRepRecord(item) ? 'OK' : item.statusTn }}
                                            </span>
                                        </td>
                                        <td>
                                            <span :class="[
                                                'check-badge',
                                                item.rowColor === 'waiting_rescreen' ? 'badge-waiting-rescreen' : '',
                                                item.rowColor === 'rescreen_pending'  ? 'badge-rescreen-pending'  : '',
                                                item.rowColor === 'scrap'             ? 'badge-skipped-scrap'     : '',
                                                item.rowColor === 'hold'              ? 'badge-skipped-hold'      : '',
                                                isRepRecord(item)                     ? 'badge-ok'                : '',
                                                item.rowColor === 'default' && item.check === 'OK' && !isRepRecord(item) ? 'badge-ok' : '',
                                                item.rowColor === 'default' && item.check !== 'OK' && !isRepRecord(item) ? 'badge-ng' : '',
                                            ]">
                                                <svg v-if="item.check === 'OK' || isRepRecord(item)" xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
                                                    <polyline points="20 6 9 17 4 12" />
                                                </svg>
                                                <svg v-else-if="item.check === 'RESCREEN'" xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                                                    <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
                                                    <line x1="12" y1="9" x2="12" y2="13" /><line x1="12" y1="17" x2="12.01" y2="17" />
                                                </svg>
                                                <svg v-else xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
                                                    <line x1="18" y1="6" x2="6" y2="18" />
                                                    <line x1="6" y1="6" x2="18" y2="18" />
                                                </svg>
                                                {{ isRepRecord(item) ? 'OK' : item.check }}
                                            </span>
                                        </td>
                                        <td>
                                            <span class="qty-text">{{ item.lotQty != null ? item.lotQty : '-' }}</span>
                                        </td>
                                    </tr>
                                    <tr v-if="lotRecords.length === 0" class="empty-state-row">
                                        <td colspan="5" class="empty-state">
                                            <div class="empty-state-content">
                                                <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                                    <circle cx="12" cy="12" r="10" />
                                                    <line x1="12" y1="8" x2="12" y2="12" />
                                                    <line x1="12" y1="16" x2="12.01" y2="16" />
                                                </svg>
                                                <p>ไม่มีข้อมูล LOT</p>
                                            </div>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>

                        <div class="stats-section">
                            <div class="stat-card">
                                <div class="stat-icon total">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                        <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
                                        <circle cx="12" cy="7" r="4" />
                                    </svg>
                                </div>
                                <div class="stat-content">
                                    <span class="stat-label">Total</span>
                                    <span class="stat-value">{{ totalLots }}</span>
                                </div>
                            </div>
                            <div class="stat-card success">
                                <div class="stat-icon ok">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                        <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
                                        <polyline points="22 4 12 14.01 9 11.01" />
                                    </svg>
                                </div>
                                <div class="stat-content">
                                    <span class="stat-label">OK</span>
                                    <span class="stat-value">{{ okCount }}</span>
                                </div>
                            </div>
                            <div class="stat-card danger">
                                <div class="stat-icon ng">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                        <circle cx="12" cy="12" r="10" />
                                        <line x1="15" y1="9" x2="9" y2="15" />
                                        <line x1="9" y1="9" x2="15" y2="15" />
                                    </svg>
                                </div>
                                <div class="stat-content">
                                    <span class="stat-label">NG</span>
                                    <span class="stat-value">{{ ngCount }}</span>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- RIGHT: Scan Form -->
                    <div class="card form-section">
                        <div class="card-header">
                            <h2>
                                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <path d="M3 3h7v7H3zM14 3h7v7h-7zM14 14h7v7h-7zM3 14h7v7H3z" />
                                </svg>
                                Scan LOT Number
                            </h2>
                        </div>

                        <div class="input-group">
                            <label>
                                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <rect x="2" y="6" width="20" height="12" rx="2" />
                                    <path d="M7 11h.01M12 11h.01M17 11h.01M7 15h.01M12 15h.01M17 15h.01" />
                                </svg>
                                LOT No.
                            </label>
                            <div class="input-wrapper">
                                <input v-model="lotNo" type="text" @keyup.enter="searchLot"
                                    placeholder="Scan barcode or enter LOT number..." class="input-modern"
                                    ref="lotInput" />
                            </div>
                            <p v-if="totalLots >= 8" class="info-text">
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <circle cx="12" cy="12" r="10" />
                                    <line x1="12" y1="16" x2="12" y2="12" />
                                    <line x1="12" y1="8" x2="12.01" y2="8" />
                                </svg>
                                MC {{ selectedMc }} มี {{ totalLots }} LOT ทั้งหมด
                            </p>
                        </div>

                        <div class="info-box">
                            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                <circle cx="12" cy="12" r="10" />
                                <line x1="12" y1="16" x2="12" y2="12" />
                                <line x1="12" y1="8" x2="12.01" y2="8" />
                            </svg>
                            <div>
                                <strong>สำคัญ:</strong> จะต้องเรียง LOT ตามลำดับเท่านั้น เพื่อป้องกัน LOT ซ้ำและงานข้าม LOT<br>
                                <small style="opacity: 0.8; margin-top: 4px; display: block;">
                                    • ถ้าไม่มี LOT เลย ต้องเริ่มด้วย 001 เท่านั้น<br>
                                    • ถ้ามี LOT แล้ว ต้องเพิ่มตามลำดับต่อเนื่อง<br>
                                    • <strong>REP Product</strong> (format: XXXXX-REP-001-N) บันทึกโดยไม่ต้องใส่ Qty
                                </small>
                            </div>
                        </div>

                        <transition name="fade">
                            <div v-if="successMessage" class="alert alert-success">
                                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" />
                                    <polyline points="22 4 12 14.01 9 11.01" />
                                </svg>
                                {{ successMessage }}
                            </div>
                        </transition>
                    </div>
                </div>
            </div>

            <!-- LOT Information Modal -->
            <transition name="modal">
                <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
                    <div class="modal-container">
                        <div class="modal-header" :class="getModalHeaderClass()">
                            <div class="modal-icon">
                                <svg v-if="getStatusIcon() === 'check'" xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" /><polyline points="22 4 12 14.01 9 11.01" />
                                </svg>
                                <svg v-else-if="getStatusIcon() === 'alert'" xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
                                    <line x1="12" y1="9" x2="12" y2="13" /><line x1="12" y1="17" x2="12.01" y2="17" />
                                </svg>
                                <svg v-else-if="getStatusIcon() === 'info'" xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <circle cx="12" cy="12" r="10" />
                                    <line x1="12" y1="16" x2="12" y2="12" /><line x1="12" y1="8" x2="12.01" y2="8" />
                                </svg>
                                <svg v-else xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <circle cx="12" cy="12" r="10" />
                                    <line x1="15" y1="9" x2="9" y2="15" /><line x1="9" y1="9" x2="15" y2="15" />
                                </svg>
                            </div>
                            <h3>LOT Information</h3>
                            <button class="modal-close" @click="closeModal">
                                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" />
                                </svg>
                            </button>
                        </div>

                        <div class="modal-body">
                            <div class="lot-info-grid">
                                <!-- REP: แสดง PO LOT แทน IMOBILE LOT -->
                                <div class="info-item" v-if="!isRepProduct">
                                    <label>IMOBILE LOT</label>
                                    <div class="info-value">{{ modalData?.imobileLot }}</div>
                                </div>
                                <div class="info-item" :class="{ 'rep-full-width': isRepProduct }">
                                    <label>PO LOT</label>
                                    <div class="info-value">
                                        {{ modalData?.poLot }}
                                        <span v-if="isRepProduct" class="rep-badge" style="margin-left:8px">REP</span>
                                    </div>
                                </div>
                            </div>

                            <!-- ✅ Duplicate Banner -->
                            <div v-if="modalData?.isDuplicate" class="duplicate-banner">
                                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>
                                </svg>
                                <div>
                                    <strong>LOT นี้เคยถูกบันทึกแล้ว</strong>
                                    <span>การบันทึกใหม่จะอัปเดต Qty</span>
                                </div>
                            </div>

                            <!-- ✅ Cassette Number — แสดงเสมอสำหรับ non-REP -->
                            <div v-if="!isRepProduct" class="cassette-section">
                                <div class="cassette-label">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                        <rect x="2" y="4" width="20" height="16" rx="2"/>
                                        <circle cx="8" cy="12" r="2"/>
                                        <circle cx="16" cy="12" r="2"/>
                                        <path d="M10 12h4"/>
                                    </svg>
                                    Cassette No.
                                </div>
                                <div class="cassette-value">{{ modalData?.cassetteNo || '—' }}</div>
                            </div>

                            <!-- REP Product note -->
                            <div v-if="isRepProduct" class="rep-info-banner">
                                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/>
                                </svg>
                                <div>
                                    <strong>REP Product</strong> — บันทึกผ่าน PO LOT โดยตรง
                                </div>
                            </div>

                            <div class="status-display">
                                <div class="status-main" :class="getStatusClass()">
                                    <div class="status-icon-wrapper">
                                        <svg v-if="getStatusIcon() === 'check'" xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                            <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" /><polyline points="22 4 12 14.01 9 11.01" />
                                        </svg>
                                        <svg v-else-if="getStatusIcon() === 'alert'" xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                            <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
                                            <line x1="12" y1="9" x2="12" y2="13" /><line x1="12" y1="17" x2="12.01" y2="17" />
                                        </svg>
                                        <svg v-else-if="getStatusIcon() === 'info'" xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                            <circle cx="12" cy="12" r="10" />
                                            <line x1="12" y1="16" x2="12" y2="12" /><line x1="12" y1="8" x2="12.01" y2="8" />
                                        </svg>
                                        <svg v-else xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                            <circle cx="12" cy="12" r="10" />
                                            <line x1="15" y1="9" x2="9" y2="15" /><line x1="9" y1="9" x2="15" y2="15" />
                                        </svg>
                                    </div>
                                    <div class="status-text">
                                        <span class="status-label">Status</span>
                                        <span class="status-value">{{ modalData?.statusTn?.toUpperCase() }}</span>
                                    </div>
                                </div>
                                <div v-if="showSubStatus()" class="status-sub" :class="getSubStatusClass()">
                                    <div class="status-icon-wrapper">
                                        <svg v-if="modalData?.checkSt" xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                            <polyline points="20 6 9 17 4 12" />
                                        </svg>
                                        <svg v-else xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                            <circle cx="12" cy="12" r="10" />
                                            <line x1="12" y1="8" x2="12" y2="12" /><line x1="12" y1="16" x2="12.01" y2="16" />
                                        </svg>
                                    </div>
                                    <div class="status-text">
                                        <span class="status-label">TH100 Status</span>
                                        <span class="status-value">{{ getSubStatusValue() }}</span>
                                    </div>
                                </div>
                            </div>

                            <!-- qty input แสดงทุก lot รวมถึง REP -->
                            <div v-if="canSaveLot()" class="qty-section">
                                <h4>Lot Qty</h4>
                                <div class="qty-input-group">
                                    <input v-model="lotQty" type="number" placeholder="0" class="qty-input-single" min="0" />
                                </div>
                            </div>

                            <div v-if="!canSaveLot()" :class="['warning-box', getWarningBoxClass()]">
                                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
                                    <line x1="12" y1="9" x2="12" y2="13" /><line x1="12" y1="17" x2="12.01" y2="17" />
                                </svg>
                                <div>
                                    <strong>{{ getWarningTitle() }}</strong>
                                    <p>{{ getWarningMessage() }}</p>
                                </div>
                            </div>
                        </div>

                        <div class="modal-footer">
                            <button v-if="canSaveLot()" class="btn btn-save" @click="handleSave" :disabled="!isQtyValid()">
                                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <polyline points="20 6 9 17 4 12" />
                                </svg>
                                บันทึก
                            </button>
                            <button v-else class="btn btn-sendback" @click="handleSendBack">
                                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <polyline points="15 18 9 12 15 6" />
                                </svg>
                                Send Back
                            </button>
                            <button class="btn btn-cancel" @click="closeModal">ยกเลิก</button>
                        </div>
                    </div>
                </div>
            </transition>

            <!-- Error Modal -->
            <transition name="modal">
                <div v-if="showErrorModal" class="modal-overlay" @click.self="closeErrorModal">
                    <div class="error-modal-container">
                        <div class="error-modal-header" :class="getErrorModalHeaderClass()">
                            <div class="error-modal-icon">
                                <svg v-if="errorStatus === 'HOLD'" xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <circle cx="12" cy="12" r="10" /><line x1="12" y1="8" x2="12" y2="12" /><line x1="12" y1="16" x2="12.01" y2="16" />
                                </svg>
                                <svg v-else-if="errorStatus === 'SCRAP'" xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <circle cx="12" cy="12" r="10" /><line x1="15" y1="9" x2="9" y2="15" /><line x1="9" y1="9" x2="15" y2="15" />
                                </svg>
                                <svg v-else xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
                                    <line x1="12" y1="9" x2="12" y2="13" /><line x1="12" y1="17" x2="12.01" y2="17" />
                                </svg>
                            </div>
                            <h3>คำเตือน</h3>
                            <button class="modal-close-error" @click="closeErrorModal">
                                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" />
                                </svg>
                            </button>
                        </div>

                        <div class="error-modal-body">
                            <div v-if="errorStatus" class="error-status-badge" :class="getErrorStatusBadgeClass()">
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <circle cx="12" cy="12" r="10" /><line x1="12" y1="8" x2="12" y2="12" /><line x1="12" y1="16" x2="12.01" y2="16" />
                                </svg>
                                <span>Status: {{ errorStatus }}</span>
                            </div>
                            <div v-if="errorData.currentLot" class="info-box-modern current">
                                <div class="info-label">🔍 LOT ที่พยายามเพิ่ม</div>
                                <div class="info-value">{{ errorData.currentLot }}</div>
                            </div>
                            <div v-if="errorData.requiredLot" class="info-box-modern required">
                                <div class="info-label">✅ ต้องเริ่มด้วย LOT</div>
                                <div class="info-value">{{ errorData.requiredLot }}</div>
                            </div>
                            <div class="error-message-modern" :class="getErrorMessageBoxClass()">
                                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
                                    <line x1="12" y1="9" x2="12" y2="13" /><line x1="12" y1="17" x2="12.01" y2="17" />
                                </svg>
                                <div>
                                    <strong>{{ getErrorTitle() }}</strong>
                                    <p>{{ getErrorDescription() }}</p>
                                </div>
                            </div>
                            <div v-if="hasMissingLots()" class="lots-section missing">
                                <div class="lots-header">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                        <line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" />
                                    </svg>
                                    <strong>❌ LOT ที่ยังไม่ได้เพิ่ม (ต้องเพิ่มก่อน)</strong>
                                </div>
                                <ul class="lots-list">
                                    <li v-for="lot in errorData.missingLots" :key="lot" class="lot-item missing">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                            <line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" />
                                        </svg>
                                        {{ lot }}
                                    </li>
                                </ul>
                            </div>
                            <div v-if="hasSkippedLots()" class="lots-section skipped">
                                <div class="lots-header success">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                        <polyline points="20 6 9 17 4 12" />
                                    </svg>
                                    <strong>✅ LOT ที่ข้ามได้ (SCRAP/HOLD/Rescreen)</strong>
                                </div>
                                <div class="success-notice">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                        <circle cx="12" cy="12" r="10" /><line x1="12" y1="16" x2="12" y2="12" /><line x1="12" y1="8" x2="12.01" y2="8" />
                                    </svg>
                                    <span>LOT เหล่านี้เป็น SCRAP/HOLD หรือมีใน Rescreen Check สามารถข้ามได้</span>
                                </div>
                                <ul class="lots-list">
                                    <li v-for="lot in errorData.scrapHoldLots" :key="lot" class="lot-item skipped">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                            <polyline points="20 6 9 17 4 12" />
                                        </svg>
                                        {{ lot }}
                                    </li>
                                </ul>
                            </div>
                        </div>

                        <div class="error-modal-footer">
                            <button class="btn btn-error-ok" :class="getErrorButtonClass()" @click="closeErrorModal">ตกลง</button>
                        </div>
                    </div>
                </div>
            </transition>

            <!-- Loading Overlay -->
            <transition name="fade">
                <div v-if="isLoading" class="loading-overlay">
                    <div class="loading-spinner">
                        <div class="spinner"></div>
                        <p>Processing...</p>
                    </div>
                </div>
            </transition>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'

const isMounted = ref(true)

interface LotRecord {
    id: string | null
    poLot: string
    imobileLot: string | null
    statusTn: string
    checkSt: boolean
    check: string
    lotQty: number | null
    mcNo?: string
    rowColor?: string
}

interface ModalData {
    imobileLot: string | null
    poLot: string
    statusTn: string
    checkSt: boolean
    mcNo: string
    isRepProduct?: boolean
    existingQty?: number
    quantity?: number
    // ✅ NEW: Cassette Number จาก TH_Record.ca5_in9
    cassetteNo?: string | null
}

const emit = defineEmits(['go-to-summary', 'go-to-rescreen', 'goToSummary', 'goToRescreen'])

// Dark Mode State
const isDark = ref(false)

const toggleDark = () => {
    isDark.value = !isDark.value
    localStorage.setItem('theme', isDark.value ? 'dark' : 'light')
}

// State
const lotNo = ref('')
const successMessage = ref('')
const isLoading = ref(false)
const lotInput = ref<HTMLInputElement | null>(null)

// Modal States
const showModal = ref(false)
const modalData = ref<ModalData | null>(null)
const lotQty = ref('')

// Error Modal States
const showErrorModal = ref(false)
const errorMessage = ref('')
const errorStatus = ref('')
const errorData = ref({
    missingLots: [],
    scrapHoldLots: [],
    currentLot: '',
    requiredLot: ''
})

// Data from DB
const allDisplayRecords = ref<LotRecord[]>([])
const lotRecords = ref<LotRecord[]>([])
const totalLots = ref(0)
const okCount = ref(0)
const ngCount = ref(0)

// MC Selection
const mcList = ref<string[]>([])
const selectedMc = ref('')

const currentDate = computed(() => {
    const today = new Date()
    return today.toLocaleDateString('th-TH', { year: 'numeric', month: 'long', day: 'numeric' })
})

const getTodayYMD = (): string => {
    const today = new Date()
    const yyyy = today.getFullYear()
    const mm = String(today.getMonth() + 1).padStart(2, '0')
    const dd = String(today.getDate()).padStart(2, '0')
    return `${yyyy}-${mm}-${dd}`
}

// Helper: ตรวจว่าเป็น REP record หรือไม่
const isRepRecord = (item: LotRecord): boolean => {
    return item.mcNo === 'REP' || (item.poLot?.split('-')[1]?.toUpperCase() === 'REP')
}

// computed: ตรวจว่า modal ปัจจุบันเป็น REP product หรือไม่
const isRepProduct = computed(() => {
    return modalData.value?.mcNo === 'REP' || modalData.value?.isRepProduct === true
})

const handleGoToSummary = () => { emit('go-to-summary'); emit('goToSummary') }
const handleGoToRescreen = () => { emit('go-to-rescreen'); emit('goToRescreen') }

const getStatusBadgeClass = (status: string): string => {
    const statusLower = status?.toLowerCase() || ''
    if (statusLower === 'ok' || statusLower.includes('ok')) return 'status-badge-ok'
    if (statusLower === 'hold') return 'status-badge-hold'
    if (statusLower === 'scrap') return 'status-badge-scrap'
    if (statusLower === 'rescreen') return 'status-badge-rescreen'
    return 'status-badge-default'
}

const showErrorModalFunc = (message: string, status?: string, additionalData?: any) => {
    errorMessage.value = message
    errorStatus.value = status || ''
    errorData.value = additionalData ? {
        missingLots: additionalData.missingLots || [],
        scrapHoldLots: additionalData.scrapHoldLots || [],
        currentLot: additionalData.currentLot || '',
        requiredLot: additionalData.requiredLot || ''
    } : { missingLots: [], scrapHoldLots: [], currentLot: '', requiredLot: '' }
    showErrorModal.value = true
    playErrorBeep()
}

const closeErrorModal = () => {
    showErrorModal.value = false
    errorMessage.value = ''
    errorStatus.value = ''
    errorData.value = { missingLots: [], scrapHoldLots: [], currentLot: '', requiredLot: '' }
    lotNo.value = ''
    lotInput.value?.focus()
}

const fetchMcList = async () => {
    try {
        const { data: response, error: fetchError } = await useFetch('/api/SI25031/get-mc-list', {
            baseURL: useRuntimeConfig().public.apiBase,
            params: { date: getTodayYMD() }
        })
        if (!fetchError.value && response.value?.success) {
            mcList.value = response.value.data || []
            if (mcList.value.length > 0 && !selectedMc.value) {
                selectedMc.value = mcList.value[0]
                await fetchLotData()
            }
        }
    } catch (err) { console.error('Error fetching MC list:', err) }
}

const fetchLotData = async () => {
    if (!selectedMc.value) return
    try {
        const { data: response, error: fetchError } = await useFetch('/api/SI25031/get-lots-by-mc', {
            baseURL: useRuntimeConfig().public.apiBase,
            params: { mcNo: selectedMc.value, date: getTodayYMD() },
            key: `lots-all-${selectedMc.value}-${Date.now()}`
        })
        if (!fetchError.value && response.value?.success) {
            allDisplayRecords.value = response.value.data || []
            lotRecords.value = allDisplayRecords.value
            totalLots.value = response.value.totalCount || 0
            okCount.value = response.value.okCount || 0
            ngCount.value = response.value.ngCount || 0
        }
    } catch (err) { console.error('Error fetching lot data:', err) }
}

const handleMcChange = () => { fetchLotData() }
const focusInput = () => { lotInput.value?.focus() }

const searchLot = async () => {
    if (!lotNo.value.trim()) { showErrorModalFunc('กรุณาใส่ LOT Number'); return }
    await fetchLotData()
    isLoading.value = true
    try {
        const { data: response, error: fetchError } = await useFetch('/api/SI25031/search-lot', {
            baseURL: useRuntimeConfig().public.apiBase,
            method: 'POST',
            body: { lotNumber: lotNo.value }
        })
        if (fetchError.value) {
            const errorDataValue = fetchError.value.data || {}
            showErrorModalFunc(errorDataValue.message || 'เกิดข้อผิดพลาดในการค้นหา LOT', errorDataValue.status || '', errorDataValue)
            lotNo.value = ''
            return
        }
        if (response.value?.success && response.value?.data) {
            modalData.value = response.value.data
            showModal.value = true
            lotQty.value = String(response.value.data.existingQty || response.value.data.quantity || 0)
        } else {
            const responseData = response.value || {}
            showErrorModalFunc(responseData.message || 'ไม่พบข้อมูล LOT นี้ในระบบ', responseData.status || '', responseData)
            lotNo.value = ''
        }
    } catch (err) {
        showErrorModalFunc('เกิดข้อผิดพลาดในการค้นหา LOT')
    } finally { isLoading.value = false }
}

const closeModal = () => { showModal.value = false; modalData.value = null; lotQty.value = '' }

// REP product บันทึกได้เสมอ
const canSaveLot = () => {
    if (!modalData.value) return false
    if (isRepProduct.value) return true
    const status = modalData.value.statusTn?.toLowerCase()
    if (status === 'ok') return true
    if (status === 'rescreen' && modalData.value.checkSt) return true
    return false
}

const showSubStatus = () => {
    if (!modalData.value) return false
    if (isRepProduct.value) return false
    return modalData.value.statusTn?.toLowerCase() === 'rescreen'
}
const getSubStatusValue = () => { if (!modalData.value) return ''; return modalData.value.checkSt ? 'OK' : 'Pending' }

const getStatusClass = () => {
    if (!modalData.value) return ''
    if (isRepProduct.value) return 'status-ok'
    const status = modalData.value.statusTn?.toLowerCase()
    if (status === 'ok') return 'status-ok'
    if (status === 'rescreen') return 'status-rescreen'
    if (status === 'hold') return 'status-hold'
    if (status === 'scrap') return 'status-scrap'
    return 'status-default'
}

const getSubStatusClass = () => { if (!modalData.value) return ''; return modalData.value.checkSt ? 'substatus-ok' : 'substatus-pending' }

const getModalHeaderClass = () => {
    if (!modalData.value) return ''
    if (isRepProduct.value) return 'header-rep'
    const status = modalData.value.statusTn?.toLowerCase()
    if (status === 'ok') return 'header-ok'
    if (status === 'rescreen' && modalData.value.checkSt) return 'header-rescreen-ok'
    if (status === 'rescreen') return 'header-rescreen-pending'
    if (status === 'hold') return 'header-hold'
    if (status === 'scrap') return 'header-scrap'
    return 'header-default'
}

const getStatusIcon = () => {
    if (!modalData.value) return 'check'
    if (isRepProduct.value) return 'check'
    const status = modalData.value.statusTn?.toLowerCase()
    if (status === 'ok') return 'check'
    if (status === 'rescreen') return 'alert'
    if (status === 'hold') return 'info'
    if (status === 'scrap') return 'x'
    return 'check'
}

const getWarningBoxClass = () => {
    if (!modalData.value) return ''
    const status = modalData.value.statusTn?.toLowerCase()
    if (status === 'hold') return 'warning-hold'
    if (status === 'scrap') return 'warning-scrap'
    if (status === 'rescreen') return 'warning-rescreen'
    return ''
}

const getWarningTitle = () => {
    if (!modalData.value) return ''
    const status = modalData.value.statusTn?.toLowerCase()
    if (status === 'rescreen' && !modalData.value.checkSt) return 'Rescreen Pending'
    if (status === 'hold') return 'Hold Status'
    if (status === 'scrap') return 'Scrap Status'
    return 'Cannot Save'
}

const getWarningMessage = () => {
    if (!modalData.value) return ''
    const status = modalData.value.statusTn?.toLowerCase()
    if (status === 'rescreen' && !modalData.value.checkSt) return 'LOT นี้อยู่ในสถานะ Rescreen และยังไม่มีข้อมูลใน TH100 กรุณาส่งกลับไปตรวจสอบ'
    if (status === 'hold') return 'LOT นี้อยู่ในสถานะ Hold ไม่สามารถบันทึกเข้าระบบได้ กรุณาส่งกลับ'
    if (status === 'scrap') return 'LOT นี้อยู่ในสถานะ Scrap ไม่สามารถบันทึกเข้าระบบได้ กรุณาส่งกลับ'
    return 'ไม่สามารถบันทึก LOT นี้เข้าระบบได้'
}

// REP product ไม่ต้องตรวจ qty
const isQtyValid = () => {
    return (parseInt(lotQty.value) || 0) > 0
}

const handleSave = async () => {
    if (!isQtyValid()) { showErrorModalFunc('กรุณาระบุจำนวน Lot Qty'); return }
    isLoading.value = true
    try {
        // REP: ส่ง poLot + lotQty  |  ปกติ: ส่ง imobileLot + lotQty
        const body = isRepProduct.value
            ? { poLot: modalData.value?.poLot, lotQty: parseInt(lotQty.value) || 0 }
            : { imobileLot: modalData.value?.imobileLot, lotQty: parseInt(lotQty.value) || 0 }

        const { data: response, error: fetchError } = await useFetch('/api/SI25031/save-lot', {
            method: 'POST',
            baseURL: useRuntimeConfig().public.apiBase,
            body
        })
        if (fetchError.value) {
            showErrorModalFunc(fetchError.value.data?.message || 'เกิดข้อผิดพลาดในการบันทึก')
            closeModal(); isLoading.value = false; return
        }
        if (response.value?.success && response.value?.data) {
            const savedData = response.value.data
            const mcNo = savedData.mcNo || 'REP'
            if (mcNo !== selectedMc.value) {
                await fetchMcList()
                selectedMc.value = mcNo
            }
            await fetchLotData()
            successMessage.value = `✓ เพิ่ม LOT ${savedData.poLot} สำเร็จ${isRepProduct.value ? ' (REP)' : ` (MC: ${mcNo})`}`
            playBeep()
            setTimeout(() => { if (isMounted.value) { successMessage.value = ''; lotNo.value = ''; closeModal(); focusInput() } }, 2000)
        } else {
            showErrorModalFunc(response.value?.message || 'ไม่สามารถบันทึกข้อมูลได้')
            closeModal()
        }
    } catch (err: any) {
        showErrorModalFunc('เกิดข้อผิดพลาดในการบันทึก'); closeModal()
    } finally { isLoading.value = false }
}

const handleSendBack = () => {
    successMessage.value = `LOT ${modalData.value?.poLot} ถูกส่งกลับแล้ว`
    playErrorBeep()
    setTimeout(() => { if (isMounted.value) { successMessage.value = ''; lotNo.value = ''; closeModal(); focusInput() } }, 2000)
}

const playBeep = () => {
    const audioContext = new (window.AudioContext || (window as any).webkitAudioContext)()
    const oscillator = audioContext.createOscillator()
    const gainNode = audioContext.createGain()
    oscillator.connect(gainNode); gainNode.connect(audioContext.destination)
    oscillator.frequency.value = 800; oscillator.type = 'sine'
    gainNode.gain.setValueAtTime(0.3, audioContext.currentTime)
    gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.1)
    oscillator.start(audioContext.currentTime); oscillator.stop(audioContext.currentTime + 0.1)
}

const playErrorBeep = () => {
    const audioContext = new (window.AudioContext || (window as any).webkitAudioContext)()
    const oscillator = audioContext.createOscillator()
    const gainNode = audioContext.createGain()
    oscillator.connect(gainNode); gainNode.connect(audioContext.destination)
    oscillator.frequency.value = 400; oscillator.type = 'sine'
    gainNode.gain.setValueAtTime(0.3, audioContext.currentTime)
    gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.2)
    oscillator.start(audioContext.currentTime); oscillator.stop(audioContext.currentTime + 0.2)
}

const getErrorModalHeaderClass = () => {
    if (errorStatus.value === 'DUPLICATE') return 'error-header-duplicate'
    if (errorStatus.value === 'HOLD') return 'error-header-hold'
    if (errorStatus.value === 'SCRAP') return 'error-header-scrap'
    if (errorData.value.missingLots && errorData.value.missingLots.length > 0) return 'error-header-sequence'
    return 'error-header-default'
}

const getErrorStatusBadgeClass = () => {
    if (errorStatus.value === 'DUPLICATE') return 'status-badge-duplicate'
    if (errorStatus.value === 'HOLD') return 'status-badge-hold'
    if (errorStatus.value === 'SCRAP') return 'status-badge-scrap'
    return 'status-badge-default'
}

const getErrorMessageBoxClass = () => {
    if (errorStatus.value === 'DUPLICATE') return 'error-box-duplicate'
    if (errorStatus.value === 'HOLD') return 'error-box-hold'
    if (errorStatus.value === 'SCRAP') return 'error-box-scrap'
    if (errorData.value.missingLots && errorData.value.missingLots.length > 0) return 'error-box-sequence'
    return 'error-box-default'
}

const getErrorButtonClass = () => {
    if (errorStatus.value === 'DUPLICATE') return 'btn-error-duplicate'
    if (errorStatus.value === 'HOLD') return 'btn-error-hold'
    if (errorStatus.value === 'SCRAP') return 'btn-error-scrap'
    if (errorData.value.missingLots && errorData.value.missingLots.length > 0) return 'btn-error-sequence'
    return 'btn-error-default'
}

const hasMissingLots = () => errorData.value.missingLots && errorData.value.missingLots.length > 0
const hasSkippedLots = () => errorData.value.scrapHoldLots && errorData.value.scrapHoldLots.length > 0

const getErrorTitle = () => {
    if (errorStatus.value === 'DUPLICATE') return '🔁 LOT นี้เคยถูกบันทึกแล้ว'
    if (hasMissingLots()) return '❌ ไม่สามารถเพิ่ม LOT นี้ได้'
    if (errorData.value.requiredLot) return '⚠️ ไม่มี LOT ในระบบ'
    if (errorStatus.value === 'HOLD') return '🔒 LOT อยู่ในสถานะ HOLD'
    if (errorStatus.value === 'SCRAP') return '🗑️ LOT อยู่ในสถานะ SCRAP'
    return '❌ ไม่สามารถบันทึก LOT'
}

const getErrorDescription = () => {
    if (errorStatus.value === 'DUPLICATE') return 'LOT นี้ถูกบันทึกเข้าระบบแล้ว ไม่สามารถเพิ่มซ้ำได้'
    if (hasMissingLots()) return 'ยังมี LOT ก่อนหน้าที่ยังไม่ได้เพิ่ม กรุณาเพิ่ม LOT ตามลำดับก่อน'
    if (errorData.value.requiredLot) return 'ไม่มี LOT ใด ๆ ในระบบ กรุณาเริ่มต้นด้วย LOT แรก'
    if (errorStatus.value === 'HOLD') return 'LOT นี้อยู่ในสถานะ HOLD ไม่สามารถบันทึกเข้าระบบได้'
    if (errorStatus.value === 'SCRAP') return 'LOT นี้อยู่ในสถานะ SCRAP ไม่สามารถบันทึกเข้าระบบได้'
    return errorMessage.value || 'ไม่สามารถบันทึก LOT นี้เข้าระบบได้'
}

onMounted(() => {
    const savedTheme = localStorage.getItem('theme')
    if (savedTheme === 'dark') {
        isDark.value = true
    } else if (savedTheme === null) {
        isDark.value = window.matchMedia('(prefers-color-scheme: dark)').matches
    }
    focusInput()
    fetchMcList()
})

onUnmounted(() => {
    isMounted.value = false
})
</script>

<style scoped>
/* === Page === */
.main-section {
  min-height: 100vh;
  background: linear-gradient(160deg, #e8eaf6 0%, #f3f4fd 40%, #eef0f8 100%);
  transition: background var(--t-slow) var(--ease);
}
.main-section.dark {
  background: linear-gradient(160deg, #0d0e1a 0%, #12142a 40%, #0f1020 100%);
}

/* === Header === */
.main-header {
  background: rgba(255,255,255,0.96);
  backdrop-filter: blur(16px);
  border-bottom: 1px solid rgba(92,107,192,0.2);
  padding: var(--s-3) var(--s-6);
  display: flex; align-items: center; justify-content: space-between;
  position: sticky; top: 0; z-index: 50;
  box-shadow: 0 2px 16px rgba(92,107,192,0.1);
}
.main-section.dark .main-header {
  background: rgba(13,14,26,0.97);
  border-bottom-color: rgba(92,107,192,0.15);
}

.header-logo {
  width: 36px; height: 36px;
  background: linear-gradient(135deg, var(--brand) 0%, var(--brand-dark) 100%);
  border-radius: var(--r-md);
  display: flex; align-items: center; justify-content: center;
  color: #fff; font-size: 18px; font-weight: 700; flex-shrink: 0;
}

.header-title { font-size: var(--fs-md); font-weight: 800; color: var(--brand); letter-spacing: -0.02em; }
.header-subtitle { font-size: var(--fs-xs); color: var(--text-3); }

/* === Lot Input Card === */
.lot-card {
  background: rgba(255,255,255,0.97);
  border-radius: var(--r-xl);
  border: 1.5px solid rgba(92,107,192,0.2);
  box-shadow: 0 4px 24px rgba(92,107,192,0.1), var(--shadow-sm);
  padding: var(--s-6);
  margin-bottom: var(--s-5);
  transition: box-shadow var(--t-mid) var(--ease);
}
.main-section.dark .lot-card {
  background: rgba(18,20,42,0.97);
  border-color: rgba(92,107,192,0.15);
}

.lot-input {
  font-size: var(--fs-2xl) !important;
  font-weight: 800 !important;
  font-family: var(--font-mono) !important;
  text-align: center !important;
  letter-spacing: 0.06em !important;
  border: 2px solid rgba(92,107,192,0.3) !important;
  border-radius: var(--r-lg) !important;
  padding: var(--s-4) !important;
  background: var(--surface) !important;
  color: var(--brand) !important;
  width: 100%; outline: none;
  transition: all var(--t-fast) var(--ease);
}
.lot-input:focus {
  border-color: var(--brand) !important;
  box-shadow: 0 0 0 4px rgba(92,107,192,0.15) !important;
  transform: scale(1.01);
}

.btn-submit-lot {
  padding: var(--s-4) var(--s-8);
  background: linear-gradient(135deg, var(--brand) 0%, var(--brand-dark) 100%);
  color: #fff; border: none; border-radius: var(--r-lg);
  font-size: var(--fs-lg); font-weight: 800; font-family: var(--font);
  cursor: pointer; width: 100%;
  box-shadow: 0 4px 20px rgba(92,107,192,0.4);
  transition: all var(--t-mid) var(--ease);
  display: flex; align-items: center; justify-content: center; gap: var(--s-3);
  letter-spacing: 0.01em;
}
.btn-submit-lot:hover:not(:disabled) { transform: translateY(-2px); box-shadow: 0 8px 28px rgba(92,107,192,0.5); }
.btn-submit-lot:active:not(:disabled) { transform: scale(0.99); }
.btn-submit-lot:disabled { opacity: 0.4; cursor: not-allowed; transform: none; }

/* === MC Selector === */
.mc-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(120px, 1fr)); gap: var(--s-2); }

.mc-btn {
  padding: var(--s-3) var(--s-2);
  border: 1.5px solid var(--border);
  border-radius: var(--r-md);
  background: var(--surface);
  cursor: pointer; font-size: var(--fs-sm); font-weight: 600;
  font-family: var(--font); color: var(--text-1);
  transition: all var(--t-fast) var(--ease); text-align: center;
}
.mc-btn:hover { border-color: var(--brand); background: var(--brand-xlight); color: var(--brand); }
.mc-btn.active { border-color: var(--brand); background: var(--brand-light); color: var(--brand); box-shadow: 0 0 0 3px rgba(92,107,192,0.15); }

/* === Status Cards (OK/NG/HOLD) === */
.status-card {
  border-radius: var(--r-xl); padding: var(--s-8);
  text-align: center; border: 2px solid;
  transition: all var(--t-mid) var(--ease);
  position: relative; overflow: hidden;
}
.status-card::before {
  content: ''; position: absolute; inset: 0; opacity: 0.04;
  background: radial-gradient(circle at 50% 0%, currentColor 0%, transparent 70%);
}

.status-ok-card     { background: var(--success-bg); border-color: var(--success); color: var(--success); }
.status-ng-card     { background: var(--error-bg);   border-color: var(--error);   color: var(--error); }
.status-hold-card   { background: var(--warning-bg); border-color: var(--warning); color: var(--warning); }
.status-scrap-card  { background: #fce4ec;           border-color: #e91e63;         color: #880e4f; }
.status-rescreen-card { background: var(--info-bg);  border-color: var(--info);     color: var(--info); }
.status-unknown-card  { background: var(--surface-2); border-color: var(--border); color: var(--text-3); }

.status-icon { font-size: 56px; line-height: 1; margin-bottom: var(--s-3); display: block; }
.status-text { font-size: var(--fs-3xl); font-weight: 900; letter-spacing: -0.04em; }
.status-sub  { font-size: var(--fs-sm); font-weight: 500; opacity: 0.75; margin-top: var(--s-2); }

/* === LOT Info === */
.lot-info-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(160px, 1fr)); gap: var(--s-3); }

.lot-info-item {
  background: var(--surface); border-radius: var(--r-md); border: 1px solid var(--border-light);
  padding: var(--s-3) var(--s-4);
}
.lot-info-label { font-size: var(--fs-xs); font-weight: 700; color: var(--text-3); text-transform: uppercase; letter-spacing: 0.06em; margin-bottom: var(--s-1); }
.lot-info-value { font-size: var(--fs-sm); font-weight: 700; color: var(--text-1); font-family: var(--font-mono); }

/* === Success Message === */
.success-toast {
  position: fixed; bottom: var(--s-6); right: var(--s-6); z-index: 100;
  background: var(--success); color: #fff;
  padding: var(--s-4) var(--s-6); border-radius: var(--r-lg);
  box-shadow: 0 8px 32px rgba(46,125,50,0.4);
  font-weight: 700; font-size: var(--fs-sm);
  display: flex; align-items: center; gap: var(--s-3);
  animation: slideInRight 0.3s var(--ease);
}
@keyframes slideInRight { from { opacity: 0; transform: translateX(20px); } to { opacity: 1; transform: translateX(0); } }

/* === Modal === */
.confirm-modal-overlay {
  position: fixed; inset: 0; z-index: 200;
  background: rgba(10,11,20,0.6);
  backdrop-filter: blur(6px);
  display: flex; align-items: center; justify-content: center; padding: var(--s-4);
}
.confirm-modal {
  background: var(--surface); border-radius: var(--r-xl);
  box-shadow: var(--shadow-xl);
  padding: var(--s-8); max-width: 480px; width: 100%;
  border: 1px solid var(--border);
  animation: scaleIn 0.25s var(--ease);
}
@keyframes scaleIn { from { opacity: 0; transform: scale(0.95); } to { opacity: 1; transform: scale(1); } }

/* === Nav buttons === */
.btn-nav {
  padding: var(--s-2) var(--s-4); border-radius: var(--r-md);
  border: 1.5px solid rgba(92,107,192,0.3);
  background: transparent; color: var(--brand);
  font-size: var(--fs-sm); font-weight: 600; font-family: var(--font);
  cursor: pointer; transition: all var(--t-fast) var(--ease);
  display: inline-flex; align-items: center; gap: var(--s-2);
}
.btn-nav:hover { background: var(--brand-light); border-color: var(--brand); }

.btn-secondary-f {
  padding: var(--s-3) var(--s-6); border-radius: var(--r-md);
  border: 1.5px solid var(--border);
  background: var(--surface-2); color: var(--text-1);
  font-size: var(--fs-sm); font-weight: 600; font-family: var(--font);
  cursor: pointer; transition: all var(--t-fast) var(--ease);
}
.btn-secondary-f:hover { background: var(--border); }

/* === Misc === */
.scan-count-badge {
  display: inline-flex; align-items: center; gap: var(--s-1);
  padding: 3px var(--s-2); background: var(--brand-light); color: var(--brand);
  border-radius: var(--r-full); font-size: var(--fs-xs); font-weight: 700;
  border: 1px solid rgba(92,107,192,0.2);
}

.empty-state { text-align: center; padding: var(--s-12) var(--s-6); color: var(--text-3); }
.empty-icon  { font-size: 64px; margin-bottom: var(--s-4); opacity: 0.5; }
.empty-text  { font-size: var(--fs-lg); font-weight: 700; color: var(--text-2); margin-bottom: var(--s-2); }

/* === Animations === */
@keyframes pulse-brand {
  0%, 100% { box-shadow: 0 0 0 0 rgba(92,107,192,0); }
  50% { box-shadow: 0 0 0 8px rgba(92,107,192,0.15); }
}
.pulse { animation: pulse-brand 2s infinite; }

/* === Dark Mode === */
.main-section.dark .lot-card { background: rgba(18,20,42,0.97); border-color: rgba(92,107,192,0.15); }
.main-section.dark .mc-btn { background: rgba(18,20,42,0.8); border-color: rgba(92,107,192,0.15); color: var(--text-2); }
.main-section.dark .mc-btn:hover { background: rgba(92,107,192,0.1); border-color: var(--brand); color: var(--brand); }
.main-section.dark .lot-info-item { background: rgba(18,20,42,0.8); border-color: rgba(92,107,192,0.1); }
.main-section.dark .confirm-modal { background: #181a28; border-color: rgba(92,107,192,0.2); }

/* === Responsive === */
@media (max-width: 768px) {
  .lot-input { font-size: var(--fs-xl) !important; }
  .status-icon { font-size: 44px; }
  .status-text { font-size: var(--fs-2xl); }
  .mc-grid { grid-template-columns: repeat(auto-fill, minmax(90px, 1fr)); }
}
</style>