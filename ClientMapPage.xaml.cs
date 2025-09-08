using Microsoft.Maui.Controls;

namespace MobileApp
{
    [QueryProperty(nameof(Latitude), "lat")]
    [QueryProperty(nameof(Longitude), "lng")]
    public partial class ClientMapPage : ContentPage
    {
        double latitude, longitude;

        public double Latitude
        {
            get => latitude;
            set
            {
                latitude = value;
                if (IsLoaded)
                    UpdateMap();
            }
        }

        public double Longitude
        {
            get => longitude;
            set
            {
                longitude = value;
                if (IsLoaded)
                    UpdateMap();
            }
        }

        bool IsLoaded = false;

        public ClientMapPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            IsLoaded = true;
            UpdateMap();
        }

        void UpdateMap()
        {
            if (Latitude == 0 && Longitude == 0)
                return; 

            var html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <link rel=""stylesheet"" href=""https://unpkg.com/leaflet@1.9.3/dist/leaflet.css"" />
                    <script src=""https://unpkg.com/leaflet@1.9.3/dist/leaflet.js""></script>
                    <style>
                        html, body, #map {{
                            height: 100%;
                            margin: 0;
                        }}
                    </style>
                </head>
                <body>
                    <div id='map'></div>
                    <script>
                        var map = L.map('map').setView([{Latitude}, {Longitude}], 15);
                        L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
                            attribution: '© OpenStreetMap contributors'
                        }}).addTo(map);
                        L.marker([{Latitude}, {Longitude}]).addTo(map)
                            .bindPopup('Dernière position connue du client')
                            .openPopup();
                    </script>
                </body>
                </html>";

            MapWebView.Source = new HtmlWebViewSource
            {
                Html = html
            };
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
