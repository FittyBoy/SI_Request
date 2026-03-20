export interface Attachment {
    id: string;
    attachmentName: string;
    attachmentPath: string;
    attachmentType: string;
  }
  
  export interface Drawing {
    id: string;
    drawingName: string;
  }
  
  export interface Section {
    id: string;
    sectionName: string;
  }
  
  export interface Status {
    Id: string;
    StatusName: string;
    Ordinal: number;
  }
  
  export interface User {
    Id: string;
    UserName: string;
  }
  
  export interface DwRequestDTO {
    Id: string;
    DrawingCode: string;
    RequestCode: string;
    DrawingName: string;
    SectionId: string | null;
    DrawingTypeId: string | null;
    StatusId?: string | null;
    CreatedDate: string; // DateOnly เป็น string
    CreatedBy: string;
    UpdateDate?: string | null;
    UpdateBy?: string;
    DrawingDescription: string;
    UserId?: string | null;
    AttachmentId?: string | null;
    Active?: boolean;
    IsDelete?: boolean;
    Attachment?: Attachment[] | null;  // ใช้พหูพจน์สำหรับหลาย attachment
    DrawingType?: Drawing;
    Section?: Section;
    Status?: Status;
    User?: User;
  }
  
  
  