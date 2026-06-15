import api from "../../../lib/axios";
import type { AuthResponse, LoginFormValues, RegisterFormValues } from "../types";
import type { ApiResponse } from "../../../types/api";

export const authApi = {
    login: async (data: LoginFormValues): Promise<AuthResponse> => {
        const response = await api.post<ApiResponse<AuthResponse>>("/users/login", data);
        return response.data.data;
    },
    register: async (data: RegisterFormValues): Promise<void> => {
        await api.post("/users/register", data);
    },
};
