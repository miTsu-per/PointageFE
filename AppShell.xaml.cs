namespace MobileApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(SuperVClientDetails), typeof(SuperVClientDetails));
            Routing.RegisterRoute(nameof(ClientMapPage), typeof(ClientMapPage));
        }
    }
}
