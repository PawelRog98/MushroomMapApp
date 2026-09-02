import { useMutation } from "@tanstack/react-query";
import { adminApi } from "../api/admin";
import { queryClient } from "../../../lib/query-client";
import type { UnsuspendUserRequest } from "../types";

export const useUnsuspendUser = () => {
    return useMutation({
        mutationFn: (data: UnsuspendUserRequest) => adminApi.unsuspendUser(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["admin-users"] });
        },
    });
};
