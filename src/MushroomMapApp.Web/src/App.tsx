import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { QueryClientProvider } from "@tanstack/react-query";
import { queryClient } from "./lib/query-client";
import { ProtectedRoute } from "./components/ProtectedRoute";
import { MainLayout } from "./layouts/MainLayout";
import { AuthLayout } from "./layouts/AuthLayout";
import { AuthPage } from "./pages/AuthPage";
import { getProtectedRoutes } from "./router/routes";
import { UserProfilePage } from "./features/auth/components/UserProfilePage";
import { MapLayout } from "./layouts/MapLayout";
import { HomePage } from "./pages/HomePage";
import { ProfileAuthPage } from "./features/auth/components/ProfileAuthPage";

const App = () => {
    return (
        <QueryClientProvider client={queryClient}>
            <BrowserRouter>
                <Routes>
                    <Route path="/auth" element={<AuthLayout />}>
                        <Route path="login" element={<AuthPage />} />
                        <Route path="register" element={<AuthPage />} />
                        <Route path="verify-email" element={< ProfileAuthPage />} />
                        <Route index element={<Navigate to="login" replace />} />
                    </Route>

                    <Route element={<ProtectedRoute />}>
                        <Route element={<MapLayout />}>
                            <Route path="/locations" element={<HomePage />} />
                        </Route>

                        <Route element={<MainLayout />}>
                            <Route path="/" element={<Navigate to="/locations" replace />} />
                            {getProtectedRoutes()}
                            <Route path="/profile/:id" element={<UserProfilePage />} />
                        </Route>
                    </Route>

                    <Route path="*" element={<Navigate to="/" replace />} />
                </Routes>
            </BrowserRouter>
        </QueryClientProvider>
    );
}

export default App;
