﻿namespace SnacksApp.Services
{
    using System.Net;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using Microsoft.Extensions.Logging;
    using SnacksApp.Models;

    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://9dd4djnz-7075.uks1.devtunnels.ms/";
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

        public async Task<ApiResponse<bool>> AddItemToCart(Cart cart)
        {
            try
            {
                var json = JsonSerializer.Serialize(cart, _serializerOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await PostRequest("api/CartItems", content);

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
                    Data = true,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding item to cart: {ex.Message}");

                return new ApiResponse<bool>
                {
                    ErrorMessage = ex.Message,
                };
            }
        }

        public async Task<ApiResponse<bool>> UploadUserImage(byte[] imageArray)
        {
            try
            {
                var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(imageArray), "image", "image.jpg");

                var response = await PostRequest("api/Users/uploaduserphoto", content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = response.StatusCode == HttpStatusCode.Unauthorized
                        ? "Unauthorized"
                        : $"Error sending HTTP request: {response.StatusCode}";

                    _logger.LogError($"Error sending HTTP request: {response.StatusCode}");

                    return new ApiResponse<bool>
                    {
                        ErrorMessage = errorMessage,
                    };
                }

                return new ApiResponse<bool>
                {
                    Data = true,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading user's image: {ex.Message}");

                return new ApiResponse<bool>
                {
                    ErrorMessage = ex.Message,
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
                return new HttpResponseMessage(HttpStatusCode.BadRequest);          
            }
        }

        private async Task<HttpResponseMessage> PutRequest(string uri, HttpContent content)
        {
            var urlAddress = AppConfig.BaseUrl + uri;

            try
            {
                AddAuthorizationHeader();

                var result = await _httpClient.PutAsync(urlAddress, content);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending PUT request to {uri}: {ex.Message}");
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public async Task<(List<Category>? Categories, string? ErrorMessage)> GetCategories() 
        {
            return await GetAsync<List<Category>>("/api/categories");
        }

        public async Task<(List<Product>? Products, string? ErrorMessage)> GetProducts(string productType, string categoryId) 
        { 
            string endpoint = $"api/Products?productType={productType}&categoryId={categoryId}";
            return await GetAsync<List<Product>>(endpoint);
        }

        public async Task<(Product? ProductDetails, string? ErrorMessage)> GetProductDetails(int productId)
        {
            string endpoint = $"api/products/{productId}";
            return await GetAsync<Product>(endpoint);
        }

        public async Task<(List<CartItem>? CartItems, string? ErrorMessage)> GetCartItems(int userId)
        {
            var endpoint = $"api/CartItems/{userId}";
            return await GetAsync<List<CartItem>>(endpoint);
        }

        public async Task<(ProfileImage? ProfileImage, string? ErrorMessage)> GetUserProfileImage()
        {
            string endpoint = "api/users/UserProfileImage";
            return await GetAsync<ProfileImage>(endpoint);
        }

        public async Task<(List<OrderByUser>? Orders, string? ErrorMessage)> GetOrdersByUser(int userId)
        {
            string endpoint = $"api/orders/OrdersByUser/{userId}";
            return await GetAsync<List<OrderByUser>>(endpoint);
        }

        public async Task<(List<OrderDetail>? OrderDetails, string? ErrorMessage)> GetOrderDetails(int orderId) 
        { 
            string endpoint = $"api/orders/OrderDetails/{orderId}";
            return await GetAsync<List<OrderDetail>>(endpoint);
        }

        private async Task<(T? Data, string? ErrorMessage)> GetAsync<T>(string endpoint) 
        {
            try
            {
                AddAuthorizationHeader();

                var response = await _httpClient.GetAsync(_baseUrl + endpoint);

                if (response.IsSuccessStatusCode) 
                { 
                    var responseString = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<T>(responseString, _serializerOptions);

                    return (data ?? Activator.CreateInstance<T>(), null);
                }
                else 
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized) 
                    {
                        string errorMessage = "Unauthorized";
                        _logger.LogWarning(errorMessage);
                        return (default, errorMessage);
                    }

                    string generalErrorMessage = $"Request error: {response.ReasonPhrase}";
                    _logger.LogError(generalErrorMessage);
                    return (default, generalErrorMessage);
                }
            }
            catch (HttpRequestException ex) 
            {
                string errorMessage = $"HTTP request error: {ex.Message}";
                _logger.LogError(errorMessage);
                return (default, errorMessage);
            }
            catch (JsonException ex) 
            {
                string errorMessage = $"JSON deserialization error: {ex.Message}";
                _logger.LogError(errorMessage);
                return (default, errorMessage);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Unexpected error: {ex.Message}";
                _logger.LogError(errorMessage);
                return (default, errorMessage);
            }
        }

        private void AddAuthorizationHeader() 
        { 
            var token = Preferences.Get("accessToken", string.Empty);

            if (!string.IsNullOrEmpty(token)) 
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<(bool Data, string? ErrorMessage)> UpdateCartItemQuantity(int productId, string action)
        {
            try
            {
                var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                var response = await PutRequest($"api/CartItems?productId={productId}&action={action}", content);

                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        string errorMessage = "Unauthorized";
                        _logger.LogWarning(errorMessage);

                        return (false, errorMessage);
                    }

                    string generalErrorMessage = $"Request error: {response.ReasonPhrase}";
                    _logger.LogError(generalErrorMessage);
                    return (false, generalErrorMessage);
                }
            }
            catch (HttpRequestException ex)
            {
                string errorMessage = $"HTTP request error: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return (false, errorMessage);
            }
            catch (Exception ex)  
            { 
                string errorMessage = $"Unexpected error: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return (false, errorMessage);
            }
        }

        public async Task<ApiResponse<bool>> ConfirmOrder(Order order) 
        { 
            try 
            {
                var json = JsonSerializer.Serialize(order, _serializerOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await PostRequest("api/Orders", content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = response.StatusCode == HttpStatusCode.Unauthorized
                        ? "Unauthorized"
                        : $"Error sending HTTP request: {response.StatusCode}";
                }

                _logger.LogError($"Error sending HTTP request: {response.StatusCode}");
                
                return new ApiResponse<bool>
                {
                    Data = true,
                };
            }
            catch (Exception ex)  
            {
                _logger.LogError($"Error confirming request: {ex.Message}");

                return new ApiResponse<bool>
                {
                    ErrorMessage = ex.Message,
                };
            }
        }    
    }
}
