import { useReactions } from "../hooks/useReactions";
import { useReactionTypes } from "../hooks/useReactionTypes";
import { useToggleReaction } from "../hooks/useToggleReaction";
import type { ReactionDto } from "../types";

export type ReactionRowProps = {
    locationPublicId: string;
};

export const ReactionRow = ({ locationPublicId }: ReactionRowProps) => {
    const { data: types } = useReactionTypes();
    const { data: reactions } = useReactions(locationPublicId);
    const { mutate: toggleReaction } = useToggleReaction(locationPublicId);

    if (!types)
        return null;

    const reactionMap = new Map<string, ReactionDto>(
        (reactions ?? []).map((r) => [r.publicId, r]),
    );

    return (
        <div className="flex gap-1 pt-2 border-t border-gray-100 mt-2">
            {types.map((type) => {
                const reaction = reactionMap.get(type.publicId);
                return (
                    <button
                        key={type.publicId}
                        onClick={() =>
                            toggleReaction({
                                locationPublicId,
                                reactionTypePublicId: type.publicId,
                            })
                        }
                        className="flex items-center gap-1 px-2 py-1 text-sm rounded-full
                                border border-gray-200 hover:border-gray-400
                                transition-colors cursor-pointer"
                    >
                        <span>{type.icon}</span>
                        <span className="text-xs text-gray-500">{reaction?.count ?? 0}</span>
                    </button>
                );
            })}
        </div>
    );
};