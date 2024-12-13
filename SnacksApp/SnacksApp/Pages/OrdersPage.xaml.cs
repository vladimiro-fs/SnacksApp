namespace SnacksApp.Pages;

using SnacksApp.Models;
using SnacksApp.Services;
using SnacksApp.Validations;

public partial class OrdersPage : ContentPage
{
	private readonly ApiService _apiService;
	private readonly FavoritesService _favoritesService;
	private readonly IValidator _validator;
	private bool _loginPageDisplayed = false;

	public OrdersPage(ApiService apiService, FavoritesService favoritesService, 
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
		await GetOrdersList();
	}

    private async Task GetOrdersList()
    {
		try
		{
			// Displaying the activity indicator
			loadOrdersIndicator.IsRunning = true;
			loadOrdersIndicator.IsVisible = true;

			var (orders, errorMessage) = await _apiService.GetOrdersByUser(Preferences.Get("userId", 0));

			if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
			{
				await DisplayLoginPage();
				return;
			}
			if (errorMessage == "Not Found")
			{
				await DisplayAlert("Warning", "The user has no orders", "OK");
				return;
			}
			if (orders == null)
			{
				await DisplayAlert("Error", errorMessage ?? "Unable to get orders", "OK");
				return;
			}
			else
			{
				cvOrders.ItemsSource = orders;
			}
		}
		catch (Exception)
		{
			await DisplayAlert("Error", "An error occurred while trying to get the orders. Try again later.", "OK");
		}
		finally 
		{
			// Hiding the activity indicator
			loadOrdersIndicator.IsRunning = false;
			loadOrdersIndicator.IsVisible = false;
		}
    }

    private void cvOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
		var selectedItem = e.CurrentSelection.FirstOrDefault() as OrderByUser;

		if (selectedItem == null) 
		{ 
			return;
		}

		Navigation.PushAsync(new OrderDetailsPage(selectedItem.Id, selectedItem.TotalOrder,
											_apiService, _favoritesService, _validator));

		((CollectionView)sender).SelectedItem = null;
    }

	private async Task DisplayLoginPage () 
	{ 
		_loginPageDisplayed = true;
		await Navigation.PushAsync(new LoginPage(_apiService, _favoritesService, _validator));
	}
}