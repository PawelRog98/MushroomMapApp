import { useMutation } from "@tanstack/react-query";
import { locationsApi } from "../api/locations";
import type { UpdateLocationRequest } from "../types";
import { queryClient } from "../../../lib/query-client";

export const useUpdateLocation = () => {
    return useMutation({
        mutationFn: ({ id, data }: { id: string; data: UpdateLocationRequest }) => {
            const formData = new FormData();

            formData.append("name", data.name);
            formData.append("text", data.text);

            data.images.forEach((file) =>{
                formData.append("Images", file);
            })

            data.keepImageIds.forEach((id, index) => {
                formData.append(`KeepImageIds[${index}]`, id);
            });

            return locationsApi.updateLocation(id, formData)
        },
        onSuccess: () => {
            queryClient.invalidateQueries({queryKey: ["locations"]});
        },
    });
};
