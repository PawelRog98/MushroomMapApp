import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { adminApi } from "../api/admin";
import { DEFAULT_SORT, type SortBy } from "../../../types/api";

export const DEFAULT_PAGE_SIZE = 20;

export const useUsers = (page = 1, pageSize = DEFAULT_PAGE_SIZE, sort: SortBy = DEFAULT_SORT) => {
    return useQuery({
        queryKey: ["admin-users", page, pageSize, sort.by, sort.dir],
        queryFn: () => adminApi.getAllUsers({ page, pageSize, sortBy: sort.by, sortDir: sort.dir }),
        placeholderData: keepPreviousData,
    });
};
