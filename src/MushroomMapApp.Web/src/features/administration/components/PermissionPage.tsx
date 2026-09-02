import { useState } from "react";
import { useUsers } from "../hooks/useUsers";
import { useSetPermissions } from "../hooks/useSetPermissions";
import type { UserListItem } from "../types";
import { Loader2, Shield } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "../../../components/ui/Card";
import { Button } from "../../../components/ui/Button";
import { UserPermissionsPopup } from "./UserPermissionsPopup";

export const PermissionPage = () => {
    const { data: users, isLoading } = useUsers();
    const { mutate: setPermissions, isPending } = useSetPermissions();

    const [isModalOpen, setModalOpen] = useState(false);
    const [selectedUser, setSelectedUser] = useState<UserListItem | null>(null);

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
                    <div className="overflow-x-auto">
                        <table className="w-full text-sm">
                            <thead>
                                <tr className="border-b border-mushroom-200">
                                    <th className="text-left py-3 px-4 font-medium text-mushroom-700">Nick</th>
                                    <th className="text-left py-3 px-4 font-medium text-mushroom-700">First Name</th>
                                    <th className="text-left py-3 px-4 font-medium text-mushroom-700">Last Name</th>
                                    <th className="text-left py-3 px-4 font-medium text-mushroom-700">Email</th>
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
