namespace SnacksApp.Pages;

using SnacksApp.Services;

public partial class MyAccountPage : ContentPage
{
	private readonly ApiService _apiService;

	private const string UserNameKey = "UserName";
	private const string UserEmailKey = "UserEmail";
	private const string UserPhoneNumberKey = "UserPhoneNumber";

	public MyAccountPage(ApiService apiService)
	{
		InitializeComponent();
		_apiService = apiService;	
	}

	protected override async void OnAppearing() 
	{ 
		base.OnAppearing();
		LoadUserInfo();
		BtnProfileImage.Source = await GetProfileImageAsync();
	}

    private void LoadUserInfo() 
	{ 
		lblUserName.Text = Preferences.Get(UserNameKey, string.Empty);
		EntName.Text = lblUserName.Text;
		EntEmail.Text = Preferences.Get(UserEmailKey, string.Empty);
		EntPhoneNumber.Text = Preferences.Get(UserPhoneNumberKey, string.Empty);
	}

    private async Task<string?> GetProfileImageAsync()
    {
        string defaultImage = AppConfig.DefaultProfileImage;

		var (response, errorMessage) = await _apiService.GetUserProfileImage();

        if (errorMessage != null) 
		{
			switch (errorMessage) 
			{
				case "Unauthorized":
					await DisplayAlert("Error", "Not authorized", "OK");
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

    private async void BtnSave_Clicked(object sender, EventArgs e)
    {
		Preferences.Set(UserNameKey, EntName.Text);
		Preferences.Set(UserEmailKey, EntEmail.Text);
		Preferences.Set(UserPhoneNumberKey, EntPhoneNumber.Text);

		await DisplayAlert("Info saved", "Your info was successfully saved!", "OK");
    }
}