namespace SnacksApp
{
    using SnacksApp.Pages;
    using SnacksApp.Services;
    using SnacksApp.Validations;

    public partial class AppShell : Shell
    {
        private readonly ApiService _apiService;
        private readonly IValidator _validator;

        public AppShell(ApiService apiService, IValidator validator)
        {
            InitializeComponent();
            _apiService = apiService;
            _validator = validator;

            ConfigureShell();
        }

        private void ConfigureShell()
        {
            var homePage = new HomePage(_apiService, _validator);
            var cartPage = new CartPage();
            var favoritesPage = new FavoritesPage();
            var profilePage = new ProfilePage();

            Items.Add(new TabBar
            {
                Items =
                {
                    new ShellContent { Title = "Home", Icon = "home", Content = homePage },
                    new ShellContent { Title = "Cart", Icon = "cart", Content = cartPage },
                    new ShellContent { Title = "Favorites", Icon = "heart", Content = favoritesPage },
                    new ShellContent { Title = "Profile", Icon = "profile", Content = profilePage },
                }
            });
        }
    }
}
