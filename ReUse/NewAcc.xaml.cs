using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace MobileApp.ReUse
{
    public partial class NewAcc : ContentView
    {
        public NewAcc()
        {
            InitializeComponent();
            RolePicker.SelectedIndex = 0;
        }

        public event Action OnCreateSuccess;


        private async void OnCreateClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NomEntry.Text) ||
                string.IsNullOrWhiteSpace(PrenomEntry.Text) ||
                string.IsNullOrWhiteSpace(EmailEntry.Text) ||
                string.IsNullOrWhiteSpace(PasswordEntry.Text) ||
                RolePicker.SelectedItem == null)
            {
                MyPopup.Show("⚠️", "Veuillez remplir tous les champs.");
                return;
            }

            var newUser = new
            {
                Nom = NomEntry.Text.Trim(),
                Prenom = PrenomEntry.Text.Trim(),
                Email = EmailEntry.Text.Trim(),
                Password = PasswordEntry.Text,
                Role = RolePicker.SelectedItem.ToString()
            };

            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.PostAsJsonAsync("http://10.0.2.2:5147/api/users/register", newUser);

                if (response.IsSuccessStatusCode)
                {
                    this.IsVisible = false;
                    NomEntry.Text = "";
                    PrenomEntry.Text = "";
                    EmailEntry.Text = "";
                    PasswordEntry.Text = "";
                    RolePicker.SelectedIndex = -1;
                    OnCreateSuccess?.Invoke();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MyPopup.Show("Erreur", error);
                }
            }
            catch (Exception ex)
            {
                MyPopup.Show("Erreur", $"Erreur réseau : {ex.Message}");
            }
        }
        private void OnCloseClicked(object sender, EventArgs e)
        {
            this.IsVisible = false;
        }

    }
}
