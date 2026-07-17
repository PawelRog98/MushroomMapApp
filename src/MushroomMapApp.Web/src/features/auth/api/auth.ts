import api from "../../../lib/axios";
import type { AuthResponse, LoginFormValues, RegisterFormValues, UserPermissions } from "../types";
import type { ApiResponse } from "../../../types/api";

export const authApi = {
    login: async (data: LoginFormValues): Promise<AuthResponse> => {
        const response = await api.post<ApiResponse<AuthResponse>>("/users/login", data);
        return response.data.data;
    },
    register: async (data: RegisterFormValues): Promise<void> => {
        await api.post("/users/register", data);
    },
    refresh: async (refreshToken: string): Promise<AuthResponse> => {
        const response = await api.post<ApiResponse<AuthResponse>>("/users/refresh", { refreshToken });
        return response.data.data;
    },
    logout: async (): Promise<void> => {
        await api.post("/users/logout");
    },
    getPermissions: async (): Promise<UserPermissions> => {
        const response = await api.get<ApiResponse<UserPermissions>>("users/get-permissions");
        return response.data.data;
    }
};
