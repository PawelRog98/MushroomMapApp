import { Permissions } from "../features/auth/types/permissions";
import { Map, UserCheck, UserKey, UserLock, type LucideIcon } from "lucide-react";

export interface SidebarItem {
    title: string;
    href: string;
    icon: LucideIcon;
    permission?: string;
    children?: SidebarItem[]
}

export const sidebarItems: SidebarItem[] = [
    {
        title: "Locations",
        href: "/locations",
        icon: Map,
        permission: Permissions.Locations.View,
    },
    {
        title: "Admin dashboard",
        href: "/dashboard",
        icon: UserKey,
        permission: Permissions.AdministratorDashboard.View,
        children: [
            {
                title: "Suspensions",
                href: "/dashboard/suspensions",
                icon: UserLock,
                permission: Permissions.AdministratorDashboard.UserManagment
            },
            {
                title: "Permissions",
                href: "/dashboard/permissions",
                icon: UserCheck,
                permission: Permissions.AdministratorDashboard.PermissionsEdit
            },
        ]
    }
];
