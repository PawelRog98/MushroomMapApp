import { useQuery } from "@tanstack/react-query";
import type { GetWeatherRequest } from "../types";
import { weatherApi } from "../api/weather";

export const useWeather = (data: GetWeatherRequest) => {
    return useQuery({
        queryKey: ["weather-location", data],
        queryFn: () => weatherApi.getWeather(data),
        enabled: data?.latitude != null && data?.longitude != null,
        staleTime: 30 * 60 *1000,
    });
}