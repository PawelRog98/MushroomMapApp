import api from "../../../lib/axios";
import type { AddReactionRequest, ReactionDto, ReactionTypeDto } from "../types";
import type { ApiResponse } from "../../../types/api";

export const reactionsApi = {
    getReactionsForLocation: async (id: string): Promise<ReactionDto[]> => {
        const response = await api.get<ApiResponse<ReactionDto[]>>(`/reactions/get-reactions/${id}`);
        return response.data.data;
    },

    getTypes: async (): Promise<ReactionTypeDto[]> => {
        const response = await api.get<ApiResponse<ReactionTypeDto[]>>("/reactions/types");
        return response.data.data;
    },

    toggle: async (data: AddReactionRequest): Promise<void> => {
        await api.post("/reactions/add-reaction", data);
    },
};