using Microsoft.AspNetCore.Http;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrahamSchoolAdminSystemAccess
{
    public class FileUploadHandler
    {
        // Allowed file extensions
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pneg", ".pdf" };

        // Image extensions that need compression
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".pneg" };

        // Maximum file size (10MB)
        private const long MaxFileSize = 10 * 1024 * 1024;

        /// <summary>
        /// Validates if the uploaded file is acceptable
        /// </summary>
        public static ValidationResult ValidateFile(IFormFile file)
        {
            var result = new ValidationResult();

            // Check if file exists
            if (file == null || file.Length == 0)
            {
                result.IsValid = false;
                result.ErrorMessage = "No file uploaded.";
                return result;
            }

            // Check if file name is empty
            if (string.IsNullOrWhiteSpace(file.FileName))
            {
                result.IsValid = false;
                result.ErrorMessage = "File name cannot be empty.";
                return result;
            }

            // Get file extension
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            // Check if extension is allowed
            if (!AllowedExtensions.Contains(extension))
            {
                result.IsValid = false;
                result.ErrorMessage = $"Invalid file type. Only {string.Join(", ", AllowedExtensions)} files are allowed.";
                return result;
            }

            // Check file size
            if (file.Length > MaxFileSize)
            {
                result.IsValid = false;
                result.ErrorMessage = $"File size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)}MB.";
                return result;
            }

            result.IsValid = true;
            result.ErrorMessage = "File is valid.";
            return result;
        }

        /// <summary>
        /// Processes the uploaded file - compresses images or copies PDF
        /// Generates a unique filename and returns the full path
        /// </summary>
        /// <param name="file">The uploaded file</param>
        /// <param name="destinationPath">Directory path to save the file</param>
        /// <returns>ProcessResult with the full file path including unique filename</returns>
        public static ProcessResult ProcessFile(IFormFile file, string destinationPath)
        {
            var result = new ProcessResult();

            try
            {
                // Validate file first
                var validation = ValidateFile(file);
                if (!validation.IsValid)
                {
                    result.Success = false;
                    result.Message = validation.ErrorMessage;
                    return result;
                }

                // Ensure output directory exists
                Directory.CreateDirectory(destinationPath);

                // Generate unique filename
                string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                string originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
                string uniqueFileName = GenerateUniqueFileName(originalFileName, extension);
                string fullPath = Path.Combine(destinationPath, uniqueFileName);

                // Open file stream
                using (var fileStream = file.OpenReadStream())
                {
                    // Check if it's an image that needs compression
                    if (ImageExtensions.Contains(extension))
                    {
                        result = CompressImage(fileStream, fullPath, extension);
                    }
                    // If it's a PDF, just copy it
                    else if (extension == ".pdf")
                    {
                        result = CopyPdfFile(fileStream, fullPath);
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = "Unsupported file type.";
                        return result;
                    }
                }

                // Set the output file path to the unique filename path
                if (result.Success)
                {
                    result.OutputFilePath = fullPath;
                    result.FileName = uniqueFileName;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error processing file: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Generates a unique filename using GUID
        /// </summary>
        private static string GenerateUniqueFileName(string originalFileName, string extension)
        {
            // Sanitize original filename (remove invalid characters)
            string sanitizedName = SanitizeFileName(originalFileName);

            // Generate unique identifier
            string uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);

            // Combine: originalname_uniqueid.extension
            return $"{sanitizedName}_{uniqueId}{extension}";
        }

        /// <summary>
        /// Sanitizes filename by removing invalid characters
        /// </summary>
        private static string SanitizeFileName(string fileName)
        {
            // Get invalid characters
            char[] invalidChars = Path.GetInvalidFileNameChars();

            // Replace invalid characters with underscore
            string sanitized = new string(fileName.Select(ch =>
                invalidChars.Contains(ch) ? '_' : ch
            ).ToArray());

            // Limit length to 50 characters
            if (sanitized.Length > 50)
            {
                sanitized = sanitized.Substring(0, 50);
            }

            return sanitized;
        }

        /// <summary>
        /// Compresses image file
        /// </summary>
        private static ProcessResult CompressImage(Stream fileStream, string outputPath, string extension)
        {
            var result = new ProcessResult();

            try
            {
                using (var image = Image.FromStream(fileStream))
                {
                    // Get the image codec info for the specified format
                    ImageCodecInfo codec = GetEncoderInfo(extension);

                    if (codec == null)
                    {
                        result.Success = false;
                        result.Message = "Could not find encoder for the specified image format.";
                        return result;
                    }

                    // Set compression quality (70% quality for good balance)
                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 70L);

                    // Save compressed image
                    image.Save(outputPath, codec, encoderParameters);

                    // Get file sizes for comparison
                    var originalSize = fileStream.Length;
                    var compressedSize = new FileInfo(outputPath).Length;
                    var compressionRatio = ((double)(originalSize - compressedSize) / originalSize) * 100;

                    result.Success = true;
                    result.Message = $"Image compressed successfully. Original: {FormatFileSize(originalSize)}, Compressed: {FormatFileSize(compressedSize)}, Saved: {compressionRatio:F2}%";
                    result.OutputFilePath = outputPath;
                    result.OriginalSize = originalSize;
                    result.ProcessedSize = compressedSize;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error compressing image: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Copies PDF file without compression
        /// </summary>
        private static ProcessResult CopyPdfFile(Stream fileStream, string outputPath)
        {
            var result = new ProcessResult();

            try
            {
                using (var fileStreamOutput = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    fileStream.CopyTo(fileStreamOutput);
                }

                var fileSize = new FileInfo(outputPath).Length;

                result.Success = true;
                result.Message = $"PDF file copied successfully. Size: {FormatFileSize(fileSize)}";
                result.OutputFilePath = outputPath;
                result.OriginalSize = fileSize;
                result.ProcessedSize = fileSize;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error copying PDF: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Gets the encoder info for the specified image extension
        /// </summary>
        private static ImageCodecInfo GetEncoderInfo(string extension)
        {
            string mimeType;

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    mimeType = "image/jpeg";
                    break;
                case ".png":
                case ".pneg":
                    mimeType = "image/png";
                    break;
                default:
                    return null;
            }

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            return codecs.FirstOrDefault(codec => codec.MimeType == mimeType);
        }

        /// <summary>
        /// Formats file size to human-readable format
        /// </summary>
        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }

    /// <summary>
    /// Validation result model
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Process result model
    /// </summary>
    public class ProcessResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string OutputFilePath { get; set; }
        public string FileName { get; set; }
        public long OriginalSize { get; set; }
        public long ProcessedSize { get; set; }
    }
}
