import { permissionStore } from "../../../store/permission-store"
import { sidebarItems, type SidebarItem } from "../../../utils/sidebar-items";

export const useSidebarPermission = () =>{
    const permissions = permissionStore(state => state.permissions);

    const filterItems = (items: SidebarItem[], permissions: Set<string>): SidebarItem[] =>{
        return items
            .filter(item => !item.permission || permissions.has(item.permission))
            .map(item => ({
                ...item,
                children: item.children
                    ? filterItems(item.children, permissions) : undefined
            }));
    };

    return filterItems(sidebarItems, permissions);
}