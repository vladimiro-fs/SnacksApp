namespace SnacksApp.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? ImageUrl { get; set; }

        public string? ImagePath => AppConfig.BaseUrl + ImageUrl;
    }
}
