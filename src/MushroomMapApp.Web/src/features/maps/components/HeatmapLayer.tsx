import { useMap } from "react-leaflet";
import type { HeatmapLayerProps } from "../types";
import { useEffect } from "react";
import L from "leaflet";
import "leaflet.heat";   

const GRADIENT: Record<number, string> = {
    0.2: "#3a6f4d",
    0.4: "#6ca67f",
    0.6: "#c5dfcd",
    0.75: "#e2ad0d",
    0.9: "#f48b40",
    1.0: "#dc2626",
};

export const HeatmapLayer = ({points, radius = 25, blur =12, maxZoom = 16} : HeatmapLayerProps) => {
    const map = useMap();

    useEffect(() => {
        if(points.length === 0)
            return;

        const maxWeight = Math.max(1, ...points.map(({ weight }) => weight ?? 1));

        const latLngs: [number, number, number][] = points.map(({ lat, lng, weight }) => [
            lat,
            lng,
            (weight ?? 1) / maxWeight,
        ]);

        const layer = L.heatLayer(latLngs, {
            radius,
            blur,
            maxZoom,
            minOpacity: 0.25,
            gradient: GRADIENT,
        });

        layer.addTo(map);

        return () => {
            map.removeLayer(layer);
        }
    }, [map, points, radius, blur, maxZoom]);

    return null;
}