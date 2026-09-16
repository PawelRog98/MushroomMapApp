import { useState } from "react";
import { useAuthStore } from "../store/auth-store";
import { permissionStore } from "../store/permission-store";
import { Link, Outlet } from "react-router-dom";
import { MushroomIcon } from "../components/icons/MushroomIcon";
import { Sidebar } from "../components/Sidebar";
import { authApi } from "../features/auth/api/auth";

export const MapLayout = () => {
    const { userNick, clearAuth } = useAuthStore();
    const { clear } = permissionStore();
    const [sidebarCollapsed, setSidebarCollapsed] = useState(true); // Default collapsed for map view

    const handleLogout = async () => {
        try {
            await authApi.logout();
        } catch {
            // Ignore errors — clear local state regardless
        }
        clearAuth();
        clear();
    };

    return(
        <div className="h-screen bg-mushroom-50 flex flex-col overflow-hidden">
            <header className="bg-white border-b border-mushroom-200 shrink-0 h-16">
                <div className="h-full flex items-center px-4 sm:px-6 lg:px-8">
                    <Link to="/" className="flex items-center gap-2">
                        <MushroomIcon className="h-8 w-8 text-forest-600" />
                        <span className="text-xl font-bold text-forest-800">Mushroom Map</span>
                    </Link>
                </div>
            </header>

            <div className="flex flex-1 min-h-0">
                <Sidebar
                    collapsed={sidebarCollapsed}
                    onToggle={() => setSidebarCollapsed((p) => !p)}
                    userNick={userNick}
                    onLogout={handleLogout}
                />
                <main className="flex-1 min-h-0">
                    <Outlet />
                </main>
            </div>
        </div>
    )
}
