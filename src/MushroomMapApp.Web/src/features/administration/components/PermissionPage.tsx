import { useState } from "react";
import { useUsers, DEFAULT_PAGE_SIZE } from "../hooks/useUsers";
import { useSetPermissions } from "../hooks/useSetPermissions";
import type { UserListItem } from "../types";
import { Loader2, Shield } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "../../../components/ui/Card";
import { Button } from "../../../components/ui/Button";
import { Pagination } from "../../../components/ui/Pagination";
import { SortHeader } from "../../../components/ui/SortHeader";
import { cn } from "../../../lib/utils";
import { getApiErrorMessage } from "../../../lib/api-error";
import { DEFAULT_SORT, type SortBy } from "../../../types/api";
import { UserPermissionsPopup } from "./UserPermissionsPopup";

export const PermissionPage = () => {
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);
    const [sort, setSort] = useState<SortBy>(DEFAULT_SORT);
    const { data, isLoading, isFetching, isPlaceholderData, isError, error } = useUsers(page, pageSize, sort);
    const { mutate: setPermissions, isPending } = useSetPermissions();

    const users = data?.items;
    const paging = data?.paging;
    const loadErrorMessage = isError ? getApiErrorMessage(error, "Failed to load users.") : null;

    const [isModalOpen, setModalOpen] = useState(false);
    const [selectedUser, setSelectedUser] = useState<UserListItem | null>(null);

    if (paging && paging.totalPages > 0 && page > paging.totalPages) {
        setPage(paging.totalPages);
    }

    const handleSort = (next: SortBy) => {
        setSort(next);
        setPage(1);
    };

    const openModal = (data: UserListItem) => {
        setModalOpen(true);
        setSelectedUser(data);
    };

    const closeModal = () => {
        setModalOpen(false);
        setSelectedUser(null);
    };

    const handleSetPermissions = (permissions: string[]) => {
        if (!selectedUser) return;
        setPermissions(
            { userPublicId: selectedUser.publicId, permissions },
            { onSuccess: () => closeModal() }
        );
    };

    if (isLoading) {
        return (
            <div className="flex items-center justify-center h-64">
                <Loader2 className="h-6 w-6 animate-spin text-mushroom-400" />
            </div>
        );
    }

    return (
        <div className="max-w-full mx-auto p-6 space-y-6">
            <Card>
                <CardHeader>
                    <CardTitle>Users</CardTitle>
                </CardHeader>
                <CardContent>
                    {loadErrorMessage && (
                        <div className="mb-4 rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                            {loadErrorMessage}
                        </div>
                    )}
                    <div className={cn("overflow-x-auto", isPlaceholderData && "opacity-60 transition-opacity")}>
                        <table className="w-full text-sm">
                            <thead>
                                <tr className="border-b border-mushroom-200">
                                    <SortHeader label="Nick" column="nick" sort={sort} onSort={handleSort} />
                                    <SortHeader label="First Name" column="firstname" sort={sort} onSort={handleSort} />
                                    <SortHeader label="Last Name" column="lastname" sort={sort} onSort={handleSort} />
                                    <SortHeader label="Email" column="email" sort={sort} onSort={handleSort} />
                                    <th className="text-left py-3 px-4 font-medium text-mushroom-700">Role</th>
                                    <th className="text-right py-3 px-4 font-medium text-mushroom-700">Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {users?.map((user) => (
                                    <tr key={user.publicId} className="border-b border-mushroom-200 hover:bg-mushroom-50">
                                        <td className="py-3 px-4 text-mushroom-900">{user.publicNick}</td>
                                        <td className="py-3 px-4 text-mushroom-700">{user.firstName}</td>
                                        <td className="py-3 px-4 text-mushroom-700">{user.lastName}</td>
                                        <td className="py-3 px-4 text-mushroom-700">{user.email}</td>
                                        <td className="py-3 px-4 text-mushroom-700">{user.roleName}</td>
                                        <td className="py-3 px-4 text-right">
                                            <Button
                                                variant="outline"
                                                size="sm"
                                                onClick={() => openModal(user)}
                                                className="text-forest-600 border-forest-500 hover:text-forest-700 hover:bg-forest-200"
                                            >
                                                <Shield className="h-4 w-4 mr-1" />
                                                Permissions
                                            </Button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                    <Pagination
                        page={page}
                        pageSize={pageSize}
                        totalCount={paging?.totalCount ?? 0}
                        onPageChange={setPage}
                        onPageSizeChange={(size) => {
                            setPageSize(size);
                            setPage(1);
                        }}
                        isFetching={isFetching}
                    />
                </CardContent>
            </Card>

            <UserPermissionsPopup
                key={selectedUser?.publicId ?? "none"}
                isOpen={isModalOpen}
                onClose={closeModal}
                user={selectedUser}
                onConfirm={handleSetPermissions}
                isPending={isPending}
            />
        </div>
    );
};
