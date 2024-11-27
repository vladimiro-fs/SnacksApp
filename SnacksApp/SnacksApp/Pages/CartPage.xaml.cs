namespace SnacksApp.Pages;

using System.Collections.ObjectModel;
using SnacksApp.Models;
using SnacksApp.Services;
using SnacksApp.Validations;

public partial class CartPage : ContentPage
{
	private readonly ApiService _apiService;
	private readonly FavoritesService _favoritesService;
	private readonly IValidator _validator;
	private bool _loginPageDisplayed = false;
	private bool _isNavigatingToEmptyCartPage = false;

	private ObservableCollection<CartItem> 
		CartItems = new ObservableCollection<CartItem>();

	public CartPage(ApiService apiService, FavoritesService favoritesService,
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
		
		if (IsNavigatingToEmptyCartPage()) 
		{
			return;
		}

		bool hasItems = await GetCartItems();

		if (hasItems)
		{
			ShowAddress();
		}
		else 
		{
			await GoToEmptyCartPage();
		}		
    }

    private bool IsNavigatingToEmptyCartPage()
    {
        if (_isNavigatingToEmptyCartPage) 
		{
			_isNavigatingToEmptyCartPage = false;
			return true;
		}

		return false;
    }

	private void ShowAddress() 
	{
		bool savedAddress = Preferences.ContainsKey("address");

		if (savedAddress) 
		{ 
			string name = Preferences.Get("name", string.Empty);
			string address = Preferences.Get("address", string.Empty);
			string phoneNumber = Preferences.Get("phoneNumber", string.Empty);

			lblAddress.Text = $"{name} \n{address} \n{phoneNumber}";
		}
		else 
		{
			lblAddress.Text = "Fill the address field.";
		}
	}

	private async Task GoToEmptyCartPage() 
	{ 
		lblAddress.Text = string.Empty;
		_isNavigatingToEmptyCartPage = true;
		await Navigation.PushAsync(new EmptyCartPage());
	}

    private async Task<bool> GetCartItems() 
	{ 
		try 
		{
			var userId = Preferences.Get("userId", 0);
			var (cartItems, errorMessage) = await _apiService.GetCartItems(userId);

			if (errorMessage == "Unauthorized" && !_loginPageDisplayed) 
			{
				await DisplayLoginPage();
				return false;
			}

			if (cartItems == null) 
			{
				await DisplayAlert("Error", errorMessage ?? "Unable to get cart items", "OK");
				return false;
			}

			CartItems.Clear();

			foreach (var item in cartItems) 
			{ 
				CartItems.Add(item);
			}

			cvCart.ItemsSource = CartItems;
			UpdateTotalPrice();

			if (!CartItems.Any()) 
			{ 
				return false;
			}

			return true;
		}
		catch (Exception ex) 
		{
			await DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
			return false;
		}
	}

	private void UpdateTotalPrice() 
	{
		try
		{
			var totalPrice = CartItems.Sum(item => item.Price * item.Quantity);
			lblTotalPrice.Text = totalPrice.ToString();
		}
		catch (Exception ex) 
		{ 
			DisplayAlert("Error", $"An error occurred while updating the price: {ex.Message}", "OK");
		}
	}

	private async Task DisplayLoginPage() 
	{ 
		_loginPageDisplayed = true;
		await Navigation.PushAsync( new LoginPage(_apiService, _favoritesService, _validator));
	}

    private async void BtnDecrease_Clicked(object sender, EventArgs e)
    {
		if (sender is Button button && button.BindingContext is CartItem cartItem) 
		{
			if (cartItem.Quantity == 1) 
			{ 
				return;
			}
			else
			{
				cartItem.Quantity--;
				UpdateTotalPrice();
				await _apiService.UpdateCartItemQuantity(cartItem.ProductId, "decrease"); 
			}
		}
    }

    private async void BtnIncrease_Clicked(object sender, EventArgs e)
    {
		if (sender is Button button && button.BindingContext is CartItem cartItem) 
		{
			cartItem.Quantity++;
			UpdateTotalPrice();
			await _apiService.UpdateCartItemQuantity(cartItem.ProductId, "increase");
		}
    }

    private async void BtnDelete_Clicked(object sender, EventArgs e)
    {
		if (sender is ImageButton button && button.BindingContext is CartItem cartItem) 
		{
			bool response = await DisplayAlert("Confirmation", 
				"Are you sure you want to remove this item?", "Yes", "No");

			if (response) 
			{ 
				CartItems.Remove(cartItem);
				UpdateTotalPrice();
				await _apiService.UpdateCartItemQuantity(cartItem.ProductId, "delete");
			}
		}
    }

    private void BtnEditAddress_Clicked(object sender, EventArgs e)
    {
		Navigation.PushAsync(new AddressPage());
    }

    private async void TapConfirmOrder_Tapped(object sender, TappedEventArgs e)
    {
		if (CartItems == null || !CartItems.Any()) 
		{
			await DisplayAlert("Information", "Your cart is empty or your request was already confirmed.", "OK");
			return;
		}

		var order = new Order
		{
			Address = lblAddress.Text,
			UserId = Preferences.Get("userId", 0),
			TotalValue = Convert.ToDecimal(lblTotalPrice.Text),
		};

		var response = await _apiService.ConfirmOrder(order);

		if (response.HasError) 
		{
			if (response.ErrorMessage == "Unauthorized")
			{
				await DisplayLoginPage();
				return; 
			}

			await DisplayAlert("Whoops!", $"Something went wrong: {response.ErrorMessage}", "Cancel");
			return;
		}

		CartItems.Clear();
		lblAddress.Text = "Enter your address";
		lblTotalPrice.Text = "0.00";
		await Navigation.PushAsync(new ConfirmedOrderPage());
    }
}