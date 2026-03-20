export interface LoginResponse {
  token: string;
  user: {
    id: string;
    userId: string;
    userName: string;
    roleName: string; // หรือสามารถใช้ roleName ตามที่คุณส่งกลับมา
  };
  sectionId: string;
  sectionName: string;
}
