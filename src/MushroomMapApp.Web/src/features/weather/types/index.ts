export type WeatherData = {
    time: string;
    temperature: number;
    precipitation: number;
    windSpeed: number;
    weatherCode: number;
}

export type GetWeatherRequest = {
    latitude: number | null;
    longitude: number | null;
}

export type WeatherCardProps = {
    data: WeatherData,
    clasName?: string,
}

export type WeatherFloatingPanelProps ={
    latitude?: number | null;
    longitude?: number | null;
}