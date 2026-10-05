import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "../../../components/ui/Card";
import { Button } from "../../../components/ui/Button";
import { Pagination } from "../../../components/ui/Pagination";
import { SortHeader } from "../../../components/ui/SortHeader";
import { cn } from "../../../lib/utils";
import { getApiErrorMessage } from "../../../lib/api-error";
import { DEFAULT_SORT, type SortBy } from "../../../types/api";
import { useUsers, DEFAULT_PAGE_SIZE } from "../hooks/useUsers";
import { useSuspendUser } from "../hooks/useSuspendUser";
import { useUnsuspendUser } from "../hooks/useUnsuspendUser";
import { SuspensionPopup } from "./SuspensionPopup";
import { UnsuspendPopup } from "./UnsuspendPopup";
import type { UserListItem } from "../types";
import { Shield, ShieldOff, Loader2 } from "lucide-react";

export const SuspensionPage = () => {
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);
    const [sort, setSort] = useState<SortBy>(DEFAULT_SORT);
    const { data, isLoading, isFetching, isPlaceholderData, isError, error } = useUsers(page, pageSize, sort);
    const { mutate: suspendUser, isPending } = useSuspendUser();
    const { mutate: unsuspendUser, isPending: isUnsuspendPending } = useUnsuspendUser();

    const users = data?.items;
    const paging = data?.paging;
    const loadErrorMessage = isError ? getApiErrorMessage(error, "Failed to load users.") : null;

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [selectedUser, setSelectedUser] = useState<UserListItem | null>(null);
    const [isUnsuspendModalOpen, setIsUnsuspendModalOpen] = useState(false);
    const [unsuspendTarget, setUnsuspendTarget] = useState<UserListItem | null>(null);

    if (paging && paging.totalPages > 0 && page > paging.totalPages) {
        setPage(paging.totalPages);
    }

    const handleSort = (next: SortBy) => {
        setSort(next);
        setPage(1);
    };

    const openModal = (user: UserListItem) => {
        setSelectedUser(user);
        setIsModalOpen(true);
    };

    const closeModal = () => {
        setIsModalOpen(false);
        setSelectedUser(null);
    };

    const handleUnsuspend = (user: UserListItem) => {
        setUnsuspendTarget(user);
        setIsUnsuspendModalOpen(true);  
    };

    const closeUnsuspendModal = () => {
        setIsUnsuspendModalOpen(false);
        setUnsuspendTarget(null);
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
                                    <th className="text-left py-3 px-4 font-medium text-mushroom-700">Status</th>
                                    <th className="text-right py-3 px-4 font-medium text-mushroom-700">Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {users?.map((user) => (
                                    <tr key={user.publicId} className="border-b border-mushroom-100 hover:bg-mushroom-50">
                                        <td className="py-3 px-4 text-mushroom-900">{user.publicNick}</td>
                                        <td className="py-3 px-4 text-mushroom-700">{user.firstName}</td>
                                        <td className="py-3 px-4 text-mushroom-700">{user.lastName}</td>
                                        <td className="py-3 px-4 text-mushroom-700">{user.email}</td>
                                        <td className="py-3 px-4">
                                            {user.isActiveSuspension ? (
                                                <span className="inline-flex items-center gap-1 px-2 py-1 text-xs font-medium rounded-full bg-red-100 text-red-700">
                                                    <ShieldOff className="h-3 w-3" />
                                                    Suspended
                                                </span>
                                            ) : (
                                                <span className="inline-flex items-center gap-1 px-2 py-1 text-xs font-medium rounded-full bg-green-100 text-green-700">
                                                    <Shield className="h-3 w-3" />
                                                    Active
                                                </span>
                                            )}
                                        </td>
                                        <td className="py-3 px-4 text-right">
                                            {user.isActiveSuspension ? (
                                                <Button
                                                    variant="ghost"
                                                    size="sm"
                                                    onClick={() => handleUnsuspend(user)}
                                                    className="text-green-600 hover:text-green-700"
                                                >
                                                    Unsuspend
                                                </Button>
                                            ) : (
                                                <Button
                                                    variant="outline"
                                                    size="sm"
                                                    onClick={() => openModal(user)}
                                                    className="text-red-600 border-red-200 hover:bg-red-50"
                                                >
                                                    Suspend
                                                </Button>
                                            )}
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

            <SuspensionPopup
                key={`suspend-${selectedUser?.publicId ?? "closed"}`}
                isOpen={isModalOpen}
                onClose={closeModal}
                user={selectedUser}
                onConfirm={(days, reason) => {
                    suspendUser(
                        { userPublicId: selectedUser!.publicId, days, reason },
                        { onSuccess: closeModal }
                    );
                }}
                isPending={isPending}
            />

            <UnsuspendPopup
                key={`unsuspend-${unsuspendTarget?.publicId ?? "closed"}`}
                isOpen={isUnsuspendModalOpen}
                onClose={closeUnsuspendModal}
                user={unsuspendTarget}
                onConfirm={() => {
                    unsuspendUser(
                        { userPublicId: unsuspendTarget!.publicId },
                        { onSuccess: closeUnsuspendModal }
                    );
                }}
                isPending={isUnsuspendPending}
            />
        </div>
    );
};
