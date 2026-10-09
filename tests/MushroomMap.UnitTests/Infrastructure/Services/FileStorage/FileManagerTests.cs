using FluentAssertions;
using Microsoft.Extensions.Options;
using MushroomMapApp.Infrastructure.Services.FileStorage;
using Xunit;

namespace MushroomMap.UnitTests.Infrastructure.Services.FileStorage;

public class FileManagerTests : IDisposable
{
    private readonly string _storageRoot;
    private readonly FileManager _fileManager;

    public FileManagerTests()
    {
        _storageRoot = Path.Combine(Path.GetTempPath(), $"filemanager-tests-{Guid.NewGuid():N}");
        var options = Options.Create(new FileStorageOptions { RootPath = _storageRoot });
        _fileManager = new FileManager(options);
    }

    public void Dispose()
    {
        if (Directory.Exists(_storageRoot))
            Directory.Delete(_storageRoot, recursive: true);
    }

    [Fact]
    public void Constructor_Throws_WhenRootPathIsEmpty()
    {
        var options = Options.Create(new FileStorageOptions { RootPath = "" });

        var act = () => new FileManager(options);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("File storage root path is not configured.");
    }

    [Fact]
    public void Constructor_CreatesDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), $"filemanager-tests-{Guid.NewGuid():N}");
        var options = Options.Create(new FileStorageOptions { RootPath = root });

        _ = new FileManager(options);

        Directory.Exists(root).Should().BeTrue();

        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }

    [Fact]
    public async Task UploadFile_CreatesFile()
    {
        var content = new byte[] { 1, 2, 3, 4, 5 };
        var relativePath = "test/file.txt";

        await _fileManager.UploadFile(new MemoryStream(content), relativePath);

        var fullPath = Path.Combine(_storageRoot, relativePath);
        File.Exists(fullPath).Should().BeTrue();
        File.ReadAllBytes(fullPath).Should().BeEquivalentTo(content);
    }

    [Fact]
    public async Task UploadFile_CreatesSubdirectories()
    {
        var content = new byte[] { 1, 2, 3 };
        var relativePath = "2026/10/08/test.txt";

        await _fileManager.UploadFile(new MemoryStream(content), relativePath);

        var fullPath = Path.Combine(_storageRoot, relativePath);
        File.Exists(fullPath).Should().BeTrue();
    }

    [Fact]
    public async Task DownloadFile_ReturnsFileStream()
    {
        var content = new byte[] { 1, 2, 3, 4, 5 };
        var relativePath = "test/file.txt";
        await _fileManager.UploadFile(new MemoryStream(content), relativePath);

        var stream = await _fileManager.DownloadFile(relativePath);

        stream.Should().NotBeNull();
        using var memoryStream = new MemoryStream();
        await stream!.CopyToAsync(memoryStream);
        memoryStream.ToArray().Should().BeEquivalentTo(content);
    }

    [Fact]
    public async Task DownloadFile_Throws_WhenFileNotFound()
    {
        var act = () => _fileManager.DownloadFile("non-existent.txt");

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task DeleteFile_RemovesFile()
    {
        var relativePath = "test/file.txt";
        await _fileManager.UploadFile(new MemoryStream(new byte[] { 1 }), relativePath);

        await _fileManager.DeleteFile(relativePath);

        var fullPath = Path.Combine(_storageRoot, relativePath);
        File.Exists(fullPath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFile_DoesNotThrow_WhenFileNotFound()
    {
        var act = () => _fileManager.DeleteFile("non-existent.txt");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteFile_DoesNotThrow_WhenPathIsEmpty()
    {
        var act = () => _fileManager.DeleteFile("");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteFile_DoesNotThrow_WhenPathIsNull()
    {
        var act = () => _fileManager.DeleteFile(null!);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UploadFile_OverwritesExistingFile()
    {
        var relativePath = "test/file.txt";
        await _fileManager.UploadFile(new MemoryStream(new byte[] { 1, 2, 3 }), relativePath);

        var newContent = new byte[] { 4, 5, 6 };
        await _fileManager.UploadFile(new MemoryStream(newContent), relativePath);

        var fullPath = Path.Combine(_storageRoot, relativePath);
        File.ReadAllBytes(fullPath).Should().BeEquivalentTo(newContent);
    }
}
