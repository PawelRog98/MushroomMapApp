import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";

type AuthState = {
    accessToken: string | null;
    refreshToken: string | null;
    userNick: string | null;
    isAuthenticated: boolean;
    setAuth: (data: { accessToken: string; refreshToken: string; userNick: string }) => void;
    clearAuth: () => void;
};

export const useAuthStore = create<AuthState>()(
    persist(
        (set) => ({
            accessToken: null,
            refreshToken: null,
            userNick: null,
            isAuthenticated: false,
            setAuth: (data) =>
                set({
                    accessToken: data.accessToken,
                    refreshToken: data.refreshToken,
                    userNick: data.userNick,
                    isAuthenticated: true,
                }),
            clearAuth: () =>
                set({
                    accessToken: null,
                    refreshToken: null,
                    userNick: null,
                    isAuthenticated: false,
                }),
        }),
        {
            name: "auth-storage",
            storage: createJSONStorage(() => localStorage),
        },
    ),
);
