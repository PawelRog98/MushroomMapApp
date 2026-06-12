import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { registerSchema, type RegisterFormValues } from "../types";
import { useRegister } from "../hooks/useRegister";
import { Button } from "../../../components/ui/Button";
import { Input } from "../../../components/ui/Input";
import {
    Card,
    CardContent,
    CardDescription,
    CardFooter,
    CardHeader,
    CardTitle,
} from "../../../components/ui/Card";
import { AlertCircle, Loader2, CheckCircle } from "lucide-react";

export const RegisterForm = ({ onToggleMode }: { onToggleMode: () => void }) => {
    const { mutate: registerUser, isPending, error, isSuccess } = useRegister();

    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<RegisterFormValues>({
        resolver: zodResolver(registerSchema),
    });

    const onSubmit = (data: RegisterFormValues) => {
        registerUser(data);
    };

    if (isSuccess) {
        return (
            <Card className="w-full max-w-md mx-auto">
                <CardHeader>
                    <div className="flex justify-center mb-4">
                        <CheckCircle className="h-12 w-12 text-green-500" />
                    </div>
                    <CardTitle className="text-center">Account Created!</CardTitle>
                    <CardDescription className="text-center">
                        Your account has been successfully created. You can now log in.
                    </CardDescription>
                </CardHeader>
                <CardFooter>
                    <Button onClick={onToggleMode} className="w-full">
                        Go to Login
                    </Button>
                </CardFooter>
            </Card>
        );
    }

    return (
        <Card className="w-full max-w-lg mx-auto">
            <CardHeader>
                <CardTitle>Create an Account</CardTitle>
                <CardDescription>Join the mushroom mapping community</CardDescription>
            </CardHeader>
            <form onSubmit={handleSubmit(onSubmit)}>
                <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {error && (
                        <div className="col-span-full flex items-center gap-2 p-3 text-sm text-red-600 bg-red-50 rounded-md">
                            <AlertCircle className="h-4 w-4" />
                            <span>Registration failed. Please try again.</span>
                        </div>
                    )}

                    <div className="space-y-2 col-span-full">
                        <label className="text-sm font-medium">Email</label>
                        <Input
                            type="email"
                            {...register("email")}
                            className={errors.email ? "border-red-500" : ""}
                        />
                        {errors.email && (
                            <p className="text-xs text-red-500">{errors.email.message}</p>
                        )}
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium">Public Nick</label>
                        <Input
                            {...register("publicNick")}
                            className={errors.publicNick ? "border-red-500" : ""}
                        />
                        {errors.publicNick && (
                            <p className="text-xs text-red-500">{errors.publicNick.message}</p>
                        )}
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium">Date of Birth</label>
                        <Input
                            type="date"
                            {...register("dateOfBirth")}
                            className={errors.dateOfBirth ? "border-red-500" : ""}
                        />
                        {errors.dateOfBirth && (
                            <p className="text-xs text-red-500">{errors.dateOfBirth.message}</p>
                        )}
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium">First Name</label>
                        <Input
                            {...register("firstName")}
                            className={errors.firstName ? "border-red-500" : ""}
                        />
                        {errors.firstName && (
                            <p className="text-xs text-red-500">{errors.firstName.message}</p>
                        )}
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium">Last Name</label>
                        <Input
                            {...register("lastName")}
                            className={errors.lastName ? "border-red-500" : ""}
                        />
                        {errors.lastName && (
                            <p className="text-xs text-red-500">{errors.lastName.message}</p>
                        )}
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium">Password</label>
                        <Input
                            type="password"
                            {...register("password")}
                            className={errors.password ? "border-red-500" : ""}
                        />
                        {errors.password && (
                            <p className="text-xs text-red-500">{errors.password.message}</p>
                        )}
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium">Confirm Password</label>
                        <Input
                            type="password"
                            {...register("confirmPassword")}
                            className={errors.confirmPassword ? "border-red-500" : ""}
                        />
                        {errors.confirmPassword && (
                            <p className="text-xs text-red-500">{errors.confirmPassword.message}</p>
                        )}
                    </div>
                </CardContent>
                <CardFooter className="flex flex-col gap-4">
                    <Button type="submit" className="w-full" disabled={isPending}>
                        {isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                        Register
                    </Button>
                    <p className="text-sm text-center text-mushroom-500">
                        Already have an account?{" "}
                        <button
                            type="button"
                            onClick={onToggleMode}
                            className="text-forest-600 hover:underline font-medium"
                        >
                            Login here
                        </button>
                    </p>
                </CardFooter>
            </form>
        </Card>
    );
};
