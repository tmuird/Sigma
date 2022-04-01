using SigmaApp.Models;
using System.Collections.Specialized;

namespace SigmaApp.Views;

public partial class MessagePage : ContentPage
{
    public Conversation CurrentConversation { get; set; }

    public MessagePage()
    {
       
        this.BindingContext = App.chat;
    
        InitializeComponent();
        CurrentConversation = App.chat.CurrentConvo;

        //trigger event when list updated (scroll to bottom)
        CurrentConversation.Messages.CollectionChanged += listChanged;

        


    }
    private async void Send_Clicked(object sender, EventArgs e)
    {
       
        message.Text = "";
    }

    private void listChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
        try
        {
            MessageList.ScrollTo(CurrentConversation.Messages[CurrentConversation.Messages.Count - 1], 0, true);
        }
        catch (Exception ex)
        {

            DisplayAlert("Alert", ex.Message, "OK");
        }
       
    }

}