import type { Location } from "../../../store/locations-store";

export type MapMarkerProps = {
    location: Location;
    index: number;
    onDelete: (id: string | null, lat: number, lng: number) => void;
};

export type NewMarkerPopupProps = {
    onSave: (name: string, text: string) => void;
    onCancel: () => void;
};
