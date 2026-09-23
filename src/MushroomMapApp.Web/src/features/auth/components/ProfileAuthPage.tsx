import { useEffect, useState } from "react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "../../../components/ui/Card";
import { useActivateAccount } from "../hooks/useActivateAccount";
import { Input } from "../../../components/ui/Input";
import { Button } from "../../../components/ui/Button";
import { AlertCircle, CheckCircle, Loader2 } from "lucide-react";
import { useResendCode } from "../hooks/useResendCode";

export const ProfileAuthPage = () => {
    const [code, setCode] = useState("");
    const [resendCooldown, setResendCooldown] = useState(0);
    const location = useLocation();
    const navigate = useNavigate();

    const email = location.state?.email;

    useEffect(() => {
        if (resendCooldown <= 0) {
            return;
        }

        const timer = setInterval(() => {
            setResendCooldown((previous) =>
                Math.max(0, previous - 1)
            );
        }, 1000);

        return () => clearInterval(timer);
    }, [resendCooldown]);

    const {
        mutate: activateAccount,
        isPending,
        error,
        isSuccess,
        reset: resetActivation,
    } = useActivateAccount();
    const {
        mutate: resendCode,
        isPending: isResending,
        error: resendError,
        isSuccess: isResendSuccess,
    } = useResendCode();

    if (!email) {
        return <Navigate to="/login" replace />
    }

    const onSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        const trimmedCode = code.trim();

        if (!trimmedCode || isPending || isResending) {
            return;
        }

        activateAccount({ code: trimmedCode, email });
    };

    const onSubmitResend = () => {
        if (resendCooldown > 0 || isResending) {
            return;
        }

        resendCode(
            { email },
            {
                onSuccess: () => {
                    setResendCooldown(30);
                    setCode("");
                }
            }
        );
    };

    const onTryAgain = () => {
        resetActivation();
        setCode("");
    };

    if (isSuccess) {
        return (
            <div className="max-w-3xl mx-auto p-6 space-y-6">
                <Card>
                    <CardHeader>
                        <div className="flex justify-center mb-2">
                            <CheckCircle className="h-12 w-12 text-mushroom-500" />
                        </div>
                        <CardTitle className="text-center">Account Verified!</CardTitle>
                        <CardDescription className="text-center">
                            Your email has been successfully confirmed.
                            You can now log in with your confirmed email.
                        </CardDescription>
                    </CardHeader>
                    <CardContent>
                        <Button onClick={() => navigate("/auth/login", { replace: true })} className="w-full">
                            Go to Login
                        </Button>
                    </CardContent>
                </Card>
            </div>
        );
    }

    if (error) {
        return (
            <div className="max-w-3xl mx-auto p-6 space-y-6">
                <Card>
                    <CardHeader>
                        <CardTitle>Verification Failed</CardTitle>
                        <CardDescription className="flex items-center gap-2 text-red-500">
                            <AlertCircle className="h-4 w-4 shrink-0" />
                            <span>{error.message || "Verification failed. Please try again."}</span>
                        </CardDescription>
                    </CardHeader>
                    <CardContent>
                        <Button onClick={onTryAgain} className="w-full">
                            Try Again
                        </Button>
                    </CardContent>
                </Card>
            </div>
        );
    }

    return (
        <div className="max-w-3xl mx-auto p-6 space-y-6">
            <Card>
                <CardHeader>
                    <CardTitle>Verification email</CardTitle>
                    <CardDescription>Enter your verification code</CardDescription>
                </CardHeader>
                <form onSubmit={onSubmit} className="p-4 space-y-4">
                    <div className="space-y-2">
                        {resendError && (
                            <p className="text-xs text-red-500">
                                {(resendError as { message?: string })?.message || "Could not resend the code. Please try again."}
                            </p>
                        )}

                        {isResendSuccess && (
                            <p className="text-xs text-green-600">
                                A new verification code has been
                                sent to your email.
                            </p>
                        )}
                        <label className="text-sm font-medium">Verification code</label>
                        <Input
                            type="text"
                            value={code}
                            onChange={(e) => setCode(e.target.value)}
                            placeholder="12345"
                            className={error ? "border-red-500" : ""}
                        />
                    </div>
                    <Button
                        type="submit"
                        disabled={isPending || !code.trim()}
                        className="w-full"
                    >
                        {isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                        {isPending ? "Verifying..." : "Verify account"}
                    </Button>
                    <Button
                        type="button"
                        onClick={onSubmitResend}
                        disabled={isPending ||
                            isResending ||
                            resendCooldown > 0
                        }
                        className="w-full"
                        variant="outline"
                    >
                        {isResending ? (
                            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                        ) : resendCooldown > 0 ? (
                            `Resend in ${resendCooldown}s`
                        ) : (
                            "Resend code"
                        )}
                    </Button>
                </form>
            </Card>
        </div>
    )
}
