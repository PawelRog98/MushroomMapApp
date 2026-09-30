import { ChevronDown, ChevronsUpDown, ChevronUp } from "lucide-react";
import type { SortBy, SortDir } from "../../types/api";
import { cn } from "../../lib/utils";

export interface SortHeaderProps {
    label: string;
    column: string;
    sort: SortBy;
    onSort:(next: SortBy) => void;
    className?: string;
}

export const SortHeader = ({label, column, sort, onSort, className}: SortHeaderProps) => {
    const isActive = sort.by === column;
    const dir: SortDir | null = isActive ? sort.dir : null;

    const handleClick = () => {
        if(isActive){
            onSort({by: column, dir: sort.dir === "asc" ? "desc" : "asc"});
        } else {
            onSort({by: column, dir: "asc"});
        }
    };

    const ariaSort = dir === "asc" ? "ascending" : dir === "desc" ? "descending" : "none";
    const Icon = dir === "asc" ? ChevronUp : dir === "desc" ? ChevronDown : ChevronsUpDown;

    return(
        <th scope="col" 
            aria-sort={ariaSort}
            className={cn("text-left py-3 px-4 font-medium text-mushroom-700", className)}>
            <button type="button"
                onClick={handleClick}
                className={cn("inline-flex items-center gap-1 transition-colors hover:text-forest-600 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-forest-500 rounded-sm",
                isActive && "text-forest-700 hover:text-forest-800")}>
                {label}
                <Icon className="h-4 w-4 shrink-0" aria-hidden="true"/>
            </button>
        </th>
    );
};