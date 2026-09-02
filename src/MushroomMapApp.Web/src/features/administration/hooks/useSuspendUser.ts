import { useMutation } from "@tanstack/react-query";
import { adminApi } from "../api/admin";
import { queryClient } from "../../../lib/query-client";
import type { SuspendUserRequest } from "../types";

export const useSuspendUser = () => {
    return useMutation({
        mutationFn: (data: SuspendUserRequest) => adminApi.suspendUser(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["admin-users"] });
        },
    });
};
