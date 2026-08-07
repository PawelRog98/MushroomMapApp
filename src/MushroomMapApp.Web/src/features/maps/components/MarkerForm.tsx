import { useEffect, useMemo } from "react";
import type { Image, MarkerFormValues } from "../types"
import { ImagePlus, Loader2, X } from "lucide-react";
import { Input } from "../../../components/ui/Input";
import { Button } from "../../../components/ui/Button";

export type MapMaprkerFormProps = {
    values: MarkerFormValues;
    images: File[];
    isSubmitting: boolean;
    lat?: number;
    lng?: number;
    existingImages?: Image[];
    onImageChange: (files: File[]) => void; 
    onExistingImageRemove?: (id: string) => void;
    onValuesChange: (values: MarkerFormValues) => void;
    onSubmit: ()=> void;
    onCancel: ()=> void;
}

export const MapMarkerForm =({
    values,
    images,
    isSubmitting,
    existingImages,
    onExistingImageRemove,
    onImageChange,
    onValuesChange,
    onSubmit,
    onCancel,
}: MapMaprkerFormProps) => {

    const previews = useMemo(() =>{
        return images.map(file => ({
            file,
            url: URL.createObjectURL(file)
        }));
    }, [images]);

    useEffect(() => {
        return () => {
            previews.forEach(p => URL.revokeObjectURL(p.url));
        };
    },[previews]);

    const removeImage = (index: number) => {
        onImageChange(images.filter((_,i) => i !== index));
    };

    return(
        <div className="p-2 space-y-3 min-w-[200px]">
                <div className="flex justify-between items-center">
                    <h3 className="font-bold text-forest-900">New Spot</h3>
                    <button
                        onClick={onCancel}
                        className="text-mushroom-400 hover:text-mushroom-600"
                        disabled={isSubmitting}
                    >
                        <X className="h-4 w-4" />
                    </button>
                </div>
                <div className="space-y-2">
                    <Input
                        placeholder="Spot name..."
                        value={values.name}
                        onChange={(e) => onValuesChange({...values, name: e.target.value})}
                        autoFocus
                        disabled={isSubmitting}
                    />
                    <Input
                        placeholder="Description..."
                        value={values.text}
                        onChange={(e) => onValuesChange({...values, text: e.target.value})}
                        disabled={isSubmitting}
                    />
                    {existingImages && existingImages.length > 0 &&(
                        existingImages.map(image => (
                            <div key={image.publicId} className="relative">
                                <img
                                    src={image.thumbnailUrl}
                                    alt=""
                                    className="w-20 h-20 rounded object-cover"
                                />

                                <button
                                    type="button"
                                    onClick={() => onExistingImageRemove?.(image.publicId)}
                                    className="absolute top-1 right-1 rounded-full bg-white p-1 shadow"
                                >
                                <X size={12} />
                                </button>
                            </div>
                        ))
                    )}
                    <label className="flex h-10 w-full cursor-pointer items-center justify-center gap-2 rounded-md border border-dashed border-mushroom-300 bg-mushroom-50 text-sm text-mushroom-500 transition-colors hover:border-forest-400 hover:bg-forest-50 hover:text-forest-600 disabled:opacity-50 disabled:cursor-not-allowed">
                        <ImagePlus className="h-4 w-4" />
                        <span>{images.length > 0 ? `${images.length} image${images.length > 1 ? "s" : ""} selected` : "Upload images"}</span>
                        <input
                            type="file"
                            accept="image/*"
                            multiple
                            disabled={isSubmitting}
                            onChange={(e) => {
                                const files = Array.from(e.target.files ?? []);
                                onImageChange(files);
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
                    onClick={onSubmit}
                    disabled={!values.name || isSubmitting}
                >
                    {isSubmitting ? (
                        <>
                            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                            Saving...
                        </>
                    ) : (
                        "Save Location"
                    )}
                </Button>
            </div>

    );
}