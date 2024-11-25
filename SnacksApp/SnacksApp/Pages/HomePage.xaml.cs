namespace SnacksApp.Pages;

using SnacksApp.Models;
using SnacksApp.Services;
using SnacksApp.Validations;

public partial class HomePage : ContentPage
{
	private readonly ApiService _apiService;
	private readonly IValidator _validator;
	private bool _loginPageDisplayed = false;

	public HomePage(ApiService apiService, IValidator validator)
	{
		InitializeComponent();
		lblUserName.Text = "Hello, " + Preferences.Get("username", string.Empty);
		_apiService = apiService;
		_validator = validator;
		Title = AppConfig.HomePageTitle;
	}

	protected override async void OnAppearing() 
	{ 
		base.OnAppearing();

		await GetCategoriesList();
		await GetBestSellers();
		await GetMostPopular();
	}

	private async Task<IEnumerable<Category>> GetCategoriesList() 
	{ 
		try 
		{
			var (categories, errorMessage) = await _apiService.GetCategories();

			if (errorMessage == "Unauthorized" && !_loginPageDisplayed) 
			{
				await DisplayLoginPage();
				return Enumerable.Empty<Category>();
			}

			if (categories == null) 
			{
				await DisplayAlert("Error", errorMessage ?? "Unable to get categories.", "OK");
				return Enumerable.Empty<Category>();
			}

			cvCategorias.ItemsSource = categories;

			return categories;
		}
		catch (Exception ex) 
		{
			await DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
			return Enumerable.Empty<Category>();
		}
	} 

	private async Task<IEnumerable<Product>> GetBestSellers() 
	{
		try
		{
			var (products, errorMessage) = await _apiService.GetProducts("bestSellers", string.Empty);

			if (errorMessage == "Unauthorized" && !_loginPageDisplayed) 
			{
				await DisplayLoginPage();
				return Enumerable.Empty<Product>();
			}

			if (products == null) 
			{
				await DisplayAlert("Error", errorMessage ?? "Unable to get products", "OK");
				return Enumerable.Empty<Product>(); 
			}

			cvBestSellers.ItemsSource = products;

			return products;
		}
		catch (Exception ex) 
		{
			await DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
			return Enumerable.Empty<Product>();
		}
	}

	private async Task<IEnumerable<Product>> GetMostPopular() 
	{
		try
		{
			var (products, errorMessage) = await _apiService.GetProducts("popular", string.Empty);

			if (errorMessage == "Unauthorized" && !_loginPageDisplayed) 
			{
				await DisplayLoginPage();
				return Enumerable.Empty<Product>();
			}

			if (products == null) 
			{
				await DisplayAlert("Error", errorMessage ?? "Unable to get products", "OK");
				return Enumerable.Empty<Product>();
			}

			cvMostPopular.ItemsSource = products;

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

		await Navigation.PushAsync(new LoginPage(_apiService, _validator));	
	}

    private void cvCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
		var currentSelection = e.CurrentSelection.FirstOrDefault() as Category;

		if (currentSelection == null) 
		{
			return;
		}

		Navigation.PushAsync(new ProductsListPage(currentSelection.Id, currentSelection.Name!,
													_apiService, _validator));

		((CollectionView)sender).SelectedItem = null;
    }

    private void cvBestSellers_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
		if (sender is CollectionView collectionView) 
		{ 
			NavigateToProductDetailsPage(collectionView, e);
		}
    }

    private void cvMostPopular_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
		if (sender is CollectionView collectionView) 
		{
			NavigateToProductDetailsPage(collectionView, e);
		}
    }

    private void NavigateToProductDetailsPage(CollectionView collectionView, SelectionChangedEventArgs e)
    {
		var currentSelection = e.CurrentSelection.FirstOrDefault() as Product;

		if (currentSelection == null) 
		{
			return;
		}

		Navigation.PushAsync(new ProductDetailsPage(currentSelection.Id, currentSelection.Name!,
													_apiService, _validator));

		collectionView.SelectedItem = null;
    }
}