using DevDiary.Enums;
using DevDiary.Services.Interfaces;

namespace DevDiary.Services
{
    public class ImageService : IImageService
    {
        private readonly string? _defaultBlogImage = "~/img/MicrosoftTeams-image (2).png";
        private readonly string? _defaultUserImage = "~/img/MicrosoftTeams-image (2).png";
        private readonly string? _defaultCategoryImage = "~/img/MicrosoftTeams-image (2).png";
        private readonly string? _blogAuthorImage = "~/img/MicrosoftTeams-image (2).png";

        public string? ConvertByteArrayToFile(byte[]? fileData, string? extension, DefaultImage defaultImage)
        {
            try
            {
                if (fileData == null || fileData.Length == 0)
                {
                    switch (defaultImage)
                    {
                        case DefaultImage.AuthorImage: return _blogAuthorImage;
                        case DefaultImage.BlogPostImage: return _defaultBlogImage;
                        case DefaultImage.BlogUserImage: return _defaultUserImage;
                        case DefaultImage.CategoryImage: return _defaultCategoryImage;
                    }
                }
                string? imageBase64Data = Convert.ToBase64String(fileData!);
                imageBase64Data = string.Format($"data:{extension};base64,{imageBase64Data}");

                return imageBase64Data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile? file)
        {
            try
            {
                using MemoryStream memoryStream = new MemoryStream();
                await file!.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();
                memoryStream.Close();
                return byteFile;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
