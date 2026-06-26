import { useMutation } from "@tanstack/react-query";
import { locationsApi } from "../api/locations";
import { useLocationStore } from "../../../store/locations-store";
import type { CreateLocationRequest } from "../types";

export const useCreateLocation = () => {
    const addLocation = useLocationStore((state) => state.addLocation);

    return useMutation({
        mutationFn: (data: CreateLocationRequest) => locationsApi.createLocation(data),
        onSuccess: (newLocation) => {
            addLocation(newLocation);
        },
    });
};
