namespace Tillegal.Application.Features.Attachments.Common;

public static class FileSignatureValidator
{
    private static readonly byte[] PdfMagic = "%PDF"u8.ToArray();
    private static readonly byte[] PngMagic = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] JpegMagic = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] ZipDocxMagic = [0x50, 0x4B, 0x03, 0x04];
    private static readonly byte[] DocOleMagic = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1];
    private static readonly byte[] RiffMagic = "RIFF"u8.ToArray();
    private static readonly byte[] WebpMagic = "WEBP"u8.ToArray();

    public static bool IsValidSignature(Stream stream, string? extension)
    {
        if (stream is null || !stream.CanRead || string.IsNullOrWhiteSpace(extension))
        {
            return false;
        }

        var ext = extension.Trim().ToLowerInvariant();
        if (!ext.StartsWith('.'))
        {
            ext = "." + ext;
        }

        var originalPosition = stream.CanSeek ? stream.Position : 0;
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        var buffer = new byte[512];
        var bytesRead = stream.Read(buffer, 0, buffer.Length);

        if (stream.CanSeek)
        {
            stream.Position = originalPosition;
        }

        if (bytesRead == 0)
        {
            return false;
        }

        return ext switch
        {
            ".pdf" => bytesRead >= PdfMagic.Length
                && buffer.AsSpan(0, PdfMagic.Length).SequenceEqual(PdfMagic),

            ".png" => bytesRead >= PngMagic.Length
                && buffer.AsSpan(0, PngMagic.Length).SequenceEqual(PngMagic),

            ".jpg" or ".jpeg" => bytesRead >= JpegMagic.Length
                && buffer.AsSpan(0, JpegMagic.Length).SequenceEqual(JpegMagic),

            ".docx" => bytesRead >= ZipDocxMagic.Length
                && buffer.AsSpan(0, ZipDocxMagic.Length).SequenceEqual(ZipDocxMagic),

            ".doc" => bytesRead >= DocOleMagic.Length
                && buffer.AsSpan(0, DocOleMagic.Length).SequenceEqual(DocOleMagic),

            ".webp" => bytesRead >= 12
                && buffer.AsSpan(0, 4).SequenceEqual(RiffMagic)
                && buffer.AsSpan(8, 4).SequenceEqual(WebpMagic),

            ".txt" => IsValidPlainText(buffer.AsSpan(0, bytesRead)),

            _ => false
        };
    }

    private static bool IsValidPlainText(ReadOnlySpan<byte> span)
    {
        // Reject common executable signatures disguised as text
        if (span.Length >= 2 && span[0] == 0x4D && span[1] == 0x5A) // MZ (Windows executable)
        {
            return false;
        }

        if (span.Length >= 4 && span[0] == 0x7F && span[1] == 0x45 && span[2] == 0x4C && span[3] == 0x46) // ELF
        {
            return false;
        }

        // Check for null bytes indicating binary content
        foreach (var b in span)
        {
            if (b == 0x00)
            {
                return false;
            }
        }

        return true;
    }
}

