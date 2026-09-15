import { useMutation } from "@tanstack/react-query"
import { authApi } from "../api/auth"
import { queryClient } from "../../../lib/query-client"
import type { UpdateProfileInput } from "../types";

export const useUpdateProfile = () => {
    return useMutation({
        mutationFn: async ({data, avatar} : UpdateProfileInput ) =>{ 
            await authApi.updateUserData(data);

            if(avatar){
                const formData = new FormData();
                formData.append("image", avatar);

                await authApi.updateUserAvatar(formData);
            }
        },
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ["profile"]});
        },
    });
};