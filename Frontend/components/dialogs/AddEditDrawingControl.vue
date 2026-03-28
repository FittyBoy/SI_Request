<script setup lang="ts">
// ========================================
// IMPORTS
// ========================================
import { VForm } from 'vuetify/components/VForm'
import { ref, computed, watch, onMounted, watchEffect, nextTick, type PropType } from 'vue';
import Swal from 'sweetalert2';
import type { DwRequestDTO } from '@/types/dwRequest';
import * as pdfjsLib from 'pdfjs-dist/legacy/build/pdf';

// ========================================
// PDF.JS CONFIGURATION
// ========================================
pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdn.jsdelivr.net/npm/pdfjs-dist@3.11.174/build/pdf.worker.min.js';

// ========================================
// INTERFACES & TYPES
// ========================================
interface Attachment {
    Id: string;
    AttachmentName: string;
    AttachementType: string;
    AttachementPath: string;
}

interface RequestStatusResponse {
    statusSteps: any[];
    currentStep: string | null;
}

// ========================================
// PROPS & EMITS
// ========================================
const Props = defineProps({
    IsDialogVisible: Boolean,
    Request: Object as PropType<DwRequestDTO>,
});

const Emit = defineEmits(['update:IsDialogVisible', 'submit', 'refreshData']);

// ========================================
// CONSTANTS
// ========================================
const ReviseStatusId = '54415422-e4a1-4308-8a8f-bcfb2723ae3f';
const SwalInstance = Swal;

const FileHeaders = [
    { title: 'Attachment Name', value: 'AttachmentName' },
    { title: 'Actions', value: 'Actions', sortable: false }
];

// ========================================
// USER & AUTH DATA
// ========================================
const Token = useCookie('token');
const UserId = useCookie('userId');
const UserName = useCookie('userName');
const Role = useCookie('roleName');

const CurrentUser = ref({
    Id: UserId.value,
    Role: Role.value,
});

// ========================================
// REACTIVE REFS
// ========================================
// Form & UI References
const RefVFormRequest = ref<VForm>();
const PdfContainer = ref<HTMLElement | null>(null);
const PdfCanvas = ref(null);
const IsSubmitting = ref(false);

// PDF State
let IsRendering = false;
let CurrentPdfUrl = '';
const PdfUrlFormInput = ref("");

// Form Data
const FormInput = ref<DwRequestDTO>({
    Id: "",
    DrawingCode: "",
    RequestCode: "",
    DrawingName: "",
    SectionId: null,
    DrawingTypeId: null,
    StatusId: "",
    CreatedDate: "",
    CreatedBy: "",
    UpdateDate: null,
    UpdateBy: "",
    DrawingDescription: "",
    UserId: null,
    AttachmentId: null,
    Active: true,
    IsDelete: false,
    DrawingType: undefined,
    Section: undefined,
    Status: undefined,
    User: undefined,
    Attachment: [] as Attachment[],
});

// Dropdown Data
const SectionsDDR = ref<{ Id: string; SectionName: string }[]>([]);
const DrawingTypeDDR = ref<{ Id: string; DrawingName: string }[]>([]);
const SelectedSection = ref<string | null>(null);
const SelectedDrawing = ref<string | null>(null);

// Status & Progress
const statusSteps = ref<any[]>([]);
const currentStep = ref<string | null>(null);

// ========================================
// VALIDATION RULES
// ========================================
const RequiredValidator = (value: string) => !!value || 'This field is required.';

// ========================================
// COMPUTED PROPERTIES
// ========================================
const CanEdit = computed(() => {
    return CurrentUser.value.Role === "admin" || CurrentUser.value.Id === FormInput.value.UserId;
});

const CanApprove = computed(() => {
    const StatusRole = Role.value?.toLowerCase();
    const Status = FormInput.value.Status?.StatusName?.toLowerCase() || '';

    if (StatusRole === 'admin' || StatusRole === 'manager') {
        return true;
    }

    const AllowedOfficeStatuses = ['request', 'test', 'install', 'mass production'];
    const AllowedFactoryStatuses = ['modify', 'ecr'];

    if (StatusRole === 'office' && AllowedOfficeStatuses.includes(Status)) {
        return true;
    }
    if (StatusRole === 'factory' && AllowedFactoryStatuses.includes(Status)) {
        return true;
    }

    return false;
});

const IsAdmin = computed(() => Role.value === 'admin');

const ProgressWidth = computed(() => {
    if (statusSteps.value.length <= 1 || currentStep.value === null) return '0%';
    const CurrentIndex = Number(currentStep.value) - 1;
    const Percentage = (CurrentIndex / (statusSteps.value.length - 1)) * 100;
    return `${Percentage}%`;
});

// ========================================
// API COMPOSABLES
// ========================================
const { data: Data, error: Error, execute: FetchRequestStatus } = useFetch<RequestStatusResponse>(
    () => `/api/SI25007/GetRequestStatus/${Props.Request?.Id}`,
    {
        method: 'GET',
        baseURL: useRuntimeConfig().public.apiBase,
        credentials: 'include',
        headers: { Authorization: `Bearer ${Token.value}` },
        immediate: false,
    }
);

