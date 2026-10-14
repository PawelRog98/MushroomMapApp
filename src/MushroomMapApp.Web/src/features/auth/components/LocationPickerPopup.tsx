import { useEffect, useState } from "react";
import type { Coordinates, LocationPickerPopupProps } from "../types";
import { useMapClick } from "../../maps/hooks/useMap";
import { MapContainer, Marker, TileLayer, useMap } from "react-leaflet";
import { useGeolocation } from "../../maps/hooks/useGeolocation";
import { Button } from "../../../components/ui/Button";
import { Loader2, MapPin, X } from "lucide-react";

const DEFAULT_CENTER: Coordinates = { lat: 52.2297, lng: 21.0122 };

const ClickCapture = ({onPick} : {onPick: (lat: number, lng: number) => void}) => {
    useMapClick(onPick);
    return null;
}

const MoveToTarget = ({target} : {target: Coordinates | null}) => {
    const map = useMap();

    useEffect(() => {
        if(target){
            map.flyTo([target.lat, target.lng], Math.max(map.getZoom(), 13));
        }
    }, [target, map]);

    return null;
};

export const LocationPickerPopup = ({
    isOpen,
    onClose,
    location,
    onSave,
    isPending
} : LocationPickerPopupProps) => {

    const [picked, setPicked] = useState<Coordinates | null>(() => 
        location ? {lat: location.lat, lng: location.lng} : null,
    );

    const [flyTarget, setFlyTarget] = useState<Coordinates | null>(null);
    const [lastHandledGeoAt, setLastHandledGeoAt] = useState<number | null>(null);
    const geo = useGeolocation();

    const center: Coordinates = location ?? DEFAULT_CENTER;

    if (geo.locatedAt !== null && geo.latitude !== null && geo.longitude !== null && geo.locatedAt !== lastHandledGeoAt) {
        setLastHandledGeoAt(geo.locatedAt);
        const target = {lat: geo.latitude, lng: geo.longitude};

        setPicked(target);
        setFlyTarget(target);
    }

    useEffect(() => {
        if(!isOpen)
            return;

        const handleEsc = (e: KeyboardEvent) => {
            if(e.key === "Escape")
                onClose();
        };
        window.addEventListener("keydown", handleEsc);
        return () => window.removeEventListener("keydown", handleEsc);
    }, [isOpen, onClose]);

    useEffect(() => {
        if(isOpen){
            document.body.style.overflow = "hidden";
        }
        return () => {
            document.body.style.overflow = "";
        };
    }, [isOpen]);

    const handleUseMyPosition = () => {
        geo.clearError();
        geo.locate();
    }

    return(
        <div
            className="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
            onClick={onClose}
        >
            <div
                className="bg-white rounded-lg shadow-xl w-full max-w-2xl mx-4 p-6 space-y-5"
                onClick={(e) => e.stopPropagation()}
            >
                <div className="flex justify-between items-start">
                    <div>
                        <h2 className="text-lg font-semibold text-mushroom-900">
                            My localization
                        </h2>
                        <p className="text-sm text-mushroom-500 mt-1">
                            Click on the map to select your location
                        </p>
                    </div>
                    <button
                        onClick={onClose}
                        className="text-mushroom-400 hover:text-mushroom-600"
                        disabled={isPending}
                    >
                        <X className="h-5 w-5" />
                    </button>
                </div>

                <div className="h-80 w-full rounded-md overflow-hidden border border-mushroom-200">
                    <MapContainer
                        center={[center.lat, center.lng]}
                        zoom={10}
                        className="h-full w-full"
                    >
                        <TileLayer
                            attribution="&copy; OpenStreetMap contributors"
                            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                        />
                        <ClickCapture onPick={(lat, lng) => setPicked({ lat, lng })} />
                        <MoveToTarget target={flyTarget} />
                        {picked && <Marker position={[picked.lat, picked.lng]} />}
                    </MapContainer>
                </div>

                <div className="flex items-center justify-between gap-3">
                    <div className="space-y-1">
                        <Button
                            variant="outline"
                            size="sm"
                            onClick={handleUseMyPosition}
                            disabled={isPending || geo.isLoading}
                        >
                            {geo.isLoading ? (
                                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                            ) : (
                                <MapPin className="mr-2 h-4 w-4" />
                            )}
                            Use my current location
                        </Button>
                        {geo.error && (
                            <p className="text-xs text-red-500">{geo.error}</p>
                        )}
                    </div>
                    <p className="text-xs text-mushroom-500">
                        {picked
                            ? `${picked.lat.toFixed(5)}, ${picked.lng.toFixed(5)}`
                            : "No location selected"}
                    </p>
                </div>

                <div className="flex justify-end gap-3 pt-2">
                    <Button variant="ghost" onClick={onClose} disabled={isPending}>
                        Cancel
                    </Button>
                    <Button
                        onClick={() => picked && onSave(picked.lat, picked.lng)}
                        disabled={!picked || isPending}
                    >
                        {isPending ? (
                            <>
                                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                Saving...
                            </>
                        ) : (
                            "Save"
                        )}
                    </Button>
                </div>
            </div>
        </div>
    )
}