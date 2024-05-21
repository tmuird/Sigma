namespace SigmaApp.Views
{
    public partial class ConversationPage : ContentPage
    {
        public ConversationPage()
        {
            InitializeComponent();
            this.BindingContext = App.chat;
        }
    }
}
