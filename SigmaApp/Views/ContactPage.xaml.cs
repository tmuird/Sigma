namespace SigmaApp.Views
{
    public partial class ContactPage : ContentPage
    {
        public ContactPage()
        {
            InitializeComponent();
            this.BindingContext = App.chat;
        }
    }
}
