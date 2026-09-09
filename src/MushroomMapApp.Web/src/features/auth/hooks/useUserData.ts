import { useQuery } from "@tanstack/react-query"
import { authApi } from "../api/auth"

export const useUserData = (userPublicId: string) => {
    return useQuery({
        queryKey: ["profile", userPublicId],
        queryFn: () => authApi.getUserData(userPublicId),
    });
};