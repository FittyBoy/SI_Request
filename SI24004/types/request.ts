import type { AttachementDTO } from "./attachement"

export interface RequestDTO {
    id: string | null
    requestCode: string
    requestName: string 
    userId: string 
    requestDescription?: string | null
    requestDate?: Date | null
    requestApprove: boolean 
    approveDate?: Date | null
    active: boolean
    isDeleted: boolean
    attachement: AttachementDTO[]
}

defineProps<{
    request: RequestDTO | null;
}>();
