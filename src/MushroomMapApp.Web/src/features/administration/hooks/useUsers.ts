import { useQuery } from "@tanstack/react-query";
import { adminApi } from "../api/admin";

export const useUsers = () => {
    return useQuery({
        queryKey: ["admin-users"],
        queryFn: () => adminApi.getAllUsers(),
    });
};
