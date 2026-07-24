import { useState } from "react";
import { useMap, useMapEvents } from "react-leaflet";
import { useDebounce } from "../../../hooks/useDebounce";
import { useLocations } from "../hooks/useLocations";
import type { Location, LocationPermissions } from "../types";
import type { ItemWithMeta } from "../../../types/api";

export type Bounds = {
    south: number; 
    west: number; 
    north: number; 
    east: number;
}

type MapBoundsHandlerProps = {
    search?: string | null;
    onLocationChange: (locations: ItemWithMeta<Location, LocationPermissions>[]) => void;
};

export const BoundsListener = ({search, onLocationChange} : MapBoundsHandlerProps) => {
    const map = useMap();

    const getBounds = () => {
        const b = map.getBounds();

        return {
            south: b.getSouth(),
            west: b.getWest(),
            north: b.getNorth(),
            east: b.getEast(),
        };
    };

    const [bounds, setBounds] = useState(getBounds);

    useMapEvents({
        moveend: () => setBounds(getBounds()),
    });

    const debouncedBounds = useDebounce(bounds, 300);
    console.log("data: -> "+debouncedBounds.north);

    useLocations({search: search ?? null, 
        ...debouncedBounds,
    }, onLocationChange);

    return null;
}