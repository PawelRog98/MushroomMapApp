import api from "../../../lib/axios";
import type { ClusterLocationResponse, GetClusterLocationRequest, GetLocationRequest, Location, LocationPermissions, MapLocationResponse } from "../types";
import type { ApiResponse, ItemWithMeta, PagingMeta } from "../../../types/api";

export const locationsApi = {
    createLocation: async (formData: FormData): Promise<Location> => {
        for (const [key, value] of formData.entries()) {
            console.log(key, value);
        }
        const response = await api.post<ApiResponse<Location>>("/locations/create-location", formData);
        return response.data.data;
    },

    getLocations: async (data: GetLocationRequest, signal?: AbortSignal): Promise<MapLocationResponse> => {
        const response = await api.get<ApiResponse<MapLocationResponse>>("/locations/get-locations", { params: data, signal });
        return response.data.data;
    },

    getClusterLocations: async (data: GetClusterLocationRequest, signal?: AbortSignal): Promise<ClusterLocationResponse> => {
        const response = await api.get<ApiResponse<ItemWithMeta<Location, LocationPermissions>[]>>("/locations/get-cluster-locations", { params: data, signal });

        const items = response.data.data ?? [];
        const meta = response.data.metaData as PagingMeta | null;

        return {
            items,
            paging: meta ?? {
                currentPage: data.page,
                pageSize: data.pageSize,
                totalCount: items.length,
                totalPages: 1,
                hasPrevious: false,
                hasNext: false,
            },
        };
    },

    updateLocation: async (id: string, formData: FormData): Promise<Location> => {
        const response = await api.put<ApiResponse<Location>>(`/locations/update-location/${id}`, formData);
        return response.data.data;
    },

    deleteLocation: async (id: string): Promise<any> => {
        const response = await api.delete<ApiResponse<any>>(`/locations/delete-location/${id}`);
        return response.data.data;
    },
};
