import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";

type AuthState = {
    accessToken: string | null;
    refreshToken: string | null;
    userNick: string | null;
    userId: string | null;
    isAuthenticated: boolean;
    setAuth: (data: { accessToken: string; refreshToken: string; userNick: string; userId: string }) => void;
    setNewTokens: (data: { accessToken: string; refreshToken: string }) => void;
    clearAuth: () => void;
};

export const useAuthStore = create<AuthState>()(
    persist(
        (set) => ({
            accessToken: null,
            refreshToken: null,
            userNick: null,
            userId: null,
            isAuthenticated: false,
            setAuth: (data) =>
                set({
                    accessToken: data.accessToken,
                    refreshToken: data.refreshToken,
                    userNick: data.userNick,
                    userId: data.userId,
                    isAuthenticated: true,
                }),
            setNewTokens: (data) =>
                set({
                    accessToken: data.accessToken,
                    refreshToken: data.refreshToken,
                }),
            clearAuth: () =>
                set({
                    accessToken: null,
                    refreshToken: null,
                    userNick: null,
                    userId: null,
                    isAuthenticated: false,
                }),
        }),
        {
            name: "auth-storage",
            storage: createJSONStorage(() => localStorage),
        },
    ),
);
