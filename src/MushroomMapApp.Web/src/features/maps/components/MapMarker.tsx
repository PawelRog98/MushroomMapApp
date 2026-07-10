import { Marker, Popup } from "react-leaflet";
import { Trash2 } from "lucide-react";
import type { MapMarkerProps } from "../types";
import { ReactionRow } from "../../reactions/components/ReactionRow";

export const MapMarker = ({ location, index, onDelete }: MapMarkerProps) => {
    return (
        <Marker
            key={location.publicId || `${location.lat}-${location.lng}-${index}`}
            position={[location.lat, location.lng]}
        >
            <Popup>
                <div className="p-1 min-w-[150px]">
                    <div className="flex justify-between items-start mb-2">
                        <h3 className="font-bold text-forest-900 pr-4">{location.name}</h3>
                        <button
                            onClick={() => onDelete(location.publicId, location.lat, location.lng)}
                            className="text-red-400 hover:text-red-600 transition-colors"
                            title="Delete location"
                        >
                            <Trash2 className="h-4 w-4" />
                        </button>
                    </div>
                    <p className="text-sm text-mushroom-600">{location.text}</p>
                    {location.images.length > 0 && (
                        <div className="flex gap-2 overflow-x-auto mt-2">
                            {location.images.map(img => (
                                <img 
                                key={img.publicId}
                                src={"/"+img.thumbnailUrl}
                                alt=""
                                className="w-20 h-20 object-cover rounded cursor-pointer"/>
                            ))}
                        </div>
                    )}
                    <ReactionRow locationPublicId={location.publicId!}/>
                </div>
            </Popup>
        </Marker>
    );
};
