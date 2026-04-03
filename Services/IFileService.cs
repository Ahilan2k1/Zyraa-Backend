using Microsoft.AspNetCore.Http;

namespace MyShop.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string filePath);
        bool IsValidImage(IFormFile file);
        long GetFileSize(IFormFile file);
        Task<string> SaveFileWithThumbnailAsync(IFormFile file, string folder);
        Task<string> SaveOptimizedImageAsync(IFormFile file, string folder);
    }
}