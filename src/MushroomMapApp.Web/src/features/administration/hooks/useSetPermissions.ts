import { useMutation } from "@tanstack/react-query"
import type { SetPermissionsRequest } from "../types";
import { adminApi } from "../api/admin";
import { queryClient } from "../../../lib/query-client";

export const useSetPermissions = () =>{
    return useMutation({
        mutationFn: (data: SetPermissionsRequest) => adminApi.setPermissions(data),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["admin-users"]});
        }
    });
}