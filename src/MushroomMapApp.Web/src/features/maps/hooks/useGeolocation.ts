import { useState, useCallback } from "react";

type GeolocationState = {
    latitude: number | null;
    longitude: number | null;
    error: string | null;
    isLoading: boolean;
};

export const useGeolocation = () => {
    const [state, setState] = useState<GeolocationState>({
        latitude: null,
        longitude: null,
        error: null,
        isLoading: false,
    });

    const locate = useCallback(() => {
        // Check if geolocation is supported
        if (!navigator.geolocation) {
            setState((prev) => ({
                ...prev,
                error: "Geolocation is not supported by your browser",
            }));
            return;
        }

        // Check if page is served over HTTPS (required for geolocation)
        if (window.location.protocol !== "https:" && window.location.hostname !== "localhost") {
            setState((prev) => ({
                ...prev,
                error: "Geolocation requires HTTPS in production",
            }));
            return;
        }

        setState((prev) => ({ ...prev, isLoading: true, error: null }));

        navigator.geolocation.getCurrentPosition(
            (position) => {
                setState({
                    latitude: position.coords.latitude,
                    longitude: position.coords.longitude,
                    error: null,
                    isLoading: false,
                });
            },
            (error) => {
                let errorMessage = "Failed to get your location";

                switch (error.code) {
                    case error.PERMISSION_DENIED:
                        errorMessage = "Location access denied. Please enable location permissions.";
                        break;
                    case error.POSITION_UNAVAILABLE:
                        errorMessage = "Location information unavailable.";
                        break;
                    case error.TIMEOUT:
                        errorMessage = "Location request timed out.";
                        break;
                }

                setState((prev) => ({
                    ...prev,
                    isLoading: false,
                    error: errorMessage,
                }));
            },
            {
                enableHighAccuracy: true,
                timeout: 10000,
                maximumAge: 0,
            }
        );
    }, []);

    const clearError = useCallback(() => {
        setState((prev) => ({ ...prev, error: null }));
    }, []);

    return {
        ...state,
        locate,
        clearError,
    };
};
