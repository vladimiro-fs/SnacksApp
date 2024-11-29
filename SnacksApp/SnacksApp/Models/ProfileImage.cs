namespace SnacksApp.Models
{
    public class ProfileImage
    {
        public string? ImageUrl { get; set; }

        public string? ImagePath => AppConfig.BaseUrl + ImageUrl;
    }
}
