import api from "../../../lib/axios";
import type { ApiResponse } from "../../../types/api";
import type { UserListItem, SuspendUserRequest, UnsuspendUserRequest, UserPermissionsDto, SetPermissionsRequest } from "../types";

export const adminApi = {
    getAllUsers: async (): Promise<UserListItem[]> => {
        const response = await api.get<ApiResponse<UserListItem[]>>("/users/get-all");
        return response.data.data;
    },
    suspendUser: async (data: SuspendUserRequest): Promise<void> => {
        await api.post("/users/suspend", data);
    },
    unsuspendUser: async (data: UnsuspendUserRequest): Promise<void> => {
        await api.post("/users/unsuspend", data);
    },
    getAllPermissions: async (): Promise<UserPermissionsDto> => {
        const response = await api.get<ApiResponse<UserPermissionsDto>>("/users/get-permissions");
        return response.data.data;
    },
    setPermissions: async (data: SetPermissionsRequest): Promise<void> => {
        await api.post("/users/set-permission", data);
    }
};
