export type MapMarkerProps = {
    location: Location;
    index: number;
    onDelete: (id: string | null, lat: number, lng: number) => void;
};

export type NewMarkerPopupProps = {
    lat: number;
    lng: number;
    onSaveSuccess: () => void;
    onCancel: () => void;
};

export type CreateLocationRequest = {
    name: string;
    text: string;
    lat: number;
    lng: number;
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
};

export type Location = {
    publicId: string | null;
    name: string;
    text: string;
    lat: number;
    lng: number;
};
