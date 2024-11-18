namespace SnacksApp.Pages;

using SnacksApp.Services;

public partial class RegisterPage : ContentPage
{
    private readonly ApiService _apiService;

	public RegisterPage(ApiService apiService)
	{
		InitializeComponent();
        _apiService = apiService;
	}

    private async void BtnSignup_Clicked(object sender, EventArgs e)
    {
        var response = await _apiService.RegisterUser(EntName.Text, EntEmail.Text, EntPhoneNumber.Text, EntPassword.Text);

        if (!response.HasError) 
        {
            await DisplayAlert("Warning", "Your account was successfully created!", "OK");
            await Navigation.PushAsync(new LoginPage(_apiService));
        }
        else 
        {
            await DisplayAlert("Error", "Something went wrong!", "Cancel");
        }
    }

    private async void TapLogin_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new LoginPage(_apiService));
    }
}