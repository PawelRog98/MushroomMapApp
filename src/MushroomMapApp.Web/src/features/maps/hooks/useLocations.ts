import { useQuery } from "@tanstack/react-query";
import type { GetLocationRequest } from "../types";
import { locationsApi } from "../api/locations";

export const useLocations = (filters: GetLocationRequest) => {
    return useQuery({
        queryKey: ["locations", filters],
        queryFn: () => locationsApi.getLocations(filters),
        placeholderData: (previous) => previous,
        staleTime: 30000, // 30 seconds - prevent refetching on window focus
        refetchOnWindowFocus: false,
    });
};