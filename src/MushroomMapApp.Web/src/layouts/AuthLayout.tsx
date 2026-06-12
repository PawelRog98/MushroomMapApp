import { Outlet } from "react-router-dom";

export const AuthLayout = () => {
    return (
        <div className="min-h-screen flex items-center justify-center bg-mushroom-50 p-4">
            <div className="w-full">
                <Outlet />
            </div>
        </div>
    );
};
