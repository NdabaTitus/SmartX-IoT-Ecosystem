using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;

namespace SmartX.WPF
{
    public partial class LoginWindow : Window
    {
        // Your actual Firebase Web API Key
        private const string FirebaseApiKey = "AIzaSyCHCTbAxZMYrU2Rez8VdlCcPf0KqWztHiw";
        private static readonly HttpClient _http = new HttpClient();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;
            LoadingText.Visibility = Visibility.Visible;
            LoginButton.IsEnabled = false;

            string email = EmailBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter both email and password.");
                return;
            }

            try
            {
                // Firebase Auth REST API Endpoint
                string url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={FirebaseApiKey}";

                var payload = new
                {
                    email = email,
                    password = password,
                    returnSecureToken = true
                };

                var response = await _http.PostAsJsonAsync(url, payload);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Success! Open the Landing Window
                    var landing = new LandingWindow();
                    landing.Show();
                    this.Close();
                }
                else
                {
                    // Parse Firebase Error
                    var errorDoc = JsonDocument.Parse(result);
                    var errorMsg = errorDoc.RootElement.GetProperty("error").GetProperty("message").GetString();

                    if (errorMsg == "INVALID_LOGIN_CREDENTIALS" || errorMsg == "EMAIL_NOT_FOUND")
                        ShowError("Invalid email or password.");
                    else
                        ShowError($"Login failed: {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Network error: {ex.Message}");
            }
            finally
            {
                LoadingText.Visibility = Visibility.Collapsed;
                LoginButton.IsEnabled = true;
            }
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}