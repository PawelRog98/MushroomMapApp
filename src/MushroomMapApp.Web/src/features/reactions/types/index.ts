export type ReactionDto = {
  publicId: string;
  key: string;
  name: string;
  icon: string;
  count: number;
};

export type ReactionTypeDto = {
  publicId: string;
  key: string;
  name: string;
  icon: string;
};

export type AddReactionRequest = {
  locationPublicId: string;
  reactionTypePublicId: string;
};