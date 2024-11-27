namespace SnacksApp
{
    using SnacksApp.Pages;
    using SnacksApp.Services;
    using SnacksApp.Validations;

    public partial class AppShell : Shell
    {
        private readonly ApiService _apiService;
        private readonly FavoritesService _favoritesService;
        private readonly IValidator _validator;

        public AppShell(ApiService apiService, FavoritesService favoritesService, 
                        IValidator validator)
        {
            InitializeComponent();
            _apiService = apiService;
            _favoritesService = favoritesService;
            _validator = validator;

            ConfigureShell();
        }

        private void ConfigureShell()
        {
            var homePage = new HomePage(_apiService, _favoritesService, _validator);
            var cartPage = new CartPage(_apiService, _favoritesService, _validator);
            var favoritesPage = new FavoritesPage(_apiService, _favoritesService, _validator);
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
