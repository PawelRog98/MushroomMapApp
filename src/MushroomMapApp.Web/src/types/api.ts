export interface ApiResponse<T> {
    data: T;
    success: boolean;
    message: string;
    errors: string[] | null;
    metaData: any | null;
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
