import { useState, useEffect, useRef, useCallback } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "../components/ui/Card";
import { useAuthStore } from "../store/auth-store";
import { useLocationStore } from "../store/locations-store";
import { MapPin, Plus } from "lucide-react";
import { Button } from "../components/ui/Button";
import { MushroomIcon } from "../components/icons/MushroomIcon";
import { MapContainer, Marker, TileLayer } from "react-leaflet";
import { Marker as LeafletMarker } from "leaflet";
import { useMapClick } from "../features/maps/hooks/useMap";
import { MapMarker } from "../features/maps/components/MapMarker";
import { NewMarkerPopup } from "../features/maps/components/NewMarkerPopup";

const MapEvents = ({ onMapClick }: { onMapClick: (lat: number, lng: number) => void }) => {
    useMapClick(onMapClick);
    return null;
};

export const HomePage = () => {
    const userNick = useAuthStore((state) => state.userNick);
    const { locations, addLocation, removeLocation } = useLocationStore();

    const [isAddingMode, setIsAddingMode] = useState(false);
    const [tempCoords, setTempCoords] = useState<{ lat: number; lng: number } | null>(null);

    const newMarkerRef = useRef<LeafletMarker>(null);

    const handleMapClick = useCallback((lat: number, lng: number) => {
        if (isAddingMode) {
            setTempCoords({ lat, lng });
        }
    }, [isAddingMode]);

    const handleSaveLocation = (name: string, text: string) => {
        if (tempCoords && name) {
            addLocation({
                publicId: null,
                name,
                text,
                lat: tempCoords.lat,
                lng: tempCoords.lng,
            });
            resetAdding();
        }
    };

    const resetAdding = () => {
        setIsAddingMode(false);
        setTempCoords(null);
    };

    return (
        <div className="space-y-12">
            <section className="relative h-[75vh] w-full rounded-3xl overflow-hidden shadow-2xl border-3 border-white group">
                <MapContainer
                    center={[52.2297, 21.0122]}
                    zoom={10}
                    closePopupOnClick={false}
                    className={`h-full w-full z-10 ${isAddingMode ? "cursor-crosshair" : ""}`}
                >
                    <TileLayer
                        attribution="&copy; OpenStreetMap contributors"
                        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                    />

                    <MapEvents onMapClick={handleMapClick} />

                    {locations.map((location, index) => (
                        <MapMarker
                            key={location.publicId || `${location.lat}-${location.lng}-${index}`}
                            location={location}
                            index={index}
                            onDelete={removeLocation}
                        />
                    ))}

                    {tempCoords && (
                        <Marker
                            ref={newMarkerRef}
                            position={[tempCoords.lat, tempCoords.lng]}
                            eventHandlers={{
                                add: (e) => {
                                    e.target.openPopup();
                                },
                            }}
                        >
                            <NewMarkerPopup
                                onSave={handleSaveLocation}
                                onCancel={resetAdding}
                            />
                        </Marker>
                    )}
                </MapContainer>
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
