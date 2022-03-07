namespace SigmaApp.Views;

public partial class ConversationPage : ContentPage

{

    public ConversationPage()
    {
        InitializeComponent();
        //FriendsViewModel contacts = new FriendsViewModel();

        this.BindingContext = App.chat;
    }
}