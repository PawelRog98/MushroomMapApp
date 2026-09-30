import { Button } from "./Button";
import { cn } from "../../lib/utils";
import { ChevronLeft, ChevronRight } from "lucide-react";

export interface PaginationProps {
    page: number;
    pageSize: number;
    totalCount: number;
    onPageChange: (page: number) => void;
    onPageSizeChange?: (size: number) => void;
    pageSizeOptions?: number[];
    isFetching?: boolean;
    className?: string;
}

const DEFAULT_PAGE_SIZE_OPTIONS = [10, 20, 50, 100];

export const Pagination = ({
    page,
    pageSize,
    totalCount,
    onPageChange,
    onPageSizeChange,
    pageSizeOptions = DEFAULT_PAGE_SIZE_OPTIONS,
    isFetching = false,
    className,
}: PaginationProps) => {
    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
    const currentPage = Math.min(Math.max(page, 1), totalPages);
    const hasPrevious = currentPage > 1;
    const hasNext = currentPage < totalPages;
    const rangeStart = totalCount === 0 ? 0 : (currentPage - 1) * pageSize + 1;
    const rangeEnd = Math.min(currentPage * pageSize, totalCount);

    const windowStart = Math.max(1, currentPage - 2);
    const windowEnd = Math.min(totalPages, currentPage + 2);
    const pageNumbers = Array.from({ length: windowEnd - windowStart + 1 }, (_, i) => windowStart + i);

    return (
        <nav
            aria-label="Pagination"
            className={cn("grid grid-cols-1 gap-4 pt-4 items-center md:grid-cols-[1fr_auto_1fr]", className)}
        >
            <span className="text-sm text-mushroom-700 md:justify-self-start">
                {totalCount === 0
                    ? "No results"
                    : `Showing ${rangeStart}-${rangeEnd} of ${totalCount}`}
            </span>
            <div className="flex items-center justify-center gap-3">
                <Button
                    variant="outline"
                    size="sm"
                    aria-label="Previous page"
                    disabled={!hasPrevious || isFetching}
                    onClick={() => onPageChange(currentPage - 1)}
                >
                    <ChevronLeft />
                </Button>
                <div className="flex items-center gap-1">
                    {pageNumbers.map((p) => (
                        <Button
                            key={p}
                            variant={p === currentPage ? "default" : "ghost"}
                            size="sm"
                            aria-current={p === currentPage ? "page" : undefined}
                            aria-label={`Page ${p}`}
                            disabled={isFetching}
                            onClick={() => onPageChange(p)}
                            className="min-w-9 px-2"
                        >
                            {p}
                        </Button>
                    ))}
                </div>
                <Button
                    variant="outline"
                    size="sm"
                    aria-label="Next page"
                    disabled={!hasNext || isFetching}
                    onClick={() => onPageChange(currentPage + 1)}
                >
                    <ChevronRight />
                </Button>
            </div>
            <div className="flex items-center justify-center gap-3 md:justify-self-end">
                {onPageSizeChange && (
                    <label className="flex items-center gap-2 text-sm text-mushroom-700">
                        Per page
                        <select
                            value={pageSize}
                            onChange={(e) => onPageSizeChange(Number(e.target.value))}
                            disabled={isFetching}
                            className="h-9 rounded-md border border-forest-600 bg-white px-2 text-sm text-forest-600 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-forest-500 disabled:pointer-events-none disabled:opacity-50"
                        >
                            {pageSizeOptions.map((option) => (
                                <option key={option} value={option}>
                                    {option}
                                </option>
                            ))}
                        </select>
                    </label>
                )}
            </div>
        </nav>
    );
};
