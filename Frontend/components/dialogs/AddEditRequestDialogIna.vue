<script setup lang="ts">
import { VForm } from 'vuetify/components/VForm';
import { onMounted, watch, watchEffect, ref, computed, nextTick } from 'vue';
import Swal from 'sweetalert2';
import type { InaRequestDTO } from '@/types/inaRequest';

import '@mdi/font/css/materialdesignicons.css';
import { v4 as uuidv4 } from 'uuid';

// ===================================
// CONSTANTS & GLOBALS
// ===================================
const swal = Swal;
const token = useCookie('token');
const userId = useCookie('userId');
const role = useCookie('roleName');
const maxLotNumbers = 10;

// ===================================
// PROPS & EMITS
// ===================================
const props = defineProps({
    isDialogVisible: Boolean,
    request: Object as PropType<InaRequestDTO>,
});

const emit = defineEmits(['update:isDialogVisible', 'submit', 'refreshData']);

// ===================================
// REACTIVE REFS
// ===================================
const refVFormRequest = ref<VForm>();
const isSubmitting = ref(false);
const loadingMachines = ref(false);

// Lot Numbers
const lotNumbers = ref<string[]>(['']);

// Machine Options
const machineOptions = ref<any[]>([]);
const machineSearchText = ref('');

// Attachments
const attachments = ref<any[]>([]);
const recipeAttachment = ref<any>(null);

// Other Programs - เปลี่ยนเป็น array สำหรับมากกว่า 1
const otherProgramsAttachments = ref<any[]>([]);

// Status & Steps
const statusSteps = ref<any[]>([]);
const currentStep = ref<string | null>(null);

const formInput = ref<InaRequestDTO>({
    Id: '',
    RequestCode: '',
    UserId: '',
    RequestDescription: '', // สามารถ blank ได้
    StatusId: undefined,
    Active: true,
    IsDeleted: false,
    RequestMachine: '',
    RequestProduct: '',
    RequestMass: false,
    RequestTest: true,
    AttachmentId: '',
    Status: {
        Id: undefined,
        StatusName: '',
        Ordinal: undefined,
        IsDeleted: false,
    },
    RequestComment1: '',
    RequestComment2: '',
    RequestComment3: '',
    Recipe: '',
    OtherPrograms: '', // จะถูกแปลงเป็น array ใน computed
    RequestBy: '',
    RequestProcess: '',
    RequestPurpose: '', // ย้าย Purpose มาช่องแรก
    RequestStartDate: null,
    RequestFinishDate: null,
    Attachments: [] as any[],
    LotNumbers: [] as string[],
    FlTgDeleted: false,
    FlMcDeleted: false,
    FlCheckMass: false,
    FlDeletedOther: false,
    CtCopyRp: false,
    CtRpDeleted: false,
    CtBookCheck: false,
    CtDeletedOther: false,
    FlTgDeletedComment: '',
    FlMcDeletedComment: '',
    FlCheckMassComment: '',
    FlDeletedOtherComment: '',
    CtCopyRpComment: '',
    CtRpDeletedComment: '',
    CtBookCheckComment: '',
    CtDeletedOtherComment: '',
    RequestMcNo: '',
    RequestBook: '',
    RequestInstallDate: null,
    RequestClearDate: null,
});

// ===================================
// COMPUTED PROPERTIES
// ===================================
const progressWidth = computed(() => {
    const stepCount = statusSteps.value.length;
    const current = Number(currentStep.value);
    if (stepCount <= 1 || !current) return '0%';
    return `${((current - 1) / (stepCount - 1)) * 100}%`;
});

const canApprove = computed(() => {
    const statusRole = role.value?.toLowerCase();
    const status = formInput.value.Status?.StatusName.toLowerCase() || '';

    if (statusRole === 'admin' || statusRole === 'manager') return true;

    const allowedOfficeStatuses = ['request', 'test', 'install', 'mass production'];
    const allowedFactoryStatuses = ['modify', 'ecr'];

    if (statusRole === 'office' && allowedOfficeStatuses.includes(status)) return true;
    if (statusRole === 'factory' && allowedFactoryStatuses.includes(status)) return true;

    return false;
});
const canApproveWithComments = computed(() => {
    if (!canApprove.value) return false;

    const commentValidation = validateRequiredComments();
    return commentValidation.isValid;
});


const canAddLotNumber = computed(() => {
    return lotNumbers.value.length < maxLotNumbers;
});

// Machine filtering for autocomplete
const filteredMachineOptions = computed(() => {
    if (!machineSearchText.value) return machineOptions.value;

    return machineOptions.value.filter(machine =>
        machine.name.toLowerCase().includes(machineSearchText.value.toLowerCase())
    );
});

// เพิ่ม computed สำหรับแสดงชื่อเครื่องจักร
const selectedMachineName = computed(() => {
    if (!formInput.value.RequestMachine) return '';

    const selectedMachine = machineOptions.value.find(
        machine => machine.value === formInput.value.RequestMachine
    );

    return selectedMachine ? selectedMachine.name : formInput.value.RequestMachine;
});

// Other Programs จัดการเป็น array
const otherProgramsList = computed({
    get: () => {
        if (Array.isArray(formInput.value.OtherPrograms)) {
            return formInput.value.OtherPrograms;
        }
        // แปลง string เป็น array ถ้าจำเป็น
        if (typeof formInput.value.OtherPrograms === 'string' && formInput.value.OtherPrograms) {
            return formInput.value.OtherPrograms.split(',').map(p => p.trim()).filter(p => p);
        }
        return [];
    },
    set: (value: string[]) => {
        formInput.value.OtherPrograms = value;
    }
});
const canFinishTestWithComments = computed(() => {
    if (formInput.value.Status?.Ordinal !== 4) return false;

    const commentValidation = validateRequiredComments();
    return commentValidation.isValid;
});

// ===================================
// VALIDATION RULES
// ===================================
const requiredValidator = (value: string) => !!value || 'This field is required.';

// ดัก Lot Number validation - แค่ required field
const lotNumberValidator = (value: string) => {
    if (!value || value.trim() === '') return 'Lot Number is required.';
    return true;
};

// ===================================
// UTILITY FUNCTIONS
// ===================================
const formatDateForInput = (dateString: string | null | undefined): string => {
    if (!dateString) return '';

    try {
        const date = new Date(dateString);
        return date.toISOString().split('T')[0];
    } catch (error) {
        console.error('Error formatting date:', error);
        return '';
    }
};

const formatDateForAPI = (dateString: string): string => {
    if (!dateString) return '';
    try {
        const date = new Date(dateString);
        return date.toISOString();
    } catch (error) {
        console.error('Error formatting date for API:', error);
        return '';
    }
};

const copyPath = async (path: string) => {
    try {
        await navigator.clipboard.writeText(path);
        // ใช้ toast แทน Swal เพื่อไม่ให้ปิด modal หลัก
        Swal.fire({
            icon: 'success',
            title: 'คัดลอก Path สำเร็จ',
            text: path,
            timer: 1500,
            showConfirmButton: false,
            toast: true,
            position: 'top-end',
            backdrop: false,
            allowOutsideClick: true,
            allowEscapeKey: true
        });
    } catch (err) {
        Swal.fire({
            icon: 'error',
            title: 'เกิดข้อผิดพลาด',
            text: 'ไม่สามารถคัดลอก path ได้',
            toast: true,
            position: 'top-end',
            timer: 2000,
            showConfirmButton: false,
            backdrop: false,
            allowOutsideClick: true,
            allowEscapeKey: true
        });
        console.error(err);
    }
};

const validateRequiredComments = () => {
    const ordinal = formInput.value.Status?.Ordinal || 0;
    const errors: string[] = [];

    if (ordinal >= 2 && (!formInput.value.RequestComment1 || formInput.value.RequestComment1.trim() === '')) {
        errors.push('Form Confirm comment is required');
    }

    if (ordinal >= 3 && (!formInput.value.RequestComment2 || formInput.value.RequestComment2.trim() === '')) {
        errors.push('Form Modify comment is required');
    }

    if (ordinal >= 4 && (!formInput.value.RequestComment3 || formInput.value.RequestComment3.trim() === '')) {
        errors.push('Form Test comment is required');
    }

    return {
        isValid: errors.length === 0,
        errors: errors
    };
};

