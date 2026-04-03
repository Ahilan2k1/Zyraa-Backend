namespace MyShop.Models;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
    public DateTime UploadedAt { get; set; } = DateTime.Now;
    public Product? Product { get; set; } = null;
}