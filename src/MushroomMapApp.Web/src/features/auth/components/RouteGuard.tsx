import { Navigate } from "react-router-dom";
import { useAuthStore } from "../../../store/auth-store";
import { useHasPermission } from "../hooks/useHasPermission";
import { permissionStore } from "../../../store/permission-store";
import type { ReactNode } from "react";

interface RouteGuardProps {
    permission?: string;
    children: ReactNode;
}

export const RouteGuard = ({ permission, children }: RouteGuardProps) => {
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    const hasPermission = useHasPermission(permission ?? "");
    const loaded = permissionStore((s) => s.loaded);

    if (!isAuthenticated) {
        return <Navigate to="/auth/login" replace />;
    }

    if (permission && !loaded) {
        return null;
    }

    if (permission && !hasPermission) {
        return <Navigate to="/" replace />;
    }

    return <>{children}</>;
}
