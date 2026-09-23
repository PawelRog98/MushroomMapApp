import { z } from "zod";

export const loginSchema = z.object({
    email: z.string().email("Invalid email address"),
    password: z.string().min(6, "Password must be at least 8 characters"),
});

export const registerSchema = z
    .object({
        email: z.string().email("Invalid email address"),
        password: z.string().min(6, "Password must be at least 6 characters"),
        confirmPassword: z.string().min(6, "Confirm password must be at least 6 characters"),
        publicNick: z.string().min(3, "Public nick must be at least 3 characters"),
        firstName: z.string().min(2, "First name must be at least 2 characters"),
        lastName: z.string().min(2, "Last name must be at least 2 characters"),
        dateOfBirth: z.string().refine((date) => !isNaN(Date.parse(date)), {
            message: "Invalid date of birth",
        }),
    })
    .refine((data) => data.password === data.confirmPassword, {
        message: "Passwords don't match",
        path: ["confirmPassword"],
    });

export const updateProfileSchema = z.object({
    publicNick: z.string().min(1, "Nick is required").max(100),
    firstName: z.string().min(1, "First name is required"),
    lastName: z.string().min(1, "Last name is required"),
    dateOfBirth: z.string().refine((date) => !isNaN(Date.parse(date)), {
        message: "Invalid date of birth",
    }),
    accountInfo: z.string().optional(),
});


export type LoginFormValues = z.infer<typeof loginSchema>;
export type RegisterFormValues = z.infer<typeof registerSchema>;
export type UpdateUserDataFormValues = z.infer<typeof updateProfileSchema>;

export type UpdateProfileInput = {
    data: UpdateUserDataFormValues;
    avatar?: File | null;
}

export interface AuthResponse {
    accessToken: string;
    refreshToken: string;
    userNick: string;
    userId: string;
}

export interface UserPermissions {
    permissions: string[];
}

export interface PermissionContextValue {
    permissions: ReadonlySet<string>;
    has(permissions: string): boolean;
    reload(): Promise<void>;
    clear(): void;
    isLoaded: boolean;
}

export interface UserProfile {
    publicNick: string;
    firstName: string;
    lastName: string;
    email: string;
    dateOfBirth: string;
    accountInfo: string | null;
    isEmailConfirmed: boolean;
    roleName: string;
    createdAtUtc: Date;
    avatarUrl: string | null;
    avatarThumbnailUrl: string | null;
}

export interface CreateNewVerificationTokenRequest{
    email: string;
}

export interface AcctivateAccountRequest{
    code: string;
    email: string;
}
