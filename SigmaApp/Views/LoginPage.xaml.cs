using Microsoft.EntityFrameworkCore;
using SigmaApp.Data;
using SigmaApp.

Models;
using System.Collections.ObjectModel;

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
            try
            {
                //Ensure database from previous sessions is cleared
                using (var context = new LocalContext())
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                }
         
            }
            catch (Exception ex)
            {

                await DisplayAlert("Alert", ex.Message, "OK");
            }
         
            try
            {
                //Validation for username
                if (username.Text != null)
                {
                    userId = username.Text.ToString();
                    try
                    {
                        if (!userId.Contains(" "))
                        {
                            if (!(userId.Length > 12))
                            {
                                if (App.chat.api.GetUserExists(userId).GetAwaiter().GetResult())
                                {
                                    await DisplayAlert("Alert", "User with this name already exists!", "OK");
                                }
                                else
                                {
                                    try
                                    {
                                        int keySize = 256;
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
                                        }


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
                            else
                            {
                                await DisplayAlert("Alert", "Your username must be under 12 characters", "OK");
                            }

                        }
                        else
                        {
                            await DisplayAlert("Alert", "The username may not contain spaces", "OK");
                        }
                    }
                    catch (Exception ex)
                    {

                        await DisplayAlert("Alert", ex.Message, "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Alert", "Please enter a username", "OK");
                }
            }
            catch (Exception ex)
            {

                await DisplayAlert("Alert", ex.Message, "OK");
            }









        }

        


    }
}
