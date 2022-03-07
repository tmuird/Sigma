using SigmaApp.Models;



namespace SigmaApp.Views;

public partial class MessagePage : ContentPage
{
    public Conversation CurrentConversation { get; set; }

    public MessagePage()
    {

        InitializeComponent();
        CurrentConversation = App.chat.CurrentConvo;
        this.BindingContext = App.chat;
        App.Current.MainPage.DisplayAlert("Warning", CurrentConversation.Recipient.UserId, "Ok");

    }

}