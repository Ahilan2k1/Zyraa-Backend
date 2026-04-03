using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace MyShop.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5 MB;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly string[] _allowedContentTypes = { "image/jpeg", "image/png", "image/gif" ,"image/webp"};

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            if (!IsValidImage(file))
            {
                throw new InvalidOperationException("Invalid file type or size.");
            }

            var uploadPath = Path.Combine(_environment.WebRootPath, folder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            using(var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{folder}/{uniqueFileName}";
        }

        public Task<bool> DeleteFileAsync(string filePath)
{
    try
    {
        var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.FromResult(true);
    }
    catch
    {
        return Task.FromResult(false);
    }
}

        public long GetFileSize(IFormFile file)
        {
            return _maxFileSize;
        }

        public bool IsValidImage(IFormFile file)
        {
            if(file == null || file.Length == 0 || file.Length > _maxFileSize)
            {
                return false;
            }

            if(!_allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return false;
            }

            var extensions = Path.GetExtension(file.FileName).ToLowerInvariant();
            if(!_allowedExtensions.Contains(extensions))
            {
                return false;
            }

            return true;
        }

        public async Task<String> SaveFileWithThumbnailAsync(IFormFile file, string folder)
        {
            var filePath = await SaveFileAsync(file, folder);

            var uploadPath = Path.Combine(_environment.WebRootPath, folder);
            var originalPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
            var thumbnailPath = Path.Combine(uploadPath, $"thumb_{Path.GetFileName(filePath)}");

            using(var image = await Image.LoadAsync(originalPath))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(200, 200),
                    Mode = ResizeMode.Crop
                }));

                await image.SaveAsync(thumbnailPath);
            }

            return filePath;
        }

        public async Task<string> SaveOptimizedImageAsync(IFormFile file, string folder)
        {
            var uploadPath = Path.Combine(_environment.WebRootPath, folder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            using(var image = await Image.LoadAsync(file.OpenReadStream()))
            {
                if(image.Width > 1920 || image.Height > 1920)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(1024, 1024),
                        Mode = ResizeMode.Max
                    }));
                }

                var encoder = new JpegEncoder
                {
                    Quality = 75
                };

                await image.SaveAsync(filePath, encoder);
            };

            return $"/{folder}/{uniqueFileName}";
        }
    }
}