import api from "../../../lib/axios";
import type { ApiResponse } from "../../../types/api";
import type { ImageResultDto } from "../types";

export const filesApi = {
    getImage: async (id: string): Promise<Blob> => {
        const response = await api.get<ApiResponse<ImageResultDto>>(`/files/get-image/${id}`);
        const { fileBytes, contentType } = response.data.data;

        const byteString = atob(fileBytes);
        const bytes = new Uint8Array(byteString.length);
        for (let i = 0; i < byteString.length; i++) {
            bytes[i] = byteString.charCodeAt(i);
        }

        return new Blob([bytes], { type: contentType || "image/jpeg" });
    }
};