import { useMutation } from "@tanstack/react-query";
import { locationsApi } from "../api/locations";
import { useLocationStore } from "../../../store/locations-store";

export const useDeleteLocation = () => {
    const removeLocation = useLocationStore((state) => state.removeLocation);

    return useMutation({
        mutationFn: (id: string) => locationsApi.deleteLocation(id),
        onSuccess: (_, id) => {
            removeLocation(id);
        },
    });
};