// ========================================
// API FUNCTIONS
// ========================================
const FetchSection = async () => {
    try {
        const { data, error } = await useFetch('/api/Roles/SectionDDR', {
            method: 'GET',
            baseURL: useRuntimeConfig().public.apiBase,
            credentials: 'include',
        });

        if (error.value) {
            throw new Error('โหลดสถานะไม่สำเร็จ');
        }

        SectionsDDR.value = Array.isArray(data.value) ? data.value : [];
    } catch (error) {
        console.error('เกิดข้อผิดพลาด:', error);
    }
};

const FetchDrawing = async () => {
    try {
        const { data, error } = await useFetch('/api/Roles/DrawingDDR', {
            method: 'GET',
            baseURL: useRuntimeConfig().public.apiBase,
            credentials: 'include',
        });

        if (error.value) {
            throw new Error('โหลดสถานะไม่สำเร็จ');
        }

        DrawingTypeDDR.value = Array.isArray(data.value) ? data.value : [];
    } catch (error) {
        console.error('เกิดข้อผิดพลาด:', error);
    }
};

// ========================================
// PDF FUNCTIONS
// ========================================
const RenderPdf = async (url: string) => {
    // ป้องกันการ render ซ้ำ
    if (IsRendering || CurrentPdfUrl === url) {
        return;
    }

    IsRendering = true;
    CurrentPdfUrl = url;

    // รอให้ DOM พร้อม
    await nextTick();

    if (!PdfContainer.value) {
        setTimeout(async () => {
            IsRendering = false;
            await RenderPdf(url);
        }, 100);
        return;
    }

    // เคลียร์เนื้อหาเก่า
    try {
        PdfContainer.value.innerHTML = '';
    } catch (error) {
        console.error('Error clearing container:', error);
        IsRendering = false;
        return;
    }

    try {
        const LoadingTask = pdfjsLib.getDocument({
            url,
            cMapUrl: 'https://cdn.jsdelivr.net/npm/pdfjs-dist@3.11.174/cmaps/',
            cMapPacked: true,
            withCredentials: true,
            httpHeaders: {
                'Authorization': `Bearer ${Token.value}`
            }
        });

        const Pdf = await LoadingTask.promise;

        // เพิ่ม loading indicator
        if (PdfContainer.value) {
            PdfContainer.value.innerHTML = '<div style="text-align: center; padding: 20px;">Loading PDF...</div>';
        }

        for (let PageNum = 1; PageNum <= Pdf.numPages; PageNum++) {
            if (!PdfContainer.value) {
                console.error('PdfContainer became null during rendering');
                break;
            }

            const Page = await Pdf.getPage(PageNum);
            const Viewport = Page.getViewport({ scale: 1.5 });

            const Canvas = document.createElement('canvas');
            const Context = Canvas.getContext('2d');
            if (!Context) {
                console.error('Failed to get 2D context');
                continue;
            }

            Canvas.height = Viewport.height;
            Canvas.width = Viewport.width;
            Canvas.style.display = 'block';
            Canvas.style.margin = '10px auto';
            Canvas.style.border = '1px solid #ddd';

            await Page.render({ canvasContext: Context, viewport: Viewport }).promise;

            // เคลียร์ loading indicator ก่อนเพิ่ม canvas แรก
            if (PageNum === 1 && PdfContainer.value) {
                PdfContainer.value.innerHTML = '';
            }

            if (PdfContainer.value) {
                PdfContainer.value.appendChild(Canvas);
            } else {
                console.error('PdfContainer is null, cannot append canvas');
                break;
            }
        }

    } catch (error) {
        console.error('Error rendering PDF:', error);
        if (PdfContainer.value) {
            PdfContainer.value.innerHTML = `
                <div style="text-align: center; padding: 20px; color: red;">
                    <p>Error loading PDF: ${error.message}</p>
                    <p>URL: ${url}</p>
                </div>
            `;
        }
    } finally {
        IsRendering = false;
    }
};
const CanDownload = computed(() => {
    const isAdmin = Role.value === 'admin';
    const isApproved = FormInput.value.Status?.StatusName?.toLowerCase() === 'approved';

    return isAdmin && isApproved;
});
// ========================================
// FILE FUNCTIONS
// ========================================
const DownloadFile = (index: number) => {
    if (!FormInput.value.Attachment || FormInput.value.Attachment.length === 0) {
        console.warn("No attachment found.");
        return;
    }
    const Item = FormInput.value.Attachment[0];
    // const fileUrl = `https://localhost:7247/api/SI25007/download/${encodeURIComponent(item.AttachmentName)}`;
    const FileUrl = `http://172.18.106.100:9011/api/SI25007/download/${encodeURIComponent(Item.AttachmentName)}`;

    const Link = document.createElement('a');
    Link.href = FileUrl;
    Link.setAttribute('download', Item.AttachmentName + Item.AttachementType);
    document.body.appendChild(Link);
    Link.click();
    document.body.removeChild(Link);
};

