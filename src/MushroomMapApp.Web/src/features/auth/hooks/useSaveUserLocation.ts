import { useMutation } from "@tanstack/react-query"
import type { CreateUserLocationRequest } from "../types"
import { authApi } from "../api/auth"
import { queryClient } from "../../../lib/query-client"

export const useSaveUserLocation = () => {
    return useMutation({
        mutationFn: (data: CreateUserLocationRequest) => authApi.saveUserLocation(data),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["my-location"]});
        },
    });
}