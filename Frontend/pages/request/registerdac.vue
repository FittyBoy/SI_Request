<script setup lang="ts">
// ========================================
// IMPORTS
// ========================================
import { ref, onMounted, computed, nextTick } from 'vue';
import { v4 as uuidv4 } from 'uuid';
import Swal from 'sweetalert2';
import type { DwRequestDTO } from '@/types/dwRequest';
import AddEditDrawingControl from '@/components/dialogs/AddEditDrawingControl.vue';

// ========================================
// ROUTER & AUTH
// ========================================
const Router = useRouter();
const Token = useCookie('token');
const UserId = useCookie('userId');
const UserName = useCookie('userName');
const Role = useCookie('roleName');

// ========================================
// REACTIVE REFS
// ========================================
const Statuses = ref<{ Id: string | null; StatusName: string }[]>([]);
const IsLoading = ref(true);
const IsRequestDialogVisible = ref(false);
const Search = ref('');
const Page = ref(1);
const ItemsPerPage = ref(10);
const SelectedStatusId = ref(null);
const SelectedItem = ref<DwRequestDTO | null>(null);

// ========================================
// CONSTANTS
// ========================================
const Config = useRuntimeConfig ? useRuntimeConfig() : { public: { apiBase: '/api' } };
const SwalInstance = Swal;

const Headers = [
    { title: 'ลำดับ', key: 'No', width: '80px', sortable: false },
    { title: 'รหัสคำขอ', key: 'RequestCode', width: '140px' },
    { title: 'รหัสแบบ', key: 'DrawingCode', width: '140px' },
    { title: 'ชื่อแบบ', key: 'DrawingName', width: '250px' },
    { title: 'วันที่อัพเดต', key: 'UpdateDate', width: '140px' },
    { title: 'สถานะ', key: 'Status.StatusName', width: '140px' },
    { title: 'สถานะการใช้งาน', key: 'Active', width: '160px' },
    { title: 'การกระทำ', key: 'Actions', width: '140px', sortable: false },
];

// ========================================
// API COMPOSABLES
// ========================================
const { data: RequestData, refresh: RefreshData, pending: IsLoadingData } = await useFetch('/api/SI25007', {
    method: 'GET',
    baseURL: useRuntimeConfig().public.apiBase,
    server: false,
    lazy: true,
});

// ========================================
// COMPUTED PROPERTIES
// ========================================
const ItemDetail = computed(() => {
    return RequestData?.value || [];
});

const FilteredItems = computed(() => {
    if (!Array.isArray(ItemDetail?.value)) return [];

    return ItemDetail.value.filter((item: DwRequestDTO) => {
        const DrawingName = item?.DrawingName || '';
        const DrawingCode = item?.DrawingCode || '';
        const RequestCode = item?.RequestCode || '';

        const MatchesSearch = Search?.value === '' ||
            DrawingName.toLowerCase().includes((Search?.value || '').toLowerCase()) ||
            DrawingCode.toLowerCase().includes((Search?.value || '').toLowerCase()) ||
            RequestCode.toLowerCase().includes((Search?.value || '').toLowerCase());

        const MatchesStatus = SelectedStatusId?.value === null ||
            item?.StatusId === SelectedStatusId?.value;

        return MatchesSearch && MatchesStatus;
    });
});

const TotalItems = computed(() => FilteredItems?.value?.length || 0);
const ActiveItems = computed(() => FilteredItems?.value?.filter(item => item?.Active)?.length || 0);
const InactiveItems = computed(() => (TotalItems?.value || 0) - (ActiveItems?.value || 0));

// ค่า default เมื่อ SelectedItem เป็น null
const DefaultRequest: DwRequestDTO = {
    Id: '',
    DrawingCode: '',
    RequestCode: '',
    DrawingName: '',
    SectionId: null,
    DrawingTypeId: null,
    StatusId: null,
    CreatedDate: '',
    CreatedBy: '',
    UpdateDate: null,
    UpdateBy: '',
    DrawingDescription: '',
    UserId: '',
    AttachmentId: '',
    Active: true,
    IsDelete: false,
    Attachment: undefined,
    DrawingType: undefined,
    Section: undefined,
    Status: undefined,
    User: undefined
};

const ResolvedRequest = computed(() => SelectedItem.value || DefaultRequest);

