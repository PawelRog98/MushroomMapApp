import { useMutation } from "@tanstack/react-query";
import { locationsApi } from "../api/locations";
import { useLocationStore } from "../../../store/locations-store";
import type { UpdateLocationRequest } from "../types";

export const useUpdateLocation = () => {
    const updateLocation = useLocationStore((state) => state.updateLocation);

    return useMutation({
        mutationFn: ({ id, data }: { id: string; data: UpdateLocationRequest }) =>
            locationsApi.updateLocation(id, data),
        onSuccess: (updatedLocation) => {
            updateLocation(updatedLocation);
        },
    });
};
