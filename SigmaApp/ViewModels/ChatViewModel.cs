using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Windows.Input;
using crypto;
using Microsoft.AspNetCore.SignalR.Client;
using Prism.Commands;
using Refit;
using SigmaApp.API;
using SigmaApp.Models;
using Microsoft.EntityFrameworkCore.Sqlite;
using SigmaApp.Data;

namespace SigmaApp.ViewModels
{
    public class ChatViewModel 
    {



        //many variables within the viewmodel are public as they need to be accessed between classes / pages
        public string CurrentMessage { get; set; }
        private readonly HubConnection _connection;
        public IUserInfo api;
        public Conversation CurrentConvo { get; set; }
     
        public User Sender { get; set; }
        public ObservableCollection<User> Contacts { get; set; }

        public ObservableCollection<Conversation> Conversations { get; set; }
        private const string _url = "https://nea.speedi.codes";

        //public commands to be accessed from xaml
        public ICommand GoToChatCommand { get; }
        public ICommand StartChatCommand { get; }
        public ICommand DeleteChatCommand { get; }
        public ICommand DeleteContactCommand { get; }
  
        public Command GoToAddContactCommand { get; }
        public Command SendMessageCommand { get; }
        public Command LogoutCommand { get; }

        public Dictionary<string, string> UserKeys;

        private byte[] _myRsaAes;

