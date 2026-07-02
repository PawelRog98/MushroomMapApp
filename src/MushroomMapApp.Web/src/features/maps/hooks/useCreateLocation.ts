import { useMutation } from "@tanstack/react-query";
import { locationsApi } from "../api/locations";
import type { CreateLocationRequest } from "../types";
import { queryClient } from "../../../lib/query-client";

export const useCreateLocation = () => {

    return useMutation({
        mutationFn: (data: CreateLocationRequest) => locationsApi.createLocation(data),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["locations"]})
        },
    });
};
