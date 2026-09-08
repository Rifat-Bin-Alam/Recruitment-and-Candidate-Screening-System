using System.Text;
using DocumentFormat.OpenXml.Packaging;
using UglyToad.PdfPig;
using NPOI.XWPF.UserModel;

namespace RecruitmentAndCandidateScreeningSystem.Services;

public class CVTextExtractionService
{
    private readonly IWebHostEnvironment _environment;

    public CVTextExtractionService(
        IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> ExtractTextAsync(
        string relativeFilePath)
    {
        if (string.IsNullOrWhiteSpace(relativeFilePath))
        {
            throw new ArgumentException(
                "CV file path is empty.");
        }

        var secureRoot = Path.GetFullPath(
            Path.Combine(
                _environment.ContentRootPath,
                "SecureUploads"));

        var cleanRelativePath =
            relativeFilePath
                .Replace('/', Path.DirectorySeparatorChar)
                .TrimStart(
                    Path.DirectorySeparatorChar);

        var fullPath = Path.GetFullPath(
            Path.Combine(
                secureRoot,
                cleanRelativePath));

        if (!fullPath.StartsWith(
                secureRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException(
                "Invalid CV file path.");
        }

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                $"CV file was not found: {fullPath}");
        }

        var extension =
            Path.GetExtension(fullPath)
                .ToLowerInvariant();

        var text = extension switch
        {
            ".pdf" =>
                ExtractPdf(fullPath),

            ".docx" =>
                ExtractDocx(fullPath),

            _ =>
                throw new NotSupportedException(
                    $"CV format '{extension}' is not supported for AI processing.")
        };

        await Task.CompletedTask;

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException(
                "The CV file was found, but no readable text could be extracted.");
        }

        return text;
    }

    private static string ExtractPdf(
        string filePath)
    {
        var text = new StringBuilder();

        using var document =
            PdfDocument.Open(filePath);

        foreach (var page in document.GetPages())
        {
            text.AppendLine(page.Text);
        }

        return CleanText(text.ToString());
    }

    private static string ExtractDocx(
        string filePath)
    {
        var text = new StringBuilder();

        using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        using var document =
            WordprocessingDocument.Open(
                stream,
                false);

        var body =
            document.MainDocumentPart?
                .Document
                .Body;

        if (body != null)
        {
            foreach (var paragraph in body.Descendants<
                         DocumentFormat.OpenXml.Wordprocessing.Text>())
            {
                text.AppendLine(paragraph.Text);
            }
        }

        return CleanText(text.ToString());
    }

    private static string CleanText(
        string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var lines = text
            .Split(
                new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line =>
                !string.IsNullOrWhiteSpace(line));

        return string.Join(
            Environment.NewLine,
            lines);
    }
}