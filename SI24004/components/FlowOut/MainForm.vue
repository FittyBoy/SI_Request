<template>
    <div class="main-section" :class="{ 'dark': isDark }">
        <!-- Dark Mode Toggle -->
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
                        <h1>Flow-out Prevention System</h1>
                        <p class="subtitle">ระบบป้องกัน LOT ซ้ำและงานข้าม LOT</p>
                    </div>
                </div>
                <div class="header-actions">
                    <button class="btn-rescreen" @click="handleGoToRescreen">
                        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none"
                            stroke="currentColor" stroke-width="2">
                            <path d="M9 11l3 3L22 4" />
                            <path d="M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11" />
                        </svg>
                        Rescreen Check
                    </button>
                    <button class="btn-summarise" @click="handleGoToSummary">
                        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none"
                            stroke="currentColor" stroke-width="2">
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
                    <!-- LEFT: Dashboard/Table Section -->
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
                                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24"
                                    fill="none" stroke="currentColor" stroke-width="2">
                                    <rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
                                    <line x1="16" y1="2" x2="16" y2="6" />
                                    <line x1="8" y1="2" x2="8" y2="6" />
                                    <line x1="3" y1="10" x2="21" y2="10" />
                                </svg>
                                {{ currentDate }}
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
                                    <tr v-for="(item, index) in lotRecords" :key="item.id" class="data-row">
                                        <td>
                                            <span :class="['lot-number', item.check !== 'OK' ? 'lot-ng' : '']">
                                                {{ String(index + 1).padStart(2, '0') }}
                                            </span>
                                        </td>
                                        <td class="text-left">
                                            <div class="po-lot-cell">{{ item.poLot }}</div>
                                        </td>
                                        <td>
                                            <span :class="['status-badge', getStatusBadgeClass(item.statusTn)]">
                                                {{ item.statusTn }}
                                            </span>
                                        </td>
                                        <td>
                                            <span :class="['check-badge', item.check === 'OK' ? 'badge-ok' : 'badge-ng']">
                                                <svg v-if="item.check === 'OK'" xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
                                                    <polyline points="20 6 9 17 4 12" />
                                                </svg>
                                                <svg v-else xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
                                                    <line x1="18" y1="6" x2="6" y2="18" />
                                                    <line x1="6" y1="6" x2="18" y2="18" />
                                                </svg>
                                                {{ item.check }}
                                            </span>
                                        </td>
                                        <td>
                                            <span class="qty-text">{{ item.lotQty || '-' }}</span>
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

                        <!-- Stats Footer -->
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

                    <!-- RIGHT: Scan Form Section -->
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
                            <p v-if="lotRecords.length >= 8" class="info-text">
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <circle cx="12" cy="12" r="10" />
                                    <line x1="12" y1="16" x2="12" y2="12" />
                                    <line x1="12" y1="8" x2="12.01" y2="8" />
                                </svg>
                                แสดงเฉพาะ 8 LOT ล่าสุดของ MC {{ selectedMc }} (มี {{ totalLots }} LOT ทั้งหมด)
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
                                    • ถ้ามี LOT แล้ว ต้องเพิ่มตามลำดับต่อเนื่อง
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
                                <div class="info-item">
                                    <label>IMOBILE LOT</label>
                                    <div class="info-value">{{ modalData?.imobileLot }}</div>
                                </div>
                                <div class="info-item">
                                    <label>PO LOT</label>
                                    <div class="info-value">{{ modalData?.poLot }}</div>
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
                                    <span>LOT เหล่านี้เป็น SCRAP/HOLD หรือมีใน Rescreen Check สามารถข้ามได้ไม่ต้องเพิ่ม</span>
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
import { ref, onMounted, computed } from 'vue'

interface LotRecord {
    id: string
    poLot: string
    imobileLot: string
    statusTn: string
    checkSt: boolean
    check: string
    lotQty: number
}

