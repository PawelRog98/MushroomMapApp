import { useMapEvents } from "react-leaflet";
import { useRef, useEffect } from "react";

export const useMapClick = (onClick: (lat: number, lng: number) => void) => {
    const callbackRef = useRef(onClick);

    useEffect(() => {
        callbackRef.current = onClick;
    }, [onClick]);

    useMapEvents({
        click: (e) => {
            callbackRef.current(e.latlng.lat, e.latlng.lng);
        },
    });
};
