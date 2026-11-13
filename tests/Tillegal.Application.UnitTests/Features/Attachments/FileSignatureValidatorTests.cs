using FluentAssertions;
using Tillegal.Application.Features.Attachments.Common;
using Xunit;

namespace Tillegal.Application.UnitTests.Features.Attachments;

public sealed class FileSignatureValidatorTests
{
    [Fact]
    public void IsValidSignature_WhenPdfIsValid_ShouldReturnTrue()
    {
        using var stream = new MemoryStream("%PDF-1.4 sample content"u8.ToArray());
        var result = FileSignatureValidator.IsValidSignature(stream, ".pdf");
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidSignature_WhenPdfIsMalformed_ShouldReturnFalse()
    {
        using var stream = new MemoryStream([1, 2, 3, 4]);
        var result = FileSignatureValidator.IsValidSignature(stream, ".pdf");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidSignature_WhenExeDisguisedAsPdf_ShouldReturnFalse()
    {
        var fakePdf = new byte[] { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00 }; // MZ
        using var stream = new MemoryStream(fakePdf);
        var result = FileSignatureValidator.IsValidSignature(stream, ".pdf");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidSignature_WhenPngIsValid_ShouldReturnTrue()
    {
        var pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00 };
        using var stream = new MemoryStream(pngBytes);
        var result = FileSignatureValidator.IsValidSignature(stream, ".png");
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidSignature_WhenJpegIsValid_ShouldReturnTrue()
    {
        var jpegBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 };
        using var stream = new MemoryStream(jpegBytes);
        var result = FileSignatureValidator.IsValidSignature(stream, ".jpg");
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidSignature_WhenDocxIsValid_ShouldReturnTrue()
    {
        var docxBytes = new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x14, 0x00 };
        using var stream = new MemoryStream(docxBytes);
        var result = FileSignatureValidator.IsValidSignature(stream, ".docx");
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidSignature_WhenDocOleIsValid_ShouldReturnTrue()
    {
        var docBytes = new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };
        using var stream = new MemoryStream(docBytes);
        var result = FileSignatureValidator.IsValidSignature(stream, ".doc");
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidSignature_WhenWebpIsValid_ShouldReturnTrue()
    {
        var webpBytes = new byte[]
        {
            0x52, 0x49, 0x46, 0x46, // RIFF
            0x24, 0x00, 0x00, 0x00, // length
            0x57, 0x45, 0x42, 0x50  // WEBP
        };
        using var stream = new MemoryStream(webpBytes);
        var result = FileSignatureValidator.IsValidSignature(stream, ".webp");
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidSignature_WhenTxtIsPlainText_ShouldReturnTrue()
    {
        using var stream = new MemoryStream("This is regular plain text."u8.ToArray());
        var result = FileSignatureValidator.IsValidSignature(stream, ".txt");
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidSignature_WhenTxtContainsNullBytes_ShouldReturnFalse()
    {
        var binaryText = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x00, 0x57, 0x6F, 0x72, 0x6C, 0x64 };
        using var stream = new MemoryStream(binaryText);
        var result = FileSignatureValidator.IsValidSignature(stream, ".txt");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidSignature_WhenTxtIsExecutable_ShouldReturnFalse()
    {
        var exeText = new byte[] { 0x4D, 0x5A, 0x90, 0x00 }; // MZ
        using var stream = new MemoryStream(exeText);
        var result = FileSignatureValidator.IsValidSignature(stream, ".txt");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidSignature_WhenStreamIsEmpty_ShouldReturnFalse()
    {
        using var stream = new MemoryStream([]);
        var result = FileSignatureValidator.IsValidSignature(stream, ".pdf");
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidSignature_WhenExtensionIsUnsupported_ShouldReturnFalse()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        var result = FileSignatureValidator.IsValidSignature(stream, ".exe");
        result.Should().BeFalse();
    }
}

