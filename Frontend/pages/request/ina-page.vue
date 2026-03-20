<script setup lang="ts">
import { ref, onMounted, computed, nextTick, watch, onUnmounted } from 'vue';
import { v4 as uuidv4 } from 'uuid';
import Swal from 'sweetalert2';
import type { InaRequestDTO } from '@/types/inaRequest';

// ===== CONSTANTS =====
const REFRESH_INTERVAL = 30000; // 30 seconds
const SWAL_TIMER = 2000;

// ===== STATE =====
const autoRefreshInterval = ref<NodeJS.Timeout | null>(null);
const isAutoRefresh = ref(false);
const isTableLoading = ref(false);
const isRequestDialogVisible = ref(false);
const search = ref('');
const page = ref(1);
const itemsPerPage = ref(10);
const statuses = ref<{ Id: string | null; StatusName: string }[]>([]);
const selectedItem = ref<InaRequestDTO | null>(null);
const selectedStatusFilter = ref<string | null>(null);
const config = useRuntimeConfig ? useRuntimeConfig() : { public: { apiBase: '/api' } }

// ===== ROUTER & COOKIES =====
const router = useRouter();
const token = useCookie('token');
const userId = useCookie('userId');
const userName = useCookie('userName');
const role = useCookie('roleName');

// ===== SWEET ALERT =====
const swal = Swal;

// ===== TABLE CONFIGURATION =====
const headers = [
    { title: 'No.', key: 'no', sortable: false, width: '80px' },
    { title: 'Request Code', key: 'RequestCode', width: '140px' },
    { title: 'RequestPurpose', key: 'RequestPurpose', width: '180px' },
    { title: 'Description', key: 'RequestDescription', width: '200px' },
    { title: 'Requester', key: 'RequestBy', width: '120px' },
    { title: 'Product', key: 'RequestProduct', width: '120px' },
    { title: 'Status', key: 'Status.StatusName', width: '140px' },
    { title: 'Active', key: 'Active', width: '100px' },
    { title: 'Actions', key: 'actions', sortable: false, width: '120px' },
];

// ===== API CALLS =====
const { data: requestData, execute: fetchRequest, error: requestError } = useFetch('/api/SI24001INA', {
    method: 'GET',
    baseURL: useRuntimeConfig().public.apiBase,
    server: false,
    lazy: true
});

// ===== COMPUTED PROPERTIES =====
const itemDetail = computed(() => {
    if (!requestData.value || !Array.isArray(requestData.value)) return [];
    return requestData.value;
});

const defaultRequest: InaRequestDTO = {
    Id: '',
    RequestCode: '',
    RequestPurpose: '',
    UserId: '',
    RequestDescription: '',
    RequestDate: null,
    StatusId: null,
    Active: true,
    IsDeleted: false,
    Attachement: [],
    AttachmentId: '',
    RequestMachine: '',
    RequestProduct: '',
    RequestMass: false,
    RequestTest: true,
    Status: {
        Id: '',
        StatusName: '',
        Ordinal: null,
        IsDeleted: false
    },
    RequestComment1: '',
    RequestComment2: '',
    RequestComment3: '',
    Recipe: '',
    OtherPrograms: '',
    Attachment_Id_Size: '',
    RequestBy: '',
    RequestProcess: '',
    RequestStartDate: null,
    RequestFinishDate: null,
    FlTgDeleted: null,
    FlMcDeleted: null,
    FlCheckMass: null,
    FlDeletedOther: null,
    CtCopyRp: null,
    CtRpDeleted: null,
    CtBookCheck: null,
    CtDeletedOther: null,
    FlTgDeletedComment: null,
    FlMcDeletedComment: null,
    FlCheckMassComment: null,
    FlDeletedOtherComment: null,
    CtCopyRpComment: null,
    CtRpDeletedComment: null,
    CtBookCheckComment: null,
    CtDeletedOtherComment: null,
    RequestMcNo: '',
    RequestBook: '',
    RequestInstallDate: null,
    RequestClearDate: null,
};

const resolvedRequest = computed(() => selectedItem.value || defaultRequest);