// ===================================
// MACHINE FUNCTIONS
// ===================================
const handleMachineKeydown = (event: KeyboardEvent) => {
    // แก้ปัญหา Enter select ไม่ได้
    if (event.key === 'Enter') {
        event.preventDefault();
        event.stopPropagation();

        const filteredOptions = filteredMachineOptions.value;
        if (filteredOptions.length > 0 && machineSearchText.value) {
            // หาตัวเลือกที่ตรงกับการพิมพ์
            const exactMatch = filteredOptions.find(machine =>
                machine.name.toLowerCase() === machineSearchText.value.toLowerCase()
            );

            if (exactMatch) {
                formInput.value.RequestMachine = exactMatch.value;
                machineSearchText.value = exactMatch.name;
            } else {
                // เลือกตัวแรกถ้าไม่มีที่ตรงกันทุกอักษร
                formInput.value.RequestMachine = filteredOptions[0].value;
                machineSearchText.value = filteredOptions[0].name;
            }
        }
    }
};

const handleMachineSelect = (selectedValue: any) => {
    formInput.value.RequestMachine = selectedValue;
    const selectedMachine = machineOptions.value.find(machine => machine.value === selectedValue);
    if (selectedMachine) {
        machineSearchText.value = selectedMachine.name;
    }
};

// ===================================
// LOT NUMBER FUNCTIONS
// ===================================
const addLotNumber = () => {
    if (canAddLotNumber.value) {
        lotNumbers.value.push('');
    }
};

const removeLotNumber = (index: number) => {
    if (lotNumbers.value.length > 1) {
        lotNumbers.value.splice(index, 1);
    }
};

const fetchLots = async (requestId: string) => {
    try {
        const { data, error } = await useFetch(`/api/SI24001INA/GetLots/${requestId}`, {
            method: 'GET',
            baseURL: useRuntimeConfig().public.apiBase,
            headers: { Authorization: `Bearer ${token.value}` },
        });

        if (error.value) throw error.value;

        if (data.value && Array.isArray(data.value)) {
            lotNumbers.value = data.value.map(lot => lot.LotNo || lot.lotNo);
        }
    } catch (error) {
        console.error('Error fetching lots:', error);
    }
};

const deleteLot = async (lotId: string) => {
    try {
        const { data, error } = await useFetch(`/api/SI24001INA/DeleteLot/${lotId}`, {
            method: 'DELETE',
            baseURL: useRuntimeConfig().public.apiBase,
            headers: { Authorization: `Bearer ${token.value}` },
        });

        if (error.value) throw error.value;

        if (formInput.value.Id) {
            await fetchLots(formInput.value.Id);
        }
    } catch (error) {
        console.error('Error deleting lot:', error);
    }
};

// ===================================
// ATTACHMENT FUNCTIONS
// ===================================
const addAttachment = (type: string) => {
    const newAttachment = {
        Id: '',
        AttachmentName: '',
        AttachementPath: '',
        AttachementType: '',
        AttachmentSize: null,
        AttachementFileData: null,
        Category: type,
        IsDeleted: false,
    };

    switch (type) {
        case 'recipe':
            recipeAttachment.value = newAttachment;
            break;
        case 'otherprograms':
            otherProgramsAttachments.value.push(newAttachment);
            break;
        default:
            attachments.value.push(newAttachment);
            break;
    }
};

const removeAttachment = (index: number) => {
    const attachment = attachments.value[index];

    if (attachment && attachment.Id) {
        attachment.IsDeleted = true;
    } else {
        attachments.value.splice(index, 1);
    }

    attachments.value = [...attachments.value];
};

const removeRecipeAttachment = () => {
    if (recipeAttachment.value) {
        if (recipeAttachment.value.Id) {
            recipeAttachment.value.IsDeleted = true;
        } else {
            recipeAttachment.value = null;
        }
    }
};

const removeOtherProgramsAttachment = (index: number) => {
    const attachment = otherProgramsAttachments.value[index];

    if (attachment && attachment.Id) {
        attachment.IsDeleted = true;
    } else {
        otherProgramsAttachments.value.splice(index, 1);
    }

    otherProgramsAttachments.value = [...otherProgramsAttachments.value];
};

const getTotalAttachments = () => {
    let total = attachments.value.filter(att => !att.IsDeleted).length;
    if (recipeAttachment.value && !recipeAttachment.value.IsDeleted) total += 1;
    total += otherProgramsAttachments.value.filter(att => !att.IsDeleted).length;
    return total;
};

const clearAllAttachments = () => {
    attachments.value = [];
    recipeAttachment.value = null;
    otherProgramsAttachments.value = [];
};

const loadAttachmentsFromData = (attachmentData: any) => {
    if (!attachmentData) return;

    clearAllAttachments();

    // Handle new structure
    if (attachmentData.Recipe && Array.isArray(attachmentData.Recipe) && attachmentData.Recipe.length > 0) {
        recipeAttachment.value = {
            ...attachmentData.Recipe[0],
            Category: 'recipe',
            IsDeleted: false,
            AttachmentSize: attachmentData.Recipe[0].AttachmentSize || null
        };
    }

    // Handle multiple other programs
    if (attachmentData.OtherPrograms && Array.isArray(attachmentData.OtherPrograms)) {
        otherProgramsAttachments.value = attachmentData.OtherPrograms.map(att => ({
            ...att,
            Category: 'otherprograms',
            IsDeleted: false,
            AttachmentSize: att.AttachmentSize || null
        }));
    }

    if (attachmentData.General && Array.isArray(attachmentData.General)) {
        attachments.value = attachmentData.General.map(att => ({
            ...att,
            Category: 'general',
            IsDeleted: false,
            AttachmentSize: att.AttachmentSize || null
        }));
    }

    // Fallback: Handle old structure
    if (Array.isArray(attachmentData)) {
        attachmentData.forEach(att => {
            const attachmentWithFlag = {
                ...att,
                IsDeleted: false,
                AttachmentSize: att.AttachmentSize || null
            };
            if (att.Category === 'recipe') {
                recipeAttachment.value = attachmentWithFlag;
            } else if (att.Category === 'otherprograms') {
                otherProgramsAttachments.value.push(attachmentWithFlag);
            } else {
                attachments.value.push(attachmentWithFlag);
            }
        });
    }
};

