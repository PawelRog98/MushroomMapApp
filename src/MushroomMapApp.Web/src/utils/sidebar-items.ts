import { Permissions } from "../features/auth/types/permissions";
import { Map, type LucideIcon } from "lucide-react";

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
        icon: Map,
        permission: Permissions.AdministratorDashboard.View,
        children: [
            {
                title: "Suspensions",
                href: "/dashboard/suspensions",
                icon: Map,
                permission: Permissions.AdministratorDashboard.UserManagment
            },
            {
                title: "Permissions",
                href: "/dashboard/permissions",
                icon: Map,
                permission: Permissions.AdministratorDashboard.PermissionsEdit
            },
        ]
    }
];
