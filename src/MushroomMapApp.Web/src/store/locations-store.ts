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

    setLocations: (locations: Location[]) => void;

    addLocation: (location: Location) => void;

    updateLocation: (location: Location) => void;

    removeLocation: (id: string) => void;
};

export const useLocationStore = create<LocationStore>()(
    devtools(
        persist(
            (set) => ({
                locations: [],
                setLocations: (locations) => set({ locations }),

                addLocation: (data) =>
                    set((state) => ({
                        locations: [...state.locations, { ...data }],
                    })),

                updateLocation: (updatedLoc) =>
                    set((state) => ({
                        locations: state.locations.map((loc) =>
                            loc.publicId && loc.publicId === updatedLoc.publicId ? updatedLoc : loc
                        ),
                    })),

                removeLocation: (id) =>
                    set((state) => ({
                        locations: state.locations.filter((loc) => {
                            if (id && loc.publicId === id) return false;
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
