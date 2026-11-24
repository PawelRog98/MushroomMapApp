import { useCallback, useState } from "react";
import { Flame, Plus, X } from "lucide-react";
import { MushroomMap } from "../features/maps/components/MushroomMap";
import { ClusterLocationsPanel } from "../features/maps/components/ClusterLocationsPanel";
import type {
    Location,
    LocationCluster,
    LocationPermissions,
    MapLocationResponse,
} from "../features/maps/types";
import { useDeleteLocation } from "../features/maps/hooks/useDeleteLocation";
import type { ItemWithMeta } from "../types/api";
import { WeatherFloatingPanel } from "../features/weather/components/WeatherFloatingPanel";

const EMPTY_MAP_DATA: MapLocationResponse = {
    mode: "markers",
    totalCount: 0,
    truncated: false,
    markers: [],
    clusters: [],
};

export const HomePage = () => {
    const [locations, setLocations] = useState<MapLocationResponse>(EMPTY_MAP_DATA);
    const [selectedCluster, setSelectedCluster] = useState<LocationCluster | null>(null);
    const [focusedLocation, setFocusedLocation] = useState<ItemWithMeta<Location, LocationPermissions> | null>(null);
    const [focusToken, setFocusToken] = useState(0);
    const [flyTo, setFlyTo] = useState<{ lat: number; lng: number } | null>(null);
    const [fitBoundsTarget, setFitBoundsTarget] = useState<LocationCluster | null>(null);
    const [isAddingMode, setIsAddingMode] = useState(false);
    const [heatmapEnabled, setHeatmapEnabled] = useState(false);

    const search: string | null = null;

    const { mutate: deleteLocation } = useDeleteLocation();

    const handleDeleteLocation = useCallback((id: string | null) => {
        if (id) {
            deleteLocation(id);
            setFocusedLocation((current) =>
                current?.data.publicId === id ? null : current
            );
        }
    }, [deleteLocation]);

    const handleLocationChange = useCallback((next: MapLocationResponse) => {
        setLocations(next);
    }, []);

    const handleClusterClick = useCallback((cluster: LocationCluster) => {
        setSelectedCluster(cluster);
        setFocusedLocation(null);
    }, []);

    const handleClusterClose = useCallback(() => {
        setSelectedCluster(null);
        setFocusedLocation(null);
    }, []);

    const handleSelectClusterLocation = useCallback(
        (item: ItemWithMeta<Location, LocationPermissions>) => {
            setFocusedLocation(item);
            setFocusToken((token) => token + 1);
            setFlyTo({ lat: item.data.lat, lng: item.data.lng });
        },
        [],
    );

    const handleZoomToArea = useCallback((cluster: LocationCluster) => {
        setFitBoundsTarget(cluster);
    }, []);

    const handleFlyHandled = useCallback(() => setFlyTo(null), []);
    const handleFitBoundsHandled = useCallback(() => setFitBoundsTarget(null), []);

    const toggleAddingMode = () => {
        setIsAddingMode((prev) => {
            if (!prev) {
                setHeatmapEnabled(false);
                setSelectedCluster(null);
            }
            return !prev;
        });
    };

    return (
        <div className="h-full w-full relative">
            <MushroomMap
                locations={locations}
                isAddingMode={isAddingMode}
                search={search}
                onAddingComplete={() => setIsAddingMode(false)}
                heatMapEnabled={heatmapEnabled}
                focusedLocation={focusedLocation}
                focusToken={focusToken}
                flyTo={flyTo}
                fitBoundsTarget={fitBoundsTarget}
                onDeleteLocation={handleDeleteLocation}
                onLocationChange={handleLocationChange}
                onClusterClick={handleClusterClick}
                onFlyHandled={handleFlyHandled}
                onFitBoundsHandled={handleFitBoundsHandled}
            />

            {selectedCluster && (
                <ClusterLocationsPanel
                    key={`${selectedCluster.zoom}:${selectedCluster.cellX}:${selectedCluster.cellY}:${search ?? ""}`}
                    cluster={selectedCluster}
                    search={search}
                    onClose={handleClusterClose}
                    onSelect={handleSelectClusterLocation}
                    onZoomToArea={handleZoomToArea}
                />
            )}

            <WeatherFloatingPanel />

            {locations.truncated && (
                <div className="absolute left-1/2 top-4 z-[1000] -translate-x-1/2 rounded-full bg-forest-800 px-4 py-2 text-xs font-medium text-white shadow-lg">
                    Too many results to show at once — zoom in to see them all.
                </div>
            )}

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
