using SigmaApp.API;
using SigmaApp.Models;
using SigmaApp.ViewModels;

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
        GoToLogin();
    }

    public async Task GoToLogin()
    {
        await Shell.Current.GoToAsync("LoginPage");
    }


}