const filteredItems = computed(() => {
    if (!Array.isArray(itemDetail.value)) return [];

    return itemDetail.value.filter((item: any) => {
        // Handle both uppercase and lowercase field names from API
        const name = (item.RequestPurpose || item.RequestPurpose || '').toLowerCase();
        const description = (item.RequestDescription || item.requestDescription || '').toLowerCase();
        const code = (item.RequestCode || item.requestCode || '').toLowerCase();
        const requestBy = (item.RequestBy || item.requestBy || '').toLowerCase();
        const machine = (item.RequestMachine || item.requestMachine || '').toLowerCase();
        const product = (item.RequestProduct || item.requestProduct || '').toLowerCase();
        const searchTerm = search.value.toLowerCase();

        const matchesSearch = !searchTerm ||
            name.includes(searchTerm) ||
            description.includes(searchTerm) ||
            code.includes(searchTerm) ||
            requestBy.includes(searchTerm) ||
            machine.includes(searchTerm) ||
            product.includes(searchTerm);

        const statusId = item.StatusId || item.statusId;
        const matchesStatus = selectedStatusFilter.value === null ||
            statusId === selectedStatusFilter.value;

        return matchesSearch && matchesStatus;
    });
});

// ===== SUMMARY STATISTICS =====
const totalItems = computed(() => {
    return itemDetail.value.length;
});

const activeItems = computed(() => {
    const activeCount = itemDetail.value.filter(item =>
        (item.Active === true || item.active === true)
    ).length;
    return activeCount;
});

const inactiveItems = computed(() => {
    const inactiveCount = itemDetail.value.filter(item =>
        (item.Active === false || item.active === false)
    ).length;
    return inactiveCount;
});

// ===== UTILITY FUNCTIONS =====
const showToast = (icon: 'success' | 'error' | 'info', title: string) => {
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 1500,
        timerProgressBar: true,
    });

    Toast.fire({ icon, title });
};

const showConfirmDialog = (title: string, text: string) => {
    return Swal.fire({
        title,
        text,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'ใช่, ดำเนินการ!',
        cancelButtonText: 'ยกเลิก'
    });
};

const showSuccessDialog = (title: string, text: string) => {
    return Swal.fire({
        title,
        text,
        icon: 'success',
        confirmButtonText: 'ตกลง',
        timer: SWAL_TIMER,
        timerProgressBar: true
    });
};

const showErrorDialog = (title: string, text: string) => {
    return Swal.fire({
        title,
        text,
        icon: 'error',
        confirmButtonText: 'ตกลง',
    });
};

const isValidGuid = (guid: string): boolean => {
    const guidPattern = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;
    return guidPattern.test(guid);
};

const formatDate = (dateString: string | Date | null): string => {
    if (!dateString) return '';

    try {
        const date = new Date(dateString);
        const day = date.getDate().toString().padStart(2, '0');
        const month = (date.getMonth() + 1).toString().padStart(2, '0');
        const year = date.getFullYear().toString().slice(-2);
        return `${day}/${month}/${year}`;
    } catch (error) {
        console.error('Error formatting date:', error);
        return '';
    }
};

const getStatusColor = (StatusName: string | null | undefined): string => {
    if (!StatusName || typeof StatusName !== 'string') return '#6c757d';

    const normalizedStatus = StatusName.trim().toLowerCase();

    const statusColors: Record<string, string> = {
        'request': '#155724',
        'confirm': '#155724',
        'install': '#155724',
        'modify': '#ffc107',
        'test': '#ffc107',
        'rejected': '#dc3545',
        'mass production': '#4e6cef',
        'completed': '#28a745',
        'in progress': '#17a2b8',
        'pending': '#6c757d',
    };

    return statusColors[normalizedStatus] || '#6c757d';
};

// ===== DIALOG FUNCTIONS =====
const openAddDialog = () => {
    selectedItem.value = { ...defaultRequest };
    isRequestDialogVisible.value = true;
};

const handleEdit = (item: InaRequestDTO) => {
    selectedItem.value = { ...item };
    isRequestDialogVisible.value = true;
};

