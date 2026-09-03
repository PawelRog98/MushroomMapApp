import { NavLink } from "react-router-dom";
import { useSidebarPermission } from "../features/auth/hooks/useSidebarPermission";
import { cn } from "../lib/utils";
import { ChevronDown, LogOut, PanelLeftClose, PanelLeftOpen, User } from "lucide-react";
import { useState } from "react";
import type { SidebarItem } from "../utils/sidebar-items";

interface SidebarProps {
    collapsed: boolean;
    onToggle: () => void;
    userNick?: string;
    onLogout?: () => void;
}

const SidebarNavItem = ({ item, collapsed }: { item: SidebarItem; collapsed: boolean }) => {
    const [expanded, setExpanded] = useState(false);
    const hasChildren = item.children && item.children.length > 0;

    return (
        <div>
            <NavLink
                to={item.href}
                end
                className={({ isActive }) =>
                    cn(
                        "flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors",
                        collapsed ? "justify-center px-0 mx-1" : "",
                        isActive
                            ? "bg-forest-600 text-white shadow-sm"
                            : "text-forest-100 hover:bg-forest-700 hover:text-white",
                    )
                }
                onClick={(e) => {
                    if (hasChildren) {
                        e.preventDefault();
                        setExpanded((p) => !p);
                    }
                }}
                title={collapsed ? item.title : undefined}
            >
                {({ isActive }) => (
                    <>
                        <item.icon
                            className={cn(
                                "h-5 w-5 shrink-0",
                                isActive ? "text-white" : "text-forest-300",
                            )}
                        />
                        {!collapsed && (
                            <>
                                <span className="truncate">{item.title}</span>
                                {hasChildren && (
                                    <ChevronDown
                                        className={cn(
                                            "h-4 w-4 ml-auto transition-transform",
                                            expanded && "rotate-180",
                                        )}
                                    />
                                )}
                            </>
                        )}
                    </>
                )}
            </NavLink>
            {!collapsed && hasChildren && expanded && (
                <div className="ml-4 mt-1 space-y-1">
                    {item.children!.map((child) => (
                        <SidebarNavItem key={child.href} item={child} collapsed={collapsed} />
                    ))}
                </div>
            )}
        </div>
    );
}

export const Sidebar = ({ collapsed, onToggle, userNick, onLogout }: SidebarProps) => {
    const items = useSidebarPermission();

    return (
        <aside
            className={cn(
                "bg-forest-800 flex flex-col shrink-0 border-r border-forest-700 transition-all duration-300",
                collapsed ? "w-16" : "w-64",
            )}
        >
            <div
                className={cn(
                    "flex items-center border-b border-forest-700 h-14",
                    collapsed ? "justify-center px-0" : "justify-between px-4",
                )}
            >
                {!collapsed && (
                    <span className="font-semibold text-sm text-forest-100">Menu</span>
                )}
                <button
                    onClick={onToggle}
                    className="p-1.5 rounded-md text-forest-300 hover:text-white hover:bg-forest-700 transition-colors"
                    title={collapsed ? "Expand sidebar" : "Collapse sidebar"}
                >
                    {collapsed ? (
                        <PanelLeftOpen className="h-5 w-5" />
                    ) : (
                        <PanelLeftClose className="h-5 w-5" />
                    )}
                </button>
            </div>
            {(userNick || onLogout) && (
                <div className={cn(
                    "border-b border-forest-700 p-3",
                    collapsed ? "flex flex-col items-center gap-2" : "space-y-2",
                )}>
                    {!collapsed && userNick && (
                        <div className="flex items-center gap-2 text-forest-200 px-1">
                            <User className="h-4 w-4 shrink-0" />
                            <span className="text-sm font-medium truncate">{userNick}</span>
                        </div>
                    )}
                    {collapsed && userNick && (
                        <div className="text-forest-300" title={userNick}>
                            <User className="h-5 w-5" />
                        </div>
                    )}
                    {onLogout && (
                        <button
                            onClick={onLogout}
                            className={cn(
                                "flex items-center gap-2 rounded-lg text-sm font-medium text-red-400 hover:text-red-300 hover:bg-forest-700 transition-colors",
                                collapsed ? "justify-center p-1.5" : "w-full px-3 py-2.5",
                            )}
                            title={collapsed ? "Logout" : undefined}
                        >
                            <LogOut className="h-4 w-4 shrink-0" />
                            {!collapsed && <span>Logout</span>}
                        </button>
                    )}
                </div>
            )}
            <nav className="flex-1 py-4 space-y-1 overflow-y-auto">
                {items.map((item) => (
                    <SidebarNavItem key={item.href} item={item} collapsed={collapsed} />
                ))}
                {items.length === 0 && (
                    <p
                        className={cn(
                            "text-xs text-forest-400 text-center",
                            collapsed ? "px-1 py-4" : "px-3 py-4",
                        )}
                    >
                        {collapsed ? "—" : "No items available"}
                    </p>
                )}
            </nav>
        </aside>
    );
};
