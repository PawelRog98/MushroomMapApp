import { useMap } from "react-leaflet";
import type { LocationCluster } from "../types";
import { useEffect } from "react";

export const MapEffects = ({
    flyTo,
    fitBoundsTarget,
    onFlyHandled,
    onFitBoundsHandled
} : {
    flyTo: { lat: number; lng: number } | null;
    fitBoundsTarget: LocationCluster | null;
    onFlyHandled: () => void;
    onFitBoundsHandled: () => void;
}) => {
    const map = useMap();

    useEffect(() => {
        if(!flyTo)
            return;

        map.flyTo(
            [flyTo.lat, flyTo.lng],
            Math.max(map.getZoom(), 13),
            { duration: 0.6 },
        );
        onFlyHandled();
    }, [flyTo, map, onFlyHandled]);

    useEffect(() => {
        if(!fitBoundsTarget)
            return;

        map.fitBounds(
            [
                [fitBoundsTarget.south, fitBoundsTarget.west],
                [fitBoundsTarget.north, fitBoundsTarget.east],
            ],
            { padding: [24, 24]},
        );
        onFitBoundsHandled();
    }, [fitBoundsTarget, map, onFitBoundsHandled]);

    return null;
};