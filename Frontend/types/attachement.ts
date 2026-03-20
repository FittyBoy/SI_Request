export interface AttachementDTO {
    id: string;
    AttachmentName: string;
    AttachementPath: string | null;
    AttachementType: string | null;
    UploadDate: Date | null;
    requestApprove: boolean;
    attachmentFileData: string | null;
    isDeleted: boolean;

}