        public Aes AesService = Aes.Create();

        


        
        public ChatViewModel()
        {
           
            api = RestService.For<IUserInfo>($"{_url}");

            Sender = App.CurrentUser;

            _connection = new HubConnectionBuilder()
             .WithUrl($"{_url}/chatHub",
               options =>
               {
                   options.HttpMessageHandlerFactory = handler =>
                   {
                       var newHandler = new HttpClientHandler();
                       if (true)
                       {
                           newHandler.ServerCertificateCustomValidationCallback =
                             (message, certificate, chain, sslPolicyErrors) => true;
                       }
                       return newHandler;
                   };
               }
                )
             .Build();

            //assign commands to their corresponding functions (functions will be triggered when command is executed in XAML)
            StartChatCommand = new DelegateCommand<User>(StartChat);
            GoToChatCommand = new DelegateCommand<Conversation>(GoToChat);
         
            DeleteChatCommand = new DelegateCommand<Conversation>(DeleteChat);
            DeleteContactCommand = new DelegateCommand<User>(DeleteContact);
            GoToAddContactCommand = new Command(GoToAddContact);
   
            SendMessageCommand = new Command(Send);
            LogoutCommand = new Command(Logout);

            //instantiate collections
            Contacts = new ObservableCollection<User>();
            Conversations = new ObservableCollection<Conversation>();
            UserKeys = new Dictionary<string, string>();

            try
            {
                //signalr event for recieving messages
                _connection.On<Message>("Receive", (message) =>
                {
                    try
                    {
                        //creates the HMAC to compare against the message HMAC to verify integrity/authenticity
                        string localHMAC = Hashing.GetHMAC(Convert.ToBase64String(Combine(AesService.Key, Convert.FromBase64String(message.Content), _myRsaAes)), Convert.ToBase64String(AesService.Key));
                        if (message.HMAC == localHMAC)
                        {
                            Console.WriteLine("Valid HMAC!");

                        }
                        else
                        {
                            App.Current.MainPage.DisplayAlert("Warning", "Invalid hash", "Ok");

                        }
                        //sets the messageId to null to avoid conflicts with localdb, upon being added an autoincremented ID is added
                        message.MessageID = null;
                        //shows that the message is being received and not send so when formatting the messages it can be displayed in the correct position
                        message.IsMine = false;

                        //decrypts the message content using RSA decrypted AES
                        message.Content = (DecryptStringFromBytes_Aes(Convert.FromBase64String(message.Content), Convert.FromBase64String(UserKeys[(message.Sender.UserID)]), Convert.FromBase64String(message.InitVector)));
                        using (var context = new LocalContext())
                        {
                           
                        }
                           
                    }
                    catch (Exception ext)
                    {

                        App.Current.MainPage.DisplayAlert("Warning", ext.Message, "Ok");

                    }

                   

                    try
                    {

                        //if conversation already exists add the message to it, otherwise create a new conversation
                        if (Conversations.Any(convo => convo.Recipient.UserID == message.Sender.UserID))

                        {

                            Conversation userConvo = Conversations.Single(s => s.Recipient.UserID == message.Sender.UserID);
                            CurrentConvo = userConvo;
                          
                            AddMessage(message);
                            
                        }
                        else
                        {

                            Conversation newConvo = new Conversation()
                            {
                                Recipient = message.Sender,

                            };

                            try
                            {
                                CurrentConvo = newConvo;
                               
                                AddMessage(message);
                             
                            }
                            catch (Exception ex)
                            {

                                App.Current.MainPage.DisplayAlert("Warning", ex.Message, "Ok");
                            }
                            Conversations.Add(newConvo);
                        }
                    }
                    catch (Exception ex)
                    {

                        App.Current.MainPage.DisplayAlert("Warning", ex.Message, "Ok");
                    }

                });

                //signalr event for receiving the recipient/sender RSA key to be used for decrypting the AES key
                _connection.On<string[]>("ReceiveRSA", (key) =>
                {
                    try
                    {
                        byte[] RSAAES = Convert.FromBase64String(key[1]);
                        if (!UserKeys.ContainsKey(key[0]))
                        {
                            UserKeys.Add(key[0], crypto.RSA.Decrypt(App.KeyChain['d'], App.KeyChain['N'], RSAAES));
                        }
                       

                    }
                    catch (Exception exc)
                    {

                        App.Current.MainPage.DisplayAlert("Warning", "Error: " + exc, "Ok");
                    }


                });
            }
            catch (Exception)
            {
                string message = Conversations[0].Messages.Last().Content;
                throw;
            }


        }

    
        /// <summary>
        /// Deletes a contact from the DB and Program
        /// </summary>
        /// <param name="contact"></param>
        private void DeleteContact(User contact)
        {
            try
            {
                if (Conversations.Any(s => s.Recipient == contact))
                {
                    DeleteChat(Conversations.Single(s => s.Recipient == contact));
                }
                
                Contacts.Remove(contact);
                using (var context = new LocalContext())
                {
                    context.Users.Remove(contact);
                    context.SaveChanges();
                }
                    
            
               
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
           
        }

        private void DeleteChat(Conversation convo)
        {
            try
            {
                Conversations.Remove(convo);
                using (var context = new LocalContext())
                {
                    context.Conversations.Remove(convo);
                    context.SaveChanges();
                }
                    
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            
        }

        /// <summary>
        /// initialises the AES service
        /// </summary>
        public void InitCrypt()
        {

            //Upon the AES being initiliased, the padding is unsured 
            AesService.Padding = PaddingMode.PKCS7;
            AesService.GenerateKey();
            AesService.GenerateIV();

        }


        /// <summary>
        /// Takes the user to the add contact view
        /// </summary>
        private async void GoToAddContact()
        {
            try
            {
                await Shell.Current.GoToAsync($"AddContactPage");
            }
            catch (Exception ex)
            {

                App.Current.MainPage.DisplayAlert("Warning", "Error: " + ex, "Ok");
            }


        }

        /// <summary>
        /// sets up the environment for a conversation, including necessary cryptography
        /// </summary>
        /// <param name="reciever"></param>
        private async void StartChat(User reciever)
        {
            try
            {


                if (Conversations.Any(convo => convo.Recipient.UserID == reciever.UserID))
                {

              
                    GoToChat(Conversations.Single(s => s.Recipient.UserID == reciever.UserID));
                }
                else
                {

                    Conversation newConvo = new Conversation()
                    {
                     
                        Recipient = reciever,
                    };
                 
                   
                    _myRsaAes = crypto.RSA.Encrypt(Convert.FromBase64String(reciever.PublicKey.Split(",")[0]), Convert.FromBase64String(reciever.PublicKey.Split(",")[1]), Convert.ToBase64String(AesService.Key));
                    SendRSAAES(Convert.ToBase64String(_myRsaAes), reciever.UserID);
                    Conversations.Add(newConvo);
                   
                    CurrentConvo = newConvo;
                    await Shell.Current.GoToAsync($"MessagePage?recipient={reciever}");
                    using (var context = new LocalContext())
                    {
                        context.Conversations.Add(newConvo);
                        context.SaveChanges();
                    }
                        
                }
              
            }
            catch (Exception ex)
            {

                await App.Current.MainPage.DisplayAlert("Warning", ex.Message, "Ok");
            }
        
        }
        /// <summary>
        /// Sends the message to the signalR hub and then the determined user
        /// </summary>
        /// <param name="message"></param>
        private async Task SendToUser(Message message)
        {

            try
            {
              
                message.IsMine = true;
                await _connection.InvokeAsync("SendToUser",
                    message);
            }
            catch (Exception ex)
            {
                App.Current.MainPage.DisplayAlert("Warning", "Error: " + ex, "Ok");
            }
        }

        /// <summary>
        /// Sends the RSA Encrypted AES Key to the signalR Hub
        /// </summary>
        /// <param name="rsaAES"></param>
        /// <param name="receiverId"></param>
        public async void SendRSAAES(string rsaAES, string receiverId)
        {
            try
            {
                await _connection.InvokeAsync("SendRSAAES",
                    rsaAES, App.CurrentUser.UserID, receiverId);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Warning", "Error: " + ex, "Ok");
            }
        }

        private async Task GetUserId()
        {
            await _connection.InvokeAsync("GetUserId");
        }
        public async Task Connect()
        {



            try
            {
                await _connection.StartAsync();
                await _connection.InvokeAsync("UserConnected",
                   App.CurrentUser);



            }
            catch (Exception ex)
            {

            }
        }

        public async Task Disconnect()
        {



            try
            {
                await _connection.StopAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        /// <summary>
        /// Passes the selected conversation to the chat page and opens it
        /// </summary>
        /// <param name="conversation"></param>
        private async void GoToChat(Conversation conversation)
        {
            try
            {
                CurrentConvo = conversation;
                //await App.Current.MainPage.DisplayAlert("Warning", conversation.user.PublicKey, "Ok");
                _myRsaAes = crypto.RSA.Encrypt(Convert.FromBase64String(conversation.Recipient.PublicKey.Split(",")[0]), Convert.FromBase64String(conversation.Recipient.PublicKey.Split(",")[1]), Convert.ToBase64String(AesService.Key));
                SendRSAAES(Convert.ToBase64String(_myRsaAes), conversation.Recipient.UserID);

                await Shell.Current.GoToAsync($"MessagePage?recipient={conversation.Recipient.UserID}");
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }


        }
        public async void Logout()
        {
            SecureStorage.RemoveAll();
            App.CurrentUser = null;
            Conversations.Clear();
            Contacts.Clear();
            Disconnect();
            Shell.Current.GoToAsync($"LoginPage");


        }
        /// <summary>
        /// Adds a message to the database and collections
        /// </summary>
        /// <param name="message"></param>
        public void AddMessage(Message message)
        {
            try
            {
 

                message.ConversationID = CurrentConvo.ConversationID;
                (Conversations.Single(p => p.Recipient == CurrentConvo.Recipient)).RecentMessage = message.Content;
     
                CurrentConvo.Messages.Add(message);
                using (var context = new LocalContext())
                {
                  
                    context.Messages.Add(message);
                    context.SaveChanges();
                }
    

            }
            catch (Exception ex) 
            {

                App.Current.MainPage.DisplayAlert("Warning", "Error: " + ex, "Ok");
            }
           

        }


        /// <summary>
        /// sends the message allocated to the CurrentMessage property
        /// </summary>
        private void Send()
        {
            if (CurrentMessage != null && CurrentMessage != "")
            {
                //stores the message in a variable so that it can be added locally in plaintext
                string clearContent = CurrentMessage;
                Message message = new Message()
                {
                    IsMine = true,
                    Sender = App.CurrentUser,
                    Receiver = CurrentConvo.Recipient,
                    Creation = DateTime.Now,

                    Content = CurrentMessage,

                };
                AddMessage(message);
           


                //encrypts plaintext using aes
                byte[] encryptedBytes = EncryptStringToBytes_Aes(CurrentMessage, AesService.Key, AesService.IV);
                //format as base64 so that it can be sent as a string
                message.Content = Convert.ToBase64String(encryptedBytes);
                //debugging purposes
   
                //generate the hmac for hashing purposes
                message.HMAC = Hashing.GetHMAC(Convert.ToBase64String(Combine(AesService.Key, encryptedBytes, _myRsaAes)), Convert.ToBase64String(AesService.Key));
                message.InitVector = Convert.ToBase64String(AesService.IV);

                SendToUser(message);

                //set the messages content back to the plaintext content so that it shows in the UI as plaintext
                message.Content = clearContent;
            }
            else
            {
                App.Current.MainPage.DisplayAlert("Warning", "Message cannot be empty", "Ok");
            }
           
            


        }

        /// <summary>
        /// combines multiple byte arrays for cryptography purposes
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="third"></param>
        /// <returns></returns>
        private static byte[] Combine(byte[] first, byte[] second, byte[] third)
        {
            byte[] ret = new byte[first.Length + second.Length + third.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            Buffer.BlockCopy(third, 0, ret, first.Length + second.Length,
                third.Length);
            return ret;
        }



        /// <summary>
        /// encrypts a given string with aes using the user's AES key derived from RSA
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="Key"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        public byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {

            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        /// <summary>
        /// Decrypts a base64 or byte array of ciphertext using the RSA decrypted AES key
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="Key"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        public string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Declare the string used to hold
            // the decrypted text.

            string plaintext = null;
            try
            {
                // Create an Aes object
                // with the specified key and IV.
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.Key = Key;
                    aesAlg.IV = IV;

                    // Create a decryptor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    // Create the streams used for decryption.
                    using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {

                                // Read the decrypted bytes from the decrypting stream
                                // and place them in a string.
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {

                App.Current.MainPage.DisplayAlert("Warning", "Error: " + exc, "Ok");
            }





            return plaintext;
        }
    }
}
