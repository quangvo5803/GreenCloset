using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Utility.Media
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly IConfiguration _configuration;

        public CloudinaryService(IConfiguration configuration)
        {
            _configuration = configuration;
            var account = new Account(
                _configuration["Cloudinary:CloudName"],
                _configuration["Cloudinary:ApiKey"],
                _configuration["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task UploadImageAsync(IFormFile file, string folder, ItemImage itemImage)
        {
            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false,
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            itemImage.PublicId = result.PublicId;
            itemImage.ImagePath = result.SecureUrl.ToString();
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);
            return result.Result == "ok" || result.Result == "not found";
        }
    }
}