interface ModalData {
    imobileLot: string
    poLot: string
    statusTn: string
    checkSt: boolean
    mcNo: string
    existingQty?: number
    quantity?: number
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
            key: `lots-mc-${selectedMc.value}-${Date.now()}`
        })
        if (!fetchError.value && response.value?.success) {
            lotRecords.value = response.value.data || []
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

const canSaveLot = () => {
    if (!modalData.value) return false
    const status = modalData.value.statusTn?.toLowerCase()
    if (status === 'ok') return true
    if (status === 'rescreen' && modalData.value.checkSt) return true
    return false
}

const showSubStatus = () => { if (!modalData.value) return false; return modalData.value.statusTn?.toLowerCase() === 'rescreen' }
const getSubStatusValue = () => { if (!modalData.value) return ''; return modalData.value.checkSt ? 'OK' : 'Pending' }

const getStatusClass = () => {
    if (!modalData.value) return ''
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

const isQtyValid = () => (parseInt(lotQty.value) || 0) > 0

const handleSave = async () => {
    if (!isQtyValid()) { showErrorModalFunc('กรุณาระบุจำนวน Lot Qty'); return }
    isLoading.value = true
    try {
        const { data: response, error: fetchError } = await useFetch('/api/SI25031/save-lot', {
            method: 'POST',
            baseURL: useRuntimeConfig().public.apiBase,
            body: { imobileLot: modalData.value?.imobileLot, lotQty: parseInt(lotQty.value) || 0 }
        })
        if (fetchError.value) {
            showErrorModalFunc(fetchError.value.data?.message || 'เกิดข้อผิดพลาดในการบันทึก')
            closeModal(); isLoading.value = false; return
        }
        if (response.value?.success && response.value?.data) {
            const savedData = response.value.data
            if (savedData.mcNo && savedData.mcNo !== selectedMc.value) { selectedMc.value = savedData.mcNo; await fetchMcList() }
            await fetchLotData()
            successMessage.value = `✓ เพิ่ม LOT ${savedData.poLot} สำเร็จ (MC: ${savedData.mcNo})`
            playBeep()
            setTimeout(() => { successMessage.value = ''; lotNo.value = ''; closeModal(); focusInput() }, 2000)
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
    setTimeout(() => { successMessage.value = ''; lotNo.value = ''; closeModal(); focusInput() }, 2000)
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
    if (errorStatus.value === 'HOLD') return 'error-header-hold'
    if (errorStatus.value === 'SCRAP') return 'error-header-scrap'
    if (errorData.value.missingLots && errorData.value.missingLots.length > 0) return 'error-header-sequence'
    return 'error-header-default'
}

const getErrorStatusBadgeClass = () => {
    if (errorStatus.value === 'HOLD') return 'status-badge-hold'
    if (errorStatus.value === 'SCRAP') return 'status-badge-scrap'
    return 'status-badge-default'
}

const getErrorMessageBoxClass = () => {
    if (errorStatus.value === 'HOLD') return 'error-box-hold'
    if (errorStatus.value === 'SCRAP') return 'error-box-scrap'
    if (errorData.value.missingLots && errorData.value.missingLots.length > 0) return 'error-box-sequence'
    return 'error-box-default'
}

const getErrorButtonClass = () => {
    if (errorStatus.value === 'HOLD') return 'btn-error-hold'
    if (errorStatus.value === 'SCRAP') return 'btn-error-scrap'
    if (errorData.value.missingLots && errorData.value.missingLots.length > 0) return 'btn-error-sequence'
    return 'btn-error-default'
}

const hasMissingLots = () => errorData.value.missingLots && errorData.value.missingLots.length > 0
const hasSkippedLots = () => errorData.value.scrapHoldLots && errorData.value.scrapHoldLots.length > 0

const getErrorTitle = () => {
    if (hasMissingLots()) return '❌ ไม่สามารถเพิ่ม LOT นี้ได้'
    if (errorData.value.requiredLot) return '⚠️ ไม่มี LOT ในระบบ'
    if (errorStatus.value === 'HOLD') return '🔒 LOT อยู่ในสถานะ HOLD'
    if (errorStatus.value === 'SCRAP') return '🗑️ LOT อยู่ในสถานะ SCRAP'
    return '❌ ไม่สามารถบันทึก LOT'
}

const getErrorDescription = () => {
    if (hasMissingLots()) return 'ยังมี LOT ก่อนหน้าที่ยังไม่ได้เพิ่ม กรุณาเพิ่ม LOT ตามลำดับก่อน'
    if (errorData.value.requiredLot) return 'ไม่มี LOT ใด ๆ ในระบบ กรุณาเริ่มต้นด้วย LOT แรก'
    if (errorStatus.value === 'HOLD') return 'LOT นี้อยู่ในสถานะ HOLD ไม่สามารถบันทึกเข้าระบบได้'
    if (errorStatus.value === 'SCRAP') return 'LOT นี้อยู่ในสถานะ SCRAP ไม่สามารถบันทึกเข้าระบบได้'
    return errorMessage.value || 'ไม่สามารถบันทึก LOT นี้เข้าระบบได้'
}

onMounted(() => {
    // Load saved theme preference
    const savedTheme = localStorage.getItem('theme')
    if (savedTheme === 'dark') {
        isDark.value = true
    } else if (savedTheme === null) {
        // Use system preference if no saved preference
        isDark.value = window.matchMedia('(prefers-color-scheme: dark)').matches
    }
    focusInput()
    fetchMcList()
})
</script>

<style scoped>
/* ===== CSS Variables - Light Mode ===== */
.main-section {
    --bg-gradient-start: #667eea;
    --bg-gradient-end: #764ba2;
    --bg-page: linear-gradient(135deg, #667eea 0%, #764ba2 100%);

    --surface-primary: #ffffff;
    --surface-secondary: #f8fafc;
    --surface-tertiary: #f1f5f9;
    --surface-hover: #f8fafc;

    --border-primary: #e2e8f0;
    --border-secondary: #cbd5e1;

    --text-primary: #0f172a;
    --text-secondary: #1e293b;
    --text-muted: #475569;
    --text-subtle: #64748b;
    --text-placeholder: #94a3b8;

    --header-bg: rgba(255, 255, 255, 0.95);
    --header-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);

    --input-bg: #ffffff;
    --input-border: #e2e8f0;
    --input-focus-border: #667eea;
    --input-focus-shadow: rgba(102, 126, 234, 0.1);

    --stat-card-bg: #ffffff;
    --stat-card-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
    --stat-card-shadow-hover: 0 8px 16px rgba(0, 0, 0, 0.08);

    --modal-bg: #ffffff;
    --modal-footer-bg: #f8fafc;
    --modal-overlay: rgba(0, 0, 0, 0.7);
    --modal-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);

    --empty-state-color: #94a3b8;

    --info-box-bg: #eff6ff;
    --info-box-border: #3b82f6;
    --info-box-color: #1e40af;

    --alert-success-bg: #dcfce7;
    --alert-success-border: #86efac;
    --alert-success-color: #166534;

    --badge-ok-bg: #dcfce7;
    --badge-ok-color: #166534;
    --badge-ng-bg: #fee2e2;
    --badge-ng-color: #991b1b;

    --status-ok-bg: linear-gradient(135deg, #dcfce7 0%, #bbf7d0 100%);
    --status-ok-border: #86efac;
    --status-ok-color: #166534;

    --status-hold-bg: #fee2e2;
    --status-hold-border: #fca5a5;
    --status-hold-color: #991b1b;

    --status-scrap-bg: #f3f4f6;
    --status-scrap-border: #d1d5db;
    --status-scrap-color: #374151;

    --status-rescreen-bg: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
    --status-rescreen-border: #fcd34d;
    --status-rescreen-color: #92400e;

    --substatus-ok-bg: #d1fae5;
    --substatus-ok-border: #6ee7b7;
    --substatus-ok-color: #065f46;

    --substatus-pending-bg: #fed7aa;
    --substatus-pending-border: #fdba74;
    --substatus-pending-color: #9a3412;

    --warning-bg: #fef2f2;
    --warning-border: #fca5a5;
    --warning-color: #991b1b;

    --lots-missing-bg: linear-gradient(135deg, #fef2f2 0%, #fee2e2 100%);
    --lots-missing-border: #fca5a5;
    --lots-skipped-bg: linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%);
    --lots-skipped-border: #4ade80;

    --lot-item-missing-bg: #ffffff;
    --lot-item-missing-color: #991b1b;
    --lot-item-missing-border: #fecaca;
    --lot-item-skipped-bg: #ffffff;
    --lot-item-skipped-color: #166534;
    --lot-item-skipped-border: #86efac;

    --success-notice-bg: #ffffff;
    --success-notice-border: #86efac;
    --success-notice-color: #166534;

    --theme-toggle-bg: rgba(255, 255, 255, 0.2);
    --theme-toggle-color: white;
    --theme-toggle-hover: rgba(255, 255, 255, 0.35);

    min-height: 100vh;
    background: var(--bg-page);
    position: relative;
    transition: background 0.3s ease;
}

/* ===== CSS Variables - Dark Mode ===== */
.main-section.dark {
    --bg-page: linear-gradient(135deg, #1a1f3c 0%, #2d1b69 100%);

    --surface-primary: #1e2235;
    --surface-secondary: #252a40;
    --surface-tertiary: #2a2f47;
    --surface-hover: #2a2f47;

    --border-primary: #3a4060;
    --border-secondary: #4a5080;

    --text-primary: #f1f5f9;
    --text-secondary: #e2e8f0;
    --text-muted: #94a3b8;
    --text-subtle: #64748b;
    --text-placeholder: #475569;

    --header-bg: rgba(26, 31, 60, 0.97);
    --header-shadow: 0 4px 20px rgba(0, 0, 0, 0.4);

    --input-bg: #252a40;
    --input-border: #3a4060;
    --input-focus-border: #818cf8;
    --input-focus-shadow: rgba(129, 140, 248, 0.15);

    --stat-card-bg: #252a40;
    --stat-card-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
    --stat-card-shadow-hover: 0 8px 16px rgba(0, 0, 0, 0.35);

    --modal-bg: #1e2235;
    --modal-footer-bg: #252a40;
    --modal-overlay: rgba(0, 0, 0, 0.85);
    --modal-shadow: 0 20px 60px rgba(0, 0, 0, 0.6);

    --empty-state-color: #475569;

    --info-box-bg: rgba(59, 130, 246, 0.1);
    --info-box-border: #3b82f6;
    --info-box-color: #93c5fd;

    --alert-success-bg: rgba(34, 197, 94, 0.15);
    --alert-success-border: #166534;
    --alert-success-color: #86efac;

    --badge-ok-bg: rgba(34, 197, 94, 0.2);
    --badge-ok-color: #86efac;
    --badge-ng-bg: rgba(239, 68, 68, 0.2);
    --badge-ng-color: #fca5a5;

    --status-ok-bg: rgba(34, 197, 94, 0.15);
    --status-ok-border: #166534;
    --status-ok-color: #86efac;

    --status-hold-bg: rgba(239, 68, 68, 0.15);
    --status-hold-border: #991b1b;
    --status-hold-color: #fca5a5;

    --status-scrap-bg: rgba(100, 116, 139, 0.15);
    --status-scrap-border: #475569;
    --status-scrap-color: #94a3b8;

    --status-rescreen-bg: rgba(245, 158, 11, 0.15);
    --status-rescreen-border: #92400e;
    --status-rescreen-color: #fcd34d;

    --substatus-ok-bg: rgba(16, 185, 129, 0.15);
    --substatus-ok-border: #065f46;
    --substatus-ok-color: #6ee7b7;

    --substatus-pending-bg: rgba(251, 146, 60, 0.15);
    --substatus-pending-border: #9a3412;
    --substatus-pending-color: #fdba74;

    --warning-bg: rgba(239, 68, 68, 0.1);
    --warning-border: #991b1b;
    --warning-color: #fca5a5;

    --lots-missing-bg: rgba(239, 68, 68, 0.1);
    --lots-missing-border: #991b1b;
    --lots-skipped-bg: rgba(34, 197, 94, 0.1);
    --lots-skipped-border: #166534;

    --lot-item-missing-bg: rgba(239, 68, 68, 0.08);
    --lot-item-missing-color: #fca5a5;
    --lot-item-missing-border: #991b1b;
    --lot-item-skipped-bg: rgba(34, 197, 94, 0.08);
    --lot-item-skipped-color: #86efac;
    --lot-item-skipped-border: #166534;

    --success-notice-bg: rgba(34, 197, 94, 0.08);
    --success-notice-border: #166534;
    --success-notice-color: #86efac;

    --theme-toggle-bg: rgba(255, 255, 255, 0.15);
    --theme-toggle-color: #e2e8f0;
    --theme-toggle-hover: rgba(255, 255, 255, 0.25);
}

/* ===== Dark Mode Toggle Button ===== */
.theme-toggle {
    position: fixed;
    bottom: 24px;
    right: 24px;
    z-index: 1000;
    width: 48px;
    height: 48px;
    border-radius: 50%;
    border: none;
    background: var(--theme-toggle-bg);
    color: var(--theme-toggle-color);
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    backdrop-filter: blur(10px);
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.2);
    transition: all 0.3s ease;
}

.theme-toggle:hover {
    background: var(--theme-toggle-hover);
    transform: scale(1.1);
    box-shadow: 0 6px 20px rgba(0, 0, 0, 0.3);
}

/* ===== Header ===== */
.header {
    background: var(--header-bg);
    backdrop-filter: blur(10px);
    box-shadow: var(--header-shadow);
    position: sticky;
    top: 0;
    z-index: 100;
    transition: background 0.3s ease, box-shadow 0.3s ease;
}

.header-content {
    max-width: 1600px;
    margin: 0 auto;
    padding: 24px 40px;
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
    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
    flex-shrink: 0;
}

.title-section h1 {
    font-size: 28px;
    font-weight: 700;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    margin: 0;
}

.subtitle {
    font-size: 14px;
    color: var(--text-subtle);
    margin: 4px 0 0 0;
    transition: color 0.3s;
}

.header-actions {
    display: flex;
    gap: 12px;
    align-items: center;
}

.btn-rescreen {
    background: linear-gradient(135deg, #fbbf24 0%, #f59e0b 100%);
    color: white;
    border: none;
    padding: 12px 24px;
    border-radius: 12px;
    cursor: pointer;
    font-size: 16px;
    font-weight: 600;
    display: flex;
    align-items: center;
    gap: 8px;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    box-shadow: 0 4px 12px rgba(251, 191, 36, 0.3);
}

.btn-rescreen:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 20px rgba(251, 191, 36, 0.4);
}

.btn-summarise {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    border: none;
    padding: 12px 32px;
    border-radius: 12px;
    cursor: pointer;
    font-size: 16px;
    font-weight: 600;
    display: flex;
    align-items: center;
    gap: 8px;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
}

.btn-summarise:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 20px rgba(102, 126, 234, 0.4);
}

.content {
    max-width: 1600px;
    margin: 0 auto;
    padding: 32px 40px;
}

.layout-grid {
    display: grid;
    grid-template-columns: 1fr 500px;
    gap: 24px;
}

/* ===== Cards ===== */
.card {
    background: var(--surface-primary);
    border-radius: 16px;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
    overflow: hidden;
    transition: transform 0.3s, box-shadow 0.3s, background 0.3s;
    border: 1px solid var(--border-primary);
}

.dark .card {
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
}

.card:hover {
    transform: translateY(-2px);
    box-shadow: 0 12px 40px rgba(0, 0, 0, 0.12);
}

.dark .card:hover {
    box-shadow: 0 12px 40px rgba(0, 0, 0, 0.4);
}

.card-header {
    padding: 24px 32px;
    background: var(--surface-secondary);
    border-bottom: 1px solid var(--border-primary);
    transition: background 0.3s, border-color 0.3s;
}

.card-header h2 {
    font-size: 20px;
    font-weight: 600;
    color: var(--text-primary);
    display: flex;
    align-items: center;
    gap: 12px;
    margin: 0;
    transition: color 0.3s;
}

.form-section {
    padding: 32px;
}

/* ===== Form Elements ===== */
.input-group {
    margin-bottom: 24px;
}

.input-group label {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 10px;
    font-weight: 600;
    color: var(--text-secondary);
    font-size: 15px;
    transition: color 0.3s;
}

.input-wrapper {
    position: relative;
    display: flex;
    gap: 8px;
}

.input-modern {
    width: 100%;
    padding: 14px 16px;
    border: 2px solid var(--input-border);
    border-radius: 12px;
    font-size: 16px;
    background: var(--input-bg);
    color: var(--text-primary);
    transition: all 0.3s;
    font-family: inherit;
}

.input-modern::placeholder {
    color: var(--text-placeholder);
}

.input-modern:focus {
    outline: none;
    border-color: var(--input-focus-border);
    box-shadow: 0 0 0 4px var(--input-focus-shadow);
}

.info-box {
    display: flex;
    gap: 12px;
    padding: 16px;
    background: var(--info-box-bg);
    border-left: 4px solid var(--info-box-border);
    border-radius: 12px;
    color: var(--info-box-color);
    margin-top: 8px;
    transition: background 0.3s, color 0.3s;
}

.info-text {
    display: flex;
    align-items: center;
    gap: 8px;
    color: #3b82f6;
    font-size: 14px;
    margin-top: 8px;
    font-weight: 500;
}

.dark .info-text {
    color: #93c5fd;
}

.alert {
    padding: 16px 20px;
    border-radius: 12px;
    margin-bottom: 20px;
    display: flex;
    align-items: center;
    gap: 12px;
    font-weight: 500;
    animation: slideDown 0.3s ease-out;
}

.alert-success {
    background: var(--alert-success-bg);
    border: 1px solid var(--alert-success-border);
    color: var(--alert-success-color);
}

/* ===== MC Header ===== */
.mc-header {
    padding: 20px 32px;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    display: flex;
    justify-content: space-between;
    align-items: center;
    color: white;
}

.mc-info {
    display: flex;
    align-items: center;
    gap: 12px;
}

.mc-label {
    font-size: 16px;
    opacity: 0.9;
}

.mc-select {
    appearance: none;
    background: rgba(255, 255, 255, 0.2);
    border: 2px solid rgba(255, 255, 255, 0.3);
    color: white;
    padding: 12px 48px 12px 20px;
    border-radius: 12px;
    font-size: 24px;
    font-weight: 700;
    cursor: pointer;
    transition: all 0.3s;
    min-width: 150px;
    text-align: center;
    outline: none;
}

.mc-select:hover {
    background: rgba(255, 255, 255, 0.25);
    border-color: rgba(255, 255, 255, 0.4);
}

.mc-select option {
    background: #667eea;
    color: white;
}

.date-info {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 16px;
    opacity: 0.95;
}

/* ===== Table ===== */
.lot-table {
    width: 100%;
    border-collapse: collapse;
}

.lot-table thead {
    background: var(--surface-secondary);
    transition: background 0.3s;
}

.lot-table th {
    padding: 16px;
    text-align: center;
    font-weight: 600;
    color: var(--text-muted);
    font-size: 14px;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    border-bottom: 2px solid var(--border-primary);
    transition: color 0.3s, border-color 0.3s;
}

.lot-table .text-left { text-align: left; }

.lot-table td {
    padding: 16px;
    text-align: center;
    border-bottom: 1px solid var(--border-primary);
    transition: border-color 0.3s;
}

.data-row:hover { background: var(--surface-hover); transition: background 0.2s; }

.empty-state {
    text-align: center;
    padding: 60px 20px;
}

.empty-state-content {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 16px;
    color: var(--empty-state-color);
    transition: color 0.3s;
}

.lot-number {
    font-weight: 700;
    font-size: 18px;
    color: var(--text-primary);
    transition: color 0.3s;
}

.lot-number.lot-ng { color: #dc2626; }
.dark .lot-number.lot-ng { color: #f87171; }

.po-lot-cell {
    font-family: 'Courier New', monospace;
    font-size: 14px;
    color: var(--text-secondary);
    transition: color 0.3s;
}

/* ===== Status Badges ===== */
.status-badge {
    display: inline-flex;
    align-items: center;
    padding: 6px 12px;
    border-radius: 20px;
    font-weight: 600;
    font-size: 13px;
    text-transform: uppercase;
}

.status-badge-ok {
    background: var(--badge-ok-bg);
    color: var(--badge-ok-color);
    border: 1px solid var(--alert-success-border);
    transition: background 0.3s, color 0.3s;
}

.status-badge-hold {
    background: linear-gradient(135deg, #dbeafe 0%, #bfdbfe 100%);
    color: #1e40af;
    border: 1px solid #93c5fd;
}

.dark .status-badge-hold {
    background: rgba(59, 130, 246, 0.2);
    color: #93c5fd;
    border: 1px solid #1e40af;
}

.status-badge-scrap {
    background: var(--badge-ng-bg);
    color: var(--badge-ng-color);
    border: 1px solid;
    transition: background 0.3s, color 0.3s;
}

.status-badge-rescreen {
    background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
    color: #92400e;
    border: 1px solid #fcd34d;
}

.dark .status-badge-rescreen {
    background: rgba(245, 158, 11, 0.2);
    color: #fcd34d;
    border: 1px solid #92400e;
}

.status-badge-default {
    background: var(--surface-secondary);
    color: var(--text-muted);
    border: 1px solid var(--border-primary);
    transition: background 0.3s, color 0.3s, border-color 0.3s;
}

.qty-text {
    font-size: 14px;
    color: var(--text-secondary);
    font-weight: 600;
    transition: color 0.3s;
}

.check-badge {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    padding: 6px 16px;
    border-radius: 20px;
    font-weight: 600;
    font-size: 14px;
    transition: background 0.3s, color 0.3s;
}

.badge-ok {
    background: var(--badge-ok-bg);
    color: var(--badge-ok-color);
}

.badge-ng {
    background: var(--badge-ng-bg);
    color: var(--badge-ng-color);
}

/* ===== Stats Section ===== */
.stats-section {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 20px;
    padding: 24px 32px;
    background: var(--surface-secondary);
    border-top: 1px solid var(--border-primary);
    transition: background 0.3s, border-color 0.3s;
}

.stat-card {
    display: flex;
    align-items: center;
    gap: 16px;
    padding: 20px;
    background: var(--stat-card-bg);
    border-radius: 12px;
    box-shadow: var(--stat-card-shadow);
    border: 1px solid var(--border-primary);
    transition: all 0.3s;
}

.stat-card:hover {
    transform: translateY(-4px);
    box-shadow: var(--stat-card-shadow-hover);
}

.stat-icon {
    width: 48px;
    height: 48px;
    border-radius: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
}

.stat-icon.total { background: linear-gradient(135deg, #60a5fa 0%, #3b82f6 100%); color: white; }
.stat-icon.ok { background: linear-gradient(135deg, #86efac 0%, #22c55e 100%); color: white; }
.stat-icon.ng { background: linear-gradient(135deg, #fca5a5 0%, #ef4444 100%); color: white; }

.stat-content {
    display: flex;
    flex-direction: column;
    gap: 4px;
}

.stat-label {
    font-size: 13px;
    color: var(--text-subtle);
    font-weight: 500;
    transition: color 0.3s;
}

.stat-value {
    font-size: 28px;
    font-weight: 700;
    color: var(--text-primary);
    transition: color 0.3s;
}

/* ===== Modal Styles ===== */
.modal-overlay {
    position: fixed;
    inset: 0;
    background: var(--modal-overlay);
    backdrop-filter: blur(8px);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 9999;
    padding: 20px;
}

.modal-container {
    background: var(--modal-bg);
    border-radius: 20px;
    box-shadow: var(--modal-shadow);
    max-width: 700px;
    width: 100%;
    max-height: 90vh;
    overflow: hidden;
    display: flex;
    flex-direction: column;
    animation: modalSlideUp 0.3s ease-out;
    border: 1px solid var(--border-primary);
    transition: background 0.3s, border-color 0.3s;
}

.modal-header {
    padding: 28px 32px;
    display: flex;
    align-items: center;
    gap: 16px;
    position: relative;
    color: white;
}

.modal-header.header-ok { background: linear-gradient(135deg, #86efac 0%, #22c55e 100%); }
.modal-header.header-rescreen-ok { background: linear-gradient(135deg, #fbbf24 0%, #f59e0b 100%); }
.modal-header.header-rescreen-pending { background: linear-gradient(135deg, #fb923c 0%, #ea580c 100%); }
.modal-header.header-hold { background: linear-gradient(135deg, #60a5fa 0%, #3b82f6 100%); }
.modal-header.header-scrap { background: linear-gradient(135deg, #f87171 0%, #dc2626 100%); }

.modal-icon {
    width: 56px;
    height: 56px;
    background: rgba(255, 255, 255, 0.2);
    border-radius: 16px;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
}

.modal-header h3 {
    flex: 1;
    font-size: 24px;
    font-weight: 700;
    margin: 0;
}

.modal-close {
    background: rgba(255, 255, 255, 0.2);
    border: none;
    border-radius: 10px;
    padding: 8px;
    cursor: pointer;
    color: white;
    transition: all 0.3s;
    display: flex;
}

.modal-close:hover {
    background: rgba(255, 255, 255, 0.3);
    transform: scale(1.1);
}

.modal-body {
    padding: 32px;
    overflow-y: auto;
    flex: 1;
}

.lot-info-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 20px;
    margin-bottom: 24px;
}

.info-item label {
    font-size: 13px;
    font-weight: 600;
    color: var(--text-subtle);
    text-transform: uppercase;
    letter-spacing: 0.5px;
    transition: color 0.3s;
}

.info-value {
    font-size: 18px;
    font-weight: 700;
    color: var(--text-primary);
    font-family: 'Courier New', monospace;
    padding: 12px 16px;
    background: var(--surface-secondary);
    border-radius: 10px;
    border: 2px solid var(--border-primary);
    margin-top: 8px;
    transition: all 0.3s;
}

.status-display {
    margin-bottom: 24px;
    display: flex;
    flex-direction: column;
    gap: 12px;
}

.status-main, .status-sub {
    padding: 20px 24px;
    border-radius: 16px;
    display: flex;
    align-items: center;
    gap: 16px;
    font-weight: 600;
    border: 2px solid transparent;
    transition: all 0.3s;
}

.status-icon-wrapper {
    width: 48px;
    height: 48px;
    border-radius: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    background: rgba(255, 255, 255, 0.2);
}

.status-text {
    display: flex;
    flex-direction: column;
    gap: 4px;
    flex: 1;
}

.status-main.status-ok {
    background: var(--status-ok-bg);
    border-color: var(--status-ok-border);
    color: var(--status-ok-color);
}

.status-main.status-ok .status-icon-wrapper {
    background: linear-gradient(135deg, #86efac 0%, #22c55e 100%);
    color: white;
}

.status-main.status-rescreen {
    background: var(--status-rescreen-bg);
    border-color: var(--status-rescreen-border);
    color: var(--status-rescreen-color);
}

.status-main.status-hold {
    background: var(--status-hold-bg);
    border: 2px solid var(--status-hold-border);
    color: var(--status-hold-color);
}

.status-main.status-scrap {
    background: var(--status-scrap-bg);
    border: 2px solid var(--status-scrap-border);
    color: var(--status-scrap-color);
}

.status-sub.substatus-ok {
    background: var(--substatus-ok-bg);
    border: 2px solid var(--substatus-ok-border);
    color: var(--substatus-ok-color);
}

.status-sub.substatus-pending {
    background: var(--substatus-pending-bg);
    border: 2px solid var(--substatus-pending-border);
    color: var(--substatus-pending-color);
}

.status-label {
    font-size: 14px;
    opacity: 0.8;
}

.status-value { font-size: 18px; }

.qty-section h4 {
    font-size: 18px;
    font-weight: 700;
    color: var(--text-primary);
    margin: 0 0 16px 0;
    transition: color 0.3s;
}

.qty-input-single {
    width: 100%;
    padding: 16px 20px;
    border: 2px solid var(--input-border);
    border-radius: 12px;
    font-size: 24px;
    font-weight: 600;
    text-align: center;
    background: var(--input-bg);
    color: var(--text-primary);
    transition: all 0.3s;
}

.qty-input-single:focus {
    outline: none;
    border-color: var(--input-focus-border);
    box-shadow: 0 0 0 4px var(--input-focus-shadow);
}

.warning-box {
    display: flex;
    gap: 16px;
    padding: 20px;
    background: var(--warning-bg);
    border: 2px solid var(--warning-border);
    border-radius: 12px;
    color: var(--warning-color);
    transition: all 0.3s;
}

.warning-box svg { flex-shrink: 0; }

.warning-box strong {
    display: block;
    font-size: 16px;
    margin-bottom: 6px;
}

.warning-box p {
    margin: 0;
    font-size: 14px;
    line-height: 1.6;
}

.modal-footer {
    padding: 24px 32px;
    background: var(--modal-footer-bg);
    border-top: 1px solid var(--border-primary);
    display: flex;
    gap: 12px;
    justify-content: flex-end;
    transition: background 0.3s, border-color 0.3s;
}

/* ===== Buttons ===== */
.btn {
    padding: 14px 28px;
    border: none;
    border-radius: 12px;
    font-size: 16px;
    font-weight: 600;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 8px;
    transition: all 0.3s;
}

.btn-save { background: linear-gradient(135deg, #86efac 0%, #22c55e 100%); color: white; }
.btn-save:hover:not(:disabled) { transform: translateY(-2px); box-shadow: 0 8px 20px rgba(34, 197, 94, 0.4); }
.btn-save:disabled { opacity: 0.5; cursor: not-allowed; }

.btn-sendback { background: linear-gradient(135deg, #f87171 0%, #dc2626 100%); color: white; }
.btn-sendback:hover { transform: translateY(-2px); box-shadow: 0 8px 20px rgba(220, 38, 38, 0.4); }

.btn-cancel {
    background: var(--surface-tertiary);
    color: var(--text-muted);
    transition: background 0.3s, color 0.3s;
}

.btn-cancel:hover { background: var(--border-secondary); }

/* ===== Error Modal Styles ===== */
.error-modal-container {
    background: var(--modal-bg);
    border-radius: 20px;
    box-shadow: var(--modal-shadow);
    max-width: 500px;
    width: 100%;
    overflow: hidden;
    animation: modalSlideUp 0.3s ease-out;
    border: 1px solid var(--border-primary);
    transition: background 0.3s, border-color 0.3s;
}

.error-modal-header {
    padding: 32px;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 16px;
    position: relative;
    color: white;
}

.error-modal-header.error-header-hold { background: linear-gradient(135deg, #60a5fa 0%, #3b82f6 100%); }
.error-modal-header.error-header-scrap { background: linear-gradient(135deg, #f87171 0%, #dc2626 100%); }
.error-modal-header.error-header-sequence { background: linear-gradient(135deg, #fb923c 0%, #ea580c 100%); }
.error-modal-header.error-header-default { background: linear-gradient(135deg, #fb923c 0%, #ea580c 100%); }

.error-modal-icon {
    width: 80px;
    height: 80px;
    background: rgba(255, 255, 255, 0.2);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
}

.error-modal-header h3 {
    font-size: 24px;
    font-weight: 700;
    margin: 0;
}

.modal-close-error {
    position: absolute;
    top: 16px;
    right: 16px;
    background: rgba(255, 255, 255, 0.2);
    border: none;
    border-radius: 10px;
    padding: 8px;
    cursor: pointer;
    color: white;
    transition: all 0.3s;
    display: flex;
}

.modal-close-error:hover {
    background: rgba(255, 255, 255, 0.3);
    transform: scale(1.1);
}

.error-modal-body {
    padding: 24px;
    overflow-y: auto;
    flex: 1;
}

.error-status-badge {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    padding: 8px 16px;
    border-radius: 8px;
    font-weight: 600;
    font-size: 13px;
    text-transform: uppercase;
    margin-bottom: 16px;
}

.error-status-badge.status-badge-hold {
    background: rgba(59, 130, 246, 0.15);
    color: #60a5fa;
    border: 2px solid #1e40af;
}

.error-status-badge.status-badge-scrap {
    background: var(--badge-ng-bg);
    color: var(--badge-ng-color);
    border: 2px solid;
}

.error-status-badge.status-badge-default {
    background: rgba(251, 146, 60, 0.15);
    color: #fb923c;
    border: 2px solid #9a3412;
}

.info-box-modern {
    padding: 16px 20px;
    border-radius: 12px;
    margin-bottom: 16px;
    border: 2px solid;
}

.info-box-modern.current {
    background: rgba(59, 130, 246, 0.1);
    border-color: #60a5fa;
    color: var(--text-primary);
}

.info-box-modern.required {
    background: rgba(245, 158, 11, 0.1);
    border-color: #fbbf24;
    color: var(--text-primary);
}

.info-label {
    font-size: 13px;
    font-weight: 600;
    color: var(--text-subtle);
    margin-bottom: 8px;
    transition: color 0.3s;
}

.error-message-modern {
    display: flex;
    gap: 16px;
    padding: 20px;
    border-radius: 12px;
    border: 2px solid;
    margin-bottom: 20px;
    align-items: flex-start;
}

.error-message-modern.error-box-hold {
    background: rgba(59, 130, 246, 0.1);
    border-color: #1e40af;
    color: #93c5fd;
}

.error-message-modern.error-box-scrap {
    background: var(--badge-ng-bg);
    border-color: var(--badge-ng-color);
    color: var(--badge-ng-color);
}

.error-message-modern.error-box-sequence {
    background: rgba(251, 146, 60, 0.1);
    border-color: #fb923c;
    color: #fdba74;
}

.error-message-modern.error-box-default {
    background: rgba(251, 146, 60, 0.1);
    border-color: #fb923c;
    color: #fdba74;
}

.error-message-modern svg { flex-shrink: 0; margin-top: 2px; }

.error-message-modern strong {
    display: block;
    font-size: 16px;
    margin-bottom: 6px;
}

.error-message-modern p {
    margin: 0;
    font-size: 14px;
    line-height: 1.6;
}

.lots-section {
    margin-bottom: 16px;
    padding: 16px;
    border-radius: 12px;
    border: 2px solid;
}

.lots-section.missing {
    background: var(--lots-missing-bg);
    border-color: var(--lots-missing-border);
}

.lots-section.skipped {
    background: var(--lots-skipped-bg);
    border-color: var(--lots-skipped-border);
}

.lots-header {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 12px;
    font-size: 15px;
    color: var(--badge-ng-color);
}

.lots-header.success { color: var(--badge-ok-color); }

.success-notice {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 12px 16px;
    background: var(--success-notice-bg);
    border: 2px solid var(--success-notice-border);
    border-radius: 8px;
    margin-bottom: 12px;
    color: var(--success-notice-color);
    font-size: 13px;
    font-weight: 500;
    transition: all 0.3s;
}

.lots-list {
    list-style: none;
    padding: 0;
    margin: 0;
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.lot-item {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 10px 12px;
    border-radius: 8px;
    font-family: 'Courier New', monospace;
    font-size: 14px;
    font-weight: 600;
    transition: all 0.2s;
    border: 2px solid;
}

.lot-item.missing {
    background: var(--lot-item-missing-bg);
    color: var(--lot-item-missing-color);
    border-color: var(--lot-item-missing-border);
}

.lot-item.missing:hover { transform: translateX(4px); }

.lot-item.skipped {
    background: var(--lot-item-skipped-bg);
    color: var(--lot-item-skipped-color);
    border-color: var(--lot-item-skipped-border);
}

.lot-item.skipped:hover { transform: translateX(4px); }

.error-modal-footer {
    padding: 24px 32px;
    background: var(--modal-footer-bg);
    border-top: 1px solid var(--border-primary);
    display: flex;
    justify-content: center;
    transition: background 0.3s, border-color 0.3s;
}

.btn-error-ok {
    padding: 14px 48px;
    border: none;
    border-radius: 12px;
    font-size: 16px;
    font-weight: 600;
    cursor: pointer;
    color: white;
    transition: all 0.3s;
}

.btn-error-ok.btn-error-hold { background: linear-gradient(135deg, #60a5fa 0%, #3b82f6 100%); }
.btn-error-ok.btn-error-hold:hover { transform: translateY(-2px); box-shadow: 0 8px 20px rgba(59, 130, 246, 0.4); }
.btn-error-ok.btn-error-scrap { background: linear-gradient(135deg, #f87171 0%, #dc2626 100%); }
.btn-error-ok.btn-error-scrap:hover { transform: translateY(-2px); box-shadow: 0 8px 20px rgba(220, 38, 38, 0.4); }
.btn-error-ok.btn-error-sequence { background: linear-gradient(135deg, #fb923c 0%, #ea580c 100%); }
.btn-error-ok.btn-error-sequence:hover { transform: translateY(-2px); box-shadow: 0 8px 20px rgba(234, 88, 12, 0.4); }
.btn-error-ok.btn-error-default { background: linear-gradient(135deg, #fb923c 0%, #ea580c 100%); }
.btn-error-ok.btn-error-default:hover { transform: translateY(-2px); box-shadow: 0 8px 20px rgba(234, 88, 12, 0.4); }

/* ===== Loading ===== */
.loading-overlay {
    position: fixed;
    inset: 0;
    background: var(--modal-overlay);
    backdrop-filter: blur(4px);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 9999;
}

.loading-spinner {
    text-align: center;
    color: white;
}

.spinner {
    width: 64px;
    height: 64px;
    margin: 0 auto 20px;
    border: 4px solid rgba(255, 255, 255, 0.2);
    border-top-color: white;
    border-radius: 50%;
    animation: spin 0.8s linear infinite;
}

/* ===== Animations ===== */
@keyframes spin { to { transform: rotate(360deg); } }

@keyframes slideDown {
    from { opacity: 0; transform: translateY(-20px); }
    to { opacity: 1; transform: translateY(0); }
}

@keyframes modalSlideUp {
    from { opacity: 0; transform: translateY(30px) scale(0.95); }
    to { opacity: 1; transform: translateY(0) scale(1); }
}

/* ===== Transitions ===== */
.fade-enter-active, .fade-leave-active { transition: opacity 0.3s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
.modal-enter-active, .modal-leave-active { transition: opacity 0.3s; }
.modal-enter-from, .modal-leave-to { opacity: 0; }

/* ===== Responsive ===== */
@media (max-width: 1200px) {
    .layout-grid { grid-template-columns: 1fr; }
}

@media (max-width: 768px) {
    .header-content { flex-direction: column; gap: 16px; }
    .header-actions { width: 100%; justify-content: center; }
    .lot-info-grid { grid-template-columns: 1fr; }
    .stats-section { grid-template-columns: 1fr; }
    .theme-toggle { bottom: 16px; right: 16px; }
}

/* ===== System Dark Mode Support (auto-detect) ===== */
@media (prefers-color-scheme: dark) {
    .main-section:not(.dark):not([data-theme="light"]) {
        /* Can be enabled if you want automatic OS-level dark mode without toggle */
        /* Currently handled via JS to give user control */
    }
}
</style>