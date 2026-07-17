import { useMutation } from "@tanstack/react-query";
import { authApi } from "../api/auth";
import { useAuthStore } from "../../../store/auth-store";
import { reloadPermissions } from "../../../utils/auth-sync";
import type { LoginFormValues } from "../types";

export const useLogin = () => {
    const setAuth = useAuthStore((state) => state.setAuth);
    return useMutation({
        mutationFn: (data: LoginFormValues) => authApi.login(data),
        onSuccess: async (data) => {
            setAuth(data);
            await reloadPermissions();
        },
    });
};
