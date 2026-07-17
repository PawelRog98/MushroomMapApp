import { permissionStore } from "../../../store/permission-store"

export const useHasPermission =(permission: string) => {
    return permissionStore(state =>
        state.permissions.has(permission)
    );
}