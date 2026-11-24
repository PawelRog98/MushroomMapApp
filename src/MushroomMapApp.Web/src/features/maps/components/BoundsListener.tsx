import { useState, useEffect } from "react";
import { useMap, useMapEvents } from "react-leaflet";
import { useDebounce } from "../../../hooks/useDebounce";
import { useLocations } from "../hooks/useLocations";
import type { MapLocationResponse } from "../types";

export type Bounds = {
    south: number;
    west: number;
    north: number;
    east: number;
    zoom: number;
}

type MapBoundsHandlerProps = {
    search?: string | null;
    onLocationChange: (locations: MapLocationResponse) => void;
};

const PRECISION = 5;
const FACTOR = 10 ** PRECISION;

const roundDown = (n: number) => Math.floor(n * FACTOR) / FACTOR;
const roundUp = (n: number) => Math.ceil(n * FACTOR) / FACTOR;

const normalizeLng = (lng: number) => ((lng + 180) % 360 + 360) % 360 - 180;

export const BoundsListener = ({ search, onLocationChange }: MapBoundsHandlerProps) => {
    const map = useMap();

    const getBounds = () => {
        const b = map.getBounds();

        let west = b.getWest();
        let east = b.getEast();

        if (east - west >= 360) {
            west = -180;
            east = 180;
        } else {
            west = normalizeLng(west);
            east = normalizeLng(east);
        }

        return {
            south: b.getSouth(),
            west,
            north: b.getNorth(),
            east,
            zoom: map.getZoom(),
        };
    };

    const [bounds, setBounds] = useState(getBounds);

    useMapEvents({
        moveend: () => setBounds(getBounds()),
        zoomend: () => setBounds(getBounds()),
    });

    const debouncedBounds = useDebounce(bounds, 300);

    const { data: locations } = useLocations({
        search: search ?? null,
        south: roundDown(debouncedBounds.south),
        west: roundDown(debouncedBounds.west),
        north: roundUp(debouncedBounds.north),
        east: roundUp(debouncedBounds.east),
        zoom: Math.round(debouncedBounds.zoom),
    });

    useEffect(() => {
        if (locations) {
            onLocationChange(locations);
        }
    }, [locations, onLocationChange]);

    return null;
}
