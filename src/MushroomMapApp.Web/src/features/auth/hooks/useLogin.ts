import { useMutation } from "@tanstack/react-query";
import { authApi } from "../api/auth";
import { useAuthStore } from "../../../store/auth-store";
import { reloadPermissions } from "../../../utils/auth-sync";
import type { LoginFormValues } from "../types";
import { isEmailNotConfirmed } from "../../../utils/auth-errors";
import { useNavigate } from "react-router-dom";

export const useLogin = () => {
    const setAuth = useAuthStore((state) => state.setAuth);
    const navigate = useNavigate();
    return useMutation({
        mutationFn: (data: LoginFormValues) => authApi.login(data),
        onSuccess: async (data) => {
            setAuth(data);
            await reloadPermissions();
        },
        onError: (error, variables) =>{
            if(isEmailNotConfirmed(error)){
                navigate("/auth/verify-email", {
                    state: {
                        email: variables.email,
                    },
                });

                return;
            }
        }
    });
};
