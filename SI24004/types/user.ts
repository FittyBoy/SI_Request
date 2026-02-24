export interface UserDTO {
    Role: UserDTO | null;
    id?: string;
    userId: string;
    userPassword: string;
    loginHistory?: Date;
    roleId?: string;
    isDeleted?: boolean;
}