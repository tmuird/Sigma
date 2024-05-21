using Microsoft.EntityFrameworkCore;
using SigmaApp.API;
using SigmaApp.Data;
using SigmaApp.Models;
using SigmaApp.ViewModels;
using System.Collections.ObjectModel;

namespace SigmaApp;

public partial class App : Application
{
    public static IUserInfo api;
    public static string CurrentPage = "ChatPage";
    public static Dictionary<char, byte[]> KeyChain;
    public static User CurrentUser;
    public static ChatViewModel chat;

    public App()
    {
        InitializeComponent();

        chat = new ChatViewModel();
        MainPage = new AppShell();
        if (SecureStorage.GetAsync("Name").Result != null)
        {
            LoadCachedUser();
        }
        else
        {
            GoToLogin();
        }
    }

    public async Task GoToLogin()
    {
        await Shell.Current.GoToAsync("LoginPage");
    }

    private void LoadCachedUser()
    {
        Console.WriteLine("Loading stored keypair");
        App.CurrentUser = new User()
        {
            UserID = SecureStorage.GetAsync("Name").Result,
            PublicKey = $"{SecureStorage.GetAsync("PublicKey").Result},{SecureStorage.GetAsync("Modulus").Result}"
        };
        App.KeyChain = new Dictionary<char, byte[]>()
        {
            { 'e', Convert.FromBase64String(SecureStorage.GetAsync("PublicKey").Result) },
            { 'd', Convert.FromBase64String(SecureStorage.GetAsync("PrivateKey").Result) },
            { 'N', Convert.FromBase64String(SecureStorage.GetAsync("Modulus").Result) }
        };

        App.chat.Connect();
        App.chat.InitCrypt(); // Ensure InitCrypt is called here
        try
        {
            Console.WriteLine("\nLoading Cached Data...");
            using (var context = new LocalContext())
            {
                Console.WriteLine("Loading Contacts...");
                App.chat.Contacts = new ObservableCollection<User>(context.Users);
                Console.WriteLine("Loading Conversations...");
                App.chat.Conversations = new ObservableCollection<Conversation>(context.Conversations);
                foreach (Conversation conversation in App.chat.Conversations)
                {
                    Console.WriteLine($"\nLoading Conversation: {conversation.ConversationID}\nMessages:");
                    foreach (Message message in context.Messages.Where(m => m.Conversation.ConversationID == conversation.ConversationID))
                    {
                        if ((bool)message.IsMine)
                        {
                            message.Sender = App.CurrentUser;
                            message.Receiver = conversation.Recipient;
                        }
                        else
                        {
                            message.Sender = conversation.Recipient;
                            message.Receiver = App.CurrentUser;
                        }
                        Console.WriteLine($"Recipient: {message.Receiver.UserID} Content:{message.Content}");
                        message.Conversation = conversation;
                        conversation.Messages.Add(message);
                    }
                }
                context.SaveChanges();
            }

            Shell.Current.GoToAsync("//ConversationPage");
        }
        catch (Exception ex)
        {
            Current.MainPage.DisplayAlert("Alert", ex.Message, "Ok");
        }
    }
}