// ========================================
// DIALOG FUNCTIONS
// ========================================
const OpenAddDialog = () => {
    SelectedItem.value = {
        Id: '',
        DrawingCode: '',
        RequestCode: '',
        DrawingName: '',
        SectionId: null,
        DrawingTypeId: null,
        StatusId: null,
        CreatedDate: '',
        CreatedBy: '',
        UpdateDate: null,
        UpdateBy: '',
        DrawingDescription: '',
        UserId: UserId?.value || '',
        AttachmentId: '',
        Active: true,
        IsDelete: false,
        DrawingType: undefined,
        Section: undefined,
        Status: undefined,
        User: undefined,
        Attachment: undefined,
    };
    IsRequestDialogVisible.value = true;
};

const HandleEdit = (item: DwRequestDTO) => {
    SelectedItem.value = { ...item };
    IsRequestDialogVisible.value = true;
};

// ========================================
// CRUD FUNCTIONS
// ========================================
const HandleDelete = async (id: string) => {
    const result = await Swal.fire({
        title: 'ยืนยันการลบ',
        text: 'คุณต้องการลบรายการนี้หรือไม่?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'ใช่, ลบเลย!',
        cancelButtonText: 'ยกเลิก',
        reverseButtons: true
    });

    if (!result.isConfirmed) return;

    try {
        await useFetch(`/api/SI25007/${id}`, {
            method: 'DELETE',
            baseURL: useRuntimeConfig().public.apiBase
        });
        await RefreshData();
        Swal.fire({
            title: 'ลบสำเร็จ!',
            text: 'รายการถูกลบเรียบร้อยแล้ว',
            icon: 'success',
            timer: 2000,
            showConfirmButton: false
        });
    } catch (error) {
        Swal.fire('เกิดข้อผิดพลาด!', 'ไม่สามารถลบรายการได้', 'error');
    }
};

const OnSubmit = async (item: DwRequestDTO) => {
    try {
        const apiUrl = item.Id ? `${Config.public.apiBase}/api/SI24001INA?id=${item.Id}` : `${Config.public.apiBase}/api/SI24001INA`;
        await fetch(apiUrl, {
            method: item.Id ? 'PUT' : 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(item)
        });
        await FetchRequest();
        Swal.fire({
            title: 'สำเร็จ!',
            text: 'บันทึกข้อมูลเรียบร้อย',
            icon: 'success',
            timer: 2000,
            showConfirmButton: false
        });
    } catch (error) {
        Swal.fire('เกิดข้อผิดพลาด!', 'บันทึกข้อมูลไม่สำเร็จ', 'error');
    }
};

const OnRefresh = async () => {
    try {
        await RefreshData();
        const Toast = Swal.mixin({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 2000,
            timerProgressBar: true,
        });
        Toast.fire({
            icon: 'success',
            title: 'รีเฟรชข้อมูลเรียบร้อย'
        });
    } catch (error) {
        console.error('❌ Error refreshing data:', error);
    }
};

const FetchRequest = async () => {
    await RefreshData();
};

// ========================================
// API FUNCTIONS
// ========================================
const FetchStatuses = async () => {
    try {
        const { data, error } = await useFetch(`${Config.public.apiBase}/api/Roles/DrawingStatus`, {
            method: 'GET',
            credentials: 'include',
        });

        if (error?.value) {
            throw new Error('โหลดสถานะไม่สำเร็จ');
        }

        Statuses.value = Array.isArray(data?.value) ? data.value : [];
        Statuses.value.unshift({ Id: null, StatusName: 'แสดงทั้งหมด' });

    } catch (error) {
        console.error('เกิดข้อผิดพลาด:', error);
    }
};

// ========================================
// UTILITY FUNCTIONS
// ========================================
const GetStatusColor = (statusName: string | null | undefined) => {
    if (!statusName || typeof statusName !== 'string') return '#6c757d';

    const NormalizedStatus = statusName.trim().toLowerCase();

    switch (NormalizedStatus) {
        case 'request':
            return '#2196F3';
        case 'approved':
            return '#4CAF50';
        case 'modify':
        case 'revise':
            return '#FF9800';
        case 'test':
            return '#00BCD4';
        case 'reject':
            return '#F44336';
        case 'confirm':
            return '#9C27B0';
        default:
            return '#9E9E9E';
    }
}