// ===== CRUD OPERATIONS =====
const handleDelete = async (id: string) => {
    try {
        const result = await showConfirmDialog(
            'คุณแน่ใจหรือไม่?',
            'คุณต้องการลบรายการนี้จริงหรือไม่?'
        );

        if (!result.isConfirmed) return;

        isTableLoading.value = true;

        const { error } = await useFetch(`/api/SI24001INA/${id}`, {
            method: 'DELETE',
            baseURL: useRuntimeConfig().public.apiBase,
            credentials: 'include',
        });

        if (error.value) {
            throw new Error(error.value.message || 'ไม่สามารถลบรายการได้');
        }

        await fetchRequest();
        await showSuccessDialog('ลบสำเร็จ!', 'รายการถูกลบเรียบร้อยแล้ว');

    } catch (error: any) {
        console.error('Error deleting item:', error);
        await showErrorDialog('เกิดข้อผิดพลาด!', error.message || 'ไม่สามารถลบรายการได้');
    } finally {
        isTableLoading.value = false;
    }
};

const onSubmit = async (item: InaRequestDTO) => {
    try {
        isTableLoading.value = true;

        // Validate statusId if provided
        if (item.StatusId && !isValidGuid(item.StatusId.toString())) {
            throw new Error('สถานะไม่ถูกต้อง');
        }

        const isUpdate = Boolean(item.Id);
        let apiUrl = '/api/SI24001INA';

        if (isUpdate) {
            apiUrl += `?id=${item.Id}`;
        } else {
            item.Id = uuidv4();
            item.UserId = userId.value;
        }

        const { error } = await useFetch(apiUrl, {
            method: isUpdate ? 'PUT' : 'POST',
            baseURL: useRuntimeConfig().public.apiBase,
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(item),
        });

        if (error.value) {
            throw new Error(error.value.message || 'เกิดข้อผิดพลาดระหว่างการบันทึกข้อมูล');
        }

        await fetchRequest();
        isRequestDialogVisible.value = false;

        await showSuccessDialog(
            'สำเร็จ!',
            isUpdate ? 'อัปเดตข้อมูลเรียบร้อยแล้ว' : 'เพิ่มข้อมูลสำเร็จ!'
        );

    } catch (error: any) {
        console.error('Error saving item:', error);
        await showErrorDialog('เกิดข้อผิดพลาด!', error.message || 'เกิดข้อผิดพลาดระหว่างการบันทึก');
    } finally {
        isTableLoading.value = false;
    }
};

const onRefresh = async () => {
    isTableLoading.value = true;
    try {
        await fetchRequest();
        showToast('success', 'รีเฟรชข้อมูลสำเร็จ');
    } catch (error) {
        console.error('Error refreshing data:', error);
        showToast('error', 'ไม่สามารถรีเฟรชข้อมูลได้');
    } finally {
        isTableLoading.value = false;
    }
};

// ===== STATUS MANAGEMENT =====
const fetchStatuses = async () => {
    try {
        const { data, error } = await useFetch('/api/Roles/statuses', {
            method: 'GET',
            baseURL: useRuntimeConfig().public.apiBase,
            credentials: 'include',
        });

        if (error.value) {
            throw new Error('โหลดสถานะไม่สำเร็จ');
        }

        statuses.value = Array.isArray(data.value) ? data.value : [];
        
        statuses.value.unshift({ Id: null, StatusName: 'แสดงทั้งหมด' });

    } catch (error) {
        console.error('Error fetching statuses:', error);
        showToast('error', 'ไม่สามารถโหลดสถานะได้');
    }
};

// ===== AUTO REFRESH =====
const startAutoRefresh = (intervalMs: number = REFRESH_INTERVAL) => {
    if (autoRefreshInterval.value) {
        clearInterval(autoRefreshInterval.value);
    }

    isAutoRefresh.value = true;
    autoRefreshInterval.value = setInterval(async () => {
        if (!isRequestDialogVisible.value) {
            await fetchRequest();
        }
    }, intervalMs);
};

const stopAutoRefresh = () => {
    if (autoRefreshInterval.value) {
        clearInterval(autoRefreshInterval.value);
        autoRefreshInterval.value = null;
    }
    isAutoRefresh.value = false;
};

// ===== WATCHERS =====
watch(isRequestDialogVisible, async (newValue, oldValue) => {
    if (oldValue === true && newValue === false) {
        await fetchRequest();
    }
});

// Debug watcher to log data changes
watch(itemDetail, (newData) => {
}, { immediate: true });

