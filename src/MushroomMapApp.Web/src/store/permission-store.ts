import { create } from "zustand";

interface PermissionState {
    permissions: Set<string>;
    loaded: boolean;
    setPermissions: (permissions: string[]) => void;
    clear: () => void;
}

export const permissionStore = create<PermissionState>()(
    (set) => ({
        permissions: new Set(),
        loaded: false,
        setPermissions: (permissions) =>
            set({
                permissions: new Set(permissions),
                loaded: true
            }),
        clear: () => 
            set({
                permissions: new Set(),
                loaded: false
            })
    })
)