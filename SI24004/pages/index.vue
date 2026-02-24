<script setup lang="ts">
import type { RequestDTO } from '@/types/request';
import { ref, onMounted } from 'vue';
import { v4 as uuidv4 } from 'uuid';
import Swal from 'sweetalert2'

const router = useRouter();
const token = useCookie('token');
const userId = useCookie('userId');
const userName = useCookie('userName');
const role = useCookie('roleName');

//api
const isloading = ref(true)

const selectedItem = ref<RequestDTO | null>(null);
const isRequestDialogVisible = ref(false);
const search = ref('');
const page = ref(1);
const itemsPerPage = ref(10);

const dateStr = selectedItem.value?.approveDate

//alert
const swal = Swal

const headers = [
    { title: '::', key: 'no' },
    { title: 'Request Code', key: 'requestCode' },
    { title: 'Request Name', key: 'requestName' },
    { title: 'Request Description', key: 'requestDescription' },
    { title: 'Request Approve', key: 'requestApprove' },
    { title: 'Request Approve Date', key: 'approveDate' },
    { title: 'Request Date', key: 'requestDate' },
    { title: 'Active', key: 'active' },
    { text: 'Actions', value: 'actions' },
];

const { data: requestData } = useFetch('/api/SI24004AVI', {
    method: 'GET',
    baseURL: useRuntimeConfig().public.apiBase // ✅ ใช้ baseURL จาก .env
});



const itemDetail = computed(() => requestData.value || []);


const filteredItems = computed(() =>
    Array.isArray(itemDetail.value)
        ? itemDetail.value.filter((item: RequestDTO) => {
            const name = item.requestName || '';
            const description = item.requestDescription || '';
            return name.toLowerCase().includes(search.value.toLowerCase()) ||
                description.toLowerCase().includes(search.value.toLowerCase());
        })
        : []
);

const openAddDialog = () => {
    selectedItem.value = {
        id: '',
        requestCode: '',
        requestName: '',
        userId: '',
        requestDescription: '',
        requestDate: null,
        requestApprove: false,
        approveDate: null,
        active: true,
        isDeleted: false,
        attachement: []
    } // Clear data
    isRequestDialogVisible.value = true;
};

const handleEdit = (item: RequestDTO) => {
    selectedItem.value = item;
    isRequestDialogVisible.value = true;
};


const { data: fetchedData, pending, error } = await useFetch('/api/SI24004AVI', {
    method: 'GET',
    baseURL: useRuntimeConfig().public.apiBase, // ใช้ baseURL จาก .env
    headers: {
        'Content-Type': 'application/json',
    }
});
if (error) {
    console.error('Fetch error:', error);
} else {
}



if (error.value) {
    console.error('Failed to fetch data:', error.value);
    isloading.value = false;
}


const requestInfo = useCookie<RequestDTO>('requestData')

const fetchData = async () => {
    try {
        const { data: fetchedRequestData, pending, error } = await useFetch(
            requestInfo.value?.id
                ? `/api/SI24004AVI?id=${requestInfo.value.id}`  // ใช้ query string หากมี id
                : `/api/SI24004AVI`,  // กรณีไม่มี id
            {
                method: 'GET',
                baseURL: useRuntimeConfig().public.apiBase,
            }
        );

        if (error) {
            console.error('Fetch error:', error);
        } else {
        }
    } catch (err) {
        console.error('Error fetching data:', err);
    }
};


const handleDelete = async (id: string) => {
    const confirmed = confirm('Are you sure you want to delete this item?');
    if (confirmed) {
        const { data, error } = await useFetch(`/api/SI24004AVI/${id}`, {
            method: 'DELETE',
            baseURL: useRuntimeConfig().public.apiBase,
        });

        if (error) {
            swal.fire({
                title: 'Error!',
                text: 'Failed to delete item.',
                icon: 'error',
                confirmButtonText: 'OK',
            });
            return;
        }

        // รีโหลดข้อมูลหลังจากการลบ
        try {
            await fetchData();
        } catch (fetchError) {
            swal.fire({
                title: 'Error!',
                text: 'Failed to reload data after deletion.',
                icon: 'error',
                confirmButtonText: 'OK',
            });
        }
    }
};

