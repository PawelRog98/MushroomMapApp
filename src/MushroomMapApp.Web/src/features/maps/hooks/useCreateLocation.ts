import { useMutation } from "@tanstack/react-query";
import { locationsApi } from "../api/locations";
import type { CreateLocationRequest } from "../types";
import { queryClient } from "../../../lib/query-client";

export const useCreateLocation = () => {
    return useMutation({
        mutationFn: (data: CreateLocationRequest) => {
            const formData = new FormData();

            formData.append("name", data.name);
            formData.append("text", data.text);
            formData.append("lat", data.lat.toString());
            formData.append("lng", data.lng.toString());

            data.images.forEach((file) => {
                formData.append("Images", file);
            });

            return locationsApi.createLocation(formData);
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["locations"] });
        },
    });
};
