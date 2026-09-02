export interface UserListItem {
    publicId: string;
    publicNick: string;
    firstName: string;
    lastName: string;
    email: string;
    roleName: string;
    isActiveSuspension: boolean;
    suspensionEndDate: string | null;
    activePermissions: string[];
}

export interface SuspendUserRequest {
    userPublicId: string;
    days: number;
    reason: string;
}

export interface UnsuspendUserRequest {
    userPublicId: string;
}

export interface SetPermissionsRequest {
    userPublicId: string;
    permissions: string[];
}

export interface UserPermissionsDto{
    permissions: string[];
}

export type SuspensionUserPopupProps = {
    isOpen: boolean;
    onClose: () => void;
    user: UserListItem | null;
    onConfirm: (days: number, reason: string) => void;
    isPending: boolean;
};

export type UserPermissionsPopupProps = {
    isOpen: boolean;
    onClose: () => void;
    user: UserListItem | null;
    onConfirm: (permissions: string[]) => void;
    isPending: boolean;
}
