using SigmaApp.

Models;
namespace SigmaApp.Views
{
    public partial class LoginPage : ContentPage
    {

        public LoginPage()
        {
            InitializeComponent();

        }
        private string userId;
        private async void Register_Clicked(object sender, EventArgs e)

        {

            userId = username.Text.ToString();

            if (App.chat.api.GetUserExists(userId).GetAwaiter().GetResult())
            {
                await DisplayAlert("Alert", "User with this name already exists!", "OK");
            }
            else
            {
                InitializeComponent();
                try
                {
                    int keySize = 256;
                    App.KeyChain = crypto.RSA.GenerateKeys(keySize);

                    //await SecureStorage.SetAsync("Public", $"{Convert.ToBase64String(keyDict['e'])},{Convert.ToBase64String(keyDict['N'])}");
                    //await SecureStorage.SetAsync("Private", $"{Convert.ToBase64String(keyDict['d'])},{Convert.ToBase64String(keyDict['N'])}");
                    //await SecureStorage.SetAsync("Username", userId);

                    App.CurrentUser = new User()
                    {
                        UserId = userId,
                        PublicKey = $"{Convert.ToBase64String(App.KeyChain['e'])},{Convert.ToBase64String(App.KeyChain['N'])}"
                    };
                    await DisplayAlert("Alert", "Key Generated", "OK");
                    await App.chat.Connect();
                    App.chat.InitCrypt();
                    App.chat.Connect();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Alert", ex.Message, "OK");
                    // Possible that device doesn't support secure storage on device.
                }
                try
                {
                    await Shell.Current.GoToAsync("//ConversationPage");
                }
                catch (Exception ex)
                {

                    await DisplayAlert("Alert", ex.Message, "OK");
                }

            }

        }


    }
}
