import { useState } from "react";
import { Link } from "react-router-dom";
import type { WeatherFloatingPanelProps } from "../types";
import { useWeather } from "../hooks/useWeather";
import { getWeatherCodeInfo } from "../utils/weather-codes";
import { CloudOff, CloudSun, Loader2, MapPin, RefreshCw, X } from "lucide-react";
import { getApiErrorMessage } from "../../../lib/api-error";
import { WeatherCard } from "./WeatherCard";
import { useMyLocation } from "../../auth/hooks/useMyLocation";
import { useAuthStore } from "../../../store/auth-store";

export const WeatherFloatingPanel = ({latitude, longitude}: WeatherFloatingPanelProps) => {
    const [isExpanded, setIsExpanded] = useState(false);
    const hasExplicitCoords = latitude != null && longitude != null;
    const { data: myLocation } = useMyLocation(!hasExplicitCoords);
    const userId = useAuthStore((s) => s.userId);

    const resolvedLatitude = latitude ?? myLocation?.lat ?? null;
    const resolvedLongitude = longitude ?? myLocation?.lng ?? null;
    const hasLocation = resolvedLatitude != null && resolvedLongitude != null;

    const {data, isPending, isError, error, refetch} = useWeather({
        latitude: resolvedLatitude,
        longitude: resolvedLongitude,
    });

    const WeatherIcon = data ? getWeatherCodeInfo(data.weatherCode).Icon : CloudSun;

    if(!isExpanded){
        return(
            <button
            onClick={() => setIsExpanded(true)}
            aria-label={
                data ? `Show weather, ${Math.round(data.temperature)} degrees` : "Show weather"
            }
            className="absolute top-4 right-4 z-[1000] flex items-center gap-2 rounded-full bg-white px-3 py-2 text-mushroom-700
                shadow-lg transition-colors hover:bg-mushroom-50 focus-visible:outline-none 
                focus-visible:ring-2 focus-visible:ring-forest-500">
                    {!hasLocation ? (
                        <CloudSun className="h-5 w-5 text-forest-600" />
                    ) : isPending ? (
                        <Loader2 className="h-5 w-5 animate-spin text-forest-600" />
                    ) : isError ? (
                        <CloudOff className="h-5 w-5 text-mushroom-400" />
                    ) : data ? (
                        <>
                            <WeatherIcon className="h-5 w-5 text-forest-600"/>
                            <span className="text-sm font-medium">{Math.round(data.temperature)}°C</span>
                        </>
                    ) : (
                        <CloudSun className="h-5 w-5 text-forest-600" />
                    )}

            </button>
        )
    }

    return(
        <div className="absolute top-4 right-4 z-[1000] w-64 overflow-hidden rounded-lg border border-mushroom-200 bg-white shadow-lg">
            <div className="flex items-center justify-between border-b border-mushroom-100 px-4 py-3">
                <h3 className="text-sm font-semibold text-forest-800">Weather</h3>
                <button
                    onClick={() => setIsExpanded(false)}
                    aria-label="Collapse weather panel"
                    className="text-mushroom-400 transition-colors hover:text-mushroom-600"
                >
                    <X className="h-4 w-4" />
                </button>
            </div>

            <div className="p-4">
                {!hasLocation ? (
                    <div className="space-y-3 text-center">
                        <p className="text-sm text-mushroom-600">
                            No localization set.
                        </p>
                        {userId && (
                            <Link
                                to={`/profile/${userId}`}
                                className="inline-flex items-center gap-1.5 text-sm font-medium text-forest-600 transition-colors hover:text-forest-700"
                            >
                                <MapPin className="h-4 w-4" />
                                Set my localization
                            </Link>
                        )}
                    </div>
                ) : isPending ? (
                    <div className="flex items-center justify-center py-8">
                        <Loader2 className="h-6 w-6 animate-spin text-forest-600" />
                    </div>
                ) : isError ? (
                    <div className="space-y-3 text-center">
                        <p className="text-sm text-mushroom-600">
                            {getApiErrorMessage(error, "Failed to load weather.")}
                        </p>
                        <button
                            onClick={() => refetch()}
                            className="inline-flex items-center gap-1.5 text-sm font-medium text-forest-600 transition-colors hover:text-forest-700"
                        >
                            <RefreshCw className="h-4 w-4" />
                            Retry
                        </button>
                    </div>
                ) : data ? (
                    <WeatherCard data={data} />
                ) : null}
            </div>
        </div>
    )
}