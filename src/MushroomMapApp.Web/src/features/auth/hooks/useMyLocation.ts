import { useQuery } from "@tanstack/react-query"
import { authApi } from "../api/auth"

export const useMyLocation = (enabled: boolean) => {
    return useQuery({
        queryKey: ["my-location"],
        queryFn: async () => await authApi.getUserLocation(),
        enabled,
    });
};
