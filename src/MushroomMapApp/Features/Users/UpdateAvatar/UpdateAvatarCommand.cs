using MediatR;
using Microsoft.EntityFrameworkCore;
using MushroomMapApp.Domain.Data;
using MushroomMapApp.Domain.Entities;
using MushroomMapApp.Domain.Enums;
using MushroomMapApp.Domain.Exceptions;
using MushroomMapApp.Domain.Interfaces;

namespace MushroomMapApp.Features.Users.UpdateAvatar;

public record UpdateAvatarRequest(IFormFile? Image);
public record UpdateAvatarCommand(UpdateAvatarRequest Request, long UserId) : IRequest<Unit>;

public class UpdateAvatarCommandHandler : IRequestHandler<UpdateAvatarCommand, Unit>
{
    private readonly AppDbContext _context;
    private readonly IFileStorage _fileStorage;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IFilePathGenerator _filePathGenerator;

    public UpdateAvatarCommandHandler(AppDbContext context, IFileStorage fileStorage,
        IImageProcessingService imageProcessingService, IFilePathGenerator filePathGenerator)
    {
        _context = context;
        _fileStorage = fileStorage;
        _imageProcessingService = imageProcessingService;
        _filePathGenerator = filePathGenerator;
    }

    public async Task<Unit> Handle(UpdateAvatarCommand command, CancellationToken cancellationToken)
    {
        await using var transactions = await _context.Database.BeginTransactionAsync(cancellationToken);
        var savedImagePaths = new List<string>();
        var oldFiles = new List<string>();

        try
        {
            var user = await _context.Users
                .Include(x => x.AvatarFileResource)
                .ThenInclude(x => x.Variant)
                .FirstOrDefaultAsync(x => x.Id == command.UserId, cancellationToken);

            if (user == null)
                throw new NotFoundException("User not found.");

            var oldAvatarPath = user.AvatarFileResource?.FileName;

            var processedImage = await _imageProcessingService.ProcessImage(command.Request.Image, cancellationToken);

            var largePath = _filePathGenerator.GenerateFilePath("jpg");
            var thumbnail = _filePathGenerator.GenerateFilePath("jpg", "_thumb", "thumbnails");

            await _fileStorage.UploadFile(processedImage.LargeImage, largePath, cancellationToken);
            await _fileStorage.UploadFile(processedImage.Thumbmage, thumbnail, cancellationToken);

            savedImagePaths.Add(largePath);
            savedImagePaths.Add(thumbnail);

            if (user.AvatarFileResource != null)
            {
                oldFiles.Add(user.AvatarFileResource.FileName);

                var oldThumb = user.AvatarFileResource.Variant?.FirstOrDefault(x => x.TypeEnum == FileType.Thumbnail);
                if (oldThumb != null)
                {
                    oldFiles.Add(oldThumb.FileName);
                    await _fileStorage.DeleteFile(oldThumb.FileName, cancellationToken);
                    _context.FileResources.Remove(oldThumb);
                }

                _context.FileResources.Remove(user.AvatarFileResource);
            }

            var avatarResource = new FileResource
            {
                FileName = largePath,
                ContentType = command.Request.Image.ContentType,
                Size = processedImage.LargeImage.Length,
                CreatedAtUtc = DateTime.UtcNow,
                TypeEnum = FileType.Image,
                UserId = user.Id
            };
            await _context.FileResources.AddAsync(avatarResource, cancellationToken);

            var thumbnailResource = new FileResource
            {
                FileName = thumbnail,
                ContentType = command.Request.Image.ContentType,
                Size = processedImage.Thumbmage.Length,
                CreatedAtUtc = DateTime.UtcNow,
                TypeEnum = FileType.Thumbnail,
                UserId = user.Id,
                ParentFileResource = avatarResource
            };
            await _context.FileResources.AddAsync(thumbnailResource, cancellationToken);


            user.AvatarFileResource = avatarResource;
            user.ModifiedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            await transactions.CommitAsync(cancellationToken);

            foreach (var path in oldFiles)
            {
                await _fileStorage.DeleteFile(path, cancellationToken);
            }

            return Unit.Value;
        }
        catch (Exception ex)
        {
            await transactions.RollbackAsync(cancellationToken);
            foreach (var path in savedImagePaths)
            {
                await _fileStorage.DeleteFile(path, cancellationToken);
            }

            throw;
        }
    }
}