// ========================================
// FORM FUNCTIONS
// ========================================
const OnFormSubmit = async (status?: any) => {
    const ValidationResult = await RefVFormRequest.value?.validate();
    if (ValidationResult?.valid) {
        IsSubmitting.value = true;
        const SubmitFormData = new FormData();
        const ValidUserId = UserId.value ? UserId.value : '';
        const IsUpdate = !!FormInput.value.Id;
        const ApiUrl = IsUpdate ? `/api/SI25007` : `/api/SI25007`;

        if (IsUpdate) {
            SubmitFormData.append('Id', FormInput.value.Id);
        }

        SubmitFormData.append("Id", FormInput.value.Id || "");
        SubmitFormData.append("DrawingCode", FormInput.value.DrawingCode || "");
        SubmitFormData.append("RequestCode", FormInput.value.RequestCode || "");
        SubmitFormData.append("DrawingName", FormInput.value.DrawingName || "");
        SubmitFormData.append("SectionId", FormInput.value.SectionId || "");
        SubmitFormData.append("DrawingTypeId", FormInput.value.DrawingTypeId || "");
        SubmitFormData.append("StatusId", FormInput.value.StatusId || "");
        SubmitFormData.append("CreatedDate", FormInput.value.CreatedDate || "");
        SubmitFormData.append("CreatedBy", FormInput.value.CreatedBy || "");
        SubmitFormData.append("UpdateDate", FormInput.value.UpdateDate || "");
        SubmitFormData.append("UpdateBy", FormInput.value.UpdateBy || "");
        SubmitFormData.append("DrawingDescription", FormInput.value.DrawingDescription || "");
        SubmitFormData.append("UserId", FormInput.value.UserId || "");
        SubmitFormData.append("AttachmentId", FormInput.value.AttachmentId || "");
        SubmitFormData.append("Active", String(FormInput.value.Active ?? true));
        SubmitFormData.append("IsDelete", String(FormInput.value.IsDelete ?? false));

        if (status == "update") {
            SubmitFormData.append('IsApproved', 'false');
        }
        else if (status == "Approved") {
            SubmitFormData.append('IsApproved', 'true');
        }

        if (FormInput.value.Attachment && Array.isArray(FormInput.value.Attachment)) {
            FormInput.value.Attachment.forEach((attachment, index) => {
                if (attachment instanceof File) {
                    if (!['image/png', 'image/jpeg', 'application/pdf'].includes(attachment.type)) {
                        console.warn(`Invalid file type: ${attachment.type}. Only PNG, JPG, and PDF are allowed.`);
                        return;
                    }
                    SubmitFormData.append('AttachmentFile', attachment);
                }
            });
        }

        try {
            const { data, error } = await useFetch(ApiUrl, {
                method: IsUpdate ? 'PUT' : 'POST',
                baseURL: useRuntimeConfig().public.apiBase,
                body: SubmitFormData,
                headers: {
                    Authorization: `Bearer ${Token.value}`
                },
            });

            if (error.value) throw new Error(error.value.message);

            ShowSuccessAlert();
            Emit('refreshData');

            await nextTick();
            setTimeout(() => {
                Emit('update:IsDialogVisible', false);
            }, 500);

        } catch (error) {
            ShowErrorAlert();
        } finally {
            IsSubmitting.value = false;
        }
    }
};

const OnDialogClose = () => {
    FormInput.value = {
        Id: "",
        DrawingCode: "",
        RequestCode: "",
        DrawingName: "",
        SectionId: null,
        DrawingTypeId: null,
        StatusId: null,
        CreatedDate: "",
        CreatedBy: "",
        UpdateDate: null,
        UpdateBy: "",
        DrawingDescription: "",
        UserId: null,
        AttachmentId: null,
        Active: true,
        IsDelete: false,
        DrawingType: undefined,
        Section: undefined,
        Status: undefined,
        User: undefined,
        Attachment: [],
    };
    Emit('update:IsDialogVisible', false);
};

