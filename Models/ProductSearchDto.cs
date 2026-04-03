// Models/DTOs/ProductSearchDto.cs
public class ProductSearchDto
{
    // Free text search on name and description
    public string? Search { get; set; }

    // Filter by category
    public int? CategoryId { get; set; }

    // Price range filter
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    // Rating filter — e.g. only 4+ star products
    public double? MinRating { get; set; }

    // Only show in-stock products
    public bool? InStock { get; set; }

    // Sorting options
    public string SortBy { get; set; } = "newest"; // newest, price-asc, price-desc, rating, popular

    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12; // 12 products per page
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}