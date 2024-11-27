namespace SnacksApp.Models
{
    using SQLite;

    public class FavoriteProduct
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsFavorite { get; set; }
    }
}
