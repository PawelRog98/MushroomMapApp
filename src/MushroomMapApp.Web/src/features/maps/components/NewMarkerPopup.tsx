import { useState } from "react";
import { Popup } from "react-leaflet";
import { X } from "lucide-react";
import { Button } from "../../../components/ui/Button";
import { Input } from "../../../components/ui/Input";
import type { NewMarkerPopupProps } from "../types";

export const NewMarkerPopup = ({ onSave, onCancel }: NewMarkerPopupProps) => {
    const [newName, setNewName] = useState("");
    const [newText, setNewText] = useState("");

    return (
        <Popup closeOnClick={false}>
            <div className="p-2 space-y-3 min-w-[200px]">
                <div className="flex justify-between items-center">
                    <h3 className="font-bold text-forest-900">New Spot</h3>
                    <button
                        onClick={onCancel}
                        className="text-mushroom-400 hover:text-mushroom-600"
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
                    />
                    <Input
                        placeholder="Description..."
                        value={newText}
                        onChange={(e) => setNewText(e.target.value)}
                    />
                </div>
                <Button
                    size="sm"
                    className="w-full bg-forest-600 hover:bg-forest-700"
                    onClick={() => onSave(newName, newText)}
                    disabled={!newName}
                >
                    Save Location
                </Button>
            </div>
        </Popup>
    );
};
