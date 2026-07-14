import { useQuery } from "@tanstack/react-query"
import { filesApi } from "../api/files"

export const useImage = (publicId: string | null) => {
    return useQuery({
        queryKey: ["image", publicId],
        queryFn: () => filesApi.getImage(publicId!),
        enabled: !!publicId,
    });
};
