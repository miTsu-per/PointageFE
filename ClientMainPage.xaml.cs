using System;
using System.Net.Http.Json;
using System.Timers;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Dispatching;
using MobileApp.ReUse;
using MobileApp.Helpers;

namespace MobileApp
{
    public partial class ClientMainPage : ContentPage
    {
        private bool isStarted = false;
        private int currentTimeId = 0;
        private DateTime startTime;
        private System.Timers.Timer timer;         
        private System.Timers.Timer locationTimer; 
        private Location _currentUserLocation;

        private const double PERMITTED_RADIUS_KM = 1.0; 
        private readonly double targetLat = 37.4220936;
        private readonly double targetLon = -122.083922;

        public ClientMainPage()
        {
            InitializeComponent();

            if (!AccessControl.IsAuthorized("Employe"))
            {
                Shell.Current.DisplayAlert("Accès refusé", "Vous n'avez pas la permission.", "OK");
                Shell.Current.GoToAsync("//LoginPage");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var user = UserSession.CurrentUser;

            if (user != null)
            {
                HiLabel.Text = $"Bienvenue à toi cher : {user.Nom} {user.Prenom}👋";
                await FetchAndSetInitialLocation();
            }
            else
            {
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }

        private async void OnBtnClicked(object sender, EventArgs e)
        {
            if (sender is not Button btn) return;

            if (!isStarted)
            {
                if (_currentUserLocation == null)
                {
                    MyPopup.Show("Erreur", "Localisation non disponible. Veuillez activer la localisation et redémarrer.");
                    return;
                }

                double distance = Location.CalculateDistance(
                    _currentUserLocation.Latitude, _currentUserLocation.Longitude,
                    targetLat, targetLon,
                    DistanceUnits.Kilometers
                );

                if (distance >= PERMITTED_RADIUS_KM)
                {
                    MyPopup.Show("⚠️", "Vous êtes hors de la zone autorisée pour démarrer.");
                    return;
                }

                var response = await new HttpClient().PostAsJsonAsync(
                    "http://10.0.2.2:5147/api/times/start",
                    UserSession.CurrentUser.Id
                );

                if (!response.IsSuccessStatusCode)
                {
                    MyPopup.Show("Erreur", "Impossible de démarrer le chrono.");
                    return;
                }

                currentTimeId = await response.Content.ReadFromJsonAsync<int>();
                startTime = DateTime.Now;
                isStarted = true;

                timer = new System.Timers.Timer(500);
                timer.Elapsed += async (s, ev) =>
                {
                    if (DateTime.Now.Date > startTime.Date)
                    {
                        await StopChronoAsync(btn, AutoStopReason.EndOfDay);
                        return;
                    }

                    TimeSpan elapsed = DateTime.Now - startTime;
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Chrono.Text = $"⏱️ {elapsed:hh\\:mm\\:ss}";
                    });
                };
                timer.Start();

                locationTimer = new System.Timers.Timer(10000);
                locationTimer.Elapsed += async (s, ev) => await CheckUserLocationAsync(btn);
                locationTimer.Start();

                btn.Text = "Stop";
                btn.BackgroundColor = Colors.Red;
            }
            else
            {
                await StopChronoAsync(sender as Button, AutoStopReason.None);
            }
        }

        private async Task CheckUserLocationAsync(Button btn)
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync()
                               ?? await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High));

                if (location == null) return;

                double userLat = location.Latitude;
                double userLon = location.Longitude;

                var update = new { latitude = userLat, longitude = userLon };
                var putResponse = await new HttpClient().PutAsJsonAsync(
                    $"http://10.0.2.2:5147/api/users/{UserSession.CurrentUser.Id}/location",
                    update
                );

                UserSession.CurrentUser.Latitude = userLat;
                UserSession.CurrentUser.Longitude = userLon;

                if (!putResponse.IsSuccessStatusCode)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        MyPopup.Show("⚠️", "Échec de l'envoi de la localisation au serveur.");
                    });
                }

                double distance = Location.CalculateDistance(
                    userLat, userLon,
                    targetLat, targetLon,
                    DistanceUnits.Kilometers
                );

                if (distance >= PERMITTED_RADIUS_KM)
                {
                        await StopChronoAsync(btn, AutoStopReason.OutOfZone);
                }
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MyPopup.Show("Erreur", $"Localisation échouée: {ex.Message}");
                });
            }
        }

        private enum AutoStopReason { None, OutOfZone, EndOfDay }

        private async Task StopChronoAsync(Button btn, AutoStopReason reason = AutoStopReason.None)
        {
            timer?.Stop();
            locationTimer?.Stop();

            if (currentTimeId == 0) return;

            var response = await new HttpClient().PutAsync(
                $"http://10.0.2.2:5147/api/times/stop/{currentTimeId}",
                null
            );

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (response.IsSuccessStatusCode)
                {
                    string title = reason != AutoStopReason.None ? "⚠️" : "✅";
                    string message;
                    switch (reason)
                    {
                        case AutoStopReason.OutOfZone:
                            message = "Vous avez quitté la zone, chrono arrêté automatiquement.";
                            break;
                        case AutoStopReason.EndOfDay:
                            message = "Journée terminée. Chrono arrêté automatiquement.";
                            break;
                        default:
                            message = "Chronomètre arrêté et sauvegardé.";
                            break;
                    }
                    MyPopup.Show(title, message);
                }
                else
                {
                    MyPopup.Show("⚠️", "Erreur lors de l'arrêt du chrono.");
                }

                btn.Text = "Start";
                btn.BackgroundColor = Colors.Green;
                Chrono.Text = "⏱️ 00:00:00";

                isStarted = false;
                currentTimeId = 0;
            });
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            UserSession.CurrentUser = null;
            await Shell.Current.GoToAsync("//LoginPage");
        }

        private async Task FetchAndSetInitialLocation()
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                MyPopup.Show("Erreur", "Permission de localisation refusée.");
                _currentUserLocation = null;
                return;
            }

            try
            {
                _currentUserLocation = await Geolocation.GetLastKnownLocationAsync()
                                   ?? await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.High));
                if (_currentUserLocation != null)
                {
                    var update = new { latitude = _currentUserLocation.Latitude, longitude = _currentUserLocation.Longitude };
                    var putResponse = await new HttpClient().PutAsJsonAsync(
                        $"http://10.0.2.2:5147/api/users/{UserSession.CurrentUser.Id}/location",
                        update
                    );

                    if (!putResponse.IsSuccessStatusCode)
                    {
                        MyPopup.Show("⚠️", "Échec de l'envoi de la localisation au serveur.");
                    }
                }
                else
                {
                    MyPopup.Show("Erreur", "Impossible d’obtenir la localisation.");
                }
            }
            catch (Exception ex)
            {
                MyPopup.Show("Erreur", $"Impossible d'obtenir la localisation: {ex.Message}");
                _currentUserLocation = null;
            }
        }
    }
}
