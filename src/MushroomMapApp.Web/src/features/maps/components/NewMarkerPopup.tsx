import { useEffect, useMemo, useState } from "react";
import { Popup } from "react-leaflet";
import type { NewMarkerPopupProps } from "../types";
import { useCreateLocation } from "../hooks/useCreateLocation";
import { MapMarkerForm } from "./MarkerForm";

export const NewMarkerPopup = ({ lat, lng, onSaveSuccess, onCancel }: NewMarkerPopupProps) => {
    const [newName, setNewName] = useState("");
    const [newText, setNewText] = useState("");
    const [images, setImages] = useState<File[]>([]);
    const { mutate: createLocation, isPending: isCreating } = useCreateLocation();

    const previews = useMemo(() =>{
        return images.map(file => ({
            file,
            url: URL.createObjectURL(file)
        }));
    }, [images]);

    const handleSave = () => {
        if (!newName) return;

        createLocation(
            {
                name: newName,
                text: newText,
                lat,
                lng,
                images
            },
            {
                onSuccess: () => {
                    onSaveSuccess();
                },
            },
        );
    };

    useEffect(() => {
        return () => {
            previews.forEach(p => URL.revokeObjectURL(p.url));
        };
    });

    return (
        <Popup closeOnClick={false}>
           <MapMarkerForm 
                values={{ name: newName, text: newText }}
                images={images}
                isSubmitting={isCreating}
                onValuesChange={(n) => {setNewName(n.name); setNewText(n.text);}}
                onImageChange={setImages}
                onSubmit={handleSave}
                onCancel={onCancel} 
                />
        </Popup>
    );
};
