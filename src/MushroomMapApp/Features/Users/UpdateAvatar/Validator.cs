using FluentValidation;

namespace MushroomMapApp.Features.Users.UpdateAvatar;

public class Validator : AbstractValidator<UpdateAvatarRequest>
{
    private static readonly string[] AllowedTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxFileSize = 5 * 1024 * 1024;

    public Validator()
    {
        RuleFor(x => x.Image)
            .NotNull().WithMessage("Image is required.");

        RuleFor(x => x.Image!.ContentType)
            .Must(ct => AllowedTypes.Contains(ct))
            .WithMessage("Only JPEG, PNG, and WebP images are allowed.");

        RuleFor(x => x.Image!.Length)
            .LessThanOrEqualTo(MaxFileSize)
            .WithMessage("Image must be 5 MB or smaller.");
    }
}
