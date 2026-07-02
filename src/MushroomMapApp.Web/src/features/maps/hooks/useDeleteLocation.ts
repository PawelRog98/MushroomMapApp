import { useMutation } from "@tanstack/react-query";
import { locationsApi } from "../api/locations";
import { queryClient } from "../../../lib/query-client";

export const useDeleteLocation = () => {
    return useMutation({
        mutationFn: (id: string) => locationsApi.deleteLocation(id),
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["locations"]});
        },
    });
};
