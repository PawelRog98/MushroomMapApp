export interface ApiResponse<T> {
    data: T;
    success: boolean;
    message: string;
    errors: string[] | null;
    metaData: any | null;
}

export interface ErrorResponse{
    success?: boolean;
    data?: string;
    errors?: unknown;
    metaData?: unknown;
    message?: string;
}

export interface ItemWithMeta<TData, TMeta> {
    data: TData;
    meta: TMeta;
}

export interface ResourcePermissions{
    canView: boolean;
    canEdit: boolean;
    canDelete: boolean;
}

export interface PagingMeta{
    currentPage: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasPrevious: boolean;
    hasNext: boolean;
    sortBy?: string;
    sortDir?: SortDir;
}

export interface Paged<T> {
    items: T[];
    paging: PagingMeta;
}

export type SortDir = "asc" | "desc";

export interface SortBy {
    by: string;
    dir: SortDir;
}

export const SORTABLE_USER_COLUMNS = ["nick", "firstname", "lastname", "email"] as const;
export const DEFAULT_SORT: SortBy = { by: "nick", dir: "asc" };


