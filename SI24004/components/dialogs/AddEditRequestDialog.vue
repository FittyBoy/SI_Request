<script setup lang="ts">
import type { RequestDTO } from '@/types/request';
import { VForm } from 'vuetify/components/VForm'
import { onMounted, watch, ref } from 'vue';
import { shallowRef } from 'vue'
import Swal from 'sweetalert2'

const swal = Swal

const props = defineProps({
    isDialogVisible: Boolean,
    request: Object as PropType<RequestDTO>,
});

const formInput = ref<RequestDTO>(structuredClone(toRaw(props.request || {
    id: props.request?.id ? props.request?.id.toString() : null,
    requestCode: props.request?.requestCode || '',
    requestName: props.request?.requestName || '',
    userId: props.request?.userId || '',
    requestDescription: props.request?.requestDescription || '',
    requestDate: props.request?.requestDate || null,
    requestApprove: props.request?.requestApprove || false,
    approveDate: props.request?.approveDate || null,
    active: props.request?.active || true,
    isDeleted: props.request?.isDeleted || false,
    attachement: [],
})));

const emit = defineEmits(['update:isDialogVisible', 'submit']);
const refVFormRequest = ref<VForm>();
const isSubmitting = ref(false);

const requiredValidator = (value: string) => !!value || 'This field is required.';

const onFormSubmit = async () => {
    const validationResult = await refVFormRequest.value?.validate();
    if (validationResult && validationResult.valid) {
        isSubmitting.value = true;
        try {
            swal.fire({
                title: 'Success Alert',
                text: 'Add data Success',
                icon: 'success',
                confirmButtonText: 'OK'
            })
            await emit('submit', formInput.value);
            emit('update:isDialogVisible', false);
        } catch (error) {
            swal.fire({
                title: 'Error!',
                text: 'Error submitting data',
                icon: 'error',
                confirmButtonText: 'Back'
            }
            )
        } finally {
            isSubmitting.value = false;
        }
    }
};

const onDialogClose = () => {
    emit('update:isDialogVisible', false); // ตรวจสอบว่า isDialogVisible เป็น boolean
};

watch(() => props.request, (newRequest) => {
    if (newRequest) {
        formInput.value = structuredClone(newRequest);
    } else {
        formInput.value = {
            id: null,
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
        }; // ตั้งค่าดีฟอลต์
    }
});
const density = shallowRef('default')

</script>

<template>
    <VDialog :model-value="isDialogVisible" @update:model-value="onDialogClose">
        <VCard>
            <VCardTitle>
                {{ formInput.id != '' ? 'Edit Request' : 'Add New Request' }}
            </VCardTitle>
            <VCardText>
                <VForm ref="refVFormRequest" @submit.prevent="onFormSubmit">
                    <VRow>
                        <VCol cols="6">
                            <AppTextField v-model="formInput.requestCode" label="Request Name"
                                :value="formInput?.requestCode || ''" placeholder="Request Code Auto Generate" disabled />
                        </VCol>
                        <VCol cols="6">
                            <AppTextField v-model="formInput.requestName" :rules="[requiredValidator]"
                                label="Request Name" :value="formInput?.requestName || ''" placeholder="กรุณากรอกชื่อ"
                                required />
                        </VCol>
                    </VRow>
                    <VRow>
                        <VCol cols="6">
                            <VRow>
                                <VCol cols="6">
                                    <v-switch style="margin-top: 1rem;" v-model="formInput.requestApprove"
                                        label="Request Approve" />
                                </VCol>
                                <VCol cols="6">
                                    <v-switch style="margin-top: 1rem;" v-model="formInput.active"
                                        label="Request Active" />
                                </VCol>
                            </VRow>
                        </VCol>
                    </VRow>
                    <VRow>
                        <VCol cols="12">
                            <v-textarea v-model="formInput.requestDescription" :rules="[requiredValidator]"
                                label="Request Description" :value="formInput?.requestDescription || ''"
                                placeholder="กรุณากรอกรายละเอียด" required />
                        </VCol>
                    </VRow>
                    <VRow>
                        <VCol cols="12">
                            <v-file-input label="Upload New File" variant="outlined" required v-model="formInput.attachement"
                                :disabled="!!formInput.id"></v-file-input>
                        </VCol>
                    </VRow>
                    <VRow>
                        <VCol cols="12">

                        </VCol>
                    </VRow>
                    <VRow class="mt-4">
                        <VCol cols="12" class="d-flex justify-center gap-4">
                            <VBtn :loading="isSubmitting" type="submit">Submit</VBtn>
                            <VBtn color="secondary" variant="tonal" @click="onDialogClose">Cancel</VBtn>
                        </VCol>
                    </VRow>
                </VForm>
            </VCardText>
        </VCard>
    </VDialog>
</template>
