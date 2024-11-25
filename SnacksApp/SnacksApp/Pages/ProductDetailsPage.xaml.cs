namespace SnacksApp.Pages;

using System.Threading.Tasks;
using SnacksApp.Models;
using SnacksApp.Services;
using SnacksApp.Validations;

public partial class ProductDetailsPage : ContentPage
{
	private readonly ApiService _apiService;
	private readonly IValidator _validator;
	private int _productId;
	private bool _loginPageDisplayed = false;

	public ProductDetailsPage(int productId, string productName, 
							ApiService apiService, IValidator validator)
	{
		InitializeComponent();
		_productId = productId;
		_apiService = apiService;
		_validator = validator;
        Title = productName ?? "Product Details";
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await GetProductDetails(_productId);
    }

    private async Task<Product?> GetProductDetails(int productId)
    {
        var (productDetails, errorMessage) = await _apiService.GetProductDetails(productId);

        if (errorMessage == "Unauthorized" && !_loginPageDisplayed) 
        {
            await DisplayLoginPage();
            return null;
        }

        if (productDetails == null) 
        {
            await DisplayAlert("Error", errorMessage ?? "Unable to get product", "OK");
            return null;
        }

        if (productDetails != null)
        {
            ProductImage.Source = productDetails.ImagePath;
            lblProductName.Text = productDetails.Name;
            lblProductPrice.Text = productDetails.Price.ToString();
            lblProductDescription.Text = productDetails.Description;
            lblTotalPrice.Text = productDetails.Price.ToString();
        }
        else 
        {
            await DisplayAlert("Error", errorMessage ?? "Unable to get product details", "OK");
            return null;
        }

        return productDetails;
    }

    private void BtnFavoritesImage_Clicked(object sender, EventArgs e)
    {

    }

    private void BtnRemove_Clicked(object sender, EventArgs e)
    {
        if (int.TryParse(lblQuantity.Text, out int quantity) &&
            decimal.TryParse(lblProductPrice.Text, out decimal unitPrice))
        {
            // Decreases the quantity and doesn't allow it to be less than 1
            quantity = Math.Max(1, quantity - 1);
            lblQuantity.Text = quantity.ToString();

            var totalPrice = quantity * unitPrice;
            lblTotalPrice.Text = totalPrice.ToString();
        }
        else 
        {
            DisplayAlert("Error", "Invalid values", "OK");
        }
    }

    private void BtnAdd_Clicked(object sender, EventArgs e)
    {
        if (int.TryParse(lblQuantity.Text, out int quantity) &&
            decimal.TryParse(lblProductPrice.Text, out decimal unitPrice))
        {
            quantity++;
            lblQuantity.Text = quantity.ToString();

            var totalPrice = quantity * unitPrice;
            lblTotalPrice.Text = totalPrice.ToString();
        }
        else 
        {
            DisplayAlert("Error", "Invalid values", "OK");
        }
    }

    private async void BtnIncludeInCart_Clicked(object sender, EventArgs e)
    {
        try 
        {
            var cart = new Cart
            {
                Quantity = Convert.ToInt32(lblQuantity.Text),
                Price = Convert.ToDecimal(lblProductPrice.Text),
                TotalValue = Convert.ToDecimal(lblTotalPrice.Text),
                ProductId = _productId,
                ClientId = Preferences.Get("userId", 0),
            };

            var response = await _apiService.AddItemToCart(cart);

            if (response.Data)
            {
                await DisplayAlert("Success", "Item added to the cart", "OK");
                await Navigation.PopAsync();
            }
            else 
            {
                await DisplayAlert("Error", $"Error adding item to cart: {response.ErrorMessage}", "OK");
            }
        }
        catch (Exception ex) 
        {
            await DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
        }
    }

    private async Task DisplayLoginPage() 
    { 
        _loginPageDisplayed = true;

        await Navigation.PushAsync(new LoginPage(_apiService, _validator));
    }
}