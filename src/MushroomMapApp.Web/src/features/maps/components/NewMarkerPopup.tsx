import { useState } from "react";
import { Popup } from "react-leaflet";
import { X, Loader2 } from "lucide-react";
import { Button } from "../../../components/ui/Button";
import { Input } from "../../../components/ui/Input";
import type { NewMarkerPopupProps } from "../types";
import { useCreateLocation } from "../hooks/useCreateLocation";

export const NewMarkerPopup = ({ lat, lng, onSaveSuccess, onCancel }: NewMarkerPopupProps) => {
    const [newName, setNewName] = useState("");
    const [newText, setNewText] = useState("");
    const { mutate: createLocation, isPending: isCreating } = useCreateLocation();

    const handleSave = () => {
        if (!newName) return;

        createLocation(
            {
                name: newName,
                text: newText,
                lat,
                lng,
            },
            {
                onSuccess: () => {
                    onSaveSuccess();
                },
            },
        );
    };

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
