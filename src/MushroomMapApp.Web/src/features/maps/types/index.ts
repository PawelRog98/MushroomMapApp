import type { ResourcePermissions } from "../../../types/api";

export type MapMarkerProps = {
    location: Location;
    permissions: LocationPermissions
    index: number;
    onDelete: (id: string | null, lat: number, lng: number) => void;
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
}

export type UpdateLocationRequest = {
    name: string;
    text: string;
    images: File[];
    keepImageIds: string[];
};

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
