using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobileApp.Helpers;

namespace MobileApp
{
    public partial class ClientTabbedPage
    {
        public ClientTabbedPage() { 
            InitializeComponent();
            if (!AccessControl.IsAuthorized("Employe"))
            {
                Shell.Current.DisplayAlert("Accès refusé", "Vous n'avez pas la permission.", "OK");
                Shell.Current.GoToAsync("//LoginPage");
            }

        }
    }
}
