import api from "../../../lib/axios";
import type { AuthResponse, LoginFormValues, RegisterFormValues, UpdateUserDataFormValues, UserPermissions, UserProfile } from "../types";
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
        const response = await api.get<ApiResponse<UserPermissions>>("/users/get-permissions");
        return response.data.data;
    },
    getUserData: async (userPublicId: string): Promise<UserProfile> => {
        const response = await api.get<ApiResponse<UserProfile>>("/users/get-user-data", {
            params: { userPublicId },
        });
        return response.data.data;
    },
    updateUserData: async(data: UpdateUserDataFormValues) : Promise<void> => {
        const response = await api.put("/users/update-user-data", data);
        return response.data.data;
    }
};
