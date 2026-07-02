import { useQuery } from "@tanstack/react-query";
import type { GetLocationRequest, Location } from "../types";
import { locationsApi } from "../api/locations";

export const useLocations =(
    filters: GetLocationRequest,
    onLocationChange: (locations: Location[]) => void
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