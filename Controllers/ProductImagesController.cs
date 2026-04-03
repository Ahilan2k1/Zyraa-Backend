using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Models;
using MyShop.Services;
using Microsoft.AspNetCore.Http;

namespace MyShop.Controllers
{
    [ApiController]
    [Route("api/products/{productId}/images")]
    public class ProductImagesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFileService _fileService;

        public ProductImagesController(AppDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        /// <summary>
        /// Get all images for a product
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetProductImages(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            return Ok(product.Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.UploadedAt));
        }

        /// <summary>
        /// Upload single image for a product
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> UploadImage(int productId, IFormFile file)
        {
            // 1. Check if product exists
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            // 2. Validate file
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file provided" });
            }

            if (!_fileService.IsValidImage(file))
            {
                return BadRequest(new 
                { 
                    message = "Invalid file. Only JPG, PNG, GIF, WEBP under 5MB allowed" 
                });
            }

            try
            {
                // 3. Save file
                var filePath = await _fileService.SaveFileAsync(file, "uploads/products");

                // 4. Create database record
                var productImage = new ProductImage
                {
                    ProductId = productId,
                    FileName = file.FileName,
                    FilePath = filePath,
                    ContentType = file.ContentType,
                    IsPrimary = false,  // Set manually via another endpoint
                    UploadedAt = DateTime.UtcNow
                };

                _context.ProductImages.Add(productImage);
                await _context.SaveChangesAsync();

                return Ok(productImage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error uploading file", error = ex.Message });
            }
        }

        /// <summary>
        /// Upload multiple images for a product
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("multiple")]
        public async Task<ActionResult> UploadMultipleImages(int productId, List<IFormFile> files)
        {
            // 1. Check if product exists
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            // 2. Validate files
            if (files == null || files.Count == 0)
            {
                return BadRequest(new { message = "No files provided" });
            }

            if (files.Count > 10)
            {
                return BadRequest(new { message = "Maximum 10 images allowed" });
            }

            var uploadedImages = new List<ProductImage>();
            var errors = new List<string>();

            // 3. Upload each file
            foreach (var file in files)
            {
                if (!_fileService.IsValidImage(file))
                {
                    errors.Add($"{file.FileName}: Invalid file type or size");
                    continue;
                }

                try
                {
                    var filePath = await _fileService.SaveFileAsync(file, "uploads/products");

                    var productImage = new ProductImage
                    {
                        ProductId = productId,
                        FileName = file.FileName,
                        FilePath = filePath,
                        ContentType = file.ContentType,
                        IsPrimary = false,
                        UploadedAt = DateTime.UtcNow
                    };

                    _context.ProductImages.Add(productImage);
                    uploadedImages.Add(productImage);
                }
                catch (Exception ex)
                {
                    errors.Add($"{file.FileName}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                uploaded = uploadedImages,
                errors = errors
            });
        }

        /// <summary>
        /// Set primary image for a product
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{imageId}/set-primary")]
        public async Task<ActionResult> SetPrimaryImage(int productId, int imageId)
        {
            var image = await _context.ProductImages
                .FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == productId);

            if (image == null)
            {
                return NotFound(new { message = "Image not found" });
            }

            // Remove primary from all other images
            var otherImages = await _context.ProductImages
                .Where(i => i.ProductId == productId && i.Id != imageId)
                .ToListAsync();

            foreach (var img in otherImages)
            {
                img.IsPrimary = false;
            }

            // Set this as primary
            image.IsPrimary = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Primary image updated", image });
        }

        /// <summary>
        /// Delete an image
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{imageId}")]
        public async Task<ActionResult> DeleteImage(int productId, int imageId)
        {
            var image = await _context.ProductImages
                .FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == productId);

            if (image == null)
            {
                return NotFound(new { message = "Image not found" });
            }

            // Delete file from disk
            await _fileService.DeleteFileAsync(image.FilePath);

            // Delete from database
            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Image deleted successfully" });
        }
    }
}