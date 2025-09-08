using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileApp.ReUse;
using MobileApp.Helpers;
using System.Threading.Tasks;

namespace MobileApp
{
    public partial class AdminPage
    {
        public AdminPage()
        {
            InitializeComponent();
            if (!AccessControl.IsAuthorized("Admin"))
            {
                Shell.Current.DisplayAlert("Accès refusé", "Vous n'avez pas la permission.", "OK");
                Shell.Current.GoToAsync("//LoginPage");
            }
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            

            var user = UserSession.CurrentUser;
            if (user != null)
            {

                HiLabel.Text = $"Bienvenue à toi cher : {user.Nom} {user.Prenom}👋";
            }
            else
            {
               await Shell.Current.GoToAsync("//LoginPage");
            }
        }
        private void OnAddAccountClicked(object sender, EventArgs e)
        {
            NewAccPopup.IsVisible = true;
            NewAccPopup.OnCreateSuccess += () => {
                MyPopup.Show("✅", "Utilisateur crée avec succès.");
            };
        }
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            UserSession.CurrentUser = null;
            await Shell.Current.GoToAsync("//LoginPage");
        }
        private async void OnClientListClicked(object sender,EventArgs e)
        {
            await Shell.Current.GoToAsync("//AdminClientPage");
        }
    }
}

