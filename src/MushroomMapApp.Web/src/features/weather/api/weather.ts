import api from "../../../lib/axios"
import type { ApiResponse } from "../../../types/api"
import type { GetWeatherRequest, WeatherData } from "../types"

export const weatherApi =  {
    getWeather: async (data: GetWeatherRequest) : Promise<WeatherData> => {
        const response = await api.get<ApiResponse<WeatherData>>("/weather/get-weather", {params: data});
        return response.data.data;
    },
};