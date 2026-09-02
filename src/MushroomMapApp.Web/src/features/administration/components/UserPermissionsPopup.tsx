import { useEffect, useState } from "react";
import { usePermissions } from "../hooks/usePermissions";
import type { UserPermissionsPopupProps } from "../types";
import { X, Loader2 } from "lucide-react";
import { Button } from "../../../components/ui/Button";
import { Checkbox } from "../../../components/ui/Checkbox";

export const UserPermissionsPopup = ({
    isOpen,
    onClose,
    user,
    onConfirm,
    isPending,
}: UserPermissionsPopupProps) => {
    const { data: permissions } = usePermissions();
    const [checkedPermissions, setCheckedPermissions] = useState<string[]>(
        () => user?.activePermissions ?? []
    );

    useEffect(() => {
        if (!isOpen) return;
        const handleEsc = (e: KeyboardEvent) => {
            if (e.key === "Escape") onClose();
        };
        window.addEventListener("keydown", handleEsc);
        return () => window.removeEventListener("keydown", handleEsc);
    }, [isOpen, onClose]);

    useEffect(() => {
        if (isOpen) {
            document.body.style.overflow = "hidden";
        }
        return () => {
            document.body.style.overflow = "";
        };
    }, [isOpen]);

    const togglePermission = (code: string) => {
        setCheckedPermissions((prev) =>
            prev.includes(code)
                ? prev.filter((p) => p !== code)
                : [...prev, code]
        );
    };

    const handleConfirm = () => {
        onConfirm(checkedPermissions);
    };

    if (!isOpen || !user) 
        return null;

    return (
        <div
            className="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
            onClick={onClose}
        >
            <div
                className="bg-white rounded-lg shadow-xl w-full max-w-md mx-4"
                onClick={(e) => e.stopPropagation()}
            >
                <div className="flex justify-between items-start p-6 pb-0">
                    <div>
                        <h2 className="text-lg font-semibold text-mushroom-900">
                            Manage Permissions
                        </h2>
                        <p className="text-sm text-mushroom-500 mt-1">
                            Assign or remove permissions for {user.publicNick}
                        </p>
                    </div>
                    <button
                        onClick={onClose}
                        disabled={isPending}
                        className="text-mushroom-400 hover:text-mushroom-600"
                    >
                        <X className="h-5 w-5" />
                    </button>
                </div>

                <div className="px-6 py-4 max-h-[60vh] overflow-y-auto space-y-1">
                    {permissions?.permissions.map((permission) => (
                        <div key={permission} className="py-1">
                            <Checkbox
                                label={permission}
                                checked={checkedPermissions.includes(permission)}
                                onChange={() => togglePermission(permission)}
                                disabled={isPending}
                            />
                        </div>
                    ))}
                </div>

                <div className="flex justify-end gap-3 p-6 pt-0">
                    <Button
                        variant="ghost"
                        onClick={onClose}
                        disabled={isPending}
                    >
                        Cancel
                    </Button>
                    <Button onClick={handleConfirm} disabled={isPending}>
                        {isPending ? (
                            <>
                                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                Saving...
                            </>
                        ) : (
                            "Save"
                        )}
                    </Button>
                </div>
            </div>
        </div>
    );
};
