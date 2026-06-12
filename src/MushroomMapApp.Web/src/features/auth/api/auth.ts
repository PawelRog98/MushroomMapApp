import api from "../../../lib/axios";
import type { AuthResponse, LoginFormValues, RegisterFormValues } from "../types";

export const authApi = {
    login: async (data: LoginFormValues): Promise<AuthResponse> => {
        const response = await api.post<AuthResponse>("/users/login", data);
        return response.data;
    },

    register: async (data: RegisterFormValues): Promise<void> => {
        await api.post("/users/register", data);
    },
};
