using Microsoft.EntityFrameworkCore;
using SigmaApp.Data;
using SigmaApp.Models;
using System.Collections.ObjectModel;

namespace SigmaApp.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void Register_Clicked(object sender, EventArgs e)
        {
            try
            {
                using (var context = new LocalContext())
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Alert", ex.Message, "OK");
                return;
            }

            if (string.IsNullOrEmpty(username.Text))
            {
                await DisplayAlert("Alert", "Please enter a username", "OK");
                return;
            }

            var userId = username.Text.Trim();

            if (userId.Contains(" ") || userId.Length > 12)
            {
                await DisplayAlert("Alert", "Invalid username. Please ensure it has no spaces and is under 12 characters.", "OK");
                return;
            }

            try
            {
                if (await App.chat.Api.GetUserExists(userId))
                {
                    await DisplayAlert("Alert", "User with this name already exists!", "OK");
                    return;
                }

                var keySize = 256;
                App.KeyChain = crypto.RSA.GenerateKeys(keySize);
                await DisplayAlert("Alert", "Key Generated", "OK");

                App.CurrentUser = new User()
                {
                    UserID = userId,
                    PublicKey = $"{Convert.ToBase64String(App.KeyChain['e'])},{Convert.ToBase64String(App.KeyChain['N'])}"
                };

                await SecureStorage.SetAsync("Name", userId);
                await SecureStorage.SetAsync("PublicKey", Convert.ToBase64String(App.KeyChain['e']));
                await SecureStorage.SetAsync("PrivateKey", Convert.ToBase64String(App.KeyChain['d']));
                await SecureStorage.SetAsync("Modulus", Convert.ToBase64String(App.KeyChain['N']));

                try
                {
                    await App.chat.Connect();
                    App.chat.InitCrypt();
                }
                catch (Exception)
                {
                    await DisplayAlert("Alert", "Could not connect to server", "OK");
                    return;
                }

                await Shell.Current.GoToAsync("//ConversationPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Alert", ex.Message, "OK");
            }
        }
    }
}