function FormatDate(dateString: string | null): string {
    if (!dateString) return '-';

    const date = new Date(dateString.replace(/\.\d+Z$/, ''));
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    return `${day}/${month}/${year}`;
}

// ========================================
// LIFECYCLE HOOKS
// ========================================
onMounted(async () => {
    if (!Token?.value) {
        Router.push('/login');
        return;
    }

    await nextTick(async () => {
        await RefreshData();
        await FetchStatuses();
    });
});
</script>

<template>
    <div class="drawing-register-container">
        <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.0/font/bootstrap-icons.css" rel="stylesheet">

        <v-container fluid class="pa-6">
            <!-- Hero Header Section -->
            <v-row class="mb-8">
                <v-col cols="12">
                    <v-card elevation="0" class="hero-card gradient-primary">
                        <v-card-text class="pa-8">
                            <div class="d-flex align-center justify-space-between flex-wrap">
                                <div>
                                    <div class="d-flex align-center mb-3">
                                        <div class="icon-wrapper mr-4">
                                            <i class="bi bi-file-earmark-text"></i>
                                        </div>
                                        <div>
                                            <h1 class="text-h3 font-weight-bold mb-2 text-white">
                                                ระบบจัดการแบบร่าง
                                            </h1>
                                            <p class="text-h6 text-white opacity-90 mb-0">
                                                Drawing Register Management System
                                            </p>
                                        </div>
                                    </div>
                                    <p class="text-body-1 text-white opacity-80 mb-0 ml-16">
                                        จัดการและติดตามสถานะแบบร่างทั้งหมดอย่างมีประสิทธิภาพ
                                    </p>
                                </div>
                                <v-btn size="x-large" color="white" class="mt-4 mt-md-0 add-btn" @click="OpenAddDialog"
                                    elevation="4">
                                    <i class="bi bi-plus-circle-fill mr-2"></i>
                                    เพิ่มรายการใหม่
                                </v-btn>
                            </div>
                        </v-card-text>
                    </v-card>
                </v-col>
            </v-row>

            <!-- Enhanced Statistics Cards -->
            <v-row class="mb-8">
                <v-col cols="12" sm="6" lg="4">
                    <v-card elevation="0" class="stat-card stat-card-total h-100">
                        <v-card-text class="pa-6">
                            <div class="d-flex justify-space-between align-start">
                                <div>
                                    <div class="stat-icon-bg stat-icon-primary mb-4">
                                        <i class="bi bi-files"></i>
                                    </div>
                                    <h3 class="text-h6 text-medium-emphasis mb-2">รายการทั้งหมด</h3>
                                    <div class="d-flex align-end">
                                        <h2 class="text-h3 font-weight-bold text-primary mr-2">{{ TotalItems }}</h2>
                                        <span class="text-body-2 text-medium-emphasis mb-2">รายการ</span>
                                    </div>
                                </div>
                                <v-chip size="small" color="primary" variant="flat" class="stat-badge">
                                    100%
                                </v-chip>
                            </div>
                        </v-card-text>
                    </v-card>
                </v-col>
                <v-col cols="12" sm="6" lg="4">
                    <v-card elevation="0" class="stat-card stat-card-active h-100">
                        <v-card-text class="pa-6">
                            <div class="d-flex justify-space-between align-start">
                                <div>
                                    <div class="stat-icon-bg stat-icon-success mb-4">
                                        <i class="bi bi-check-circle-fill"></i>
                                    </div>
                                    <h3 class="text-h6 text-medium-emphasis mb-2">เปิดใช้งาน</h3>
                                    <div class="d-flex align-end">
                                        <h2 class="text-h3 font-weight-bold text-success mr-2">{{ ActiveItems }}</h2>
                                        <span class="text-body-2 text-medium-emphasis mb-2">รายการ</span>
                                    </div>
                                </div>
                                <v-chip size="small" color="success" variant="flat" class="stat-badge">
                                    {{ TotalItems > 0 ? Math.round((ActiveItems / TotalItems) * 100) : 0 }}%
                                </v-chip>
                            </div>
                        </v-card-text>
                    </v-card>
                </v-col>
                <v-col cols="12" sm="6" lg="4">
                    <v-card elevation="0" class="stat-card stat-card-inactive h-100">
                        <v-card-text class="pa-6">
                            <div class="d-flex justify-space-between align-start">
                                <div>
                                    <div class="stat-icon-bg stat-icon-error mb-4">
                                        <i class="bi bi-x-circle-fill"></i>
                                    </div>
                                    <h3 class="text-h6 text-medium-emphasis mb-2">ไม่เปิดใช้งาน</h3>
                                    <div class="d-flex align-end">
                                        <h2 class="text-h3 font-weight-bold text-error mr-2">{{ InactiveItems }}</h2>
                                        <span class="text-body-2 text-medium-emphasis mb-2">รายการ</span>
                                    </div>
                                </div>
                                <v-chip size="small" color="error" variant="flat" class="stat-badge">
                                    {{ TotalItems > 0 ? Math.round((InactiveItems / TotalItems) * 100) : 0 }}%
                                </v-chip>
                            </div>
                        </v-card-text>
                    </v-card>
                </v-col>
            </v-row>

            <!-- Main Content Card -->
            <v-card elevation="0" class="main-card">
                <!-- Enhanced Filter Section -->
                <v-card-text class="pa-6 pb-0">
                    <v-row class="mb-2">
                        <v-col cols="12" md="6" lg="4">
                            <v-select v-model="SelectedStatusId" :items="Statuses" item-title="StatusName"
                                item-value="Id" label="กรองตามสถานะ" variant="outlined" density="comfortable" clearable
                                hide-details class="filter-select">
                                <template #prepend-inner>
                                    <i class="bi bi-funnel-fill text-primary"></i>
                                </template>
                            </v-select>
                        </v-col>
                        <v-col cols="12" md="6" lg="5">
                            <v-text-field v-model="Search" label="ค้นหา..."
                                placeholder="ค้นหาจากรหัสคำขอ, รหัสแบบ, หรือชื่อแบบ" variant="outlined"
                                density="comfortable" clearable hide-details class="search-field">
                                <template #prepend-inner>
                                    <i class="bi bi-search text-primary"></i>
                                </template>
                            </v-text-field>
                        </v-col>
                        <v-col cols="12" md="12" lg="3" class="d-flex align-center justify-end">
                            <v-btn color="primary" variant="tonal" size="large" @click="OnRefresh" class="mr-2">
                                <i class="bi bi-arrow-clockwise mr-2"></i>
                                รีเฟรช
                            </v-btn>
                        </v-col>
                    </v-row>
                </v-card-text>

                <!-- Enhanced Data Table -->
                <v-card-text class="pa-6">
                    <v-data-table :headers="Headers" :items="FilteredItems" :items-per-page="ItemsPerPage"
                        :page.sync="Page" :loading="IsLoadingData" class="enhanced-table" item-key="Id"
                        show-current-page hover>
                        <template #loading>
                            <v-skeleton-loader type="table-row@6"></v-skeleton-loader>
                        </template>

                        <template #item.No="{ index }">
                            <div class="table-index">
                                {{ ((Page - 1) * ItemsPerPage) + 1 + index }}
                            </div>
                        </template>

                        <template #item.RequestCode="{ item }">
                            <v-chip size="small" color="blue-darken-1" variant="flat" class="font-weight-medium">
                                <i class="bi bi-file-text mr-1"></i>
                                {{ item.RequestCode || '-' }}
                            </v-chip>
                        </template>

                        <template #item.DrawingCode="{ item }">
                            <v-chip size="small" color="purple-darken-1" variant="flat" class="font-weight-medium">
                                <i class="bi bi-diagram-3 mr-1"></i>
                                {{ item.DrawingCode || '-' }}
                            </v-chip>
                        </template>

                        <template #item.DrawingName="{ item }">
                            <div class="drawing-name-cell" :title="item.DrawingName">
                                <i class="bi bi-pencil-square text-primary mr-2"></i>
                                <span class="font-weight-medium">{{ item.DrawingName || '-' }}</span>
                            </div>
                        </template>

                        <template #item.UpdateDate="{ item }">
                            <v-chip size="small" color="orange-darken-1" variant="flat" class="font-weight-medium">
                                <i class="bi bi-calendar-event mr-1"></i>
                                {{ FormatDate(item.UpdateDate) }}
                            </v-chip>
                        </template>

                        <template #item.Active="{ item }">
                            <v-chip :color="item.Active ? 'success' : 'error'" size="small" variant="flat"
                                class="font-weight-medium px-3">
                                <i :class="item.Active ? 'bi bi-check-circle-fill' : 'bi bi-x-circle-fill'"
                                    class="mr-1"></i>
                                {{ item.Active ? 'เปิดใช้งาน' : 'ปิดใช้งาน' }}
                            </v-chip>
                        </template>

                        <template #item.Status.StatusName="{ item }">
                            <v-chip v-if="item.Status && item.Status.StatusName"
                                :style="{ backgroundColor: GetStatusColor(item.Status.StatusName), color: 'white' }"
                                size="small" variant="flat" class="font-weight-medium px-3">
                                <i class="bi bi-circle-fill mr-1" style="font-size: 8px;"></i>
                                {{ item.Status.StatusName }}
                            </v-chip>
                            <v-chip v-else color="grey" size="small" variant="flat">
                                <i class="bi bi-dash-circle mr-1"></i>
                                ไม่ระบุ
                            </v-chip>
                        </template>

                        <template #item.Actions="{ item }">
                            <div class="action-buttons">
                                <v-btn @click="HandleEdit(item)" color="primary" size="small" variant="tonal"
                                    class="mr-2">
                                    <i class="bi bi-pencil-fill"></i>
                                </v-btn>
                                <v-btn @click="HandleDelete(item.Id)" color="error" size="small" variant="tonal">
                                    <i class="bi bi-trash-fill"></i>
                                </v-btn>
                            </div>
                        </template>

                        <template #no-data>
                            <div class="no-data-container">
                                <div class="no-data-icon">
                                    <i class="bi bi-inbox"></i>
                                </div>
                                <h3 class="text-h5 font-weight-bold mb-2">ไม่พบข้อมูล</h3>
                                <p class="text-body-1 text-medium-emphasis">
                                    ไม่มีข้อมูลที่ตรงกับการค้นหา กรุณาลองค้นหาใหม่อีกครั้ง
                                </p>
                            </div>
                        </template>
                    </v-data-table>
                </v-card-text>
            </v-card>
        </v-container>
    </div>

    <!-- Edit/Add Dialog -->
    <AddEditDrawingControl v-if="IsRequestDialogVisible" :IsDialogVisible="IsRequestDialogVisible"
        :Request="ResolvedRequest" @update:IsDialogVisible="IsRequestDialogVisible = $event" @submit="OnSubmit"
        @refreshData="OnRefresh" />
