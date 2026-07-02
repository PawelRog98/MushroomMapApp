import { useMutation } from "@tanstack/react-query";
import { locationsApi } from "../api/locations";
import type { UpdateLocationRequest } from "../types";
import { queryClient } from "../../../lib/query-client";

export const useUpdateLocation = () => {
    return useMutation({
        mutationFn: ({ id, data }: { id: string; data: UpdateLocationRequest }) =>
            locationsApi.updateLocation(id, data),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["locations"]});
        },
    });
};