// ===================================
// FORM DATA FUNCTIONS
// ===================================
const appendAttachmentsToFormData = (formData: FormData) => {
    let attachmentIndex = 0;

    // Helper function to append attachment data
    const appendAttachment = (attachment: any, category: string) => {
        if (!attachment.AttachmentName && !attachment.Id) {
            return;
        }

        if (!attachment.Id && (!attachment.AttachementPath || attachment.AttachementPath.trim() === '')) {
            return;
        }

        if (!attachment.Id && (!attachment.AttachmentName || attachment.AttachmentName.trim() === '')) {
            return;
        }

        if (attachment.Id) {
            formData.append(`Attachments[${attachmentIndex}].Id`, attachment.Id);
        }

        formData.append(`Attachments[${attachmentIndex}].AttachmentName`, attachment.AttachmentName || '');
        formData.append(`Attachments[${attachmentIndex}].AttachementPath`, attachment.AttachementPath || '');
        formData.append(`Attachments[${attachmentIndex}].AttachementType`, attachment.AttachementType || '');

        if (attachment.AttachmentSize &&
            attachment.AttachmentSize !== null &&
            attachment.AttachmentSize.toString().trim() !== '') {
            formData.append(`Attachments[${attachmentIndex}].AttachmentSize`, attachment.AttachmentSize.toString());
        }

        formData.append(`Attachments[${attachmentIndex}].Category`, category);
        formData.append(`Attachments[${attachmentIndex}].IsDeleted`, attachment.IsDeleted ? 'true' : 'false');

        if (attachment.AttachementFileData) {
            formData.append(`Attachments[${attachmentIndex}].AttachementFileData`, attachment.AttachementFileData);
        }

        if (attachment.AttachmentFileLocation) {
            formData.append(`Attachments[${attachmentIndex}].AttachmentFileLocation`, attachment.AttachmentFileLocation);
        }

        attachmentIndex++;
    };

    // Append general attachments
    attachments.value
        .filter(att => {
            const hasBasicData = !att.IsDeleted && (att.AttachmentName || att.Id);
            const hasPath = att.AttachementPath && att.AttachementPath.trim() !== '';
            const shouldInclude = hasBasicData && hasPath;
            return shouldInclude;
        })
        .forEach(attachment => {
            if (attachment.AttachmentSize === '' || attachment.AttachmentSize === null || attachment.AttachmentSize === undefined) {
                attachment.AttachmentSize = null;
            }
            appendAttachment(attachment, attachment.Category || 'general');
        });

    // Append recipe attachment
    if (recipeAttachment.value &&
        !recipeAttachment.value.IsDeleted &&
        (recipeAttachment.value.AttachmentName || recipeAttachment.value.Id) &&
        recipeAttachment.value.AttachementPath &&
        recipeAttachment.value.AttachementPath.trim() !== '') {

        if (recipeAttachment.value.AttachmentSize === '' || recipeAttachment.value.AttachmentSize === null || recipeAttachment.value.AttachmentSize === undefined) {
            recipeAttachment.value.AttachmentSize = null;
        }
        if (recipeAttachment.value.AttachmentFileLocation) {
            formData.append(`Attachments[${attachmentIndex}].AttachmentFileLocation`, recipeAttachment.value.AttachmentFileLocation);
        }
        appendAttachment(recipeAttachment.value, 'recipe');
    }

    // Append other programs attachments (multiple)
    otherProgramsAttachments.value
        .filter(att => {
            const hasBasicData = !att.IsDeleted && (att.AttachmentName || att.Id);
            const hasPath = att.AttachementPath && att.AttachementPath.trim() !== '';
            const shouldInclude = hasBasicData && hasPath;
            return shouldInclude;
        })
        .forEach(attachment => {
            if (attachment.AttachmentSize === '' || attachment.AttachmentSize === null || attachment.AttachmentSize === undefined) {
                attachment.AttachmentSize = null;
            }
            if (attachment.AttachmentFileLocation) {
                formData.append(`Attachments[${attachmentIndex}].AttachmentFileLocation`, attachment.AttachmentFileLocation);
            }
            appendAttachment(attachment, 'otherprograms');
        });
};

const buildFormData = (isApproved: boolean = false) => {
    const formData = new FormData();
    const validUserId = userId.value || '';
    const isUpdate = !!formInput.value.Id;

    // Basic form data
    if (isUpdate) {
        formData.append('Id', formInput.value.Id);
    }

    // ✅ แก้ไข: ส่ง StatusId ที่ถูกต้อง
    if (formInput.value.StatusId) {
        formData.append('StatusId', formInput.value.StatusId.toString());
    }

    // ✅ แก้ไข: เพิ่ม validation สำหรับ required fields
    formData.append('RequestPurpose', formInput.value.RequestPurpose || '');
    formData.append('UserId', validUserId);
    formData.append('RequestDescription', formInput.value.RequestDescription || '');
    formData.append('RequestMachine', formInput.value.RequestMachine || '');
    formData.append('RequestProduct', formInput.value.RequestProduct || '');
    formData.append('RequestBy', formInput.value.RequestBy || '');
    formData.append('RequestProcess', formInput.value.RequestProcess || '');

    // Boolean fields
    formData.append('RequestMass', formInput.value.RequestMass ? 'true' : 'false');
    formData.append('RequestTest', formInput.value.RequestTest ? 'true' : 'false');
    formData.append('Active', formInput.value.Active ? 'true' : 'false');
    formData.append('IsDeleted', formInput.value.IsDeleted ? 'true' : 'false');

    // Checklist fields
    formData.append('FlTgDeleted', formInput.value.FlTgDeleted ? 'true' : 'false');
    formData.append('FlMcDeleted', formInput.value.FlMcDeleted ? 'true' : 'false');
    formData.append('FlCheckMass', formInput.value.FlCheckMass ? 'true' : 'false');
    formData.append('FlDeletedOther', formInput.value.FlDeletedOther ? 'true' : 'false');
    formData.append('CtCopyRp', formInput.value.CtCopyRp ? 'true' : 'false');
    formData.append('CtRpDeleted', formInput.value.CtRpDeleted ? 'true' : 'false');
    formData.append('CtBookCheck', formInput.value.CtBookCheck ? 'true' : 'false');
    formData.append('CtDeletedOther', formInput.value.CtDeletedOther ? 'true' : 'false');

    // Comment fields
    formData.append('FlTgDeletedComment', formInput.value.FlTgDeletedComment || '');
    formData.append('FlMcDeletedComment', formInput.value.FlMcDeletedComment || '');
    formData.append('FlCheckMassComment', formInput.value.FlCheckMassComment || '');
    formData.append('FlDeletedOtherComment', formInput.value.FlDeletedOtherComment || '');
    formData.append('CtCopyRpComment', formInput.value.CtCopyRpComment || '');
    formData.append('CtRpDeletedComment', formInput.value.CtRpDeletedComment || '');
    formData.append('CtBookCheckComment', formInput.value.CtBookCheckComment || '');
    formData.append('CtDeletedOtherComment', formInput.value.CtDeletedOtherComment || '');

    // ✅ แก้ไข: New fields ที่หายไป
    formData.append('RequestObjective', formInput.value.RequestObjective || '');
    formData.append('RequestType', formInput.value.RequestType || '');
    formData.append('FlowInFactory', formInput.value.FlowInFactory || '');
    formData.append('RequestMcNo', formInput.value.RequestMcNo || '');
    formData.append('RequestBook', formInput.value.RequestBook || '');

    // Date fields - ✅ แก้ไข: ตรวจสอบค่า null/undefined
    if (formInput.value.RequestStartDate) {
        formData.append('RequestStartDate', formatDateForAPI(formInput.value.RequestStartDate));
    }
    if (formInput.value.RequestFinishDate) {
        formData.append('RequestFinishDate', formatDateForAPI(formInput.value.RequestFinishDate));
    }
    if (formInput.value.RequestInstallDate) {
        formData.append('RequestInstallDate', formatDateForAPI(formInput.value.RequestInstallDate));
    }
    if (formInput.value.RequestClearDate) {
        formData.append('RequestClearDate', formatDateForAPI(formInput.value.RequestClearDate));
    }

    // Comments based on status ordinal
    const ordinal = formInput.value.Status?.Ordinal || 0;
    if (ordinal >= 2) {
        formData.append('RequestComment1', formInput.value.RequestComment1 || '');
    }
    if (ordinal >= 3) {
        formData.append('RequestComment2', formInput.value.RequestComment2 || '');
    }
    if (ordinal >= 4) {
        formData.append('RequestComment3', formInput.value.RequestComment3 || '');
    }

    const validLotNumbers = lotNumbers.value.filter(lot => lot && lot.trim() !== '');


    if (validLotNumbers.length > 0) {
        validLotNumbers.forEach((lotNo, index) => {
            formData.append(`Lots[${index}].LotNo`, lotNo);
            // ✅ เพิ่ม: ถ้าเป็น update และมี Id ของ Lot
            if (isUpdate && formInput.value.Id) {
                formData.append(`Lots[${index}].RequestId`, formInput.value.Id);
            }
        });
    } else {
        // ส่งค่าว่างถ้าไม่มี lot numbers
        formData.append('Lots[0].LotNo', '');
    }

    // Other Programs - ส่งเป็น array
    if (Array.isArray(otherProgramsList.value) && otherProgramsList.value.length > 0) {
        otherProgramsList.value.forEach((program, index) => {
            formData.append(`OtherPrograms[${index}]`, program);
        });
    }

    // Attachments
    appendAttachmentsToFormData(formData);

    // Approval status
    formData.append('IsApproved', isApproved ? 'true' : 'false');
    return formData;
};

