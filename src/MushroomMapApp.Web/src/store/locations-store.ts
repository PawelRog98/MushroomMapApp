import { create } from "zustand";
import { createJSONStorage, devtools, persist } from "zustand/middleware";

export type Location = {
    publicId: string | null;
    name: string;
    text: string;
    lat: number;
    lng: number;
};

type LocationStore = {
    locations: Location[];

    addLocation: (location: Location) => void;

    removeLocation: (id: string | null, lat?: number, lng?: number) => void;
};

export const useLocationStore = create<LocationStore>()(
    devtools(
        persist(
            (set) => ({
                locations: [],
                addLocation: (data) =>
                    set((state) => ({
                        locations: [...state.locations, { ...data }],
                    })),

                removeLocation: (id, lat, lng) =>
                    set((state) => ({
                        locations: state.locations.filter((loc) => {
                            if (id && loc.publicId === id) return false;
                            if (!id && loc.lat === lat && loc.lng === lng) return false;
                            return true;
                        }),
                    })),
            }),
            {
                name: "mushroom-map-location-storage",
                storage: createJSONStorage(() => localStorage),
            },
        ),
    ),
);
