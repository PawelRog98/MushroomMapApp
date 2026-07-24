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
];
