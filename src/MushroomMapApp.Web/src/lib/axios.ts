import axios from "axios";
import { useAuthStore } from "../store/auth-store";
import { clearPermissions, reloadPermissions } from "../utils/auth-sync";

const api = axios.create({
    baseURL: import.meta.env.VITE_API_URL || "http://localhost:5000/api",
});

let isRefreshing = false;
let failedQueue: Array<{
    resolve: (value: unknown) => void;
    reject: (reason: unknown) => void;
}> = [];

const processQueue = (error: unknown, token: string | null = null) => {
    failedQueue.forEach((prom) => {
        if (error) {
            prom.reject(error);
        } else {
            prom.resolve(token);
        }
    });
    failedQueue = [];
};

api.interceptors.request.use(
    (config) => {
        const token = useAuthStore.getState().accessToken;
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        if (config.data instanceof FormData) {
            config.headers["Content-Type"] = "multipart/form-data";
        }
        return config;
    },
    (error) => Promise.reject(error),
);

api.interceptors.response.use(
    (response) => {
        const data = response.data?.value ?? response.data;
        if (data && typeof data === "object" && "success" in data && !data.success) {
            return Promise.reject(data);
        }
        return { ...response, data };
    },
    async (error) => {
        const originalRequest = error.config;

        if (error.response?.status === 401 && !originalRequest._retry) {
            if (isRefreshing) {
                return new Promise((resolve, reject) => {
                    failedQueue.push({ resolve, reject });
                }).then((token) => {
                    originalRequest.headers.Authorization = `Bearer ${token}`;
                    return api(originalRequest);
                });
            }

            originalRequest._retry = true;
            isRefreshing = true;

            const refreshToken = useAuthStore.getState().refreshToken;

            if (!refreshToken) {
                useAuthStore.getState().clearAuth();
                clearPermissions();
                
                isRefreshing = false;
                return Promise.reject(error);
            }

            try {
                const response = await axios.post(
                    `${api.defaults.baseURL}/users/refresh`,
                    { refreshToken },
                );

                const data = response.data?.value ?? response.data;
                const tokens = data?.data ?? data;

                useAuthStore.getState().setNewTokens({
                    accessToken: tokens.accessToken,
                    refreshToken: tokens.refreshToken,
                });

                await reloadPermissions();

                processQueue(null, tokens.accessToken);

                originalRequest.headers.Authorization = `Bearer ${tokens.accessToken}`;
                return api(originalRequest);
            } catch (refreshError) {
                processQueue(refreshError, null);
                useAuthStore.getState().clearAuth();
                return Promise.reject(refreshError);
            } finally {
                isRefreshing = false;
            }
        }

        return Promise.reject(error.response?.data?.value ?? error.response?.data ?? error);
    },
);

export default api;
