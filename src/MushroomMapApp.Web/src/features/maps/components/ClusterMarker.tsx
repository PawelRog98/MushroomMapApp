import { useMemo } from "react";
import L from "leaflet";
import { Marker } from "react-leaflet";
import type { LocationCluster } from "../types";

type ClusterMarkerProps = {
    cluster: LocationCluster;
    onClick: (cluster: LocationCluster) => void;
};

export const ClusterMarker = ({ cluster, onClick }: ClusterMarkerProps) => {
    const icon = useMemo(() => {
        const size = Math.round(Math.min(52, 28 + Math.log2(cluster.count + 1) * 5));
        const digits = String(cluster.count).length;
        const fontSize = Math.max(9, Math.round(Math.min(size * 0.42, ((size - 10) / digits) * 1.15)));

        return L.divIcon({
            className: "cluster-marker",
            html: `<div class="cluster-marker-inner" style="width:${size}px;height:${size}px;font-size:${fontSize}px">${cluster.count}</div>`,
            iconSize: [size, size],
            iconAnchor: [size / 2, size / 2],
        });
    }, [cluster.count]);

    return (
        <Marker
            position={[cluster.lat, cluster.lng]}
            icon={icon}
            alt={`${cluster.count} locations`}
            title={`${cluster.count} locations`}
            bubblingMouseEvents={false}
            eventHandlers={{ click: () => onClick(cluster) }}
        />
    );
};
