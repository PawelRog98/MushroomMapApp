import { useQuery } from "@tanstack/react-query";
import { reactionsApi } from "../api/reactions";

export const useReactionTypes = () => {
    return useQuery({
        queryKey: ["reaction-types"],
        queryFn: () => reactionsApi.getTypes(),
        staleTime: Infinity,
    });
};
