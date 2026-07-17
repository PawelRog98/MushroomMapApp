import { authApi } from "../features/auth/api/auth"
import { permissionStore } from "../store/permission-store";

export const reloadPermissions = async () => {
    const user = await authApi.getPermissions();
    permissionStore.getState()
        .setPermissions(user.permissions);
}

export const clearPermissions = () =>
{
    permissionStore.getState()
        .clear();
}