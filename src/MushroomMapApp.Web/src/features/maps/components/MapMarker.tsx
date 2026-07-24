import { Marker, Popup } from "react-leaflet";
import { Trash2, Loader2, AlertCircle } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import Lightbox from "yet-another-react-lightbox";
import "yet-another-react-lightbox/styles.css";
import type { MapMarkerProps } from "../types";
import { ReactionRow } from "../../reactions/components/ReactionRow";
import { useImage } from "../../files/hooks/useImage";

export const MapMarker = ({ location, permissions, index, onDelete }: MapMarkerProps) => {
    const [activeImageIndex, setActiveImageIndex] = useState<number | null>(null);
    
    const activeImage = activeImageIndex !== null ? location.images[activeImageIndex] : null;

    const { data: imageBlob, isLoading, isError } = useImage(activeImage?.publicId ?? null);

    const imageUrl = useMemo(() => {
        if (!imageBlob) return null;
        return URL.createObjectURL(imageBlob);
    }, [imageBlob]);

    useEffect(() => {
        return () => {
            if (imageUrl) 
                URL.revokeObjectURL(imageUrl);
        };
    }, [imageUrl]);

    const open = !!imageUrl;

    const handleImageClose = () => {
        setActiveImageIndex(null);
    };
    
    const slides = useMemo(() => {
        return location.images.map((img, index) => {
            const isActive = index === activeImageIndex;
            
            return {
                src: isActive && imageUrl
                ? imageUrl : "/"+img.thumbnailUrl,
                alt: location.name || ""
            };
        });
    }, [location.images, location.name, activeImageIndex, imageUrl])

    return (
        <>
            <Marker
                key={location.publicId || `${location.lat}-${location.lng}-${index}`}
                position={[location.lat, location.lng]}
            >
                <Popup>
                    <div className="p-1 min-w-[150px]">
                        <div className="flex justify-between items-start mb-2">
                            <h3 className="font-bold text-forest-900 pr-4">{location.name}</h3>
                            {permissions.canDelete && (
                            <button
                                onClick={() => onDelete(location.publicId, location.lat, location.lng)}
                                className="text-red-400 hover:text-red-600 transition-colors"
                                title="Delete location"
                            >
                                <Trash2 className="h-4 w-4" />
                            </button>
                            )}
                        </div>
                        <p className="text-sm text-mushroom-600">{location.text}</p>
                        {location.images.length > 0 && (
                            <div className="flex gap-2 overflow-x-auto mt-2">
                                {location.images.map((img, imageIndex) => (
                                    <div key={img.publicId} className="relative">
                                        <img
                                            src={"/" + img.thumbnailUrl}
                                            alt=""
                                            className="w-20 h-20 object-cover rounded cursor-pointer"
                                            onClick={() => {
                                                setActiveImageIndex(imageIndex);
                                            }}
                                        />
                                        {isLoading && activeImageIndex === imageIndex && (
                                            <div className="absolute inset-0 flex items-center justify-center bg-black/30 rounded">
                                                <Loader2 className="h-5 w-5 text-white animate-spin" />
                                            </div>
                                        )}
                                        {isError && activeImageIndex === imageIndex && (
                                            <div className="absolute inset-0 flex items-center justify-center bg-black/30 rounded">
                                                <AlertCircle className="h-5 w-5 text-red-300" />
                                            </div>
                                        )}
                                    </div>
                                ))}
                            </div>
                        )}
                        <ReactionRow locationPublicId={location.publicId!} />
                    </div>
                </Popup>
            </Marker>
            <Lightbox
                slides={slides}
                index={activeImageIndex ?? 0}
                on={{
                    view: ({index}) => {
                        console.log(index);
                        setActiveImageIndex(index);
                    }
                }}
                open={open}
                close={handleImageClose}
            />
        </>
    );
};
