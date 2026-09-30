import api from "../../../lib/axios";
import type { ApiResponse, Paged, PagingMeta, SortDir } from "../../../types/api";
import type { UserListItem, SuspendUserRequest, UnsuspendUserRequest, UserPermissionsDto, SetPermissionsRequest } from "../types";

export const adminApi = {
    getAllUsers: async (params: { page: number; pageSize: number; sortBy?: string; sortDir?: SortDir }): Promise<Paged<UserListItem>> => {
        const response = await api.get<ApiResponse<UserListItem[]>>("/users/get-all", { params });
        const items = response.data.data ?? [];
        const meta = response.data.metaData as PagingMeta | null;
        return {
            items,
            paging: meta ?? {
                currentPage: params.page,
                pageSize: params.pageSize,
                totalCount: items.length,
                totalPages: 1,
                hasPrevious: false,
                hasNext: false,
                sortBy: params.sortBy ?? "nick",
                sortDir: params.sortDir ?? "asc",
            },
        };
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
