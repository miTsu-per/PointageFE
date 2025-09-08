using MobileApp.Models;
using MobileApp.ReUse;
using MobileApp.ReUse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MobileApp
{
    public partial class AdminClientPage : ContentPage
    {
        private List<User> AllUsers;
        public AdminClientPage()
        {
        InitializeComponent();
        
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadClients();
        }
        private async void LoadClients()
        {
            try
            {
                AllUsers = await new HttpClient().GetFromJsonAsync<List<User>>("http://10.0.2.2:5147/api/users/NonAdmins");
                UsersCollectionView.ItemsSource = AllUsers;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Chargement échoué : {ex.Message}", "OK");
            }
        }
        private async void OnHomeClicked(object sender,EventArgs e)
        {
            if (UserSession.CurrentUser.Role == "Admin")
            {
                await Shell.Current.GoToAsync("//AdminPage");
            }
            else
            {
                UserSession.CurrentUser = null;
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }
        private void OnEditClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var user = (User)button.BindingContext;


            EditAccPopup.EditingUser = user;
            EditAccPopup.LoadUserData();
            EditAccPopup.OnEditSuccess += () => {
                LoadClients();
                MyPopup.Show("✅", "Utilisateur modifié avec succès.");
            };

            EditAccPopup.IsVisible = true;
        }
        private async void OnDeleteClicked(object sender, EventArgs e)
        {

            var button = (Button)sender;
            var user = (User)button.BindingContext;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Confirmation",
                    $"Supprimer l'utilisateur {user.Prenom} {user.Nom} ?",
                    "Oui",
                    "Non"
                );

            if (!confirm)
                return;
            var response = await new HttpClient().DeleteAsync($"http://10.0.2.2:5147/api/users/{user.Id}");
            if (response.IsSuccessStatusCode)
            {
                MyPopup.Show("✅", "Utilisateur supprimé avec succeès");
                LoadClients();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                MyPopup.Show("Erreur", error);
            }
            
        }
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchTerm = e.NewTextValue;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                UsersCollectionView.ItemsSource = AllUsers;
            }
            else
            {
                var filteredShifts = AllUsers.Where(g => g.Prenom.ToLower().Contains(searchTerm.ToLower()) || g.Nom.ToLower().Contains(searchTerm.ToLower())).ToList();
                UsersCollectionView.ItemsSource = filteredShifts;
            }
        }
    }
}
