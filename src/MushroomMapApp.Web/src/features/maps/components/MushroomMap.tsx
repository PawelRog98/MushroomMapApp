import { useState, useRef, useCallback, useMemo } from "react";
import { MapContainer, Marker, TileLayer } from "react-leaflet";
import type { Marker as LeafletMarker } from "leaflet";
import { useMapClick } from "../hooks/useMap";
import { MapMarker } from "./MapMarker";
import { ClusterMarker } from "./ClusterMarker";
import { NewMarkerPopup } from "./NewMarkerPopup";
import type { HeatmapPoint, Location, LocationCluster, LocationPermissions, MapLocationResponse } from "../types";
import { BoundsListener } from "./BoundsListener";
import type { ItemWithMeta } from "../../../types/api";
import { HeatmapLayer } from "./HeatmapLayer";
import { MapEffects } from "./MapEffects";

export type MushroomMapProps = {
    locations: MapLocationResponse;
    isAddingMode: boolean;
    search?: string | null;
    heatMapEnabled: boolean;
    focusedLocation: ItemWithMeta<Location, LocationPermissions> | null;
    focusToken?: number;
    flyTo: { lat: number; lng: number } | null;
    fitBoundsTarget: LocationCluster | null;
    onAddingComplete: () => void;
    onDeleteLocation: (id: string | null, lat: number, lng: number) => void;
    onLocationChange: (locations: MapLocationResponse) => void;
    onClusterClick: (cluster: LocationCluster) => void;
    onFlyHandled: () => void;
    onFitBoundsHandled: () => void;
};

const MapEvents = ({ onMapClick }: { onMapClick: (lat: number, lng: number) => void }) => {
    useMapClick(onMapClick);
    return null;
};

export const MushroomMap = ({
    locations,
    isAddingMode,
    search,
    heatMapEnabled,
    focusedLocation,
    focusToken = 0,
    flyTo,
    fitBoundsTarget,
    onAddingComplete,
    onDeleteLocation,
    onLocationChange,
    onClusterClick,
    onFlyHandled,
    onFitBoundsHandled
}: MushroomMapProps) => {
    const [tempCoords, setTempCoords] = useState<{ lat: number; lng: number } | null>(null);
    const newMarkerRef = useRef<LeafletMarker>(null);

    const handleMapClick = useCallback(
        (lat: number, lng: number) => {
            if (isAddingMode) {
                setTempCoords({ lat, lng });
            }
        },
        [isAddingMode],
    );

    const heatmapPoints = useMemo<HeatmapPoint[]>(() => {
        if (locations.mode === "clusters") {
            return locations.clusters.map((c) => ({
                lat: c.lat,
                lng: c.lng,
                weight: c.count,
            }));
        }

        return locations.markers
            .filter((m) => m.meta.canView)
            .map((m) => ({
                lat: m.data.lat,
                lng: m.data.lng,
                weight: 1,
            }));
    }, [locations]);

    const resetAdding = useCallback(() => {
        setTempCoords(null);
        onAddingComplete();
    }, [onAddingComplete]);

    return (
        <MapContainer
            center={[52.2297, 21.0122]}
            zoom={10}
            closePopupOnClick={false}
            className={`h-full w-full z-10 ${isAddingMode ? "cursor-crosshair" : ""}`}
        >
            <BoundsListener search={search} onLocationChange={onLocationChange} />
            <TileLayer
                attribution="&copy; OpenStreetMap contributors"
                url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            />

            <MapEffects
                flyTo={flyTo}
                fitBoundsTarget={fitBoundsTarget}
                onFlyHandled={onFlyHandled}
                onFitBoundsHandled={onFitBoundsHandled}
            />

            <MapEvents onMapClick={handleMapClick} />

            {heatMapEnabled ? (
                <HeatmapLayer points={heatmapPoints} />
            ) : locations.mode === "clusters" ? (
                locations.clusters.map((cluster) => (
                    <ClusterMarker
                        key={`${cluster.zoom}:${cluster.cellX}:${cluster.cellY}`}
                        cluster={cluster}
                        onClick={onClusterClick}
                    />
                ))
            ) : (
                locations.markers.filter((m) => m.data.publicId !== focusedLocation?.data.publicId)
                    .map((location) => (
                        <MapMarker
                            key={location.data.publicId ?? `${location.data.lat}-${location.data.lng}`}
                            location={location.data}
                            permissions={location.meta}
                            onDelete={onDeleteLocation}
                        />
                    ))
            )}

            {focusedLocation && (
                <MapMarker
                    key={`focus-${focusedLocation.data.publicId ?? `${focusedLocation.data.lat}-${focusedLocation.data.lng}`}-${focusToken}`}
                    location={focusedLocation.data}
                    permissions={focusedLocation.meta}
                    autoOpen
                    onDelete={onDeleteLocation}
                />
            )}

            {tempCoords && (
                <Marker
                    ref={newMarkerRef}
                    position={[tempCoords.lat, tempCoords.lng]}
                    eventHandlers={{
                        add: (e) => {
                            e.target.openPopup();
                        },
                    }}
                >
                    <NewMarkerPopup
                        lat={tempCoords.lat}
                        lng={tempCoords.lng}
                        onSaveSuccess={resetAdding}
                        onCancel={resetAdding}
                    />
                </Marker>
            )}
        </MapContainer>
    );
};
