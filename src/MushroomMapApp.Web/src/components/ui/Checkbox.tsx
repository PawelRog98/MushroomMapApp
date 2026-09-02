import React from "react";
import { cn } from "../../lib/utils";

export interface CheckboxProps extends React.InputHTMLAttributes<HTMLInputElement>{
    label?: string;
    description?: string;
}

const Checkbox = React.forwardRef<HTMLInputElement, CheckboxProps>(
    ({ className, label, description, id, ...props}, ref) => {
        const generatedId = React.useId();
        const checkBoxId = id ?? generatedId;

        return (
            <div className="flex items-center gap-3">
                <input
                    type="checkbox"
                    id={checkBoxId}
                    ref={ref}
                    className={cn(
                        "h-4 w-4 rounded border-mushroom-300 text-forest-600",
                        "focus:ring-forest-500 focus:ring-2 focus:ring-offset-0",
                        "cursor-pointer disabled:cursor-not-optional disabled:opacity-50",
                        className,
                    )}
                    {...props}
                />
                {label && (
                    <label htmlFor={checkBoxId} className="cursor-pointer">
                        <span className="text-sm font-medium text-mushroom-900">{label}</span>
                        {description && (
                            <span className="text-xs text-mushroom-400 block">{description}</span>
                        )}
                    </label>
                )}
            </div>
        );
    }
);
Checkbox.displayName = "Checkbox";
export {Checkbox};