// ===================================
// FORM MAPPING FUNCTIONS
// ===================================
const mapPropsToFormInput = (request: InaRequestDTO): InaRequestDTO => {
    if (!request) {
        return formInput.value;
    }

    // Map lots
    if (request?.LotNumbers && request.LotNumbers.length > 0) {
        lotNumbers.value = [...request.LotNumbers];
    } else {
        lotNumbers.value = [''];
    }

    // Map attachments
    if (request?.Attachments) {
        const hasAttachments =
            (request.Attachments.Recipe && request.Attachments.Recipe.length > 0) ||
            (request.Attachments.OtherPrograms && request.Attachments.OtherPrograms.length > 0) ||
            (request.Attachments.General && request.Attachments.General.length > 0);

        if (hasAttachments) {
            loadAttachmentsFromData(request.Attachments);
        } else {
            clearAllAttachments();
        }
    } else {
        clearAllAttachments();
    }

    const mappedData = {
        ...formInput.value,
        ...request,
        RequestStartDate: formatDateForInput(request.RequestStartDate),
        RequestFinishDate: formatDateForInput(request.RequestFinishDate),
        RequestInstallDate: formatDateForInput(request.RequestInstallDate),
        RequestClearDate: formatDateForInput(request.RequestClearDate),
        RequestMachine: typeof request.RequestMachine === 'object'
            ? (request.RequestMachine?.Id || request.RequestMachine?.Value || '')
            : (request.RequestMachine || ''),
        Attachments: request?.Attachments || {},
        LotNumbers: request?.LotNumbers || [''],
        // Map Purpose to first position
        RequestPurpose: request.RequestPurpose || '',
    };
    return mappedData;
};

// ===================================
// ALERT FUNCTIONS
// ===================================
const showSuccessAlert = () => swal.fire({
    title: 'Success!',
    text: 'Data added successfully.',
    icon: 'success',
    confirmButtonText: 'OK'
});

const showErrorAlert = () => swal.fire({
    title: 'Error!',
    text: 'An error occurred while submitting data.',
    icon: 'error',
    confirmButtonText: 'Back'
});

const showApproveConfirm = () => {
    swal.fire({
        title: 'Are you sure?',
        text: 'Do you want to approve this request?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, approve it!',
        customClass: { popup: 'swal-custom-zindex' },
        // เพิ่ม properties เหล่านี้
        backdrop: 'rgba(0, 0, 0, 0.4)',
        allowOutsideClick: false,
        allowEscapeKey: false,
        allowEnterKey: true,
        stopKeydownPropagation: false
    }).then(result => {
        if (result.isConfirmed) onFormSubmit(true);
    });
};


// ===================================
// CONFIRMATION FUNCTIONS
// ===================================
const confirmApprove = () => {
    const commentValidation = validateRequiredComments();

    if (!commentValidation.isValid) {
        swal.fire({
            title: 'กรุณากรอกคอมเม้นต์',
            html: `กรุณากรอกคอมเม้นต์ให้ครบถ้วน:<br>${commentValidation.errors.map(error => `• ${error}`).join('<br>')}`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'ดำเนินการต่อ',
            cancelButtonText: 'ยกเลิก',
            customClass: {
                popup: 'swal-custom-zindex',
                htmlContainer: 'text-left'
            },
            // เพิ่ม properties เหล่านี้
            backdrop: 'rgba(0, 0, 0, 0.4)',
            allowOutsideClick: false,
            allowEscapeKey: false,
            allowEnterKey: true
        }).then(result => {
            if (result.isConfirmed) {
                showApproveConfirm();
            }
        });
    } else {
        showApproveConfirm();
    }
};
const confirmUpdate = () => {
    swal.fire({
        title: 'Are you sure?',
        text: 'Do you want to Update this request?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, Update it!',
        customClass: { popup: 'swal-custom-zindex' },
        // เพิ่ม properties เหล่านี้เพื่อป้องกันการปิด modal
        backdrop: 'rgba(0, 0, 0, 0.4)', // หรือใช้ true
        allowOutsideClick: false,
        allowEscapeKey: false,
        allowEnterKey: true,
        stopKeydownPropagation: false
    }).then(result => {
        if (result.isConfirmed) onFormSubmit(false);
    });
};


const confirmDelete = async () => {
    const result = await Swal.fire({
        title: 'ยืนยันการลบ',
        text: 'คุณจะไม่สามารถยกเลิกได้!',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'ใช่, ลบเลย!',
        cancelButtonText: 'ยกเลิก',
        // เพิ่ม properties เหล่านี้
        backdrop: 'rgba(0, 0, 0, 0.4)',
        allowOutsideClick: false,
        allowEscapeKey: false,
        allowEnterKey: true
    });

    if (!result.isConfirmed) return;

    try {
        const { data, error } = await useFetch(`/api/SI24001INA/${formInput.value.Id}`, {
            method: 'DELETE',
            baseURL: useRuntimeConfig().public.apiBase,
            credentials: 'include',
            headers: { Authorization: `Bearer ${token.value}` },
        });

        if (error.value) throw error.value;

        Swal.fire({
            title: 'ลบแล้ว!',
            text: 'ข้อมูลถูกลบเรียบร้อย.',
            icon: 'success',
            confirmButtonText: 'ตกลง',
            // เพิ่มสำหรับ success message ด้วย
            backdrop: 'rgba(0, 0, 0, 0.4)',
            allowOutsideClick: true, // สำหรับ success message ให้คลิกข้างนอกได้
            allowEscapeKey: true
        });
        emit('refreshData');
        onDialogClose();
    } catch (e) {
        Swal.fire({
            title: 'เกิดข้อผิดพลาด!',
            text: 'ไม่สามารถลบข้อมูลได้.',
            icon: 'error',
            confirmButtonText: 'ตกลง',
            // เพิ่มสำหรับ error message ด้วย
            backdrop: 'rgba(0, 0, 0, 0.4)',
            allowOutsideClick: true,
            allowEscapeKey: true
        });
    }
};


// ===================================
// API FUNCTIONS
// ===================================
const fetchMachineOptions = async () => {
    try {
        loadingMachines.value = true;
        const { data, error } = await useFetch('/api/roles/MechineDDR', {
            method: 'GET',
            baseURL: useRuntimeConfig().public.apiBase,
            credentials: 'include',
            headers: { Authorization: `Bearer ${token.value}` },
        });

        if (error.value) {
            throw new Error(error.value.message);
        }

        if (data.value && Array.isArray(data.value)) {
            machineOptions.value = data.value.map(machine => ({
                name: machine.RequestMachineName || machine.MachineName || machine.Name,
                value: machine.Id || machine.MachineId || machine.Value || machine.Name
            }));
        }
    } catch (error) {
        console.error('Error fetching machine options:', error);
        swal.fire({
            title: 'Error!',
            text: 'Failed to load machine options. Please try again.',
            icon: 'error',
            confirmButtonText: 'OK'
        });
    } finally {
        loadingMachines.value = false;
    }
};
const confirmFinishTest = () => {
    const commentValidation = validateRequiredComments();

    if (!commentValidation.isValid) {
        // แสดงข้อผิดพลาดถ้า comment ไม่ครบ
        swal.fire({
            title: 'กรุณากรอกคอมเม้นต์',
            html: `กรุณากรอกคอมเม้นต์ให้ครบถ้วน:<br>${commentValidation.errors.map(error => `• ${error}`).join('<br>')}`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'ดำเนินการต่อ',
            cancelButtonText: 'ยกเลิก',
            // เพิ่ม properties เหล่านี้
            backdrop: 'rgba(0, 0, 0, 0.4)',
            allowOutsideClick: false,
            allowEscapeKey: false,
            allowEnterKey: true
        }).then(result => {
            if (result.isConfirmed) showFinishTestConfirm();
        });
    } else {
        showFinishTestConfirm();
    }
};


const showFinishTestConfirm = () => {
    swal.fire({
        title: 'Are you sure?',
        text: 'Do you want to finish this test?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, finish it!',
        // เพิ่ม properties เหล่านี้
        backdrop: 'rgba(0, 0, 0, 0.4)',
        allowOutsideClick: false,
        allowEscapeKey: false,
        allowEnterKey: true
    }).then(result => {
        if (result.isConfirmed) finishTest();
    });
};

