using System.Data;

namespace Letter_Request_Management_System.Services
{
    public class AdminSignatureFileService
    {
        private readonly string _signatureFolder;

        public AdminSignatureFileService(IWebHostEnvironment environment)
        {
            _signatureFolder = Path.Combine(
                environment.ContentRootPath,
                "App_Data",
                "AdminSignatures"
            );

            Directory.CreateDirectory(_signatureFolder);
        }

        public async Task<string> SaveSignatureAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Please select a signature image.");
            }

            // Maximum file size: 2 MB
            if (file.Length > 2 * 1024 * 1024)
            {
                throw new ArgumentException(
                    "The signature image must not exceed 2 MB."
                );
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (extension != ".png" &&
                extension != ".jpg" &&
                extension != ".jpeg")
            {
                throw new ArgumentException(
                    "Only PNG and JPG signature images are allowed."
                );
            }

            // Check the file's actual image signature, not only its extension.
            byte[] header = new byte[8];

            await using (var input = file.OpenReadStream())
            {
                int bytesRead = await input.ReadAsync(header.AsMemory(0, 8));

                bool isPng = bytesRead >= 8 &&
                    header[0] == 0x89 &&
                    header[1] == 0x50 &&
                    header[2] == 0x4E &&
                    header[3] == 0x47 &&
                    header[4] == 0x0D &&
                    header[5] == 0x0A &&
                    header[6] == 0x1A &&
                    header[7] == 0x0A;

                bool isJpeg = bytesRead >= 3 &&
                    header[0] == 0xFF &&
                    header[1] == 0xD8 &&
                    header[2] == 0xFF;

                if (!isPng && !isJpeg)
                {
                    throw new ArgumentException(
                        "The selected file is not a valid PNG or JPG image."
                    );
                }
            }

            // Generate our own filename; never trust the uploaded filename.
            string fileName = $"{Guid.NewGuid():N}{extension}";
            string fullPath = Path.Combine(_signatureFolder, fileName);

            await using (var output = new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None))
            {
                await file.CopyToAsync(output);
            }

            // Store this relative reference in SQL Server.
            return Path.Combine("App_Data", "AdminSignatures", fileName)
                .Replace("\\", "/");
        }

        public string? GetSignatureFullPath(string? signatureReference)
        {
            if (string.IsNullOrWhiteSpace(signatureReference))
            {
                return null;
            }

            // Normalize Windows and URL-style paths.
            string normalizedReference = signatureReference.Replace('\\', '/');
            string fileName = Path.GetFileName(normalizedReference);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            // First check the folder used by this service.
            string fullPath = Path.GetFullPath(Path.Combine(_signatureFolder, fileName));
            string folderPath = Path.GetFullPath(_signatureFolder)+ Path.DirectorySeparatorChar;

            if (!fullPath.StartsWith(folderPath,StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (File.Exists(fullPath))
            {
                return fullPath;
            }

            // Check the legacy "signatures" folder.
            string legacyFolder = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),"signatures"));
            string legacyPath = Path.GetFullPath(Path.Combine(legacyFolder, fileName));
            string legacyFolderPrefix = legacyFolder + Path.DirectorySeparatorChar;

            if (legacyPath.StartsWith(
                    legacyFolderPrefix,
                    StringComparison.OrdinalIgnoreCase)
                && File.Exists(legacyPath))
            {
                return legacyPath;
            }

            return null;
        }
    }
}
