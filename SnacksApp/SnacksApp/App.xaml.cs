namespace SnacksApp
{   
    using SnacksApp.Pages;
    using SnacksApp.Services;
    using SnacksApp.Validations;

    public partial class App : Application
    {
        private readonly ApiService _apiService;
        private readonly FavoritesService _favoritesService;
        private readonly IValidator _validator;

        public App(ApiService apiService, FavoritesService favoritesService,
                    IValidator validator)
        {
            InitializeComponent();
            _apiService = apiService;
            _favoritesService = favoritesService;
            _validator = validator;

            SetMainPage();
        }

        private void SetMainPage() 
        { 
            var accessToken = Preferences.Get("accessToken", string.Empty);

            if (string.IsNullOrEmpty(accessToken)) 
            {
                MainPage = new NavigationPage(new LoginPage(_apiService, _favoritesService, _validator));
                return;
            }

            MainPage = new AppShell(_apiService, _favoritesService, _validator);
        }
    }
}
