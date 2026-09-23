import { useMutation } from "@tanstack/react-query"
import type { CreateNewVerificationTokenRequest } from "../types"
import { authApi } from "../api/auth"

export const useResendCode = () => {
    return useMutation({
        mutationFn: (data: CreateNewVerificationTokenRequest) =>
            authApi.createNewToken(data)
    });
};