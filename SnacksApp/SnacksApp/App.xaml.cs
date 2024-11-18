﻿namespace SnacksApp
{
    using SnacksApp.Pages;
    using SnacksApp.Services;

    public partial class App : Application
    {
        private readonly ApiService _apiService;

        public App(ApiService apiService)
        {
            InitializeComponent();

            _apiService = apiService;
            MainPage = new NavigationPage(new RegisterPage(_apiService));
        }
    }
}