import { useState } from "react";
import type { EditMarkerPopupProps, Image } from "../types";
import { useUpdateLocation } from "../hooks/useUpdateLocation";
import { MapMarkerForm } from "./MarkerForm";

export const EditMapMarkerPopup = ({location, onSaveSuccess, onCancel}: EditMarkerPopupProps) => {
    const [name, setName] = useState(location.name);
    const [text, setText] = useState(location.text);
    const [images, setImages] = useState<File[]>([]);
    const [existingImages, setExistingImages] = useState<Image[]>(location.images);
    const {mutate: updateLocation, isPending} = useUpdateLocation();

    const handleSave = () => {  
        if(!name)
            return;

        updateLocation(
            {id: location.publicId!, data: 
                {name, 
                text, 
                images, 
                keepImageIds: existingImages.map(img => img.publicId) }},
            {onSuccess: () => onSaveSuccess()}
        );
    };

    const removeExistingImage = (id: string) => {
        if(!confirm("Remove this image?")){
            return;
        }

        setExistingImages(prev => 
            prev.filter(image => image.publicId !== id)
        );
    };

    return (
            <MapMarkerForm 
                values={{ name: name, text: text}}
                images={images}
                isSubmitting={isPending}
                onValuesChange={(n) => {setName(n.name); setText(n.text);}}
                existingImages={existingImages}
                onExistingImageRemove={removeExistingImage}
                onImageChange={setImages}
                onSubmit={handleSave}
                onCancel={onCancel}
            />
    )
}