const onSubmit = async (item: RequestDTO) => {
    try {
        // ตรวจสอบว่ามี ID หรือไม่ เพื่อแยกการ Update หรือ Insert
        if (item?.id) {
            // Update request
            const { data, error } = await useFetch(`/api/SI24004AVI?id=${item.id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(item),
                baseURL: useRuntimeConfig().public.apiBase,
            });

            if (error) {
                throw new Error('Failed to update data');
            }
        } else {
            // ถ้าไม่มี ID ให้สร้างใหม่
            item.id = uuidv4();
            // ตรวจสอบ userId จาก localStorage
            const userIdFromLocalStorage = localStorage.getItem('userId');
            if (!userIdFromLocalStorage) {
                swal.fire({
                    title: 'Error!',
                    text: 'User ID not found in local storage. Please log in again.',
                    icon: 'error',
                    confirmButtonText: 'OK',
                });
                return;
            }
            item.userId = userIdFromLocalStorage || item.userId || '';
            if (!item.userId) {
                throw new Error('User ID is required.');
            }
            // เพิ่มข้อมูลใหม่
            const { data, error } = await useFetch(`/api/SI24004AVI`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(item),
                baseURL: useRuntimeConfig().public.apiBase,
            });

            if (error) {
                throw new Error('Failed to add data');
            }
        }
        // รีโหลดข้อมูลใหม่หลังจาก submission
        try {
            await fetchData();  // สมมติว่า fetchData ใช้ในการดึงข้อมูลใหม่
        } catch (fetchError) {
            swal.fire({
                title: 'Error!',
                text: 'Failed to reload data after submission.',
                icon: 'error',
                confirmButtonText: 'OK',
            });
        }
    } catch (error) {
        swal.fire({
            title: 'Error!',
            text: 'An error occurred during submission.',
            icon: 'error',
            confirmButtonText: 'Back',
        });
    }
};


onMounted(async () => {
    await nextTick(async () => {
        await fetchData();
    })
    // ถ้าไม่มี token ให้กลับไปหน้า login
    if (!token.value) {
        router.push('/login');
    }
});

const data = ref(fetchedData.value || []);

function formatDate(dateString: string | null): string {
    if (!dateString) {
        return ''; // คืนค่าที่เหมาะสมถ้า dateString เป็น null หรือ undefined
    }
    const date = new Date(dateString.replace(/\.\d+Z$/, '')); // ตัดส่วนมิลลิวินาทีออกหากมี
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0'); // เดือนเริ่มจาก 0 จึงต้อง +1
    const year = date.getFullYear().toString().slice(-2); // ตัดเอาแค่สองหลักสุดท้ายของปี
    return `${day}/${month}/${year}`;
}

// ตั้งค่า default เมื่อ selectedItem เป็น null
const defaultRequest: RequestDTO = {
    id: '',
    requestCode: '',
    requestName: '',
    userId: '',
    requestDescription: '',
    requestDate: null,
    requestApprove: false,
    approveDate: null,
    active: true,
    isDeleted: false,
    attachement: [],
};

const resolvedRequest = computed(() => selectedItem.value || defaultRequest);


</script>
<template>

    <v-card>
        <v-card-title>
            <span style="font-size: 24px; font-weight: bold;">Control Version Program VIP and AVI</span>
        </v-card-title>
        <!-- <button @click="openModal">Add New Data</button> -->
        <v-row class="d-flex align-center" justify="space-between">
            <v-col cols="11">
                <v-text-field v-model="search" label="Search" placeholder="Search by request name or description"
                    style="padding-left: 1rem;" class="mb-4" />
            </v-col>
            <v-col cols="1">
                <v-btn color="primary" @click="openAddDialog"
                    style="margin-top: -1rem; padding-left: 0px; padding-right: 0px;">Add New</v-btn>
            </v-col>
        </v-row>

        <!-- Data Table -->
        <v-data-table :headers="headers" :items="filteredItems" :items-per-page="itemsPerPage" :page.sync="page"
            class="elevation-1">
            <template #item.no="{ index }">
                <div>{{ ((page - 1) * itemsPerPage) + 1 + index }}</div>
            </template>

            <!-- Custom Cells for Request Approve and Active -->
            <template #item.requestApprove="{ item }">
                <v-chip :style="{
                    backgroundColor: item.requestApprove ? '#d4edda' : '#f8d7da',
                    color: item.requestApprove ? '#155724' : '#721c24',
                    padding: '8px',
                    textAlign: 'center',
                }">
                    {{ item.requestApprove ? 'Pass' : 'Not Pass' }}
                </v-chip>
            </template>
            <template #item.approveDate="{ item }">
                {{ formatDate(item.approveDate) }}
            </template>
            <template #item.requestDate="{ item }">
                {{ formatDate(item.requestDate) }}
            </template>
            <template #item.active="{ item }">
                <v-chip :style="{
                    backgroundColor: item.active ? '#d4edda' : '#f8d7da',
                    color: item.active ? '#155724' : '#721c24',
                    padding: '8px',
                    textAlign: 'center',
                }">
                    {{ item.active ? 'Active' : 'Not Active' }}
                </v-chip>
            </template>

            <!-- Action Buttons -->
            <template #item.actions="{ item }">
                <IconBtn @click="handleEdit(item)">
                    <VIcon icon="tabler-edit" />
                </IconBtn>
                <IconBtn @click="handleDelete(item.id)" color="error">
                    <VIcon icon="tabler-trash" />
                </IconBtn>
            </template>
        </v-data-table>
    </v-card>

    <!-- Edit/Add Dialog -->
    <AddEditRequestDialog v-if="isRequestDialogVisible" v-model:isDialogVisible="isRequestDialogVisible"
        :request="resolvedRequest" @submit="onSubmit" />

</template>
