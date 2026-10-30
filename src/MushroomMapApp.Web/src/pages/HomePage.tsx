import { useState, useCallback } from "react";
import { Flame, Plus, X } from "lucide-react";
import { MushroomMap } from "../features/maps/components/MushroomMap";
import type { Location, LocationPermissions } from "../features/maps/types";
import { useDeleteLocation } from "../features/maps/hooks/useDeleteLocation";
import type { ItemWithMeta } from "../types/api";
import { WeatherFloatingPanel } from "../features/weather/components/WeatherFloatingPanel";

export const HomePage = () => {
    const [locations, setLocations] = useState<ItemWithMeta<Location, LocationPermissions>[]>([]);
    const { mutate: deleteLocation } = useDeleteLocation();

    const handleDeleteLocation = useCallback((id: string | null) => {
        if (id) {
            deleteLocation(id);
        }
    }, [deleteLocation]);

    const toggleAddingMode = () => {
        setIsAddingMode((prev) => {
            if (!prev) {
                setHeatmapEnabled(false);
            }
            return !prev;
        });
    };

    const [isAddingMode, setIsAddingMode] = useState(false);
    const [heatmapEnabled, setHeatmapEnabled] = useState(false);

    return (
        <div className="h-full w-full relative">
            <MushroomMap
                locations={locations}
                isAddingMode={isAddingMode}
                onAddingComplete={() => setIsAddingMode(false)}
                heatMapEnabled={heatmapEnabled}
                onDeleteLocation={handleDeleteLocation}
                onLocationChange={setLocations}
            />

            <WeatherFloatingPanel />

            <button
                onClick={toggleAddingMode}
                className={`absolute bottom-6 right-6 z-[1000] flex items-center gap-2 px-4 py-3 rounded-full shadow-lg transition-all ${isAddingMode
                    ? "bg-red-500 hover:bg-red-600 text-white"
                    : "bg-mushroom-600 hover:bg-mushroom-700 text-white"
                    }`}
            >
                {isAddingMode && (
                    <span className="absolute inset-0 rounded-full border-2 border-red-400 animate-ping" />
                )}
                {isAddingMode ? (
                    <>
                        <X className="h-5 w-5" />
                        <span className="font-medium">Cancel</span>
                    </>
                ) : (
                    <>
                        <Plus className="h-5 w-5" />
                        <span className="font-medium">Add Spot</span>
                    </>
                )}
            </button>
            <button
                onClick={() => setHeatmapEnabled((v) => !v)}
                aria-pressed={heatmapEnabled}
                aria-label={heatmapEnabled ? "Show individual markers" : "Show marker density heatmap"}
                title={heatmapEnabled ? "Show individual markers" : "Show density heatmap"}
                className={`absolute bottom-6 left-6 z-[1000] flex items-center gap-2 rounded-full px-4 py-3 shadow-lg transition-colors ${heatmapEnabled
                        ? "bg-forest-600 hover:bg-forest-700 text-white"
                        : "bg-white hover:bg-mushroom-50 text-mushroom-700"
                    }`}
            >
                <Flame className="h-5 w-5" />
                <span className="font-medium">Density</span>
            </button>

            {isAddingMode && (
                <div className="absolute bottom-20 right-6 z-[1000] bg-forest-800 text-white text-sm px-4 py-2 rounded-lg shadow-lg">
                    Click on the map to place a marker
                </div>
            )}
        </div>
    );
};
