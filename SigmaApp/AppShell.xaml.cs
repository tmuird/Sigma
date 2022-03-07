using SigmaApp.Views;

namespace SigmaApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("LoginPage", typeof(LoginPage));
            Routing.RegisterRoute("MessagePage", typeof(MessagePage));
            Routing.RegisterRoute("AddContactPage", typeof(AddContactPage));
        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            await Current.GoToAsync("LoginPage");
        }

    }
}
