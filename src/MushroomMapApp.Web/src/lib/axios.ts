import axios from "axios";
import { useAuthStore } from "../store/auth-store";

const api = axios.create({
    baseURL: import.meta.env.VITE_API_URL || "http://localhost:5000/api",
    headers: {
        "Content-Type": "application/json",
    },
});

api.interceptors.request.use(
    (config) => {
        const token = useAuthStore.getState().accessToken;
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
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
        if (error.response?.status === 401) {
            useAuthStore.getState().clearAuth();
        }
        return Promise.reject(error.response?.data?.value ?? error.response?.data ?? error);
    },
);

export default api;
