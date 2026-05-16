using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Interviewzwt.Backend.Interfaces;
using UglyToad.PdfPig;

namespace Interviewzwt.Backend.Services
{
    public class FileService : IFileService
    {
        public async Task<string> ExtractTextFromFile(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            if (extension == ".pdf")
            {
                return ExtractTextFromPdf(stream);
            }
            else if (extension == ".docx")
            {
                return ExtractTextFromDocx(stream);
            }
            else if (extension == ".txt")
            {
                stream.Position = 0;
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }

            throw new Exception("Unsupported file format");
        }

        private string ExtractTextFromPdf(Stream stream)
        {
            using var document = PdfDocument.Open(stream);
            var text = "";
            foreach (var page in document.GetPages())
            {
                text += page.Text + "\n";
            }
            return text.Trim();
        }

        private string ExtractTextFromDocx(Stream stream)
        {
            using var wordDocument = WordprocessingDocument.Open(stream, false);
            var body = wordDocument.MainDocumentPart?.Document.Body;
            return body?.InnerText ?? "";
        }
    }
}
