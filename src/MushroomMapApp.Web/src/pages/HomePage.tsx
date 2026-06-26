import { useState, useCallback, useEffect } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "../components/ui/Card";
import { useLocationStore } from "../store/locations-store";
import { MapPin, Plus } from "lucide-react";
import { MushroomIcon } from "../components/icons/MushroomIcon";
import { MushroomMap } from "../features/maps/components/MushroomMap";
import { useLocations } from "../features/maps/hooks/useLocations";
import { useDeleteLocation } from "../features/maps/hooks/useDeleteLocation";

export const HomePage = () => {
    const { locations, setLocations } = useLocationStore();

    const { data: apiLocations } = useLocations({ search: null });
    const { mutate: deleteLocation } = useDeleteLocation();

    useEffect(() => {
        if (apiLocations) {
            setLocations(apiLocations);
        }
    }, [apiLocations, setLocations]);

    const handleDeleteLocation = useCallback((id: string | null) => {
        if (id) {
            deleteLocation(id);
        }
    }, [deleteLocation]);

    const [isAddingMode, setIsAddingMode] = useState(false);

    return (
        <div className="space-y-12">
            <section className="relative h-[75vh] w-full rounded-3xl overflow-hidden shadow-2xl border-3 border-white group">
                <MushroomMap
                    locations={locations}
                    isAddingMode={isAddingMode}
                    onAddingComplete={() => setIsAddingMode(false)}
                    onDeleteLocation={handleDeleteLocation}
                />
            </section>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <Card className="hover:border-forest-300 transition-colors cursor-pointer group">
                    <CardHeader>
                        <MapPin className="h-8 w-8 text-forest-600 group-hover:scale-110 transition-transform" />
                        <CardTitle className="mt-4">Explore Map</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-mushroom-500 text-sm">
                            To bedzie cos innego pewnie z wyszukiwaniem
                        </p>
                    </CardContent>
                </Card>

                <Card
                    className={`transition-colors cursor-pointer group ${
                        isAddingMode ? "border-forest-500 bg-forest-50" : "hover:border-forest-300"
                    }`}
                    onClick={() => setIsAddingMode(!isAddingMode)}
                >
                    <CardHeader>
                        <Plus
                            className={`h-8 w-8 text-forest-600 transition-transform ${
                                isAddingMode ? "rotate-45" : "group-hover:scale-110"
                            }`}
                        />
                        <CardTitle className="mt-4">
                            {isAddingMode ? "Cancel Adding" : "Add Spot"}
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-mushroom-500 text-sm">
                            {isAddingMode
                                ? "Click on the map to place your marker."
                                : "Found a new patch? Mark it on the map for others (or just you)."}
                        </p>
                    </CardContent>
                </Card>
            </div>

            <Card className="bg-forest-900 text-white overflow-hidden">
                <div className="md:flex">
                    <div className="p-8 md:w-2/3 space-y-4">
                        <h2 className="text-2xl font-bold">Mushroom app hunting?</h2>
                        <p className="text-forest-100">
                            Cos tam jest fajne itd afsdfgdsgfdfgdfgdgdgfgfd
                        </p>
                        {/* <Button
                            variant="outline"
                            className="border-white text-white hover:bg-white hover:text-forest-900"
                        >
                            Open Map Dashboard
                        </Button> */}
                    </div>
                    <div className="hidden md:block md:w-1/3 bg-forest-800 flex items-center justify-center">
                        <MushroomIcon className="h-32 w-32 text-forest-700/50" />
                    </div>
                </div>
            </Card>
        </div>
    );
};
