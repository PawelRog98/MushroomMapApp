using FluentAssertions;
using MushroomMapApp.Infrastructure.Services.FileStorage;
using Xunit;

namespace MushroomMap.UnitTests.Infrastructure.Services.FileStorage;

public class FilePathGeneratorTests
{
    private readonly FilePathGenerator _generator = new();

    [Fact]
    public void GenerateFilePath_ReturnsPathWithExtension()
    {
        var path = _generator.GenerateFilePath("jpg");

        path.Should().EndWith(".jpg");
    }

    [Fact]
    public void GenerateFilePath_ContainsDateFolder()
    {
        var now = DateTime.UtcNow;
        var path = _generator.GenerateFilePath("png");

        path.Should().Contain(now.Year.ToString());
        path.Should().Contain(now.Month.ToString("D2"));
        path.Should().Contain(now.Day.ToString("D2"));
    }

    [Fact]
    public void GenerateFilePath_ContainsGuid()
    {
        var path = _generator.GenerateFilePath("jpg");

        var fileName = Path.GetFileNameWithoutExtension(path);
        Guid.TryParse(fileName, out _).Should().BeTrue();
    }

    [Fact]
    public void GenerateFilePath_WithSuffix_AppendsSuffix()
    {
        var path = _generator.GenerateFilePath("jpg", "_thumb");

        var fileName = Path.GetFileNameWithoutExtension(path);
        fileName.Should().EndWith("_thumb");
    }

    [Fact]
    public void GenerateFilePath_WithSubFolder_PrependsSubFolder()
    {
        var path = _generator.GenerateFilePath("jpg", null, "thumbnails");

        path.Should().StartWith("thumbnails");
    }

    [Fact]
    public void GenerateFilePath_WithSuffixAndSubFolder_CombinesBoth()
    {
        var path = _generator.GenerateFilePath("jpg", "_thumb", "thumbnails");

        path.Should().StartWith("thumbnails");
        var fileName = Path.GetFileNameWithoutExtension(path);
        fileName.Should().EndWith("_thumb");
    }

    [Fact]
    public void GenerateFilePath_GeneratesUniquePaths()
    {
        var path1 = _generator.GenerateFilePath("jpg");
        var path2 = _generator.GenerateFilePath("jpg");

        path1.Should().NotBe(path2);
    }

    [Fact]
    public void GenerateFilePath_NullSuffix_AppendsNoSuffix()
    {
        var path = _generator.GenerateFilePath("jpg", null);

        var fileName = Path.GetFileNameWithoutExtension(path);
        fileName.Should().NotContain("_");
    }

    [Fact]
    public void GenerateFilePath_NullSubFolder_NoSubFolder()
    {
        var path = _generator.GenerateFilePath("jpg", null, null);

        var folder = Path.GetDirectoryName(path);
        folder.Should().NotContain("thumbnails");
    }
}
