import { Route } from "react-router-dom";
import type { FC } from "react";
import { sidebarItems, type SidebarItem } from "../utils/sidebar-items";
import { RouteGuard } from "../features/auth/components/RouteGuard";
import { SuspensionPage } from "../features/administration/components/SuspensionPage";
import { PermissionPage } from "../features/administration/components/PermissionPage";

const pageRegistry: Record<string, FC> = {
    "/dashboard/suspensions": SuspensionPage,
    "/dashboard/permissions": PermissionPage,
};

const flattenItems = (items: SidebarItem[]): SidebarItem[] => {
    return items.flatMap((item) => [
        item,
        ...(item.children ? flattenItems(item.children) : []),
    ]);
}

export const getProtectedRoutes = () => {
    return flattenItems(sidebarItems).flatMap((item) => {
        const Component = pageRegistry[item.href];
        if (!Component) {
            return [];
        }

        return (
            <Route
                key={item.href}
                path={item.href}
                element={
                    <RouteGuard permission={item.permission}>
                        <Component />
                    </RouteGuard>
                }
            />
        );
    });
}
