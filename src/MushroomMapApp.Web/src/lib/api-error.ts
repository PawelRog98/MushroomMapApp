export function getApiErrorMessage(error: unknown, fallback = "Something went wrong."): string {
    if (error && typeof error === "object") {
        const { errors, message } = error as { errors?: unknown; message?: unknown };
        if (Array.isArray(errors)) {
            const messages = errors.filter((e): e is string => typeof e === "string" && e.trim().length > 0);
            if (messages.length > 0) return messages.join(" ");
        }
        if (typeof message === "string" && message.trim().length > 0) return message;
    }
    return fallback;
}