const finishTest = async () => {
    if (!formInput.value.Id) return;

    try {
        isSubmitting.value = true;

        // หา "Finish Test" status
        const { data: statusData, error: statusError } = await useFetch('/api/roles/statuses', {
            method: 'GET',
            baseURL: useRuntimeConfig().public.apiBase,
            headers: { Authorization: `Bearer ${token.value}` },
        });

        if (statusError.value) {
            throw statusError.value;
        }

        // หา status ที่มีชื่อ "Finish Test"
        const finishStatus = statusData.value?.find(s => s.StatusName === 'Finish Test'); // แก้ไข = เป็น ===

        if (!finishStatus) {
            throw new Error('Finish Test status not found');
        }

        // สร้าง FormData ก่อน
        const formData = buildFormData(false);

        // เพิ่มหรืออัพเดทค่าใหม่ใน FormData
        formData.set('StatusId', finishStatus.Id); // ใช้ .set เพื่อแทนที่ค่าเดิม
        formData.set('RequestFinishDate', new Date().toISOString().split('T')[0]);

        // อัพเดท formInput ด้วย (สำหรับ sync กับ UI)
        formInput.value.StatusId = finishStatus.Id;
        formInput.value.RequestFinishDate = new Date().toISOString().split('T')[0];

        // ส่ง PUT request ไป update ข้อมูล
        const { data, error } = await useFetch('/api/SI24001INA', {
            method: 'PUT',
            baseURL: useRuntimeConfig().public.apiBase,
            body: formData,
            headers: { Authorization: `Bearer ${token.value}` },
        });

        if (error.value) {
            throw error.value;
        }

        swal.fire({
            icon: 'success',
            title: 'สำเร็จ',
            text: 'ดำเนินการ Finish Test เรียบร้อยแล้ว'
        });

        emit('refreshData');
        fetchRequestStatus();

    } catch (error) {
        console.error('Finish Test error:', error);
        swal.fire({
            icon: 'error',
            title: 'เกิดข้อผิดพลาด',
            text: 'ไม่สามารถดำเนินการ Finish Test ได้'
        });
    } finally {
        isSubmitting.value = false;
    }
};


// ===================================
// MAIN FORM FUNCTIONS
// ===================================
const onDialogClose = () => emit('update:isDialogVisible', false);

const onFormSubmit = async (isApproved: boolean = false) => {
    const validationResult = await refVFormRequest.value?.validate();
    if (!validationResult?.valid) return;

    isSubmitting.value = true;
    const isUpdate = !!formInput.value.Id;

    try {
        const formData = buildFormData(isApproved);

        const apiUrl = `/api/SI24001INA`;
        const { data, error } = await useFetch(apiUrl, {
            method: isUpdate ? 'PUT' : 'POST',
            baseURL: useRuntimeConfig().public.apiBase,
            body: formData,
            headers: { Authorization: `Bearer ${token.value}` },
        });

        if (error.value) throw new Error(error.value.message);

        showSuccessAlert();

        emit('refreshData');

        await nextTick();
        setTimeout(() => {
            onDialogClose();
        }, 500);

    } catch (error) {
        console.error('Submit error:', error);
        showErrorAlert();
    } finally {
        isSubmitting.value = false;
    }
};

// ===================================
// REQUEST STATUS API
// ===================================
const { data, error, execute: fetchRequestStatus } = useFetch<any>(
    () => `/api/SI24001INA/GetRequestStatus/${props.request?.Id}`,
    {
        method: 'GET',
        baseURL: useRuntimeConfig().public.apiBase,
        credentials: 'include',
        headers: { Authorization: `Bearer ${token.value}` },
        immediate: false,
    }
);

// ===================================
// WATCHERS
// ===================================
watch(() => props.request, (newRequest) => {
    if (newRequest) {
        formInput.value = mapPropsToFormInput(newRequest);
    }
}, { immediate: true });

watch(() => props.request?.Id, (newId) => {
    if (newId) {
        fetchRequestStatus();
        fetchLots(newId);
    }
});

watch(lotNumbers, (newLotNumbers) => {
    formInput.value.LotNumbers = newLotNumbers.filter(lot => lot && lot.trim() !== '');
}, { deep: true });

watchEffect(() => {
    if (data.value) {
        statusSteps.value = data.value.statusSteps ?? [];
        currentStep.value = data.value.currentStep ?? null;
    }
});

watch(error, (err) => {
    if (err) console.error('Error fetching request status:', err);
});

// ===================================
// LIFECYCLE HOOKS
// ===================================
onMounted(async () => {
    await nextTick(async () => {
        await fetchMachineOptions();
        if (props.request) {
            formInput.value = mapPropsToFormInput(props.request);
        }
        if (props.request?.Id) {
            fetchRequestStatus();
            fetchLots(props.request.Id);
        }
    });
});
</script>