// ===== LIFECYCLE HOOKS =====
onMounted(async () => {
    // Check authentication
    if (!token.value) {
        router.push('/login');
        return;
    }

    await nextTick(async () => {
        await Promise.all([
            fetchRequest(),
            fetchStatuses()
        ]);
        // Set default status filter to "show all"
        selectedStatusFilter.value = null;
    });

    // Optional: Start auto-refresh
    // startAutoRefresh();
});

onUnmounted(() => {
    stopAutoRefresh();
});

// ===== DIALOG HANDLERS =====
const handleDialogSubmit = async () => {
    isRequestDialogVisible.value = false;
    await fetchRequest();
    showToast('success', 'บันทึกข้อมูลสำเร็จ');
};

const handleDialogRefresh = async () => {
    await fetchRequest();
};
</script>

<template>
    <v-card>
        <v-card-title>
            <span class="text-h5 font-weight-bold">Request Management INA</span>
        </v-card-title>

        <!-- Filters and Search -->
        <v-card-text>
            <v-row class="align-center" no-gutters>
                <v-col cols="12" md="2" class="pa-2">
                    <v-select v-model="selectedStatusFilter" :items="statuses" item-title="StatusName" item-value="Id"
                        label="Filter by Status" variant="outlined" density="compact" clearable
                        prepend-inner-icon="tabler-filter" />
                </v-col>

                <v-col cols="12" md="8" class="pa-2">
                    <v-text-field v-model="search" label="Search"
                        placeholder="Search by name, description, code, requester, product..." variant="outlined"
                        density="compact" clearable prepend-inner-icon="tabler-search" />
                </v-col>

                <v-col cols="12" md="2" class="pa-2">
                    <v-btn color="primary" @click="openAddDialog" block size="large" prepend-icon="tabler-plus">
                        Add New
                    </v-btn>
                </v-col>
            </v-row>

            <!-- Summary Cards -->
            <v-row class="mt-4">
                <v-col cols="12" sm="4" md="4">
                    <v-card color="primary" variant="tonal" class="text-center pa-4 rounded-lg">
                        <v-icon icon="tabler-list-check" size="32" class="mb-2"></v-icon>
                        <div class="text-h5 font-weight-bold">{{ totalItems }}</div>
                        <div class="text-body-2 text-medium-emphasis">Total Requests</div>
                    </v-card>
                </v-col>
                <v-col cols="12" sm="4" md="4">
                    <v-card color="success" variant="tonal" class="text-center pa-4 rounded-lg">
                        <v-icon icon="tabler-circle-check-filled" size="32" class="mb-2"></v-icon>
                        <div class="text-h5 font-weight-bold">{{ activeItems }}</div>
                        <div class="text-body-2 text-medium-emphasis">Active Requests</div>
                    </v-card>
                </v-col>
                <v-col cols="12" sm="4" md="4">
                    <v-card color="warning" variant="tonal" class="text-center pa-4 rounded-lg">
                        <v-icon icon="tabler-circle-x-filled" size="32" class="mb-2"></v-icon>
                        <div class="text-h5 font-weight-bold">{{ inactiveItems }}</div>
                        <div class="text-body-2 text-medium-emphasis">Inactive Requests</div>
                    </v-card>
                </v-col>
            </v-row>

            <v-row class="mt-2" v-if="requestError">
                <v-col cols="12">
                    <v-alert type="error" variant="outlined" rounded="lg">
                        <strong>API Error:</strong> {{ requestError }}
                    </v-alert>
                </v-col>
            </v-row>
        </v-card-text>

        <!-- Data Table -->
        <v-data-table :headers="headers" :items="filteredItems" :items-per-page="itemsPerPage" :page.sync="page"
            :loading="isTableLoading" :no-data-text="'No data available'" :loading-text="'Loading...'"
            class="elevation-2 rounded-lg" hover :items-per-page-options="[5, 10, 25, 50]" show-current-page>

            <!-- Row Number -->
            <template #item.no="{ index }">
                <div class="text-center font-weight-medium">
                    {{ ((page - 1) * itemsPerPage) + 1 + index }}
                </div>
            </template>

            <!-- Request Code -->
            <template #item.RequestCode="{ item }">
                <div class="text-body-2">
                    <v-chip color="primary" size="small" variant="outlined">
                        {{ item.RequestCode }}
                    </v-chip>
                </div>
            </template>

            <!-- Request Name -->
            <template #item.RequestPurpose="{ item }">
                <div class="text-body-1 font-weight-medium">
                    {{ item.RequestPurpose }}
                </div>
            </template>

            <!-- Request Description -->
            <template #item.RequestDescription="{ item }">
                <div class="text-body-2 text-truncate" style="max-width: 200px;">
                    <v-tooltip :text="item.RequestDescription" location="top">
                        <template #activator="{ props }">
                            <span v-bind="props">{{ item.RequestDescription }}</span>
                        </template>
                    </v-tooltip>
                </div>
            </template>

            <!-- Request By -->
            <template #item.RequestBy="{ item }">
                <div class="text-body-2">
                    <v-chip v-if="item.RequestBy" color="blue" size="small" variant="tonal" class="text-capitalize">
                        <v-icon start icon="tabler-user" size="14"></v-icon>
                        {{ item.RequestBy }}
                    </v-chip>
                    <span v-else class="text-grey">-</span>
                </div>
            </template>

            <!-- Request Product -->
            <template #item.RequestProduct="{ item }">
                <div class="text-body-2">
                    <v-chip v-if="item.RequestProduct" color="teal" size="small" variant="tonal">
                        <v-icon start icon="tabler-package" size="14"></v-icon>
                        {{ item.RequestProduct }}
                    </v-chip>
                    <span v-else class="text-grey">-</span>
                </div>
            </template>

            <!-- Active Status -->
            <template #item.Active="{ item }">
                <div class="text-center">
                    <v-chip :color="item.Active ? 'success' : 'error'" size="small" variant="flat"
                        :prepend-icon="item.Active ? 'tabler-check' : 'tabler-x'">
                        {{ item.Active ? 'Active' : 'Inactive' }}
                    </v-chip>
                </div>
            </template>

            <!-- Status -->
            <template #item.Status.StatusName="{ item }">
                <div class="text-center">
                    <v-chip v-if="item.Status?.StatusName"
                        :style="{ backgroundColor: getStatusColor(item.Status.StatusName), color: 'white' }"
                        size="small" variant="flat" class="font-weight-medium">
                        {{ item.Status.StatusName }}
                    </v-chip>
                    <v-chip v-else color="grey" size="small" variant="flat">
                        Unknown
                    </v-chip>
                </div>
            </template>

            <!-- Actions -->
            <template #item.actions="{ item }">
                <div class="d-flex justify-center ga-1">
                    <v-btn icon size="small" color="primary" variant="text" @click="handleEdit(item)"
                        density="comfortable">
                        <v-icon icon="tabler-edit" size="18"></v-icon>
                        <v-tooltip activator="parent" location="top">Edit</v-tooltip>
                    </v-btn>

                    <v-btn icon size="small" color="error" variant="text" @click="handleDelete(item.Id)"
                        density="comfortable">
                        <v-icon icon="tabler-trash" size="18"></v-icon>
                        <v-tooltip activator="parent" location="top">Delete</v-tooltip>
                    </v-btn>
                </div>
            </template>
        </v-data-table>
    </v-card>

    <!-- Edit/Add Dialog -->
    <AddEditRequestDialogIna v-if="isRequestDialogVisible" :isDialogVisible="isRequestDialogVisible"
        :request="resolvedRequest" @update:isDialogVisible="isRequestDialogVisible = $event" @submit="onSubmit"
        @refreshData="onRefresh" />
</template>

<style>
.swal2-container { z-index: 99999 !important; }
.swal2-popup { border-radius: var(--rr-xl) !important; font-family: var(--f-sans) !important; }

.v-data-table { border-radius: var(--rr-xl) !important; overflow: hidden; }
.v-data-table tbody tr:hover td { background: var(--brand-bg) !important; }
.v-chip { font-weight: 700 !important; border-radius: var(--rr-max) !important; font-size: var(--fz-2xs) !important; }
.text-medium-emphasis { opacity: 0.6; }
.v-card { transition: transform var(--dur-mid) var(--ease-out), box-shadow var(--dur-mid) var(--ease-out) !important; }
.v-card:hover { transform: translateY(-2px) !important; }
</style>