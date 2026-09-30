import { useEffect } from "react";
import type { UnsuspendUserPopupProps } from "../types";
import { X, Loader2, ShieldCheck } from "lucide-react";
import { Button } from "../../../components/ui/Button";

export const UnsuspendPopup = ({
    isOpen,
    onClose,
    user,
    onConfirm,
    isPending,
}: UnsuspendUserPopupProps) => {
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

    if (!isOpen || !user) return null;

    const suspensionEndDate = user.suspensionEndDate
        ? new Date(user.suspensionEndDate).toLocaleString()
        : null;

    return (
        <div
            className="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
            onClick={onClose}
        >
            <div
                className="bg-white rounded-lg shadow-xl w-full max-w-md mx-4 p-6 space-y-5"
                onClick={(e) => e.stopPropagation()}
            >
                <div className="flex justify-between items-start">
                    <div>
                        <h2 className="text-lg font-semibold text-mushroom-900">
                            Unsuspend {user.publicNick}
                        </h2>
                        <p className="text-sm text-mushroom-500 mt-1">
                            This will lift the active suspension immediately and
                            restore the user&apos;s access.
                        </p>
                    </div>
                    <button
                        onClick={onClose}
                        className="text-mushroom-400 hover:text-mushroom-600"
                        disabled={isPending}
                    >
                        <X className="h-5 w-5" />
                    </button>
                </div>

                <div className="flex items-start gap-3 rounded-md border border-amber-200 bg-amber-50 px-4 py-3">
                    <ShieldCheck className="h-5 w-5 shrink-0 text-amber-600" />
                    <div className="text-sm text-amber-800">
                        <p>
                            <span className="font-medium">User:</span>{" "}
                            {user.firstName} {user.lastName} ({user.email})
                        </p>
                        {suspensionEndDate && (
                            <p className="mt-1">
                                <span className="font-medium">
                                    Suspension until:
                                </span>{" "}
                                {suspensionEndDate}
                            </p>
                        )}
                    </div>
                </div>

                <div className="flex justify-end gap-3 pt-2">
                    <Button variant="ghost" onClick={onClose} disabled={isPending}>
                        Cancel
                    </Button>
                    <Button
                        onClick={onConfirm}
                        disabled={isPending}
                        className="bg-green-600 hover:bg-green-700 text-white"
                    >
                        {isPending ? (
                            <>
                                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                Unsuspending...
                            </>
                        ) : (
                            "Unsuspend User"
                        )}
                    </Button>
                </div>
            </div>
        </div>
    );
};
