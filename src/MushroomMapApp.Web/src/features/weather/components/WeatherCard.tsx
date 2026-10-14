import { Clock, Droplets, Wind } from "lucide-react";
import { cn } from "../../../lib/utils";
import type { WeatherCardProps } from "../types";
import { getWeatherCodeInfo } from "../utils/weather-codes";

export const WeatherCard = ({data, clasName} : WeatherCardProps) => {
    const { label, Icon} = getWeatherCodeInfo(data.weatherCode);

    return(
        <div className={cn("space-y-3", clasName)}>
            <div className="flex items-center gap-3">
                <Icon className="h-10 w-10 shrink-0 text-forest-600"/>
                <div>
                    <p className="text-3xl font-bold leading-none text-mushroom-900">
                        {Math.round(data.temperature)}°C
                    </p>
                    <p className="mt-1 text-sm text-mushroom-500">{label}</p>
                </div>
            </div>

            <div className="grid grid-cols-2 gap-2">
                <div className="flex items-center gap-2 rounded-md bg-mushroom-50 px-2.5 py-2">
                    <Droplets className="h-4 w-4 shrink-0 text-forest-600" />
                    <div className="min-w-0">
                        <p className="text-sm font-medium text-mushroom-900">
                            {data.precipitation.toFixed(1)} mm
                        </p>
                        <p className="text-xs text-mushroom-500">Precipitation</p>
                    </div>
                </div>
                <div className="flex items-center gap-2 rounded-md bg-mushroom-50 px-2.5 py-2">
                    <Wind className="h-4 w-4 shrink-0 text-forest-600" />
                    <div className="min-w-0">
                        <p className="text-sm font-medium text-mushroom-900">
                            {Math.round(data.windSpeed)} km/h
                        </p>
                        <p className="text-xs text-mushroom-500">Wind</p>
                    </div>
                </div>
            </div>

            <div className="flex items-center gap-1.5 border-t border-mushroom-100 pt-2 text-xs text-mushroom-400">
                <Clock className="h-3 w-3" />
                <span>{data.time}</span>
            </div>
        </div>
    )
}