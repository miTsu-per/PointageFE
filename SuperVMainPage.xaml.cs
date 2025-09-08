using Microsoft.Maui.Controls.PlatformConfiguration;
using MobileApp.Helpers;
using MobileApp.Models;
using MobileApp.ReUse;
using System.Net.Http.Json;

namespace MobileApp
{
    public partial class SuperVMainPage : ContentPage
    {
        private List<GroupedClientShifts> _allGroupedShifts;
        public SuperVMainPage()
        {
            InitializeComponent();
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

            LoadTodayTimes();
        }

        private async void LoadTodayTimes()
        {
            try
            {
                var allTimes = await new HttpClient().GetFromJsonAsync<List<TimeWithClientDto>>("http://10.0.2.2:5147/api/times/with-clients");
                var today = DateOnly.FromDateTime(DateTime.Today);

                var todayTimes = allTimes
                    .Where(t => DateOnly.FromDateTime(t.Starttime.Date) == today)
                    .ToList();

                var grouped = todayTimes
                    .GroupBy(t => t.IdClient)
                    .Select(g =>
                    {
                        var shifts = g.Select(t => new ShiftInfo
                        {
                            Starttime = t.Starttime,
                            Finishtime = t.Finishtime,
                            Totaltime = t.Totaltime ?? 0,
                            Approved = t.Approved
                        }).ToList();

                        return new GroupedClientShifts
                        {
                            ClientId = g.Key,
                            ClientName = g.First().ClientName,
                            Shifts = shifts,
                            TotalTimeFormatted = $"Durée totale : {shifts.Sum(s => s.Totaltime)} sec",
                            AllApproved = shifts.All(s => s.Approved==true) 
                        };
                    }).ToList();

                _allGroupedShifts = grouped;
                TodayTimesView.ItemsSource = _allGroupedShifts;
            }
            catch (Exception ex)
            {
                MyPopup.Show("Erreur", $"Échec du chargement des chronos : {ex.Message}");
            }
        }

        private async void OnApproveClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is GroupedClientShifts group)
            {
                try
                {
                    var http = new HttpClient();
                    var response = await http.PutAsync(
                        $"http://10.0.2.2:5147/api/times/approve/client/{group.ClientId}", null);

                    response.EnsureSuccessStatusCode();

                    MyPopup.Show("Succès", "Tous les shifts du client ont été approuvés.");
                    LoadTodayTimes();
                }
                catch (Exception ex)
                {
                    MyPopup.Show("Erreur", $"Échec de l'approbation : {ex.Message}");
                }
            }
        }
        private async void OnDisApproveClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is GroupedClientShifts group)
            {
                try
                {
                    var http = new HttpClient();
                    var response = await http.PutAsync(
                        $"http://10.0.2.2:5147/api/times/refuse/client/{group.ClientId}", null);

                    response.EnsureSuccessStatusCode();

                    MyPopup.Show("Succès", "Tous les shifts du client ont été refuser.");
                    LoadTodayTimes();
                }
                catch (Exception ex)
                {
                    MyPopup.Show("Erreur", $"Échec de l'approbation : {ex.Message}");
                }
            }
        }
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            UserSession.CurrentUser = null;
            await Shell.Current.GoToAsync("//LoginPage");
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchTerm = e.NewTextValue;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                TodayTimesView.ItemsSource = _allGroupedShifts;
            }
            else
            {
                var filteredShifts = _allGroupedShifts.Where(g => g.ClientName.ToLower().Contains(searchTerm.ToLower())).ToList();
                TodayTimesView.ItemsSource = filteredShifts;
            }
        }

    }
}
