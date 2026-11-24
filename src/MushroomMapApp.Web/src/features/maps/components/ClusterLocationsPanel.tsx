import { useEffect, useState } from "react";
import type { ItemWithMeta } from "../../../types/api";
import type { Location as MapLocation, LocationCluster, LocationPermissions } from "../types"
import { useClusterLocations } from "../hooks/useClusterLocations";
import { Pagination } from "../../../components/ui/Pagination";
import { X } from "lucide-react";

type ClusterLocationsPanelProps = {
    cluster: LocationCluster;
    search: string | null;
    onClose: () => void;
    onSelect: (item: ItemWithMeta<MapLocation, LocationPermissions>) => void;
    onZoomToArea: (cluster: LocationCluster) => void;
};

export const ClusterLocationsPanel = ({
    cluster,
    search,
    onClose,
    onSelect,
    onZoomToArea,
}: ClusterLocationsPanelProps) => {
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(25);

    const { data, isFetching } = useClusterLocations(
        cluster,
        search,
        page,
        pageSize
    );

    useEffect(() => {
        const onKeyDown = (event: KeyboardEvent) => {
            if (event.key === "Escape")
                onClose();
        };

        window.addEventListener("keydown", onKeyDown);
        return () => window.removeEventListener("keydown", onKeyDown);
    }, [onClose]);

    const totalCount = data?.paging.totalCount ?? cluster.count;

    return (
        <div
            role="dialog"
            aria-label="Locations in this cluster"
            className="absolute top-20 right-4 z-[1001] flex max-h-[70vh] w-[min(24rem,calc(100vw-2rem))] flex-col overflow-hidden rounded-xl border border-mushroom-200 bg-white shadow-xl max-sm:inset-x-4 max-sm:bottom-4 max-sm:top-auto max-sm:max-h-[55vh]"
        >
            <header className="flex items-center justify-between border-b border-mushroom-100 p-4">
                <div>
                    <h2 className="font-bold text-forest-900">
                        {totalCount} locations
                    </h2>
                    <p className="text-xs text-mushroom-600">In this area</p>
                </div>
                <button
                    type="button"
                    onClick={onClose}
                    aria-label="Close list"
                    className="text-mushroom-400 hover:text-mushroom-600"
                >
                    <X className="h-4 w-4" />
                </button>
            </header>

            <div className="border-b border-mushroom-100 px-4 py-2">
                <button
                    type="button"
                    onClick={() => onZoomToArea(cluster)}
                    className="text-sm font-medium text-forest-700 hover:underline"
                >
                    Zoom to area
                </button>
            </div>

            <div className="flex-1 divide-y divide-mushroom-100 overflow-y-auto">
                {data?.items.length === 0 && (
                    <p className="p-4 text-sm text-mushroom-600">
                        No locations found.
                    </p>
                )}

                {data?.items.map((item, index) => (
                    <button
                        type="button"
                        key={
                            item.data.publicId ??
                            `${item.data.lat}-${item.data.lng}-${index}`
                        }
                        onClick={() => onSelect(item)}
                        className="flex w-full gap-3 p-3 text-left hover:bg-mushroom-50"
                    >
                        {item.data.images[0] && (
                            <img
                                src={`/${item.data.images[0].thumbnailUrl}`}
                                alt=""
                                loading="lazy"
                                className="h-14 w-14 shrink-0 rounded object-cover"
                            />
                        )}
                        <div className="min-w-0">
                            <p className="truncate font-medium text-forest-900">
                                {item.data.name}
                            </p>
                            <p className="truncate text-xs text-mushroom-600">
                                {item.data.authorName}
                            </p>
                            <p className="line-clamp-2 text-xs text-mushroom-500">
                                {item.data.text}
                            </p>
                        </div>
                    </button>
                ))}
            </div>

            <Pagination
                page={data?.paging.currentPage ?? page}
                pageSize={pageSize}
                totalCount={totalCount}
                onPageChange={setPage}
                onPageSizeChange={(size) => {
                    setPageSize(size);
                    setPage(1);
                }}
                isFetching={isFetching}
                className="border-t border-mushroom-100 p-3"
            />
        </div>
    );


};