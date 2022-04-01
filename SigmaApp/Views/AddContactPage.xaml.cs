using Android.OS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Hosting;
using SigmaApp.Data;
using SigmaApp.Models;

namespace SigmaApp.Views;

public partial class AddContactPage : ContentPage
{
    private string contactID;
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

        contactID = name.Text.ToString();
        try
        {
            if (App.chat.api.GetUserExists(contactID).GetAwaiter().GetResult())
            {
                if (!App.chat.Contacts.Any(o => o.UserID == contactID))
                {
                    //InitializeComponent();
                    try
                    {
                        userKey = App.chat.api.GetKey(contactID).GetAwaiter().GetResult();
                        Console.WriteLine($"Retrieved key successfully {userKey}");

                        //await App.chat.Connect();
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Alert", ex.Message, "OK");
                        // Possible that device doesn't support secure storage on device.
                    }
                    User addedUser = new User()
                    {
                        PublicKey = userKey,
                        UserID = contactID,
                        
                    };

                    App.chat.Contacts.Add(addedUser);
                    using (var context = new LocalContext())
                    {
                        context.Users.Add(addedUser);
                        context.Entry(addedUser).State = EntityState.Detached;
                        context.SaveChanges();
                    }
                     
                    await Shell.Current.GoToAsync("//ContactPage");
                }
                else
                {
                    await DisplayAlert("Alert", "This contact already exists!", "OK");
                }

            }
            else
            {
                await DisplayAlert("Alert", "A user with this name doesn't exist!", "OK");
            }
        }
        catch (Exception ex)
        {

            await DisplayAlert("Alert", ex.Message, "OK");
        }
       

    }
}