</template>

<style>
/* === Page === */
.drawing-register-container {
  background: var(--bg);
  min-height: 100vh;
}

/* === Hero Card === */
.hero-card {
  border-radius: var(--r-xl) !important;
  overflow: hidden;
  border: none !important;
  box-shadow: var(--shadow-lg) !important;
}

.gradient-primary {
  background: linear-gradient(135deg, var(--brand) 0%, var(--brand-dark) 100%) !important;
  box-shadow: 0 8px 32px rgba(92,107,192,0.35) !important;
}

.icon-wrapper {
  width: 60px; height: 60px;
  background: rgba(255,255,255,0.18);
  border-radius: var(--r-lg);
  display: flex; align-items: center; justify-content: center;
  backdrop-filter: blur(8px);
  border: 1px solid rgba(255,255,255,0.25);
}
.icon-wrapper i { font-size: 28px; color: #fff; }

.add-btn {
  border-radius: var(--r-lg) !important;
  text-transform: none !important;
  font-weight: 700 !important;
  letter-spacing: 0.01em !important;
  box-shadow: 0 4px 14px rgba(0,0,0,0.15) !important;
  transition: all var(--t-mid) var(--ease) !important;
}
.add-btn:hover {
  transform: translateY(-2px) !important;
  box-shadow: 0 8px 20px rgba(0,0,0,0.2) !important;
}

/* === Stat Cards === */
.stat-card {
  border-radius: var(--r-lg) !important;
  border: 1px solid var(--border) !important;
  background: var(--surface) !important;
  box-shadow: var(--shadow-sm) !important;
  transition: transform var(--t-mid) var(--ease), box-shadow var(--t-mid) var(--ease);
  position: relative;
  overflow: hidden;
}
.stat-card::before {
  content: '';
  position: absolute;
  top: 0; left: 0; right: 0;
  height: 3px;
  background: var(--stat-accent, var(--brand));
}
.stat-card-total   { --stat-accent: var(--brand); }
.stat-card-active  { --stat-accent: var(--success); }
.stat-card-inactive{ --stat-accent: var(--error); }

.stat-card:hover {
  transform: translateY(-4px);
  box-shadow: var(--shadow-lg) !important;
}

.stat-icon-bg {
  width: 52px; height: 52px;
  border-radius: var(--r-md);
  display: flex; align-items: center; justify-content: center;
}
.stat-icon-bg i { font-size: 24px; }
.stat-icon-primary { background: var(--brand-light);  color: var(--brand); }
.stat-icon-success  { background: var(--success-bg);   color: var(--success); }
.stat-icon-error    { background: var(--error-bg);     color: var(--error); }

.stat-badge {
  font-weight: 700 !important;
  font-size: var(--fs-xs) !important;
  border-radius: var(--r-full) !important;
}

/* === Main Card === */
.main-card {
  border-radius: var(--r-xl) !important;
  border: 1px solid var(--border) !important;
  background: var(--surface) !important;
  box-shadow: var(--shadow-sm) !important;
  overflow: hidden;
}

/* === Filter & Search === */
.filter-select :deep(.v-field),
.search-field :deep(.v-field) {
  border-radius: var(--r-md) !important;
  background: var(--surface-2) !important;
  border: 1.5px solid var(--border) !important;
  font-family: var(--font) !important;
}

/* === Table === */
.enhanced-table { border-radius: 0 !important; }

.enhanced-table :deep(.v-data-table__thead th) {
  background: var(--surface-3) !important;
  color: var(--text-2) !important;
  font-weight: 700 !important;
  font-size: var(--fs-xs) !important;
  text-transform: uppercase !important;
  letter-spacing: 0.06em !important;
  padding: var(--s-3) var(--s-4) !important;
  border-bottom: 2px solid var(--border) !important;
}

.enhanced-table :deep(.v-data-table__td) {
  padding: 14px var(--s-4) !important;
  font-size: var(--fs-sm) !important;
  border-bottom: 1px solid var(--border-light) !important;
  font-family: var(--font) !important;
}

.enhanced-table :deep(tr:hover td) {
  background: var(--brand-xlight) !important;
}

.table-index {
  width: 30px; height: 30px;
  background: var(--brand-light);
  color: var(--brand);
  border-radius: var(--r-md);
  display: flex; align-items: center; justify-content: center;
  font-weight: 700;
  font-size: var(--fs-xs);
  flex-shrink: 0;
}

.drawing-name-cell {
  display: flex; align-items: center;
  max-width: 280px;
  overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
  font-weight: 500;
}

.action-buttons { display: flex; gap: var(--s-2); align-items: center; }
.action-buttons .v-btn { border-radius: var(--r-md) !important; }

/* === No Data === */
.no-data-container { text-align: center; padding: 80px var(--s-8); }
.no-data-icon {
  width: 96px; height: 96px;
  margin: 0 auto var(--s-6);
  background: var(--brand-light);
  border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
}
.no-data-icon i { font-size: 44px; color: var(--brand); }

/* === Chips === */
.v-chip { border-radius: var(--r-full) !important; font-size: var(--fs-xs) !important; font-weight: 600 !important; }

/* === Scrollbar === */
:deep(.v-data-table__wrapper)::-webkit-scrollbar { height: 5px; }
:deep(.v-data-table__wrapper)::-webkit-scrollbar-thumb { background: var(--border); border-radius: var(--r-full); }
:deep(.v-data-table__wrapper)::-webkit-scrollbar-thumb:hover { background: var(--brand); }

/* === Animations === */
@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(14px); }
  to   { opacity: 1; transform: translateY(0); }
}
.stat-card { animation: fadeInUp 0.35s var(--ease) both; }
.stat-card:nth-child(1) { animation-delay: 0.05s; }
.stat-card:nth-child(2) { animation-delay: 0.1s; }
.stat-card:nth-child(3) { animation-delay: 0.15s; }
.main-card { animation: fadeInUp 0.4s var(--ease) 0.1s both; }

