import { useQuery } from "@tanstack/react-query";
import type { GetLocationRequest } from "../types";
import { locationsApi } from "../api/locations";

export const useLocations =(filters: GetLocationRequest) => {
    return useQuery({
        queryKey: ["locations", filters],
        queryFn: () => locationsApi.getLocations(filters)
    });
}