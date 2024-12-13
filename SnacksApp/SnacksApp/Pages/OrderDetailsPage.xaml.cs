namespace SnacksApp.Pages;

using System;
using SnacksApp.Services;
using SnacksApp.Validations;

public partial class OrderDetailsPage : ContentPage
{
	private readonly ApiService _apiService;
	private readonly FavoritesService _favoritesService;
	private readonly IValidator _validator;
	private bool _loginPageDisplayed = false;

	public OrderDetailsPage(int orderId, decimal totalPrice, ApiService apiService, 
							FavoritesService favoritesService, IValidator validator)
	{
		InitializeComponent();
		_apiService = apiService;
		_favoritesService = favoritesService;
		_validator = validator;
		lblTotalPrice.Text = totalPrice + " €";

		GetOrderDetail(orderId);
	}

    private async void GetOrderDetail(int orderId)
    {
		try
		{
			// Displaying the activity indicator
			loadIndicator.IsRunning = true;
			loadIndicator.IsVisible = true;

			var (orderDetails, errorMessage) = await _apiService.GetOrderDetails(orderId);

			if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
			{
				await DisplayLoginPage();
				return;
			}

			if (orderDetails == null)
			{
				await DisplayAlert("Error", errorMessage ?? "Unable to get details for your order.", "OK");
				return;
			}
			else
			{
				cvOrderDetails.ItemsSource = orderDetails;
			}
		}
		catch (Exception)
		{
			await DisplayAlert("Error", "An error occurred while trying to get the details. Try again later.", "OK");
		}
		finally 
		{
			// Hiding the activity indicator
			loadIndicator.IsRunning = false;
			loadIndicator.IsVisible = false;
		}
    }

	private async Task DisplayLoginPage() 
	{ 
		_loginPageDisplayed = true;
		await Navigation.PushAsync(new LoginPage(_apiService, _favoritesService, _validator));
	}
}