namespace MobileApp.ReUse
{
    public partial class AlertPopup : ContentView
    {
        public AlertPopup()
        {
            InitializeComponent();
        }

        public void Show(string title, string message)
        {
            TitleLabel.Text = title;
            MessageLabel.Text = message;
            IsVisible = true;
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            IsVisible = false;
        }
    }
}
