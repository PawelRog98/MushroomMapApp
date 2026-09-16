import { useState, useCallback } from "react";
import { Plus, X } from "lucide-react";
import { MushroomMap } from "../features/maps/components/MushroomMap";
import type { Location, LocationPermissions } from "../features/maps/types";
import { useDeleteLocation } from "../features/maps/hooks/useDeleteLocation";
import type { ItemWithMeta } from "../types/api";

export const HomePage = () => {
    const [locations, setLocations] = useState<ItemWithMeta<Location, LocationPermissions>[]>([]);
    const { mutate: deleteLocation } = useDeleteLocation();

    const handleDeleteLocation = useCallback((id: string | null) => {
        if (id) {
            deleteLocation(id);
        }
    }, [deleteLocation]);

    const [isAddingMode, setIsAddingMode] = useState(false);

    return (
        <div className="h-full w-full relative">
            <MushroomMap
                locations={locations}
                isAddingMode={isAddingMode}
                onAddingComplete={() => setIsAddingMode(false)}
                onDeleteLocation={handleDeleteLocation}
                onLocationChange={setLocations}
            />

            <button
                onClick={() => setIsAddingMode(!isAddingMode)}
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

            {/* Adding mode hint */}
            {isAddingMode && (
                <div className="absolute bottom-20 right-6 z-[1000] bg-forest-800 text-white text-sm px-4 py-2 rounded-lg shadow-lg">
                    Click on the map to place a marker
                </div>
            )}
        </div>
    );
};
