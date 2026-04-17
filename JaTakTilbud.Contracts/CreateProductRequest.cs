namespace JaTakTilbud.Contracts;

/// <summary>
/// Request model used when creating a new product from admin client.
/// </summary>
public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public byte[]? ImageBlob { get; set; }
}