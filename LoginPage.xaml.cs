using MobileApp.ReUse;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MobileApp.Models;

namespace MobileApp
{
    public partial class LoginPage
    {
        private readonly HttpClient _httpClient = new();

        public LoginPage()
        {
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            EmailEntry.Text = string.Empty;
            PasswordEntry.Text = string.Empty;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string email = EmailEntry.Text?.Trim() ?? "";
            string password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MyPopup.Show("⚠️", "Veuillez remplir tous les champs.");
                return;
            }

            if (!IsValidEmail(email))
            {
                MyPopup.Show("⚠️", "L'email n'est pas valide.");
                return;
            }

            string apiUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? "http://10.0.2.2:5147/api/users/login"
                : "http://localhost:5147/api/users/login";

            var loginData = new { email, password };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(apiUrl, loginData);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var user = JsonConvert.DeserializeObject<User>(json);

                    if (user != null)
                    {
                        UserSession.CurrentUser = user;
                        switch (user.Role)
                        {
                            case "Employe":
                                await Shell.Current.GoToAsync("///ClientMainPage");
                                break;

                            case "Superieur":
                                await Shell.Current.GoToAsync("///SuperVMainPage");
                                break;

                            case "Admin":
                                await Shell.Current.GoToAsync("///AdminPage");
                                break;

                            default:
                                Console.WriteLine("Unknown role.");
                                break;
                        }
                        
                    }
                    else
                    {
                        MyPopup.Show("Erreur", "Données utilisateur invalides.");
                    }
                }
                else
                {
                    string errorText = await response.Content.ReadAsStringAsync();
                    string message = ExtractErrorMessage(errorText);
                    MyPopup.Show("Échec", message);
                }
            }
            catch (Exception ex)
            {
                MyPopup.Show("Erreur", $"Impossible de se connecter : {ex.Message}");
            }
        }

        private static bool IsValidEmail(string email) => Regex.IsMatch(email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        private static string ExtractErrorMessage(string json)
        {
            try
            {
                var error = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                return error != null && error.TryGetValue("message", out var msg)
                    ? msg
                    : "Une erreur inconnue est survenue.";
            }
            catch
            {
                return "Erreur serveur : " + json;
            }
        }
    }

}
