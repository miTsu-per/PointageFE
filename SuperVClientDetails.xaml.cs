using Microsoft.Maui.Controls;
using MobileApp.Models;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MobileApp.ReUse;
namespace MobileApp
{
    [QueryProperty(nameof(ClientId), "id")]
    public partial class SuperVClientDetails : ContentPage
    {
        private List<TimeWithClientDto> _allClientShifts;
        public int ClientId { get; set; }

        public SuperVClientDetails()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var user = UserSession.CurrentUser;

            if (user == null)
            {
                await Shell.Current.GoToAsync("//LoginPage");
            }

            try
            {
                var allClients = await new HttpClient().GetFromJsonAsync<List<TimeWithClientDto>>("http://10.0.2.2:5147/api/times/with-clients");

                var clientShifts = allClients?.Where(c => c.IdClient == ClientId).ToList();

                if (clientShifts != null && clientShifts.Any())
                {
                    _allClientShifts = clientShifts;
                    var first = _allClientShifts.First();
                    NomLabel.Text = $"Nom : {first.ClientName}";
                    EmailLabel.Text = $"Email : {first.ClientEmail}";
                    ShiftsCollectionView.ItemsSource = _allClientShifts;
                    FilterDatePicker.Date = DateTime.Today;
                }
                else
                {
                    MyPopup.Show("Erreur", "Aucun shift trouvé pour ce client.");
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                MyPopup.Show("Erreur", $"Impossible de charger les détails : {ex.Message}");
            }
        }
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            UserSession.CurrentUser = null;
            await Shell.Current.GoToAsync("//LoginPage");
        }
        private async void OnApproveClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is TimeWithClientDto time)
            {
                try
                {
                    var response = await new HttpClient().PutAsync($"http://10.0.2.2:5147/api/times/{time.Id}/approve", null);

                    if (response.IsSuccessStatusCode)
                    {
                        MyPopup.Show("Succès", "Le shift a été approuvé.");

                        time.Approved = true;
                        OnAppearing();
                    }
                    else
                    {
                        MyPopup.Show("Erreur", "Échec de l’approbation.");
                    }
                }
                catch (Exception ex)
                {
                    MyPopup.Show("Erreur", $"Exception: {ex.Message}");
                }
            }
        }
        private async void OnDisApproveClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is TimeWithClientDto time)
            {
                try
                {
                    var response = await new HttpClient().PutAsync($"http://10.0.2.2:5147/api/times/{time.Id}/refuse", null);

                    if (response.IsSuccessStatusCode)
                    {
                        MyPopup.Show("Succès", "Le shift a été refuser.");

                        time.Approved = false;
                        OnAppearing();
                    }
                    else
                    {
                        MyPopup.Show("Erreur", "Échec de l’approbation.");
                    }
                }
                catch (Exception ex)
                {
                    MyPopup.Show("Erreur", $"Exception: {ex.Message}");
                }
            }
        }

        private async void OnHomeClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//SuperVMainPage");
        }

        private void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            var selectedDate = e.NewDate.Date;
            var filteredShifts = _allClientShifts.Where(s => s.Starttime.Date == selectedDate).ToList();
            ShiftsCollectionView.ItemsSource = filteredShifts;
        }

        private async void OnViewLocationClicked(object sender, EventArgs e)
        {
            try
            {
                var location = await new HttpClient().GetFromJsonAsync<LocationDto>(
                    $"http://10.0.2.2:5147/api/users/last?clientId={ClientId}");

                if (location == null)
                {
                    MyPopup.Show("Erreur", "La position est introuvable.");
                    return;
                }

                await Shell.Current.GoToAsync($"ClientMapPage?lat={location.Latitude}&lng={location.Longitude}");

            }
            catch (Exception ex)
            {
                MyPopup.Show("Erreur", $"Erreur lors du chargement de la carte : {ex.Message}");
            }
        }

        public class LocationDto
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }




    }
}
