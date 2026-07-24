import { useState, useRef, useCallback } from "react";
import { MapContainer, Marker, TileLayer } from "react-leaflet";
import type { Marker as LeafletMarker } from "leaflet";
import { useMapClick } from "../hooks/useMap";
import { MapMarker } from "./MapMarker";
import { NewMarkerPopup } from "./NewMarkerPopup";
import type { Location, LocationPermissions } from "../types";
import { BoundsListener } from "./BoundsListener";
import type { ItemWithMeta } from "../../../types/api";

export type MushroomMapProps = {
    locations: ItemWithMeta<Location, LocationPermissions>[];
    isAddingMode: boolean;
    search?: string | null;
    onAddingComplete: () => void;
    onDeleteLocation: (id: string | null, lat: number, lng: number) => void;
    onLocationChange: (locations: ItemWithMeta<Location, LocationPermissions>[]) => void;
};

const MapEvents = ({ onMapClick }: { onMapClick: (lat: number, lng: number) => void }) => {
    useMapClick(onMapClick);
    return null;
};

export const MushroomMap = ({
    locations,
    isAddingMode,
    search,
    onAddingComplete,
    onDeleteLocation,
    onLocationChange
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
            <BoundsListener search={search} onLocationChange={onLocationChange}/>
            <TileLayer
                attribution="&copy; OpenStreetMap contributors"
                url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            />

            <MapEvents onMapClick={handleMapClick} />

            {locations.map((location, index) => (
                <MapMarker
                    key={location.data.publicId || `${location.data.lat}-${location.data.lng}-${index}`}
                    location={location.data}
                    permissions={location.meta}
                    index={index}
                    onDelete={onDeleteLocation}
                />
            ))}

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
