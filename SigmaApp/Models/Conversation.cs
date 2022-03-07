using System.Collections.ObjectModel;

namespace SigmaApp.Models
{
    public class Conversation
    {

        public ObservableCollection<Message> Messages { get; set; }


        public User Recipient { get; set; }

        public Conversation()
        {
            Messages = new ObservableCollection<Message>();
        }

    }
}
