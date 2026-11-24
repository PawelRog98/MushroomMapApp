import { keepPreviousData, useQuery } from "@tanstack/react-query";
import type { LocationCluster } from "../types";
import { locationsApi } from "../api/locations";

export const useClusterLocations = (
    cluster: LocationCluster | null,
    search: string | null,
    page: number,
    pageSize: number,
) => {
    return useQuery({
        queryKey: [
            "locations",
            "cluster",
            cluster?.zoom,
            cluster?.cellX,
            cluster?.cellY,
            search,
            page,
            pageSize,
        ],
        queryFn: ({ signal }) => {
            if (!cluster)
                throw new Error("Cluster is required");

            return locationsApi.getClusterLocations({
                cellX: cluster.cellX,
                cellY: cluster.cellY,
                zoom: cluster.zoom,
                search,
                page,
                pageSize,
            },
                signal,
            );
        },
        enabled: !!cluster,
        placeholderData: keepPreviousData,
        staleTime: 15000,
        refetchOnWindowFocus: false,
    });
};