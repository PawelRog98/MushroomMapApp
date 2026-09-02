import { useState, useEffect } from "react";
import type { SuspensionUserPopupProps } from "../types";
import { X, Loader2, ChevronDown } from "lucide-react";
import { Button } from "../../../components/ui/Button";
import { Input } from "../../../components/ui/Input";

const DURATION_OPTIONS = [
    { value: "1", label: "1 day" },
    { value: "3", label: "3 days" },
    { value: "7", label: "7 days" },
    { value: "14", label: "14 days" },
    { value: "30", label: "30 days" },
    { value: "custom", label: "Custom..." },
];

export const SuspensionPopup = ({
    isOpen,
    onClose,
    user,
    onConfirm,
    isPending,
}: SuspensionUserPopupProps) => {
    const [selectedPreset, setSelectedPreset] = useState<number | null>(null);
    const [isCustom, setIsCustom] = useState(false);
    const [customDays, setCustomDays] = useState("");
    const [reason, setReason] = useState("");

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

    const effectiveDays = isCustom
        ? parseInt(customDays, 10)
        : selectedPreset;

    const isValid =
        effectiveDays !== null && effectiveDays > 0 && reason.trim().length > 0;

    const handleConfirm = () => {
        if (!isValid) return;
        onConfirm(effectiveDays, reason.trim());
    };

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
                            Suspend {user.publicNick}
                        </h2>
                        <p className="text-sm text-mushroom-500 mt-1">
                            Select duration and provide a reason
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

                <div className="space-y-2">
                    <label className="text-sm font-medium text-mushroom-700">
                        Duration
                    </label>
                    <div className="relative">
                        <select
                            value={isCustom ? "custom" : selectedPreset?.toString() ?? ""}
                            onChange={(e) => {
                                if (e.target.value === "custom") {
                                    setIsCustom(true);
                                    setSelectedPreset(null);
                                } else {
                                    setIsCustom(false);
                                    setSelectedPreset(parseInt(e.target.value, 10));
                                    setCustomDays("");
                                }
                            }}
                            disabled={isPending}
                            className="flex h-10 w-full rounded-md border border-mushroom-200 bg-white px-3 py-2 text-sm text-mushroom-900 ring-offset-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-forest-500 appearance-none cursor-pointer disabled:cursor-not-allowed disabled:opacity-50"
                        >
                            <option value="" disabled>
                                Select duration...
                            </option>
                            {DURATION_OPTIONS.map((opt) => (
                                <option key={opt.value} value={opt.value}>
                                    {opt.label}
                                </option>
                            ))}
                        </select>
                        <ChevronDown className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-mushroom-400 pointer-events-none" />
                    </div>

                    {isCustom && (
                        <div className="flex items-center gap-2">
                            <Input
                                type="number"
                                min="1"
                                max="365"
                                placeholder="Number of days"
                                value={customDays}
                                onChange={(e) => setCustomDays(e.target.value)}
                                className="w-32"
                                autoFocus
                                disabled={isPending}
                            />
                            <span className="text-sm text-mushroom-500">days</span>
                        </div>
                    )}
                </div>

                <div className="space-y-2">
                    <label className="text-sm font-medium text-mushroom-700">
                        Reason
                    </label>
                    <textarea
                        value={reason}
                        onChange={(e) => setReason(e.target.value)}
                        placeholder="Enter reason for suspension..."
                        disabled={isPending}
                        className="flex w-full rounded-md border border-mushroom-200 bg-white px-3 py-2 text-sm text-mushroom-900 ring-offset-white placeholder:text-mushroom-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-forest-500 min-h-[80px] resize-none disabled:cursor-not-allowed disabled:opacity-50"
                    />
                </div>

                <div className="flex justify-end gap-3 pt-2">
                    <Button variant="ghost" onClick={onClose} disabled={isPending}>
                        Cancel
                    </Button>
                    <Button
                        onClick={handleConfirm}
                        disabled={!isValid || isPending}
                        className="bg-red-600 hover:bg-red-700 text-white"
                    >
                        {isPending ? (
                            <>
                                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                Suspending...
                            </>
                        ) : (
                            "Suspend User"
                        )}
                    </Button>
                </div>
            </div>
        </div>
    );
};