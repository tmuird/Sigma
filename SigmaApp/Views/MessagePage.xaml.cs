using SigmaApp.Models;
using System.Collections.Specialized;

namespace SigmaApp.Views
{
    public partial class MessagePage : ContentPage
    {
        public Conversation CurrentConversation { get; set; }

        public MessagePage()
        {
            InitializeComponent();
            this.BindingContext = App.chat;
            CurrentConversation = App.chat.CurrentConvo;

            CurrentConversation.Messages.CollectionChanged += ListChanged;
        }

        private void ListChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            try
            {
                if (CurrentConversation.Messages.Count > 0)
                {
                    MessageList.ScrollTo(CurrentConversation.Messages[CurrentConversation.Messages.Count - 1], ScrollToPosition.End, true);
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Alert", ex.Message, "OK");
            }
        }

        private void Send_Clicked(object sender, EventArgs e)
        {
            messageEntry.Text = string.Empty;
        }
    }
}
