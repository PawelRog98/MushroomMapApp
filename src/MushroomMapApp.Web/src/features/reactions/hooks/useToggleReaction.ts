import { useMutation } from "@tanstack/react-query";
import { reactionsApi } from "../api/reactions";
import { queryClient } from "../../../lib/query-client";
import type { AddReactionRequest } from "../types";

export const useToggleReaction = (locationPublicId: string) => {
    return useMutation({
        mutationFn: (data: AddReactionRequest) => reactionsApi.toggle(data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["reactions", locationPublicId]});
        },
    });
};