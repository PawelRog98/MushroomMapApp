import type { AxiosError } from "axios";
import type { ErrorResponse } from "../types/api";

export const isEmailNotConfirmed = (error: unknown) : boolean =>{
    if(!error || typeof error !== "object"){
        return false;
    }

    const axioResponse = error as AxiosError<ErrorResponse>;
    const responseData = axioResponse.response?.data;

    return (
        responseData?.success === false &&
        responseData?.data === "User is not active"
    );
}