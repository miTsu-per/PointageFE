using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using MobileApp.Models;

namespace MobileApp.ReUse
{
    public partial class EditAcc : ContentView
    {
        public EditAcc()
        {
            InitializeComponent();
        }
        public event Action OnEditSuccess;

        public User EditingUser { get; set; }
        public void LoadUserData()
        {
            if (EditingUser == null)
                return;

            NomEntry.Text = EditingUser.Nom;
            PrenomEntry.Text = EditingUser.Prenom;
            EmailEntry.Text = EditingUser.Email;
            PasswordEntry.Text = ""; 
            RolePicker.SelectedItem = EditingUser.Role;
        }



        private async void OnEditClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NomEntry.Text) ||
                string.IsNullOrWhiteSpace(PrenomEntry.Text) ||
                string.IsNullOrWhiteSpace(EmailEntry.Text) ||
                RolePicker.SelectedItem == null)
            {
                MyPopup.Show("⚠️", "Veuillez remplir tous les champs.");
                return;
            }

            var editUser = new
            {
                Nom = NomEntry.Text.Trim(),
                Prenom = PrenomEntry.Text.Trim(),
                Email = EmailEntry.Text.Trim(),
                Password = PasswordEntry.Text,
                Role = RolePicker.SelectedItem.ToString()
            };

            try
            {
                var response = await new HttpClient().PutAsJsonAsync(
                    $"http://10.0.2.2:5147/api/users/{EditingUser.Id}/update", editUser
                );

                if (response.IsSuccessStatusCode)
                {
                    this.IsVisible = false;
                    NomEntry.Text = "";
                    PrenomEntry.Text = "";
                    EmailEntry.Text = "";
                    PasswordEntry.Text = "";
                    RolePicker.SelectedIndex = -1;
                    OnEditSuccess?.Invoke();
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
