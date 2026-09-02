import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "../../../components/ui/Card";
import { Button } from "../../../components/ui/Button";
import { useUsers } from "../hooks/useUsers";
import { useSuspendUser } from "../hooks/useSuspendUser";
import { useUnsuspendUser } from "../hooks/useUnsuspendUser";
import { SuspensionPopup } from "./SuspensionPopup";
import type { UserListItem } from "../types";
import { Shield, ShieldOff, Loader2 } from "lucide-react";

export const SuspensionPage = () => {
    const { data: users, isLoading } = useUsers();
    const { mutate: suspendUser, isPending } = useSuspendUser();
    const { mutate: unsuspendUser } = useUnsuspendUser();

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [selectedUser, setSelectedUser] = useState<UserListItem | null>(null);

    const openModal = (user: UserListItem) => {
        setSelectedUser(user);
        setIsModalOpen(true);
    };

    const closeModal = () => {
        setIsModalOpen(false);
        setSelectedUser(null);
    };

    const handleUnsuspend = (user: UserListItem) => {
        if (!confirm(`Unsuspend ${user.publicNick}?`)) return;
        unsuspendUser({ userPublicId: user.publicId });
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
                    <div className="overflow-x-auto">
                        <table className="w-full text-sm">
                            <thead>
                                <tr className="border-b border-mushroom-200">
                                    <th className="text-left py-3 px-4 font-medium text-mushroom-700">Nick</th>
                                    <th className="text-left py-3 px-4 font-medium text-mushroom-700">First Name</th>
                                    <th className="text-left py-3 px-4 font-medium text-mushroom-700">Last Name</th>
                                    <th className="text-left py-3 px-4 font-medium text-mushroom-700">Email</th>
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
                </CardContent>
            </Card>

            <SuspensionPopup
                key={selectedUser?.publicId ?? "closed"}
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
        </div>
    );
};
