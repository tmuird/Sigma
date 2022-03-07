using SigmaApp.Models;

namespace SigmaApp.Views;

public partial class AddContactPage : ContentPage
{
    private string userId;
    private string userKey;
    public AddContactPage()
    {
        InitializeComponent();


    }

    /// <summary>
    /// click event for add button 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Add_Clicked(object sender, EventArgs e)
    {

        userId = name.Text.ToString();

        if (App.chat.api.GetUserExists(userId).GetAwaiter().GetResult())
        {

            InitializeComponent();
            try
            {
                userKey = App.chat.api.GetKey(userId).GetAwaiter().GetResult();
                await DisplayAlert("Alert", $"Retrieved key successfully {userKey}", "OK");

                await App.chat.Connect();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Alert", ex.Message, "OK");
                // Possible that device doesn't support secure storage on device.
            }
            User addedUser = new User()
            {
                PublicKey = userKey,
                UserId = userId,
                Creation = DateTime.Now.ToShortDateString(),
            };

            App.chat.Contacts.Add(addedUser);
            await Shell.Current.GoToAsync("//ContactPage");
        }
        else
        {
            await DisplayAlert("Alert", "A user with this name doesn't exist!", "OK");
        }

    }
}