import { useMutation } from "@tanstack/react-query"
import { authApi } from "../api/auth"
import { queryClient } from "../../../lib/query-client"

export const useUpdateProfile = () => {
    return useMutation({
        mutationFn: authApi.updateUserData,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["profile"]});
        },
    });
};