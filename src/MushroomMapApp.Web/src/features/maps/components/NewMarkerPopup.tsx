import { useEffect, useMemo, useState } from "react";
import { Popup } from "react-leaflet";
import { X, Loader2, ImagePlus } from "lucide-react";
import { Button } from "../../../components/ui/Button";
import { Input } from "../../../components/ui/Input";
import type { NewMarkerPopupProps } from "../types";
import { useCreateLocation } from "../hooks/useCreateLocation";

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

    const removeImage = (index: number) => {
        setImages(x => x.filter((_,i) => i !== index));
    };

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
            <div className="p-2 space-y-3 min-w-[200px]">
                <div className="flex justify-between items-center">
                    <h3 className="font-bold text-forest-900">New Spot</h3>
                    <button
                        onClick={onCancel}
                        className="text-mushroom-400 hover:text-mushroom-600"
                        disabled={isCreating}
                    >
                        <X className="h-4 w-4" />
                    </button>
                </div>
                <div className="space-y-2">
                    <Input
                        placeholder="Spot name..."
                        value={newName}
                        onChange={(e) => setNewName(e.target.value)}
                        autoFocus
                        disabled={isCreating}
                    />
                    <Input
                        placeholder="Description..."
                        value={newText}
                        onChange={(e) => setNewText(e.target.value)}
                        disabled={isCreating}
                    />
                    <label className="flex h-10 w-full cursor-pointer items-center justify-center gap-2 rounded-md border border-dashed border-mushroom-300 bg-mushroom-50 text-sm text-mushroom-500 transition-colors hover:border-forest-400 hover:bg-forest-50 hover:text-forest-600 disabled:opacity-50 disabled:cursor-not-allowed">
                        <ImagePlus className="h-4 w-4" />
                        <span>{images.length > 0 ? `${images.length} image${images.length > 1 ? "s" : ""} selected` : "Upload images"}</span>
                        <input
                            type="file"
                            accept="image/*"
                            multiple
                            disabled={isCreating}
                            onChange={(e) => {
                                const files = Array.from(e.target.files ?? []);
                                setImages(files);
                            }}
                            className="hidden"
                        />
                    </label>
                    {images.length > 0 &&(
                        <div className="flex gap-2 flex-wrap">
                            {previews.map(({file, url}, index) => (
                                <div key={file.name} className="relative">
                                    <img 
                                        src={url}
                                        alt={file.name}
                                        className="w-20 h-20 rounded object-cover"
                                    />

                                    <button type="button"
                                        onClick={(e) => {
                                            e.stopPropagation();
                                            removeImage(index);
                                        }}
                                        className="absolute top-1 right-1 rounded-full bg-white p-1 shadow">
                                        <X size={12}></X>
                                    </button>
                                </div>
                            ))}
                        </div>
                    )}
                </div>
                <Button
                    size="sm"
                    className="w-full bg-forest-600 hover:bg-forest-700"
                    onClick={handleSave}
                    disabled={!newName || isCreating}
                >
                    {isCreating ? (
                        <>
                            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                            Saving...
                        </>
                    ) : (
                        "Save Location"
                    )}
                </Button>
            </div>
        </Popup>
    );
};
