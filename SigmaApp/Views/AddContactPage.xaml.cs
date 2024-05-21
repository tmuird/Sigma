using Microsoft.EntityFrameworkCore;
using SigmaApp.Data;
using SigmaApp.Models;

namespace SigmaApp.Views
{
    public partial class AddContactPage : ContentPage
    {
        private string contactID;
        private string userKey;

        public AddContactPage()
        {
            InitializeComponent();
        }

        private async void Add_Clicked(object sender, EventArgs e)
        {
            contactID = name.Text;

            if (string.IsNullOrEmpty(contactID))
            {
                await DisplayAlert("Alert", "Please enter a username", "OK");
                return;
            }

            try
            {
                if (await App.chat.Api.GetUserExists(contactID))
                {
                    if (!App.chat.Contacts.Any(o => o.UserID == contactID))
                    {
                        try
                        {
                            userKey = await App.chat.Api.GetKey(contactID);
                            Console.WriteLine($"Retrieved key successfully {userKey}");
                        }
                        catch (Exception ex)
                        {
                            await DisplayAlert("Alert", ex.Message, "OK");
                            return;
                        }

                        var addedUser = new User()
                        {
                            PublicKey = userKey,
                            UserID = contactID
                        };

                        App.chat.Contacts.Add(addedUser);

                        using (var context = new LocalContext())
                        {
                            context.Users.Add(addedUser);
                            context.Entry(addedUser).State = EntityState.Detached;
                            await context.SaveChangesAsync();
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
}
