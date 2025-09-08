using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using MobileApp.Helpers;

namespace MobileApp
{
    public partial class SuperVTabbedPage : TabbedPage
    {
        public SuperVTabbedPage()
        {
            InitializeComponent();
            if (!AccessControl.IsAuthorized("Superieur"))
            {
                Shell.Current.DisplayAlert("Accès refusé", "Vous n'avez pas la permission.", "OK");
                Shell.Current.GoToAsync("//LoginPage");
            }
        }
    }
}
