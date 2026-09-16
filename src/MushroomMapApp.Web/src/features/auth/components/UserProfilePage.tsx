import { useParams } from "react-router-dom"
import { useAuthStore } from "../../../store/auth-store";
import { useUserData } from "../hooks/useUserData";
import { useUpdateProfile } from "../hooks/useUpdateProfile";
import { useForm } from "react-hook-form";
import { updateProfileSchema, type UpdateUserDataFormValues } from "../types";
import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect, useRef, useState } from "react";
import { AlertCircle, Camera, CheckCircle, Loader2, UserIcon } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "../../../components/ui/Card";
import { Input } from "../../../components/ui/Input";
import { Button } from "../../../components/ui/Button";

export const UserProfilePage = () => {
    const {id} = useParams<{id: string}>();
    const currentUserId = useAuthStore((s) => s.userId);
    const isOwnProfile = currentUserId === id;

    const fileInputRef = useRef<HTMLInputElement>(null);
    const [selectedAvatar, setSelectedAvatar] = useState<File|null>(null);
    const [avatarPreview, setAvatarPreview] = useState<string|null>(null);

    const {data: profile, isLoading, error} = useUserData(id!);
    const {mutate: updateUserData, isPending, isSuccess, error: updateError} = useUpdateProfile();

    const handleAvatarChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];

        if(!file)
            return;

        setSelectedAvatar(file);
        setAvatarPreview(URL.createObjectURL(file));
    }

    const {register, handleSubmit, reset, formState: {errors}} = useForm<UpdateUserDataFormValues>({
        resolver: zodResolver(updateProfileSchema),
    });

    useEffect(() => {
        if(profile){
            reset({
                publicNick: profile.publicNick,
                firstName: profile.firstName,
                lastName: profile.lastName,
                dateOfBirth: profile.dateOfBirth ? new Date(profile.dateOfBirth).toISOString().split("T")[0] : "",
                accountInfo: profile.accountInfo ?? ""
            });
        }
    }, [profile, reset]);

    useEffect(() => {
        return () => {
            if(avatarPreview){
                URL.revokeObjectURL(avatarPreview);
            }
        }
    }, [avatarPreview])

    const onSubmit = (data: UpdateUserDataFormValues) => {
        updateUserData({
            data,
            avatar: selectedAvatar
        });
    };

    if(isLoading){
        return (
            <div className="flex items-center justify-center h-64">
                <Loader2 className="h-6 w-6 animate-spin text-mushroom-400"/>
            </div>
        );
    }

    if (error || !profile) {
        return (
            <div className="flex items-center justify-center h-64">
                <p className="text-mushroom-500">User not found.</p>
            </div>
        );
    }

    return(
        <div className="max-w-3xl mx-auto p-6 space-y-6">
            <Card>
                <CardHeader>
                    <div className="flex items-center gap-3">
                        <div className="relative group">
                            <input
                                ref={fileInputRef}
                                type="file"
                                accept="image/jpeg,image/png,image/webp"
                                className="hidden"
                                onChange={handleAvatarChange}
                            />
                            <div onClick={() => isOwnProfile ?? fileInputRef.current?.click()}
                                className={`h-16 w-16 rounded-full bg-forest-100 flex items-center justify-center overflow-hidden transition-all 
                                ${isOwnProfile ?? "hover:ring-2 hover:ring-forest-300 cursor-pointer"}`}>
                                    
                                {avatarPreview ? (
                                    <img src={avatarPreview} alt="Avatar preview" className="h-full w-full object-cover" />
                                ) : profile.avatarThumbnailUrl ? (
                                    <img src={"/" + profile.avatarThumbnailUrl} alt={profile.publicNick} className="h-full w-full object-cover" />
                                ) : (
                                    <UserIcon className="h-8 w-8 text-forest-600"/>
                                )}
                            </div>
                            {isOwnProfile && (
                                <div onClick={() => fileInputRef.current?.click()}
                                    className="absolute inset-0 rounded-full bg-black/40 opacity-0 group-hover:opacity-100 flex items-center justify-center transition-opacity cursor-pointer">
                                    <Camera className="h-5 w-5 text-white"/>
                                </div>
                            )}
                        </div>
                        <CardTitle>{profile.publicNick}</CardTitle>
                    </div>
                </CardHeader>
                <CardContent>
                    {isOwnProfile ? (
                        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                            {updateError && (
                                <div className="flex items-center gap-2 p-3 text-sm text-red-600 bg-red-50 rounded-md">
                                    <AlertCircle className="h-4 w-4"/>
                                    <span>Failed to update profile</span>
                                </div>
                            )}
                            {isSuccess && (
                                <div className="flex items-center gap-2 p-3 text-sm text-green-600 bg-green-50 rounded-md">
                                    <CheckCircle className="h-4 w-4" />
                                    <span>Profile updated successfully</span>
                                </div>
                            )}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div className="space-y-2">
                                    <label className="text-sm font-medium">Public Nick</label>
                                    <Input {...register("publicNick")} className={errors.publicNick ? "border-red-500" : ""}/>
                                    {errors.publicNick && <p className="text-xs text-red-500">{errors.publicNick.message}</p>}
                                </div>
                                <div className="space-y-2">
                                    <label className="text-sm font-medium">First Name</label>
                                    <Input {...register("firstName")} className={errors.firstName ? "border-red-500" : ""}/>
                                    {errors.firstName && <p className="text-xs text-red-500">{errors.firstName.message}</p>}
                                </div>
                                <div className="space-y-2">
                                    <label className="text-sm font-medium">Last Name</label>
                                    <Input {...register("lastName")} className={errors.lastName ? "border-red-500" : ""}/>
                                    {errors.lastName && <p className="text-xs text-red-500">{errors.lastName.message}</p>}
                                </div>
                                <div className="space-y-2">
                                    <label className="text-sm font-medium">Date of Birth</label>
                                    <Input type="date" {...register("dateOfBirth")} className={errors.dateOfBirth ? "border-red-500" : ""}/>
                                    {errors.dateOfBirth && <p className="text-xs text-red-500">{errors.dateOfBirth.message}</p>}
                                </div>
                            </div>

                            <div className="space-y-2">
                                <label className="text-sm font-medium">Account info</label>
                                <Input {...register("accountInfo")} placeholder="About me..."/>
                            </div>
                            <Button type="submit" disabled={isPending}>
                                {isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                                Save changes
                            </Button>
                        </form>
                    ) : (
                        <div className="space-y-4">
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <p className="text-xs text-mushroom-500">Public Nick</p>
                                    <p className="text-sm font-medium">{profile.publicNick}</p>
                                </div>
                                <div>
                                    <p className="text-xs text-mushroom-500">First name</p>
                                    <p className="text-sm font-medium">{profile.firstName}</p>
                                </div>
                                <div>
                                    <p className="text-xs text-mushroom-500">Last name</p>
                                    <p className="text-sm font-medium">{profile.lastName}</p>
                                </div>
                                <div>
                                    <p className="text-xs text-mushroom-500">Date of birth</p>
                                    <p className="text-sm font-medium">{new Date(profile.dateOfBirth).toLocaleDateString()}</p>
                                </div>
                                {profile.accountInfo && (
                                    <div>
                                        <p className="text-xs text-mushroom-500">Account info</p>
                                        <p className="text-sm font-medium">{profile.accountInfo}</p>
                                    </div>
                                )}
                            </div>
                        </div>
                    )}
                    <div className="mt-6 pt-4 border-t border-mushroom-200 space-y-3">
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <p className="text-xs text-mushroom-500">Role</p>
                                <p className="text-sm font-medium">{profile.roleName}</p>
                            </div>
                            <div>
                                <p className="text-xs text-mushroom-500">Is confirmed</p>
                                <p className="text-sm font-medium">{profile.isEmailConfirmed ? "Yes" : "No"}</p>
                            </div>
                            <div>
                                <p className="text-xs text-mushroom-500">Member since</p>
                                <p className="text-sm font-medium">{new Date(profile.createdAtUtc).toLocaleDateString()}</p>
                            </div>
                        </div>
                    </div>
                </CardContent>
            </Card>
        </div>
    )
}