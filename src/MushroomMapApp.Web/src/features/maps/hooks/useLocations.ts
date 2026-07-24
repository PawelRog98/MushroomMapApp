import { useQuery } from "@tanstack/react-query";
import type { GetLocationRequest, Location, LocationPermissions } from "../types";
import { locationsApi } from "../api/locations";
import type { ItemWithMeta } from "../../../types/api";

export const useLocations =(
    filters: GetLocationRequest,
    onLocationChange: (locations: ItemWithMeta<Location, LocationPermissions>[]) => void
) => {
    return useQuery({
        queryKey: ["locations", filters],
        queryFn: async () => { 
            const locations = await locationsApi.getLocations(filters)
            onLocationChange(locations)

            return locations;
        },
        placeholderData: previous => previous
    });
}