namespace SnacksApp.Pages;

using SnacksApp.Services;
using SnacksApp.Validations;

public partial class ProfilePage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly FavoritesService _favoritesService;
    private readonly IValidator _validator;
    private bool _loginPageDisplayed = false;

	public ProfilePage(ApiService apiService, FavoritesService favoritesService, 
                        IValidator validator)
	{
		InitializeComponent();
        lblUserName.Text = Preferences.Get("userName", string.Empty);
        _apiService = apiService;
        _favoritesService = favoritesService;
        _validator = validator;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        BtnProfileImage.Source = await GetProfileImage();
    }

    private async Task<string?> GetProfileImage()
    {
        string defaultImage = AppConfig.DefaultProfileImage;

        var (response, errorMessage) = await _apiService.GetUserProfileImage();

        if (errorMessage != null) 
        {
            switch (errorMessage)
            {
                case "Unauthorized":
                    if (!_loginPageDisplayed)
                    {
                        await DisplayLoginPage();
                        return null;
                    }

                    break;

                default:
                    await DisplayAlert("Error", errorMessage ?? "Unable to get the image", "OK");
                    return defaultImage;
            }
        }

        if (response?.ImageUrl != null) 
        {
            return response.ImagePath;
        }

        return defaultImage;
    }

    private async void BtnProfileImage_Clicked(object sender, EventArgs e)
    {
        try
        {
            var imageArray = await SelectImageAsync();

            if (imageArray == null) 
            {
                await DisplayAlert("Error", "Unable to load the image", "OK");
                return;
            }

            BtnProfileImage.Source = ImageSource.FromStream(() => new MemoryStream(imageArray));

            var response = await _apiService.UploadUserImage(imageArray);

            if (response.Data)
            {
                await DisplayAlert("Success", "Image successfully sent.", "OK");
            }
            else 
            {
                await DisplayAlert("Error", response.ErrorMessage ?? "Unknown error", "Cancel");
            }
        }
        catch (Exception ex) 
        {
            await DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
        }
    }

    private async Task<byte[]?> SelectImageAsync()
    {
        try
        {
            var file = await MediaPicker.PickPhotoAsync();

            if (file == null)
            {
                return null;
            }

            using (var stream = await file.OpenReadAsync())
            {
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlert("Error", "Functionality not supported by the device.", "OK");
        }
        catch (PermissionException)
        {
            await DisplayAlert("Error", "Permissions not granted to access the camera or photo gallery.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error selecting the image: {ex.Message}", "OK");
        }

        return null;
    }

    private async void TapOrders_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new OrdersPage(_apiService, _favoritesService, _validator));
    }

    private async void MyAccount_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new MyAccountPage(_apiService));
    }

    private async void FAQ_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new FAQPage());
    }

    private void BtnLogout_Clicked(object sender, EventArgs e)
    {
        Preferences.Set("accessToken", string.Empty);
        Application.Current!.MainPage = new NavigationPage(new LoginPage(_apiService, _favoritesService, _validator));
    }

    private async Task DisplayLoginPage()
    {
        _loginPageDisplayed = true;
        await Navigation.PushAsync(new LoginPage(_apiService, _favoritesService, _validator));
    }
}