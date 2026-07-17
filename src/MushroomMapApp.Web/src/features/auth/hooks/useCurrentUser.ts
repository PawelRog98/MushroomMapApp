import { useQuery } from "@tanstack/react-query"
import { authApi } from "../api/auth"

export const useCurrentUser = () =>{
    return useQuery({
        queryKey: ["current-user"],
        queryFn: async () => authApi.getPermissions()
    });
}