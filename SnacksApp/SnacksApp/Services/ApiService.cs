namespace SnacksApp.Services
{
    using System.Text;
    using System.Text.Json;
    using Microsoft.Extensions.Logging;
    using SnacksApp.Models;

    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "";
        private readonly ILogger<ApiService> _logger;

        JsonSerializerOptions _serializerOptions;

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
        }

        public async Task<ApiResponse<bool>> RegisterUser(string name, string email, string phoneNumber, string password)
        {
            try 
            {
                var register = new Register
                {
                    Name = name,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    Password = password,
                };

                var json = JsonSerializer.Serialize(register, _serializerOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await PostRequest("api/Users/Register", content);

                if (!response.IsSuccessStatusCode) 
                {
                    _logger.LogError($"Error sending HTTP request: {response.StatusCode}");

                    return new ApiResponse<bool>
                    {
                        ErrorMessage = $"Error sending HTTP request: {response.StatusCode}"
                    };
                }

                return new ApiResponse<bool>
                {
                    Data = true
                };
            }
            catch (Exception ex)  
            { 
                _logger.LogError($"Error registering user: {ex.Message}");

                return new ApiResponse<bool> 
                { 
                    ErrorMessage = ex.Message 
                };
            }
        }
        
        public async Task<ApiResponse<bool>> Login(string email, string password) 
        {
            try
            {
                var login = new Login
                {
                    Email = email,
                    Password = password,
                };

                var json = JsonSerializer.Serialize<Login>(login, _serializerOptions);
                var content = new StringContent (json, Encoding.UTF8, "application/json");

                var response = await PostRequest("api/Users/Login", content);

                if (!response.IsSuccessStatusCode) 
                {
                    _logger.LogError($"Error sending HTTP request: {response.StatusCode}");

                    return new ApiResponse<bool>
                    {
                        ErrorMessage = $"Error sending HTTP request: {response.StatusCode}"
                    };
                }

                var jsonResult = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Token>(jsonResult, _serializerOptions);

                Preferences.Set("accessToken", result!.AccessToken);
                Preferences.Set("userId", (int)result.UserId!);
                Preferences.Set("userName", result.UserName);

                return new ApiResponse<bool>
                {
                    Data = true,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error logging in: {ex.Message}");

                return new ApiResponse<bool> 
                { 
                    ErrorMessage = ex.Message 
                };
            }
        }

        private async Task<HttpResponseMessage> PostRequest(string uri, HttpContent content) 
        { 
            var urlAddress = _baseUrl + uri;

            try 
            {
                var result = await _httpClient.PostAsync(urlAddress, content);

                return result;
            }
            catch (Exception ex) 
            {
                _logger.LogError($"Error sending POST request to {uri}: {ex.Message}");

                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);          
            }
        }


    }
}
