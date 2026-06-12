import { useMutation } from "@tanstack/react-query";
import { authApi } from "../api/auth";
import type { RegisterFormValues } from "../types";

export const useRegister = () => {
    return useMutation({
        mutationFn: (data: RegisterFormValues) => authApi.register(data),
    });
};