/* === Responsive === */
@media (max-width: 600px) {
  .add-btn { width: 100%; }
  .stat-card { margin-bottom: var(--s-4); }
}
</style>

/* === Hero Card === */
.hero-card {
  border-radius: var(--radius-xl) !important;
  overflow: hidden;
}

.gradient-primary {
  background: var(--color-request) !important;
  box-shadow: var(--shadow-lg) !important;
}

.icon-wrapper {
  width: 64px;
  height: 64px;
  background: rgba(255,255,255,0.2);
  border-radius: var(--radius-lg);
  display: flex;
  align-items: center;
  justify-content: center;
}
.icon-wrapper i { font-size: 32px; color: #fff; }

.add-btn {
  border-radius: var(--radius-lg) !important;
  text-transform: none !important;
  font-weight: 600 !important;
}

/* === Stat Cards === */
.stat-card {
  border-radius: var(--radius-lg) !important;
  border: 1px solid var(--color-border) !important;
  background: var(--color-surface) !important;
  transition: transform var(--transition-normal), box-shadow var(--transition-normal);
}
.stat-card:hover {
  transform: translateY(-3px);
  box-shadow: var(--shadow-md) !important;
}

.stat-icon-bg {
  width: 52px;
  height: 52px;
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
}
.stat-icon-bg i { font-size: 26px; }

.stat-icon-primary { background: var(--color-primary-light); color: var(--color-primary); }
.stat-icon-success  { background: var(--color-success-bg);   color: var(--color-success); }
.stat-icon-error    { background: var(--color-error-bg);     color: var(--color-error); }

.stat-badge { font-weight: 700; font-size: var(--text-xs); }

/* === Main Card === */
.main-card {
  border-radius: var(--radius-lg) !important;
  border: 1px solid var(--color-border) !important;
  background: var(--color-surface) !important;
}

/* === Filter & Search === */
.filter-select :deep(.v-field),
.search-field :deep(.v-field) {
  border-radius: var(--radius-md) !important;
  background: var(--color-surface-2) !important;
}

/* === Table === */
.enhanced-table { border-radius: var(--radius-lg) !important; }

.enhanced-table :deep(.v-data-table__thead) {
  background: var(--color-table-header);
}
.enhanced-table :deep(.v-data-table__th) {
  color: var(--color-text) !important;
  font-weight: 600 !important;
  font-size: var(--text-sm) !important;
  padding: var(--space-4) !important;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}
.enhanced-table :deep(.v-data-table__td) {
  padding: var(--space-3) var(--space-4) !important;
  font-size: var(--text-sm) !important;
}
.enhanced-table :deep(tr:hover) {
  background: var(--color-surface-2) !important;
}

.table-index {
  width: 32px;
  height: 32px;
  background: var(--color-primary-light);
  color: var(--color-primary);
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: var(--text-sm);
}

.drawing-name-cell {
  display: flex;
  align-items: center;
  max-width: 250px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.action-buttons { display: flex; gap: var(--space-2); }
.action-buttons .v-btn { border-radius: var(--radius-md) !important; }

/* === No Data === */
.no-data-container { text-align: center; padding: var(--space-10) var(--space-8); }
.no-data-icon {
  width: 100px;
  height: 100px;
  margin: 0 auto var(--space-6);
  background: var(--color-primary-light);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}
.no-data-icon i { font-size: 48px; color: var(--color-primary); }

/* === Chips === */
.v-chip {
  border-radius: var(--radius-md) !important;
  font-size: var(--text-xs) !important;
}

/* === Scrollbar === */
:deep(.v-data-table__wrapper)::-webkit-scrollbar { height: 6px; }
:deep(.v-data-table__wrapper)::-webkit-scrollbar-track { background: var(--color-surface-2); border-radius: var(--radius-full); }
:deep(.v-data-table__wrapper)::-webkit-scrollbar-thumb { background: var(--color-border); border-radius: var(--radius-full); }
:deep(.v-data-table__wrapper)::-webkit-scrollbar-thumb:hover { background: var(--color-primary); }

/* === SweetAlert === */
.swal2-container { z-index: 99999 !important; }

/* === Animations === */
@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(16px); }
  to   { opacity: 1; transform: translateY(0); }
}
.stat-card, .main-card { animation: fadeInUp 0.4s ease-out; }
.stat-card:nth-child(1) { animation-delay: 0.05s; }
.stat-card:nth-child(2) { animation-delay: 0.1s; }
.stat-card:nth-child(3) { animation-delay: 0.15s; }

/* === Responsive === */
@media (max-width: 600px) {
  .add-btn { width: 100%; }
  .stat-card { margin-bottom: var(--space-4); }
}
</style>
