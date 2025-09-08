using MobileApp.Helpers;
using MobileApp.ReUse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using MobileApp.Models;

namespace MobileApp
{
    public partial class ClientHistoriquePage
    {
        private List<TimeWithClientDto> _allClientShifts;

        public ClientHistoriquePage()
        {
            InitializeComponent();
            if (!AccessControl.IsAuthorized("Employe"))
            {
                MyPopup.Show("Accès refusé", "Vous n'avez pas la permission.");
                Shell.Current.GoToAsync("//LoginPage");
            }
        }
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            var user = UserSession.CurrentUser;

            if (user == null)
            {
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            try
            {
                var allClients = await new HttpClient().GetFromJsonAsync<List<TimeWithClientDto>>("http://10.0.2.2:5147/api/times/with-clients");

                _allClientShifts = allClients?.Where(c => c.IdClient == UserSession.CurrentUser.Id).ToList();

                if (_allClientShifts != null && _allClientShifts.Any())
                {
                    ShiftsCollectionView.ItemsSource = _allClientShifts;
                }
                else
                {
                    MyPopup.Show("Information", "Aucun shift n'a été trouvé.");
                }
            }
            catch (Exception ex)
            {
                MyPopup.Show("Erreur", $"Impossible de charger les détails : {ex.Message}");
            }
        }

        private void OnDateSelected(object sender, DateChangedEventArgs e)
        {
            var selectedDate = e.NewDate.Date;

            if (_allClientShifts == null) return;

            var filteredShifts = _allClientShifts.Where(s => s.Starttime.Date == selectedDate).ToList();
            ShiftsCollectionView.ItemsSource = filteredShifts;

            if (!filteredShifts.Any())
            {
                MyPopup.Show("Information", "Aucun shift trouvé pour cette date.");
            }
        }
    }
}
