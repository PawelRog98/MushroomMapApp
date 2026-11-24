import { keepPreviousData, useQuery } from "@tanstack/react-query";
import type { GetLocationRequest } from "../types";
import { locationsApi } from "../api/locations";

export const useLocations = (filters: GetLocationRequest) => {
    return useQuery({
        queryKey: ["locations", 
            "map",  
            filters.east,
            filters.north,
            filters.search,
            filters.south,
            filters.west,
            filters.zoom
        ],
        queryFn: ({signal}) => locationsApi.getLocations(filters, signal),
        placeholderData: keepPreviousData,
        staleTime: 30000,
        refetchOnWindowFocus: false,
    });
};