using SnacksApp.Models;
using SnacksApp.Services;
using SnacksApp.Validations;

namespace SnacksApp.Pages;

public partial class FavoritesPage : ContentPage
{
	private readonly ApiService _apiService;
	private readonly FavoritesService _favoritesService;
	private readonly IValidator _validator;

	public FavoritesPage(ApiService apiService, FavoritesService favoritesService,
						IValidator validator)
	{
		InitializeComponent();
		_apiService = apiService;
		_favoritesService = favoritesService;
		_validator = validator;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
		await GetFavoriteProducts();
    }

    private async Task GetFavoriteProducts()
    {
		try
		{
			var favoriteProducts = await _favoritesService.ReadAllAsync();

			if (favoriteProducts == null || favoriteProducts.Count == 0)
			{
				cvProducts.ItemsSource = null;
				lblWarning.IsVisible = true;
			}
			else 
			{ 
				cvProducts.ItemsSource = favoriteProducts;
				lblWarning.IsVisible = false;
			}
		}
		catch (Exception ex) 
		{
			await DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
		}
    }

    private void cvProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
		var currentSelection = e.CurrentSelection.FirstOrDefault() as FavoriteProduct;

		if (currentSelection == null) 
		{
			return;
		}

		Navigation.PushAsync(new ProductDetailsPage(currentSelection.ProductId, currentSelection.Name!,
													_apiService, _favoritesService, _validator));

		((CollectionView)sender).SelectedItem = null;
    }
}