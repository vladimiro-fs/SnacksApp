namespace SnacksApp.Pages;

using System;
using System.Threading.Tasks;
using SnacksApp.Models;
using SnacksApp.Services;
using SnacksApp.Validations;

public partial class ProductsListPage : ContentPage
{
	private readonly ApiService _apiService;
	private readonly FavoritesService _favoritesService;
	private readonly IValidator _validator;
	private int _categoryId;
	private bool _loginPageDisplayed = false;

	public ProductsListPage(int categoryId, string categoryName,
							ApiService apiService, FavoritesService favoritesService, 
							IValidator validator)
	{
		InitializeComponent();
		_categoryId = categoryId;
		_apiService = apiService;
		_favoritesService = favoritesService;
		_validator = validator;
		Title = categoryName ?? "Products";
	}

	protected override async void OnAppearing() 
	{ 
		base.OnAppearing();
        await GetProductsList(_categoryId);
	}

    private async Task<IEnumerable<Product>> GetProductsList(int categoryId)
    {
        try 
		{ 
			var (products, errorMessage) = await _apiService.GetProducts("category", categoryId.ToString());

			if (errorMessage == "Unauthorized" && !_loginPageDisplayed) 
			{
				await DisplayLoginPage();
				return Enumerable.Empty<Product>();
			}

			if (products == null) 
			{
				await DisplayAlert("Error", errorMessage ?? "Unable to get categories", "OK");
				return Enumerable.Empty<Product>();
			}

			cvProducts.ItemsSource = products;
			return products;
		}
		catch (Exception ex) 
		{ 
			await DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
			return Enumerable.Empty<Product>();
		}
    }

	private async Task DisplayLoginPage() 
	{ 
		_loginPageDisplayed = true;
		await Navigation.PushAsync(new LoginPage(_apiService, _favoritesService, _validator));
	}

    private void cvProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
		var currentSelection = e.CurrentSelection.FirstOrDefault() as Product;

		if (currentSelection == null) 
		{ 
			return;
		}

		Navigation.PushAsync(new ProductDetailsPage(currentSelection.Id, currentSelection.Name!,
													_apiService, _favoritesService, _validator));

		((CollectionView)sender).SelectedItem = null;
    }
}