import { useMutation } from "@tanstack/react-query"
import type { AcctivateAccountRequest } from "../types"
import { authApi } from "../api/auth"

export const useActivateAccount = () => {
    return useMutation({
        mutationFn: (data: AcctivateAccountRequest) =>
            authApi.acctivateAccount(data),
    });
};