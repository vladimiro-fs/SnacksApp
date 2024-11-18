using SnacksApp.Services;

namespace SnacksApp.Pages;

public partial class LoginPage : ContentPage
{
	private readonly ApiService _apiService;

	public LoginPage(ApiService apiService)
	{
		InitializeComponent();
		_apiService = apiService;
	}

    private async void BtnSignIn_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(EntEmail.Text)) 
        {
            await DisplayAlert("Error", "The email field can't be empty", "Cancel");
            return;
        }
        if (string.IsNullOrEmpty(EntPassword.Text)) 
        {
            await DisplayAlert("Error", "The password field can't be empty", "Cancel");
            return;
        }

        var response = await _apiService.Login(EntEmail.Text, EntPassword.Text);

        if (!response.HasError) 
        {
            Application.Current!.MainPage = new AppShell();
        }
        else
        {
            await DisplayAlert("Error", "Something went wrong", "Cancel");
        }
    }

    private async void TapRegister_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage(_apiService));
    }
}