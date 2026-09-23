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


