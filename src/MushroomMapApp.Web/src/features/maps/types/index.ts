import type { ItemWithMeta, PagingMeta, ResourcePermissions } from "../../../types/api";

export type MapMarkerProps = {
    location: Location;
    permissions: LocationPermissions
    onDelete: (id: string | null, lat: number, lng: number) => void;
    autoOpen?: boolean;
};

export type NewMarkerPopupProps = {
    lat: number;
    lng: number;
    onSaveSuccess: () => void;
    onCancel: () => void;
};

export type EditMarkerPopupProps = {
    location: Location;
    onSaveSuccess: () => void;
    onCancel: () => void;
}


export type HeatmapPoint = {
    lat: number;
    lng: number;
    weight?: number;
};

export type HeatmapLayerProps = {
    points: HeatmapPoint[];
    radius?: number;
    blur?: number;
    maxZoom?: number;
}

export type CreateLocationRequest = {
    name: string;
    text: string;
    lat: number;
    lng: number;
    images: File[]
};

export type GetLocationRequest = {
    search: string | null;
    south: number;
    west: number;
    north: number;
    east: number;
    zoom: number;
}

export type LocationCluster = {
    lat: number;
    lng: number;
    count: number;
    south: number;
    west: number;
    north: number;
    east: number;
    zoom: number;
    cellX: number;
    cellY: number;
}

export type MapLocationResponse = {
    mode: "markers" | "clusters";
    totalCount: number;
    truncated: boolean;
    markers: ItemWithMeta<Location, LocationPermissions>[];
    clusters: LocationCluster[];
};

export type GetClusterLocationRequest = {
    cellX: number;
    cellY: number;
    zoom: number;
    search: string | null;
    page: number;
    pageSize: number;
}

export type UpdateLocationRequest = {
    name: string;
    text: string;
    images: File[];
    keepImageIds: string[];
};

export type ClusterLocationResponse = {
    items: ItemWithMeta<Location, LocationPermissions>[];
    paging: PagingMeta;
}

export type Image = {
    publicId: string;
    thumbnailUrl: string;
    contentType: string;
}

export type Location = {
    publicId: string | null;
    authorName: string;
    authorPublicId: string;
    name: string;
    text: string;
    lat: number;
    lng: number;
    images: Image[];
};

export type MarkerFormValues = {
    name: string;
    text: string;
}

export type ExistingImageData = {
    id: string;
    thumbnailUrl: string;
}

export type LocationPermissions = ResourcePermissions
