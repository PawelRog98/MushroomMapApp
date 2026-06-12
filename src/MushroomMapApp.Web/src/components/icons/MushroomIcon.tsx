import type { LucideProps } from "lucide-react";

export const MushroomIcon = ({ ...props }: LucideProps) => (
    <svg
        xmlns="http://www.w3.org/2000/svg"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
        {...props}
    >
        <path d="M12 21v-8" />
        <path d="M12 7c-4.4 0-8 3.6-8 8h16c0-4.4-3.6-8-8-8z" />
    </svg>
);
