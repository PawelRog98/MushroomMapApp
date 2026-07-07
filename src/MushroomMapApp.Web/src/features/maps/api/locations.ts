import api from "../../../lib/axios";
import type { GetLocationRequest, UpdateLocationRequest, Location } from "../types";
import type { ApiResponse } from "../../../types/api";

export const locationsApi = {
    createLocation: async (formData: FormData): Promise<Location> => {
        for (const [key, value] of formData.entries()) {
            console.log(key, value);
        }
        const response = await api.post<ApiResponse<Location>>("/locations/create-location", formData);
        return response.data.data;
    },

    getLocations: async (data: GetLocationRequest): Promise<Location[]> => {
        const response = await api.get<ApiResponse<Location[]>>("/locations/get-locations", { params: data });
        return response.data.data;
    },

    updateLocation: async (id: string, data: UpdateLocationRequest): Promise<Location> => {
        const response = await api.put<ApiResponse<Location>>(`/locations/update-location/${id}`, data);
        return response.data.data;
    },

    deleteLocation: async (id: string): Promise<any> => {
        const response = await api.delete<ApiResponse<any>>(`/locations/delete-location/${id}`);
        return response.data.data;
    },
};
