import { Cloud, CloudDrizzle, CloudFog, CloudLightning, CloudRain, CloudSnow, CloudSun, Snowflake, Sun, type LucideIcon } from "lucide-react";

export type WeatherCodeInfo = {
    label: string;
    Icon: LucideIcon;
};

const WEATHER_CODES: Record<number, WeatherCodeInfo> = {
    0: {label: "Clear sky", Icon: Sun},
    1: { label: "Mainly clear", Icon: Sun },
    2: { label: "Partly cloudy", Icon: CloudSun },
    3: { label: "Overcast", Icon: Cloud },
    45: { label: "Fog", Icon: CloudFog },
    48: { label: "Depositing rime fog", Icon: CloudFog },
    51: { label: "Light drizzle", Icon: CloudDrizzle },
    53: { label: "Moderate drizzle", Icon: CloudDrizzle },
    55: { label: "Dense drizzle", Icon: CloudDrizzle },
    56: { label: "Light freezing drizzle", Icon: CloudDrizzle },
    57: { label: "Dense freezing drizzle", Icon: CloudDrizzle },
    61: { label: "Slight rain", Icon: CloudRain },
    63: { label: "Moderate rain", Icon: CloudRain },
    65: { label: "Heavy rain", Icon: CloudRain },
    66: { label: "Light freezing rain", Icon: CloudRain },
    67: { label: "Heavy freezing rain", Icon: CloudRain },
    71: { label: "Slight snow", Icon: CloudSnow },
    73: { label: "Moderate snow", Icon: CloudSnow },
    75: { label: "Heavy snow", Icon: CloudSnow },
    77: { label: "Snow grains", Icon: Snowflake },
    80: { label: "Slight rain showers", Icon: CloudRain },
    81: { label: "Moderate rain showers", Icon: CloudRain },
    82: { label: "Violent rain showers", Icon: CloudRain },
    85: { label: "Slight snow showers", Icon: CloudSnow },
    86: { label: "Heavy snow showers", Icon: CloudSnow },
    95: { label: "Thunderstorm", Icon: CloudLightning },
    96: { label: "Thunderstorm with slight hail", Icon: CloudLightning },
    99: { label: "Thunderstorm with heavy hail", Icon: CloudLightning },
}

const FALLBACK: WeatherCodeInfo = {label: "Unknown", Icon: Cloud};

export const getWeatherCodeInfo = (code: number): WeatherCodeInfo =>
    WEATHER_CODES[code] ?? FALLBACK;