using Microsoft.Maui.Controls;
using MobileApp.Helpers;
using MobileApp.Models;
using System.Net.Http.Json;
using MobileApp.ReUse;
namespace MobileApp
{
    public partial class SuperVClientPage : ContentPage
    {
        public SuperVClientPage()
        {
            InitializeComponent();
            LoadClients();
            if (!AccessControl.IsAuthorized("Superieur"))
            {
                Shell.Current.DisplayAlert("Accès refusé", "Vous n'avez pas la permission.", "OK");
                Shell.Current.GoToAsync("//LoginPage");
            }
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var user = UserSession.CurrentUser;

            if (user == null)
            {
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }

        private async void LoadClients()
        {
            try
            {
                var clients = await new HttpClient().GetFromJsonAsync<List<User>>("http://10.0.2.2:5147/api/users/clients");
                ClientsCollectionView.ItemsSource = clients;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erreur", $"Chargement échoué : {ex.Message}", "OK");
            }
        }
        private async void OnClientSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is User selectedUser)
            {
                await Shell.Current.GoToAsync($"//SuperVClientDetails?id={selectedUser.Id}");

                (sender as CollectionView).SelectedItem = null;
            }
        }
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            UserSession.CurrentUser = null;
            await Shell.Current.GoToAsync("//LoginPage");
        }

    }
}
