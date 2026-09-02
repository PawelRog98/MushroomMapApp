import { useQuery } from "@tanstack/react-query"
import { adminApi } from "../api/admin"

export const usePermissions = () => {
    return useQuery({
        queryKey: ["admin-permissions"],
        queryFn: () => adminApi.getAllPermissions(),
    });
}