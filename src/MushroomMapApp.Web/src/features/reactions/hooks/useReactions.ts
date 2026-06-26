import { useQuery } from "@tanstack/react-query";
import { reactionsApi } from "../api/reactions";

export const useReactions = (locationPublicId: string) => {
    return useQuery({
        queryKey: ["reactions", locationPublicId],
        queryFn: () => reactionsApi.getReactionsForLocation(locationPublicId),
        enabled: !!locationPublicId,
    });
};