<template>
    <VDialog :model-value="isDialogVisible" @update:model-value="onDialogClose" max-width="1200px">
        <VCard>
            <VCardTitle>
                {{ formInput.Id != '' ? 'Edit Request' : 'Add New Request' }}
            </VCardTitle>
            <VContainer v-if="formInput.Id">
                <div class="stepper-container">
                    <div class="progress-line" :style="{ width: progressWidth }"></div>
                    <div v-for="(step, index) in statusSteps" :key="index" class="step-item">
                        <div class="step-circle"
                            :class="{ active: index + 1 === currentStep, completed: index + 1 < currentStep }">
                            <span v-if="index + 1 < currentStep">✓</span>
                            <span v-else>{{ index + 1 }}</span>
                        </div>
                        <div class="step-label">{{ step.StatusName }}</div>
                    </div>
                </div>
            </VContainer>
            <VCardText>
                <VForm ref="refVFormRequest" @submit.prevent="onFormSubmit">
                    <VRow>
                        <VCol cols="12">
                            <AppTextField v-model="formInput.RequestCode" label="Request Code"
                                placeholder="Request Code Auto Generate" disabled />
                        </VCol>
                    </VRow>

                    <!-- Purpose ย้ายมาช่องแรก -->
                    <VRow>
                        <VCol cols="12">
                            <AppTextField v-model="formInput.RequestPurpose" :rules="[requiredValidator]"
                                label="Purpose" placeholder="วัตถุประสงค์" required />
                        </VCol>
                    </VRow>

                    <VRow>
                        <VCol cols="6">
                            <AppTextField v-model="formInput.RequestBy" :rules="[requiredValidator]" label="Request By"
                                placeholder="ผู้ขอ" required />
                        </VCol>
                        <VCol cols="6">
                            <AppTextField v-model="formInput.RequestProcess" :rules="[requiredValidator]"
                                label="Requester's Process" placeholder="กรุณากรอกชื่อ Requester Process" required />
                        </VCol>
                    </VRow>

                    <!-- Description สามารถ blank ได้ -->
                    <VRow>
                        <VCol cols="12">
                            <v-textarea v-model="formInput.RequestDescription" label="Request Description (Optional)"
                                placeholder="รายละเอียดคำขอ (ไม่บังคับ)" hint="This field is optional" />
                        </VCol>
                    </VRow>

                    <VRow>
                        <VCol cols="6">
                            <AppTextField v-model="formInput.RequestProduct" :rules="[requiredValidator]"
                                label="Product Name" placeholder="ชื่อผลิตภัณฑ์" required />
                        </VCol>
                        <VCol cols="6" style="margin-top:1.4rem">
                            <!-- Machine - แก้ปัญหา Enter select ไม่ได้ -->
                            <v-autocomplete v-model="formInput.RequestMachine" v-model:search="machineSearchText"
                                :items="filteredMachineOptions" :loading="loadingMachines" :rules="[requiredValidator]"
                                label="Request Machine" placeholder="เลือกเครื่องจักร" item-title="name"
                                item-value="value" clearable required no-data-text="No machines found"
                                @keydown="handleMachineKeydown" @update:model-value="handleMachineSelect" />
                        </VCol>
                    </VRow>

                    <VRow>
                        <VCol cols="12">
                            <AppTextField v-model="formInput.RequestStartDate" label="Plan Start Date" type="date" />
                        </VCol>
                    </VRow>

                    <!-- Dynamic LotNo Section - ดัก Lot Number -->
                    <VRow>
                        <VCol cols="12">
                            <div class="lot-number-section">
                                <div class="d-flex align-center mb-3">
                                    <h4 class="me-3">Lot Numbers</h4>
                                    <VBtn @click="addLotNumber" :disabled="!canAddLotNumber" color="primary"
                                        size="small">
                                        + Add Lot ({{ lotNumbers.length }}/{{ maxLotNumbers }})
                                    </VBtn>
                                </div>

                                <div v-for="(lotNumber, index) in lotNumbers" :key="index" class="mb-3">
                                    <div class="d-flex align-center">
                                        <VTextField v-model="lotNumbers[index]" :label="`Lot No ${index + 1}`"
                                            :placeholder="`กรุณากรอกเลข Lot ${index + 1}`" :rules="[lotNumberValidator]"
                                            class="me-2" clearable />
                                        <VBtn v-if="lotNumbers.length > 1" @click="removeLotNumber(index)" color="error"
                                            size="small" icon="$delete" />
                                    </div>
                                </div>
                            </div>
                        </VCol>
                    </VRow>

                    <!-- File Attachment Section -->
                    <VRow>
                        <VCol cols="12">
                            <v-card>
                                <v-card-title class="pa-3">
                                    <span>File Attachments</span>
                                    <span class="text-caption text-red ml-2">*Please attach a list of lots if more than
                                        1</span>
                                </v-card-title>
                                <v-table class="file-table">
                                    <thead>
                                        <tr>
                                            <th class="text-left">Item</th>
                                            <th class="text-left">Name / ชื่อไฟล์</th>
                                            <th class="text-left">Location / ที่อยู่ไฟล์</th>
                                            <th class="text-left">Size / ขนาดไฟล์</th>
                                            <th class="text-left">Description / คำอธิบาย</th>
                                            <th class="text-left">Actions</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <!-- General Attachments -->
                                        <tr v-for="(attachment, index) in attachments.filter(att => !att.IsDeleted)"
                                            :key="`general-${attachment.Id || index}`">
                                            <td>
                                                <v-chip size="small" color="primary">
                                                    Attachment {{ index + 1 }}
                                                </v-chip>
                                            </td>
                                            <td>
                                                <v-text-field v-model="attachment.AttachmentName" density="compact"
                                                    variant="outlined" hide-details placeholder="Enter file name" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="attachment.AttachementPath" density="compact"
                                                    variant="outlined" hide-details placeholder="Enter file path" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="attachment.AttachmentSize" density="compact"
                                                    variant="outlined" hide-details
                                                    placeholder="Enter file size (optional)" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="attachment.AttachementType" density="compact"
                                                    variant="outlined" hide-details placeholder="Enter description" />
                                            </td>
                                            <td>
                                                <div class="d-flex gap-1">
                                                    <v-btn icon @click="copyPath(attachment.AttachementPath)"
                                                        color="primary" size="small"
                                                        :disabled="!attachment.AttachementPath">
                                                        <v-icon>mdi-content-copy</v-icon>
                                                    </v-btn>
                                                    <v-btn icon @click="removeAttachment(index)" color="error"
                                                        size="small">
                                                        <VIcon icon="tabler-trash" />
                                                    </v-btn>
                                                </div>
                                            </td>
                                        </tr>

                                        <!-- Recipe Attachment -->
                                        <tr v-if="recipeAttachment && !recipeAttachment.IsDeleted">
                                            <td>
                                                <v-chip size="small" color="secondary">
                                                    Recipe
                                                </v-chip>
                                            </td>
                                            <td>
                                                <v-text-field v-model="recipeAttachment.AttachmentName"
                                                    density="compact" variant="outlined" hide-details
                                                    placeholder="Enter recipe file name" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="recipeAttachment.AttachementPath"
                                                    density="compact" variant="outlined" hide-details
                                                    placeholder="Enter recipe file path" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="recipeAttachment.AttachmentSize"
                                                    density="compact" variant="outlined" hide-details
                                                    placeholder="Enter file size (optional)" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="recipeAttachment.AttachementType"
                                                    density="compact" variant="outlined" hide-details
                                                    placeholder="Enter recipe description" />
                                            </td>
                                            <td>
                                                <div class="d-flex gap-1">
                                                    <v-btn icon @click="copyPath(recipeAttachment.AttachementPath)"
                                                        color="primary" size="small"
                                                        :disabled="!recipeAttachment.AttachementPath">
                                                        <v-icon>mdi-content-copy</v-icon>
                                                    </v-btn>
                                                    <v-btn icon @click="removeRecipeAttachment()" color="error"
                                                        size="small">
                                                        <VIcon icon="tabler-trash" />
                                                    </v-btn>
                                                </div>
                                            </td>
                                        </tr>

                                        <!-- Other Programs Attachments - มีมากกว่า 1 -->
                                        <tr v-for="(attachment, index) in otherProgramsAttachments.filter(att => !att.IsDeleted)"
                                            :key="`otherprograms-${attachment.Id || index}`">
                                            <td>
                                                <v-chip size="small" color="info">
                                                    Other Program {{ index + 1 }}
                                                </v-chip>
                                            </td>
                                            <td>
                                                <v-text-field v-model="attachment.AttachmentName" density="compact"
                                                    variant="outlined" hide-details
                                                    placeholder="Enter other programs file name" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="attachment.AttachementPath" density="compact"
                                                    variant="outlined" hide-details
                                                    placeholder="Enter other programs file path" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="attachment.AttachmentSize" density="compact"
                                                    variant="outlined" hide-details
                                                    placeholder="Enter file size (optional)" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="attachment.AttachementType" density="compact"
                                                    variant="outlined" hide-details
                                                    placeholder="Enter other programs description" />
                                            </td>
                                            <td>
                                                <div class="d-flex gap-1">
                                                    <v-btn icon @click="copyPath(attachment.AttachementPath)"
                                                        color="primary" size="small"
                                                        :disabled="!attachment.AttachementPath">
                                                        <v-icon>mdi-content-copy</v-icon>
                                                    </v-btn>
                                                    <v-btn icon @click="removeOtherProgramsAttachment(index)"
                                                        color="error" size="small">
                                                        <VIcon icon="tabler-trash" />
                                                    </v-btn>
                                                </div>
                                            </td>
                                        </tr>

                                        <!-- Empty State -->
                                        <tr v-if="attachments.filter(att => !att.IsDeleted).length === 0 &&
                                            (!recipeAttachment || recipeAttachment.IsDeleted) &&
                                            otherProgramsAttachments.filter(att => !att.IsDeleted).length === 0">
                                            <td colspan="6" class="text-center py-4">
                                                <v-icon color="grey" size="48">mdi-file-document-outline</v-icon>
                                                <p class="text-grey mt-2">No attachments added yet</p>
                                            </td>
                                        </tr>
                                    </tbody>
                                </v-table>

                                <!-- Add Attachment Buttons -->
                                <v-card-actions class="pa-3">
                                    <v-btn @click="addAttachment('general')" color="primary" size="small">
                                        + Add General Attachment
                                    </v-btn>
                                    <v-btn v-if="!recipeAttachment" @click="addAttachment('recipe')" color="secondary"
                                        size="small">
                                        + Add Recipe
                                    </v-btn>
                                    <v-btn @click="addAttachment('otherprograms')" color="info" size="small">
                                        + Add Other Programs
                                    </v-btn>
                                </v-card-actions>

                                <!-- Attachment Summary -->
                                <v-card-text
                                    v-if="attachments.length || recipeAttachment || otherProgramsAttachments.length"
                                    class="pt-0">
                                    <v-divider class="mb-2"></v-divider>
                                    <div class="d-flex align-center gap-2 text-caption">
                                        <v-icon size="small">mdi-information</v-icon>
                                        <span>Total Attachments: {{ getTotalAttachments() }}</span>
                                        <v-chip v-if="attachments.length" size="x-small" color="primary">
                                            General: {{attachments.filter(att => !att.IsDeleted).length}}
                                        </v-chip>
                                        <v-chip v-if="recipeAttachment && !recipeAttachment.IsDeleted" size="x-small"
                                            color="secondary">
                                            Recipe: 1
                                        </v-chip>
                                        <v-chip v-if="otherProgramsAttachments.filter(att => !att.IsDeleted).length"
                                            size="x-small" color="info">
                                            Other Programs: {{otherProgramsAttachments.filter(att =>
                                                !att.IsDeleted).length}}
                                        </v-chip>
                                    </div>
                                </v-card-text>
                            </v-card>
                        </VCol>
                    </VRow>

                    <!-- Comments Section based on Status -->
                    <VRow v-if="formInput.Status?.Ordinal >= 2">
                        <VCol cols="12">
                            <v-textarea v-model="formInput.RequestComment1" label="Form Confirm" auto-grow rows="2"
                                :rules="[requiredValidator]" required
                                :error="formInput.Status?.Ordinal >= 2 && (!formInput.RequestComment1 || formInput.RequestComment1.trim() === '')" />
                            <div v-if="formInput.Status?.Ordinal >= 2" class="text-caption text-red">
                                * This comment is required for approval
                            </div>
                        </VCol>
                    </VRow>
                    <VRow v-if="formInput.Status?.Ordinal >= 3">
                        <VCol cols="6">
                            <AppTextField v-model="formInput.RequestMcNo" :rules="[requiredValidator]"
                                label="Machine No" placeholder="255" required />
                        </VCol>
                        <VCol cols="6">
                            <AppTextField v-model="formInput.RequestBook" :rules="[requiredValidator]" label="Book No."
                                placeholder="255" required />
                        </VCol>
                        <VCol cols="12">
                            <AppTextField v-model="formInput.RequestInstallDate" label="Install Date" type="date" />
                        </VCol>
                        <VCol cols="12">
                            <v-card>
                                <v-card-title class="pa-3">
                                    <span>File Attachments</span>
                                    <span class="text-caption text-red ml-2">*Please attach a list of lots if more than
                                        1</span>
                                </v-card-title>
                                <v-table class="file-table">
                                    <thead>
                                        <tr>
                                            <th class="text-left">Item</th>
                                            <th class="text-left">Name / ชื่อไฟล์</th>
                                            <th class="text-left">Location in machine / ที่อยู่ของโปรแกรมในเครื่องจักร
                                            </th>
                                            <th class="text-left">Actions</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <!-- General Attachments -->
                                        <tr v-for="(attachment, index) in attachments.filter(att => !att.IsDeleted)"
                                            :key="`general-${attachment.Id || index}`">
                                            <td>
                                                <v-chip size="small" color="primary">
                                                    Attachment {{ index + 1 }}
                                                </v-chip>
                                            </td>
                                            <td>
                                                <v-text-field v-model="attachment.AttachmentName" density="compact"
                                                    variant="outlined" hide-details placeholder="Enter file name" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="attachment.AttachmentFileLocation"
                                                    density="compact" variant="outlined" hide-details
                                                    placeholder="Enter file path" />
                                            </td>
                                            <td>
                                                <div class="d-flex gap-1">
                                                    <v-btn icon @click="copyPath(attachment.AttachmentFileLocation)"
                                                        color="primary" size="small"
                                                        :disabled="!attachment.AttachmentFileLocation">
                                                        <v-icon>mdi-content-copy</v-icon>
                                                    </v-btn>
                                                </div>
                                            </td>
                                        </tr>

                                        <!-- Recipe Attachment -->
                                        <tr v-if="recipeAttachment && !recipeAttachment.IsDeleted">
                                            <td>
                                                <v-chip size="small" color="secondary">
                                                    Recipe
                                                </v-chip>
                                            </td>
                                            <td>
                                                <v-text-field v-model="recipeAttachment.AttachmentName"
                                                    density="compact" variant="outlined" hide-details
                                                    placeholder="Enter recipe file name" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="recipeAttachment.AttachmentFileLocation"
                                                    density="compact" variant="outlined" hide-details
                                                    placeholder="Enter recipe file path" />
                                            </td>
                                            <td>
                                                <div class="d-flex gap-1">
                                                    <v-btn icon
                                                        @click="copyPath(recipeAttachment.AttachmentFileLocation)"
                                                        color="primary" size="small"
                                                        :disabled="!recipeAttachment.AttachmentFileLocation">
                                                        <v-icon>mdi-content-copy</v-icon>
                                                    </v-btn>
                                                </div>
                                            </td>
                                        </tr>

                                        <!-- Other Programs Attachments - มีมากกว่า 1 -->
                                        <tr v-for="(attachment, index) in otherProgramsAttachments.filter(att => !att.IsDeleted)"
                                            :key="`otherprograms-${attachment.Id || index}`">
                                            <td>
                                                <v-chip size="small" color="info">
                                                    Other Program {{ index + 1 }}
                                                </v-chip>
                                            </td>
                                            <td>
                                                <v-text-field v-model="attachment.AttachmentName" density="compact"
                                                    variant="outlined" hide-details
                                                    placeholder="Enter other programs file name" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="attachment.AttachmentFileLocation"
                                                    density="compact" variant="outlined" hide-details
                                                    placeholder="Enter other programs file path" />
                                            </td>
                                            <td>
                                                <div class="d-flex gap-1">
                                                    <v-btn icon @click="copyPath(attachment.AttachmentFileLocation)"
                                                        color="primary" size="small"
                                                        :disabled="!attachment.AttachmentFileLocation">
                                                        <v-icon>mdi-content-copy</v-icon>
                                                    </v-btn>
                                                </div>
                                            </td>
                                        </tr>

                                        <!-- Empty State -->
                                        <tr v-if="attachments.filter(att => !att.IsDeleted).length === 0 &&
                                            (!recipeAttachment || recipeAttachment.IsDeleted) &&
                                            otherProgramsAttachments.filter(att => !att.IsDeleted).length === 0">
                                            <td colspan="6" class="text-center py-4">
                                                <v-icon color="grey" size="48">mdi-file-document-outline</v-icon>
                                                <p class="text-grey mt-2">No attachments added yet</p>
                                            </td>
                                        </tr>
                                    </tbody>
                                </v-table>
                                <!-- Attachment Summary -->
                                <v-card-text
                                    v-if="attachments.length || recipeAttachment || otherProgramsAttachments.length"
                                    class="pt-0">
                                    <v-divider class="mb-2"></v-divider>
                                    <div class="d-flex align-center gap-2 text-caption">
                                        <v-icon size="small">mdi-information</v-icon>
                                        <span>Total Attachments: {{ getTotalAttachments() }}</span>
                                        <v-chip v-if="attachments.length" size="x-small" color="primary">
                                            General: {{attachments.filter(att => !att.IsDeleted).length}}
                                        </v-chip>
                                        <v-chip v-if="recipeAttachment && !recipeAttachment.IsDeleted" size="x-small"
                                            color="secondary">
                                            Recipe: 1
                                        </v-chip>
                                        <v-chip v-if="otherProgramsAttachments.filter(att => !att.IsDeleted).length"
                                            size="x-small" color="info">
                                            Other Programs: {{otherProgramsAttachments.filter(att =>
                                                !att.IsDeleted).length}}
                                        </v-chip>
                                    </div>
                                </v-card-text>
                            </v-card>
                        </VCol>
                    </VRow>
                    <VRow v-if="formInput.Status?.Ordinal >= 3">
                        <VCol cols="12">
                            <v-textarea v-model="formInput.RequestComment2" label="Form Modify" auto-grow rows="2"
                                :rules="[requiredValidator]" required
                                :error="formInput.Status?.Ordinal >= 3 && (!formInput.RequestComment2 || formInput.RequestComment2.trim() === '')" />
                            <div v-if="formInput.Status?.Ordinal >= 3" class="text-caption text-red">
                                * This comment is required for approval
                            </div>
                        </VCol>
                    </VRow>
                    <VRow v-if="formInput.Status?.Ordinal >= 4">
                        <VCol cols="12">
                            <AppTextField v-model="formInput.RequestFinishDate" label="Plan Finish Date" type="date" />
                        </VCol>
                        <VCol cols="12" class="mt-2">
                            <v-textarea v-model="formInput.RequestComment3" label="Form Test" auto-grow rows="2"
                                :rules="[requiredValidator]" required
                                :error="formInput.Status?.Ordinal >= 4 && (!formInput.RequestComment3 || formInput.RequestComment3.trim() === '')" />
                            <div v-if="formInput.Status?.Ordinal >= 4" class="text-caption text-red">
                                * This comment is required for approval
                            </div>
                        </VCol>
                    </VRow>
                    <VRow v-if="formInput.Status?.StatusName?.toLowerCase() === 'finish test'">
                        <VCol cols="12">
                            <AppTextField v-model="formInput.RequestClearDate" label="Request Clear Date" type="date" />
                        </VCol>
                        <VCol cols="12">
                            <v-card>
                                <v-card-title class="pa-3">
                                    <span>ขั้นตอนการเคลียร์โปรแกรม</span>
                                </v-card-title>
                                <v-table class="checklist-table">
                                    <thead>
                                        <tr>
                                            <th class="text-left">Computer</th>
                                            <th class="text-left">Item</th>
                                            <th class="text-left">Check</th>
                                            <th class="text-left">หมายเหตุ</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <!-- FI Items -->
                                        <tr>
                                            <td>FI</td>
                                            <td>คบ Tagfile จากจาก FlexInspector</td>
                                            <td>
                                                <v-checkbox v-model="formInput.FlTgDeleted" hide-details
                                                    density="compact" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="formInput.FlTgDeletedComment" density="compact"
                                                    variant="outlined" hide-details placeholder="หมายเหตุ" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>FI</td>
                                            <td>คบ Tagfile จากจาก เครื่องจักร</td>
                                            <td>
                                                <v-checkbox v-model="formInput.FlMcDeleted" hide-details
                                                    density="compact" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="formInput.FlMcDeletedComment" density="compact"
                                                    variant="outlined" hide-details placeholder="หมายเหตุ" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>FI</td>
                                            <td>ตรวจสอบ ถือ Tagfile (Mass)*</td>
                                            <td>
                                                <v-checkbox v-model="formInput.FlCheckMass" hide-details
                                                    density="compact" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="formInput.FlCheckMassComment" density="compact"
                                                    variant="outlined" hide-details placeholder="หมายเหตุ" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>FI</td>
                                            <td>คบ โปรแกรมอื่นๆ</td>
                                            <td>
                                                <v-checkbox v-model="formInput.FlDeletedOther" hide-details
                                                    density="compact" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="formInput.FlDeletedOtherComment"
                                                    density="compact" variant="outlined" hide-details
                                                    placeholder="หมายเหตุ" />
                                            </td>
                                        </tr>

                                        <!-- Control Items -->
                                        <tr>
                                            <td>Control</td>
                                            <td>คัดลอกสำรอง Recipe ไว้ในชื่อไฟล์อื่อร์</td>
                                            <td>
                                                <v-checkbox v-model="formInput.CtCopyRp" hide-details
                                                    density="compact" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="formInput.CtCopyRpComment" density="compact"
                                                    variant="outlined" hide-details placeholder="หมายเหตุ" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>Control</td>
                                            <td>คบ Recipe หลักในไฟล์เดอร์ Recipe และ Main Recipe</td>
                                            <td>
                                                <v-checkbox v-model="formInput.CtRpDeleted" hide-details
                                                    density="compact" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="formInput.CtRpDeletedComment" density="compact"
                                                    variant="outlined" hide-details placeholder="หมายเหตุ" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>Control</td>
                                            <td>ตรวจสอบ Book no ใน Main Recipe*</td>
                                            <td>
                                                <v-checkbox v-model="formInput.CtBookCheck" hide-details
                                                    density="compact" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="formInput.CtBookCheckComment" density="compact"
                                                    variant="outlined" hide-details placeholder="หมายเหตุ" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>Control</td>
                                            <td>คบ โปรแกรมอื่นๆ</td>
                                            <td>
                                                <v-checkbox v-model="formInput.CtDeletedOther" hide-details
                                                    density="compact" />
                                            </td>
                                            <td>
                                                <v-text-field v-model="formInput.CtDeletedOtherComment"
                                                    density="compact" variant="outlined" hide-details
                                                    placeholder="หมายเหตุ" />
                                            </td>
                                        </tr>
                                    </tbody>
                                </v-table>
                            </v-card>
                        </VCol>
                    </VRow>
                    <!-- Test/Mass Production Switches -->
                    <VRow>
                        <VCol cols="6">
                            <v-switch v-model="formInput.RequestTest" label="Request Test" color="primary" />
                        </VCol>
                        <VCol cols="6">
                            <v-switch v-model="formInput.Active" label="Active" color="success" />
                        </VCol>
                    </VRow>
                    <!-- Action Buttons -->
                    <VRow class="mt-4">
                        <VCol cols="12" class="d-flex justify-center gap-4" v-if="!formInput.Id">
                            <VBtn :loading="isSubmitting" type="submit" color="primary">Submit</VBtn>
                            <VBtn color="secondary" variant="tonal" @click="onDialogClose">Cancel</VBtn>
                        </VCol>

                        <VCol cols="12" v-if="formInput.Id">
                            <VRow class="mt-4">
                                <!-- Delete Button (Left) -->
                                <VBtn color="error" variant="tonal" @click="confirmDelete">
                                    Delete
                                </VBtn>

                                <VSpacer />

                                <!-- Action Buttons (Right) -->
                                <VBtn v-if="canApprove" :loading="isSubmitting" color="info" @click="confirmUpdate">
                                    Update
                                </VBtn>
                                <VBtn color="secondary" class="ml-2" @click="onDialogClose">
                                    Cancel
                                </VBtn>
                                <VBtn v-if="formInput.Status?.Ordinal === 4" :disabled="!canFinishTestWithComments"
                                    color="success" class="ml-2"
                                    :title="!canFinishTestWithComments ? 'กรุณากรอกคอมเม้นต์ให้ครบถ้วน' : ''"
                                    @click="confirmFinishTest">
                                    Finish Test
                                </VBtn>
                                <VBtn
                                    v-if="canApprove && formInput.Status?.Ordinal != 4 && formInput.Status?.Ordinal != 5"
                                    :loading="isSubmitting" :disabled="!canApproveWithComments" color="primary"
                                    class="ml-2" @click="confirmApprove"
                                    :title="!canApproveWithComments ? 'กรุณากรอกคอมเม้นต์ให้ครบถ้วน' : ''">
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