// ========================================
// CONFIRMATION FUNCTIONS
// ========================================
const ConfirmApprove = async () => {
    const Result = await SwalInstance.fire({
        title: "Are you sure?",
        text: "Do you want to approve this request?",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, approve it!",
        customClass: { popup: "swal-custom-zindex" }
    });

    if (!Result.isConfirmed) return;

    const Input = FormInput.value;
    const Payload = {
        Id: Input.Id,
        UserId: Input.UserId
    };

    try {
        const { data, error } = await useFetch("/api/SI25007/ApprovedData", {
            method: "PUT",
            baseURL: useRuntimeConfig().public.apiBase,
            body: Payload,
            headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${Token.value}`,
            },
        });

        if (error.value) throw new Error(error.value.message);
        await SwalInstance.fire("Updated!", "Your request has been updated.", "success");

        Emit('refreshData');

        await nextTick();
        setTimeout(() => {
            Emit('update:IsDialogVisible', false);
        }, 500);

    } catch (err: any) {
        console.error("❌ Error:", err);
        await SwalInstance.fire("Error!", err?.message ?? "An unexpected error occurred.", "error");
    }
};

const ConfirmUpdate = async () => {
    const Result = await SwalInstance.fire({
        title: "Are you sure?",
        text: "Do you want to update this request?",
        icon: "question",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, update it!",
        customClass: {
            popup: "swal-custom-zindex",
        },
    });

    if (!Result.isConfirmed) return;

    if (!FormInput.value || !FormInput.value.Id) {
        console.error("🚨 ข้อมูลไม่ถูกต้อง หรือไม่มี ID สำหรับการอัปเดต");
        return;
    }

    const UpdateFormData = new FormData();
    const Input = FormInput.value;

    const AppendIfNotNull = (key: string, value: any) => {
        if (value !== null && value !== undefined) {
            UpdateFormData.append(key, value.toString());
        }
    };

    AppendIfNotNull("Id", Input.Id);
    AppendIfNotNull("DrawingCode", Input.DrawingCode);
    AppendIfNotNull("RequestCode", Input.RequestCode);
    AppendIfNotNull("DrawingName", Input.DrawingName);
    AppendIfNotNull("SectionId", Input.SectionId);
    AppendIfNotNull("DrawingTypeId", Input.DrawingTypeId);
    AppendIfNotNull("StatusId", Input.StatusId);
    AppendIfNotNull("CreatedDate", Input.CreatedDate);
    AppendIfNotNull("CreatedBy", Input.CreatedBy);
    AppendIfNotNull("UpdateDate", Input.UpdateDate ?? new Date().toISOString());
    AppendIfNotNull("UpdateBy", Input.UpdateBy);
    AppendIfNotNull("DrawingDescription", Input.DrawingDescription);
    AppendIfNotNull("UserId", Input.UserId);
    AppendIfNotNull("AttachmentId", Input.AttachmentId);

    UpdateFormData.append("Active", String(Input.Active ?? true));
    UpdateFormData.append("IsDelete", String(Input.IsDelete ?? false));

    try {
        const { data, error } = await useFetch("/api/SI25007/UpdateData", {
            method: "PUT",
            baseURL: useRuntimeConfig().public.apiBase,
            body: UpdateFormData,
            headers: {
                Authorization: `Bearer ${Token.value}`,
            },
        });

        if (error.value) {
            throw new Error(error.value.message);
        }

        await SwalInstance.fire("Updated!", "Your request has been updated.", "success");

        Emit('refreshData');

        await nextTick();
        setTimeout(() => {
            Emit('update:IsDialogVisible', false);
        }, 500);

    } catch (err) {
        console.error("❌ เกิดข้อผิดพลาดระหว่างอัปเดต:", err);
        await SwalInstance.fire("Error!", "Update failed. Please try again.", "error");
    }
};

const ConfirmDelete = async () => {
    const Result = await Swal.fire({
        title: 'คุณแน่ใจหรือไม่?',
        text: 'คุณต้องการลบรายการนี้จริงหรือไม่?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'ใช่, ลบเลย!',
        cancelButtonText: 'ยกเลิก'
    });

    if (!Result.isConfirmed) return;

    try {
        const { data, error } = await useFetch(`/api/SI25007/${FormInput.value.Id}`, {
            method: 'DELETE',
            baseURL: useRuntimeConfig().public.apiBase,
            credentials: 'include',
        });

        if (error.value) {
            console.error('เกิดข้อผิดพลาดในการลบ:', error.value);
            Swal.fire({
                title: 'เกิดข้อผิดพลาด!',
                text: 'ไม่สามารถลบรายการได้.',
                icon: 'error',
                confirmButtonText: 'ตกลง',
            });
            return;
        }

        Swal.fire({
            title: 'ลบสำเร็จ!',
            text: 'รายการถูกลบเรียบร้อยแล้ว.',
            icon: 'success',
            confirmButtonText: 'ตกลง',
        });

        Emit('refreshData');
    } catch (fetchError) {
        console.error('เกิดข้อผิดพลาดในการโหลดข้อมูลใหม่:', fetchError);
        Swal.fire({
            title: 'เกิดข้อผิดพลาด!',
            text: 'ไม่สามารถโหลดข้อมูลใหม่หลังจากการลบได้.',
            icon: 'error',
            confirmButtonText: 'ตกลง',
        });
    }
};

// ========================================
// DEBUG FUNCTIONS
// ========================================
// ========================================
// ALERT FUNCTIONS
// ========================================
const ShowSuccessAlert = () => {
    SwalInstance.fire({
        title: 'Success!',
        text: 'Data added successfully.',
        icon: 'success',
        confirmButtonText: 'OK',
    });
};

const ShowErrorAlert = () => {
    SwalInstance.fire({
        title: 'Error!',
        text: 'An error occurred while submitting data.',
        icon: 'error',
        confirmButtonText: 'Back',
    });
};

// ========================================
// WATCHERS
// ========================================
watch(() => Props.Request, (newRequest) => {
    if (!newRequest) {
        console.warn("Props.Request is not defined.");
        return;
    }

    FormInput.value = {
        Id: newRequest.Id ?? "",
        DrawingCode: newRequest.DrawingCode ?? "",
        RequestCode: newRequest.RequestCode ?? "",
        DrawingName: newRequest.DrawingName ?? "",
        SectionId: newRequest.SectionId ?? null,
        DrawingTypeId: newRequest.DrawingTypeId ?? null,
        StatusId: newRequest.StatusId ?? "",
        CreatedDate: newRequest.CreatedDate ?? "",
        CreatedBy: newRequest.CreatedBy ?? "",
        UpdateDate: newRequest.UpdateDate ?? null,
        UpdateBy: newRequest.UpdateBy ?? "",
        DrawingDescription: newRequest.DrawingDescription ?? "",
        UserId: newRequest.UserId ?? null,
        AttachmentId: newRequest.AttachmentId ?? null,
        Active: newRequest.Active ?? true,
        IsDelete: newRequest.IsDelete ?? false,
        DrawingType: newRequest.DrawingType ?? undefined,
        Section: newRequest.Section ?? undefined,
        Status: newRequest.Status ?? undefined,
        User: newRequest.User ?? undefined,
        Attachment: Array.isArray(newRequest.Attachment)
            ? newRequest.Attachment.map((att: any) => ({
                Id: att.Id?.toString() ?? "",
                AttachmentName: att.AttachmentName ?? "",
                AttachementType: att.AttachementType ?? "",
                AttachementPath: att.AttachementPath ?? "",
            }))
            : newRequest.Attachment
                ? [{
                    Id: newRequest.Attachment.Id?.toString() ?? "",
                    AttachmentName: newRequest.Attachment.AttachmentName ?? "",
                    AttachementType: newRequest.Attachment.AttachementType ?? "",
                    AttachementPath: newRequest.Attachment.AttachementPath ?? "",
                }]
                : [],
    };
}, { deep: true, immediate: true });

watch(() => Props.Request?.Id, (newId) => {
    if (newId) FetchRequestStatus();
});

watch(SelectedSection, (newValue) => {
    FormInput.value.SectionId = newValue && newValue !== "" ? newValue : null;
});

watch(SelectedDrawing, (newValue) => {
    FormInput.value.DrawingTypeId = newValue && newValue !== "" ? newValue : null;
});

watch(
    () => FormInput.value?.Attachment?.[0]?.AttachmentName,
    async (newAttachmentName) => {

        if (newAttachmentName && Role.value) {
            const PdfUrl = `${useRuntimeConfig().public.apiBase}/api/SI25007/pdf/${encodeURIComponent(newAttachmentName)}`;

            PdfUrlFormInput.value = PdfUrl;

            await nextTick();

            if (PdfContainer.value) {
                await RenderPdf(PdfUrl);
            } else {
                setTimeout(async () => {
                    if (PdfContainer.value) {
                        await RenderPdf(PdfUrl);
                    } else {
                        console.error('📄 Container still not ready after timeout');
                    }
                }, 500);
            }
        } else {
            PdfUrlFormInput.value = '';
        }
    },
    { immediate: true }
);

watch(
    () => Props.IsDialogVisible,
    async (isVisible) => {
        if (isVisible && FormInput.value?.Attachment?.[0]?.AttachmentName) {
            await nextTick();
            setTimeout(async () => {
                const AttachmentName = FormInput.value?.Attachment?.[0]?.AttachmentName;
                if (AttachmentName && Role.value) {
                    const PdfUrl = `${useRuntimeConfig().public.apiBase}/api/SI25007/pdf/${encodeURIComponent(AttachmentName)}`;
                    PdfUrlFormInput.value = PdfUrl;
                    if (PdfContainer.value) {
                        await RenderPdf(PdfUrl);
                    }
                }
            }, 300);
        }
    }
);

// ========================================
// WATCH EFFECTS
// ========================================
watchEffect(async () => {
    if (Data.value) {
        let Steps = Data.value.statusSteps ?? [];

        if (FormInput.value.StatusId === ReviseStatusId && Steps.length > 0) {
            Steps = [...Steps];
            Steps[0] = 'Revise';
        }

        statusSteps.value = Steps;
        currentStep.value = Data.value.currentStep ?? null;
    }

    if (FormInput.value?.Attachment?.[0]?.AttachmentName && Role.value) {
        const PdfUrl = `${useRuntimeConfig().public.apiBase}/api/SI25007/pdf/${encodeURIComponent(FormInput.value.Attachment[0].AttachmentName)}`;

        if (CurrentPdfUrl !== PdfUrl) {
            await nextTick();
        }
    }
});

watchEffect(() => {
    if (Props.Request) {
        SelectedSection.value = Props.Request.SectionId ?? null;
        SelectedDrawing.value = Props.Request.DrawingTypeId ?? null;
    }
});

// ========================================
// LIFECYCLE HOOKS
// ========================================
onMounted(async () => {
    await nextTick();

    // โหลดข้อมูลพื้นฐาน
    await FetchRequestStatus();
    await FetchSection();
    await FetchDrawing();

    // ถ้ามี attachment ให้ลอง render PDF
    if (FormInput.value?.Attachment?.[0]?.AttachmentName && Role.value) {
        const AttachmentName = FormInput.value.Attachment[0].AttachmentName;
        const PdfUrl = `${useRuntimeConfig().public.apiBase}/api/SI25007/pdf/${encodeURIComponent(AttachmentName)}`;

        PdfUrlFormInput.value = PdfUrl;

        // รอสักครู่เพื่อให้ DOM render เสร็จ
        setTimeout(async () => {
            if (PdfContainer.value) {
                await RenderPdf(PdfUrl);
            } else {
                console.error('📄 PDF Container not available on mount');
            }
        }, 1000);
    }
});
</script>

<template>
    <VDialog :model-value="Props.IsDialogVisible" @update:model-value="OnDialogClose">
        <VCard>
            <VCardTitle>
                {{ FormInput.Id != '' ? 'Edit Request' : 'Add New Request' }}
            </VCardTitle>

            <!-- Progress Stepper -->
            <VContainer v-if="FormInput.Id">
                <div class="stepper-container">
                    <div class="progress-line" :style="{ width: ProgressWidth }"></div>
                    <div v-for="(step, index) in statusSteps" :key="index" class="step-item">
                        <div class="step-circle" :class="{
                            active: index + 1 == currentStep && currentStep != statusSteps.length,
                            completed: index + 1 < currentStep || (index + 1 == currentStep && currentStep == statusSteps.length)
                        }">
                            <span
                                v-if="index + 1 < currentStep || (index + 1 == currentStep && currentStep == statusSteps.length)">✔</span>
                            <span v-else>{{ index + 1 }}</span>
                        </div>
                        <div class="step-label">{{ step }}</div>
                    </div>
                </div>
            </VContainer>

            <!-- Form Content -->
            <VCardText>
                <VForm ref="RefVFormRequest" @submit.prevent="OnFormSubmit">
                    <!-- Basic Information -->
                    <VRow>
                        <VCol cols="6">
                            <AppTextField v-model="FormInput.RequestCode" label="Request Code"
                                placeholder="Request Code Auto Generate" disabled />
                        </VCol>
                        <VCol cols="6">
                            <AppTextField v-model="FormInput.DrawingCode" :rules="[RequiredValidator]"
                                label="Drawing Code" placeholder="Enter Drawing Code" :readonly="!CanEdit" />
                        </VCol>
                        <VCol cols="6">
                            <AppTextField v-model="FormInput.DrawingName" :rules="[RequiredValidator]"
                                label="Drawing Name" placeholder="ชื่อ Drawing" required :readonly="!CanEdit" />
                        </VCol>
                        <VCol cols="6" style="margin-top: 1.5rem;">
                            <VSelect v-model="SelectedSection" :items="SectionsDDR" item-title="SectionName"
                                item-value="Id" label="เลือก Section" :disabled="!CanEdit" />
                        </VCol>
                    </VRow>

                    <!-- Drawing Type Selection -->
                    <VRow>
                        <VCol cols="6" style="margin-top: 1.5rem;">
                            <VSelect
                                v-model="FormInput.DrawingName"
                                :items="['Part', 'Machine', 'Table', 'Jig', 'Tool', 'Other']"
                                label="ประเภท Drawing"
                                :disabled="!CanEdit"
                            />
                        </VCol>
                        <VCol cols="6"></VCol>
                    </VRow>

                    <!-- Description -->
                    <VRow>
                        <VCol cols="12">
                            <v-textarea v-model="FormInput.DrawingDescription" :rules="[RequiredValidator]"
                                label="Request Description" placeholder="กรุณากรอกรายละเอียด" required
                                :readonly="!CanEdit" />
                        </VCol>
                    </VRow>

                    <!-- File Upload -->
                    <VRow>
                        <VCol cols="12">
                            <v-file-input v-model="FormInput.Attachment" label="Upload New File" variant="outlined"
                                multiple accept="*" :disabled="!!FormInput.AttachmentId" />
                        </VCol>
                    </VRow>

                    <!-- PDF Viewer -->
                    <VRow>
                        <VCol cols="12" v-if="FormInput.Attachment && FormInput.Attachment.length > 0">
                            <v-card>
                                <v-card-title class="d-flex justify-space-between">
                                    <span>PDF Viewer - {{ FormInput.Attachment[0]?.AttachmentName }}</span>
                                    <v-btn v-if="CanDownload" @click="DownloadFile(0)" color="primary" size="small"
                                        :disabled="!CanDownload">
                                        <v-icon start>mdi-download</v-icon>
                                        Download
                                    </v-btn>
                                    <v-tooltip
                                        v-else-if="IsAdmin && FormInput.Status?.StatusName?.toLowerCase() !== 'approved'"
                                        location="bottom">
                                        <template v-slot:activator="{ props }">
                                            <v-btn v-bind="props" color="grey" size="small" disabled>
                                                <v-icon start>mdi-download</v-icon>
                                                Download
                                            </v-btn>
                                        </template>
                                        <span>Download available only for approved projects</span>
                                    </v-tooltip>
                                </v-card-title>
                                <v-card-text style="height:600px; padding:8px;" @contextmenu.prevent>
                                    <div ref="PdfContainer" class="debug-pdf-container" @contextmenu.prevent style="
                                            width: 100%; 
                                            height: 100%; 
                                            overflow: auto; 
                                            border: 1px solid #ccc;
                                            background: #f5f5f5;
                                            display: flex;
                                            flex-direction: column;
                                            align-items: center;
                                        ">
                                        <div v-if="!PdfUrlFormInput"
                                            style="padding: 20px; text-align: center; color: #666;">
                                            No PDF to display
                                        </div>
                                    </div>
                                </v-card-text>
                            </v-card>
                        </VCol>
                        <VCol cols="12" v-else-if="FormInput.Id">
                            <v-card>
                                <v-card-text style="text-align: center; padding: 40px; color: #666;">
                                    <v-icon size="48" color="grey">mdi-file-pdf-box</v-icon>
                                    <p style="margin-top: 16px;">No attachment found</p>
                                </v-card-text>
                            </v-card>
                        </VCol>
                    </VRow>

                    <!-- Active Switch -->
                    <VRow>
                        <VCol cols="6">
                            <VRow>
                                <VCol cols="12">
                                    <v-switch style="margin-top: 1rem;" v-model="FormInput.Active"
                                        label="Request Active" :disabled="!CanEdit" />
                                </VCol>
                            </VRow>
                        </VCol>
                    </VRow>

                    <!-- Action Buttons -->
                    <VRow class="mt-4">
                        <!-- Add New Form Buttons -->
                        <VCol cols="12" class="d-flex justify-center gap-4" v-if="!FormInput.Id && CanEdit">
                            <VBtn :loading="IsSubmitting" type="submit">Submit</VBtn>
                            <VBtn color="secondary" variant="tonal" @click="OnDialogClose">Cancel</VBtn>
                        </VCol>

                        <!-- Edit Form Buttons -->
                        <VCol cols="12" v-if="FormInput.Id">
                            <VRow class="mt-4">
                                <VBtn v-if="CanEdit" color="danger" variant="tonal" class="ml-2" @click="ConfirmDelete">
                                    Delete
                                </VBtn>
                                <VSpacer />
                                <VBtn v-if="CanEdit" :loading="IsSubmitting" class="ml-2" @click="ConfirmUpdate">
                                    Update
                                </VBtn>
                                <VBtn color="secondary" class="ml-2" @click="OnDialogClose">
                                    Cancel
                                </VBtn>
                                <VBtn
                                    v-if="CanApprove && currentStep < statusSteps.length && Props.Request?.Status?.StatusName !== 'Reject'"
                                    :loading="IsSubmitting" class="ml-2" @click="ConfirmApprove">
                                    Approve
                                </VBtn>
                            </VRow>
                        </VCol>
                    </VRow>
                </VForm>
            </VCardText>
        </VCard>
    </VDialog>
</template>

<style scoped>
/* ========================================
   DIALOG CARD STYLES
   ======================================== */
.v-card {
    border-radius: 16px;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.12);
}

.v-card-title {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    padding: 24px;
    font-size: 24px;
    font-weight: 600;
    border-radius: 16px 16px 0 0;
}

.v-card-text {
    padding: 32px;
    background: #fafafa;
}

/* ========================================
   STEPPER STYLES
   ======================================== */
.stepper-container {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    position: relative;
    width: 100%;
    padding: 28px 24px 20px;
    background: #fafafa;
    border-radius: 14px;
    margin-bottom: 20px;
    border: 1px solid #f0f0f0;
}

/* Base track */
.stepper-container::before {
    content: '';
    position: absolute;
    top: 42px;
    left: 60px;
    right: 60px;
    height: 3px;
    background: #e8e8e8;
    border-radius: 99px;
    z-index: 0;
}

/* Active progress fill */
.progress-line {
    position: absolute;
    top: 42px;
    left: 60px;
    height: 3px;
    background: linear-gradient(90deg, #28c76f 0%, #7367f0 100%);
    border-radius: 99px;
    transition: width 0.7s cubic-bezier(0.4, 0, 0.2, 1);
    z-index: 1;
}

.step-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    position: relative;
    z-index: 2;
    flex: 1;
    gap: 10px;
}

.step-circle {
    width: 44px;
    height: 44px;
    border-radius: 50%;
    background: #fff;
    border: 2.5px solid #e0e0e0;
    display: flex;
    justify-content: center;
    align-items: center;
    font-weight: 700;
    font-size: 15px;
    color: #bbb;
    transition: all 0.35s cubic-bezier(0.34, 1.56, 0.64, 1);
    box-shadow: 0 2px 8px rgba(0,0,0,0.06);
    position: relative;
}

/* Glow ring for active */
.step-circle::after {
    content: '';
    position: absolute;
    inset: -5px;
    border-radius: 50%;
    background: transparent;
    border: 2px solid transparent;
    transition: all 0.35s ease;
}

.step-circle.active {
    background: #7367f0;
    border-color: #7367f0;
    color: #fff;
    transform: scale(1.18);
    box-shadow: 0 4px 18px rgba(115,103,240,0.45);
}

.step-circle.active::after {
    border-color: rgba(115,103,240,0.25);
    animation: ring-pulse 1.8s ease-out infinite;
}

.step-circle.completed {
    background: #28c76f;
    border-color: #28c76f;
    color: #fff;
    transform: scale(1.08);
    box-shadow: 0 4px 14px rgba(40,199,111,0.35);
}

/* Checkmark bounce in */
.step-circle.completed span {
    animation: check-pop 0.3s cubic-bezier(0.34, 1.56, 0.64, 1) both;
}

.step-label {
    font-size: 12px;
    font-weight: 600;
    color: #aaa;
    text-align: center;
    letter-spacing: 0.03em;
    text-transform: uppercase;
    transition: all 0.3s ease;
    white-space: nowrap;
}

.step-item:has(.step-circle.active) .step-label {
    color: #7367f0;
    font-size: 12px;
}

.step-item:has(.step-circle.completed) .step-label {
    color: #28c76f;
}

@keyframes ring-pulse {
    0%   { transform: scale(1);   opacity: 1; }
    100% { transform: scale(1.5); opacity: 0; }
}

@keyframes check-pop {
    from { transform: scale(0) rotate(-15deg); opacity: 0; }
    to   { transform: scale(1) rotate(0deg);  opacity: 1; }
}

@keyframes pulse {
    0%, 100% { box-shadow: 0 4px 18px rgba(115,103,240,0.45); }
    50%       { box-shadow: 0 4px 28px rgba(115,103,240,0.65); }
}

@keyframes ripple {
    0%   { box-shadow: 0 0 0 0  rgba(115,103,240,0.4); }
    100% { box-shadow: 0 0 0 18px rgba(115,103,240,0); }
}

/* ========================================
   FORM STYLES
   ======================================== */
.v-row {
    margin-bottom: 8px;
}

.v-text-field,
.v-select,
.v-textarea {
    border-radius: 8px;
}

:deep(.v-field) {
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
    transition: all 0.3s ease;
}

:deep(.v-field:hover) {
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}

:deep(.v-field--focused) {
    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.2);
}

/* ========================================
   BUTTON STYLES
   ======================================== */
.v-btn {
    border-radius: 8px;
    font-weight: 600;
    text-transform: none;
    letter-spacing: 0.5px;
    padding: 12px 32px;
    transition: all 0.3s ease;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.v-btn:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
}

.v-btn:active {
    transform: translateY(0);
}

.v-btn[color="primary"] {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.v-btn[color="danger"] {
    background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
}

.v-btn[color="secondary"] {
    background: linear-gradient(135deg, #a8edea 0%, #fed6e3 100%);
}

/* ========================================
   PDF VIEWER STYLES
   ======================================== */
.v-card .v-card-title {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    border-radius: 8px 8px 0 0;
    padding: 16px 20px;
}

.debug-pdf-container {
    border-radius: 8px;
    background: linear-gradient(to bottom, #f8f9fa, #ffffff);
    box-shadow: inset 0 2px 8px rgba(0, 0, 0, 0.05);
}

.debug-pdf-container canvas {
    border-radius: 4px;
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
    margin: 16px auto;
    transition: transform 0.3s ease;
}

.debug-pdf-container canvas:hover {
    transform: scale(1.02);
}

/* ========================================
   FILE INPUT STYLES
   ======================================== */
:deep(.v-file-input .v-field) {
    border: 2px dashed #667eea;
    background: #f8f9ff;
    transition: all 0.3s ease;
}

:deep(.v-file-input .v-field:hover) {
    border-color: #764ba2;
    background: #f0f2ff;
}

/* ========================================
   SWITCH STYLES
   ======================================== */
:deep(.v-switch .v-selection-control__input) {
    color: #667eea;
}

:deep(.v-switch .v-selection-control__input:hover) {
    opacity: 0.8;
}

/* ========================================
   SWEETALERT STYLES
   ======================================== */
.swal2-container {
    z-index: 99999 !important;
}

.swal-custom-zindex {
    z-index: 99999 !important;
}

/* ========================================
   RESPONSIVE STYLES
   ======================================== */
@media (max-width: 768px) {
    .stepper-container {
        padding: 30px 10px;
    }
    
    .step-circle {
        width: 40px;
        height: 40px;
        font-size: 14px;
    }
    
    .step-label {
        font-size: 12px;
    }
    
    .v-card-title {
        font-size: 20px;
        padding: 20px;
    }
    
    .v-card-text {
        padding: 20px;
    }
}

/* ========================================
   SCROLLBAR STYLES
   ======================================== */
::-webkit-scrollbar {
    width: 8px;
    height: 8px;
}

::-webkit-scrollbar-track {
    background: #f1f1f1;
    border-radius: 4px;
}

::-webkit-scrollbar-thumb {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    border-radius: 4px;
}

::-webkit-scrollbar-thumb:hover {
    background: linear-gradient(135deg, #764ba2 0%, #667eea 100%);
}
</style>