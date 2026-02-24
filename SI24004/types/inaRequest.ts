export interface InaRequestDTO {
    Id: string;
    RequestCode: string;
    UserId: string;
    RequestDescription?: string | null;
    RequestDate?: Date | null;
    StatusId?: string | null;
    Active?: boolean;
    IsDeleted?: boolean;
    Attachement?: [] | null; // เพิ่มการประกาศนี้
    AttachmentId: string;
    RequestMachine: string;
    RequestProduct: string;
    RequestMass?: boolean;
    RequestTest?: boolean;
    Status: {
        Id?: string;
        StatusName: string;
        Ordinal?: number | null;
        IsDeleted: boolean;
    };
    RequestComment1: string;
    RequestComment2: string;
    RequestComment3: string;
    Recipe: string;
    OtherPrograms: string;
    Attachment_Id_Size?: string;
    RequestBy: string;
    RequestProcess: string;
    RequestPurpose: string;
    RequestStartDate: Date | null;
    RequestFinishDate: Date | null;
    FlTgDeleted?: boolean | null;
    FlMcDeleted?: boolean | null;
    FlCheckMass?: boolean | null;
    FlDeletedOther?: boolean | null;
    CtCopyRp?: boolean | null;
    CtRpDeleted?: boolean | null;
    CtBookCheck?: boolean | null;
    CtDeletedOther?: boolean | null;
    FlTgDeletedComment?: string | null;
    FlMcDeletedComment?: string | null;
    FlCheckMassComment?: string | null;
    FlDeletedOtherComment?: string | null;
    CtCopyRpComment?: string | null;
    CtRpDeletedComment?: string | null;
    CtBookCheckComment?: string | null;
    CtDeletedOtherComment?: string | null;
    
    // เพิ่มฟิลด์เพิ่มเติมที่อาจต้องใช้
    LotNumbers?: string[];
    Attachments?: any;

    RequestObjective?: string | null; // Foreign Key (UUID)
    RequestType?: string | null; // Foreign Key (UUID)
    FlowInFactory?: string | null; // Factory name
    RequestMcNo?: string | null; // เลขเครื่องจักร
    RequestBook?: string | null; // Book number
    RequestInstallDate?: Date | null; // วันที่ติดตั้ง
    RequestClearDate?: Date | null; // วันที่เคลียร์โปรแกรม

}

defineProps<{
    request: InaRequestDTO | null;
}>();