<style>
.stepper-container {
    display: flex;
    align-items: center;
    position: relative;
    margin: 20px 0;
}

.progress-line {
    position: absolute;
    top: 50%;
    left: 0;
    height: 2px;
    background-color: #4CAF50;
    z-index: 1;
    transition: width 0.3s ease;
}

.step-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    flex: 1;
    z-index: 2;
}

.step-circle {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: #e0e0e0;
    color: #666;
    font-weight: bold;
    margin-bottom: 8px;
    transition: all 0.3s ease;
}

.step-circle.active {
    background-color: #2196F3;
    color: white;
}

.step-circle.completed {
    background-color: #4CAF50;
    color: white;
}

.step-label {
    font-size: 12px;
    text-align: center;
    max-width: 80px;
    line-height: 1.2;
}

.lot-number-section {
    border: 1px solid #e0e0e0;
    border-radius: 8px;
    padding: 16px;
    background-color: #fafafa;
}

.file-table {
    border: 1px solid #e0e0e0;
}

.file-table th {
    background-color: #f5f5f5;
    font-weight: 600;
}

.file-table td {
    padding: 8px;
    vertical-align: middle;
}

.checklist-table {
    border-collapse: collapse;
}

.checklist-table th,
.checklist-table td {
    border: 1px solid #e0e0e0;
    padding: 8px;
    vertical-align: middle;
}

.checklist-table th {
    background-color: #f5f5f5;
    font-weight: 600;
}

.checklist-table tr:nth-child(even) {
    background-color: #fafafa;
}

.checklist-table tr:hover {
    background-color: #f0f0f0;
}

/* Highlight rows with asterisk (*) */
.checklist-table tr:has(td:contains("*")) {
    background-color: #333 !important;
    color: white;
}

.checklist-table tr:has(td:contains("*")) td {
    background-color: #333;
    color: white;
}

/* Autocomplete styling */
.v-autocomplete .v-field__input {
    cursor: text;
}

/* Lot number validation styling */
.lot-number-section .v-text-field--error .v-field__outline {
    border-color: #f44336;
}

.lot-number-section .v-messages--active {
    color: #f44336;
}

/* Combobox chips styling */
.v-combobox .v-chip {
    margin: 2px;
}

.v-combobox .v-chip--closable {
    padding-right: 4px;
}
</style>