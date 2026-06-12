import { useEffect } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { LoginForm } from "../features/auth/components/LoginForm";
import { RegisterForm } from "../features/auth/components/RegisterForm";
import { useAuthStore } from "../store/auth-store";

export const AuthPage = () => {
    const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
    const navigate = useNavigate();
    const location = useLocation();

    // Derive isLogin directly from the URL to avoid redundant state synchronization
    const isLogin = !location.pathname.includes("register");

    useEffect(() => {
        if (isAuthenticated) {
            navigate("/", { replace: true });
        }
    }, [isAuthenticated, navigate]);

    const toggleMode = () => {
        navigate(isLogin ? "/auth/register" : "/auth/login");
    };

    return (
        <div className="animate-in fade-in duration-500">
            {isLogin ? (
                <LoginForm onToggleMode={toggleMode} />
            ) : (
                <RegisterForm onToggleMode={toggleMode} />
            )}
        </div>
    );
};
