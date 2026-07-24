import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";

interface PermissionState {
    permissions: Set<string>;
    loaded: boolean;
    setPermissions: (permissions: string[]) => void;
    clear: () => void;
}

export const permissionStore = create<PermissionState>()(
    persist(
        (set) => ({
            permissions: new Set(),
            loaded: false,
            setPermissions: (permissions) =>
                set({
                    permissions: new Set(permissions),
                    loaded: true,
                }),
            clear: () =>
                set({
                    permissions: new Set(),
                    loaded: false,
                }),
        }),
        {
            name: "permission-storage",
            storage: createJSONStorage(() => localStorage),
            partialize: (state) => ({
                permissions: Array.from(state.permissions),
                loaded: state.loaded,
            }),
            merge: (persisted, current) => ({
                ...current,
                ...(persisted as { permissions: string[]; loaded: boolean }),
                permissions: new Set(
                    (persisted as { permissions: string[] }).permissions,
                ),
            }),
        },
    ),
)