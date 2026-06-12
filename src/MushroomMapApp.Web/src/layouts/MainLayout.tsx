import { Outlet, Link } from "react-router-dom";
import { useAuthStore } from "../store/auth-store";
import { Button } from "../components/ui/Button";
import { LogOut, User } from "lucide-react";
import { MushroomIcon } from "../components/icons/MushroomIcon";

export const MainLayout = () => {
    const { userNick, clearAuth } = useAuthStore();

    return (
        <div className="min-h-screen bg-mushroom-50 flex flex-col">
            <header className="bg-white border-b border-mushroom-200">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
                    <Link to="/" className="flex items-center gap-2">
                        <MushroomIcon className="h-8 w-8 text-forest-600" />
                        <span className="text-xl font-bold text-forest-800">MushroomMap</span>
                    </Link>

                    <div className="flex items-center gap-4">
                        <div className="flex items-center gap-2 text-mushroom-700">
                            <User className="h-5 w-5" />
                            <span className="font-medium">{userNick}</span>
                        </div>
                        <Button
                            variant="ghost"
                            size="sm"
                            onClick={clearAuth}
                            className="flex items-center gap-2 text-red-600 hover:text-red-700 hover:bg-red-50"
                        >
                            <LogOut className="h-4 w-4" />
                            Logout
                        </Button>
                    </div>
                </div>
            </header>

            <main className="flex-1 max-w-7xl mx-auto w-full px-4 sm:px-6 lg:px-8 py-8">
                <Outlet />
            </main>

            <footer className="bg-white border-t border-mushroom-200 py-6 text-center text-sm text-mushroom-500">
                &copy; {new Date().getFullYear()} Mushroom Map App. All rights reserved.
            </footer>
        </div>
    );
};
