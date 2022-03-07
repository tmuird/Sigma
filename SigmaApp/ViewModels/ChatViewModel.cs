using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Windows.Input;
using crypto;
using Microsoft.AspNetCore.SignalR.Client;
using Prism.Commands;
using Refit;
using SigmaApp.API;
using SigmaApp.Models;


namespace SigmaApp.ViewModels
{
    public class ChatViewModel
    {






        public string CurrentMessage { get; set; }
        public HubConnection _connection;
        public IUserInfo api;

        public Conversation CurrentConvo { get; set; }
     
        public User Sender { get; set; }
        public ObservableCollection<User> Contacts { get; set; }

        public ObservableCollection<Conversation> Conversations { get; set; } = new ObservableCollection<Conversation>();
        public string url = "http://nea.speedi.codes";

        public ICommand GoToChatCommand { get; }
        public ICommand StartChatCommand { get; }


        public Command GoToAddContactCommand { get; }
        public Command SendMessageCommand { get; }
 

        public Dictionary<string, string> UserKeys = new Dictionary<string, string>();

        private byte[] myRSAAES;

        public Aes myAesService = Aes.Create();





        public ChatViewModel()
        {
            api = RestService.For<IUserInfo>($"{url}");

            Sender = App.CurrentUser;

            _connection = new HubConnectionBuilder()
             .WithUrl($"{url}/chatHub",
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

            StartChatCommand = new DelegateCommand<User>(StartChat);
            GoToChatCommand = new DelegateCommand<Conversation>(GoToChat);
            StartChatCommand = new DelegateCommand<User>(StartChat);
            GoToAddContactCommand = new Command(GoToAddContact);
            SendMessageCommand = new Command(Send);
            Contacts = new ObservableCollection<User>();

            try
            {
                //signalr event for recieving messages
                _connection.On<Message>("Receive", (message) =>
                {
                    try
                    {
                        message.Content = (DecryptStringFromBytes_Aes(Convert.FromBase64String(message.Content), Convert.FromBase64String(UserKeys[(message.Sender.UserId)]), Convert.FromBase64String(message.InitVector)));

                    }
                    catch (Exception ext)
                    {

                        App.Current.MainPage.DisplayAlert("Warning", ext.Message, "Ok");

                    }

                    message.IsMine = false;

                    try
                    {


                        if (Conversations.Any(convo => convo.Recipient.UserId == message.Sender.UserId))

                        {

                            Conversation userConvo = Conversations.Single(s => s.Recipient.UserId == message.Sender.UserId);
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

                        UserKeys.Add(key[0], crypto.RSA.Decrypt(App.KeyChain['d'], App.KeyChain['N'], RSAAES));

                    }
                    catch (Exception exc)
                    {

                        App.Current.MainPage.DisplayAlert("Warning", "Error: " + exc, "Ok");
                    }


                });
            }
            catch (Exception)
            {

                throw;
            }


        }

        /// <summary>
        /// initialises the AES service
        /// </summary>
        public void InitCrypt()
        {
            myAesService.Padding = PaddingMode.PKCS7;
            myAesService.GenerateKey();
            myAesService.GenerateIV();

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
        public async void StartChat(User reciever)
        {
            try
            {


                if (Conversations.Any(convo => convo.Recipient.UserId == reciever.UserId))
                {

                    Conversation userConvo = Conversations.Single(s => s.Recipient.UserId == reciever.UserId);

                    CurrentConvo = userConvo;
                }
                else
                {

                    Conversation newConvo = new Conversation()
                    {
                        Recipient = reciever,
                    };

                    myRSAAES = crypto.RSA.Encrypt(Convert.FromBase64String(reciever.PublicKey.Split(",")[0]), Convert.FromBase64String(reciever.PublicKey.Split(",")[1]), Convert.ToBase64String(myAesService.Key));
                    SendRSAAES(Convert.ToBase64String(myRSAAES), reciever.UserId);
                    Conversations.Add(newConvo);
                    CurrentConvo = newConvo;
                }
            }
            catch (Exception ex)
            {

                App.Current.MainPage.DisplayAlert("Warning", ex.Message, "Ok");
            }
            await Shell.Current.GoToAsync($"MessagePage?recipient={reciever}");
        }
        /// <summary>
        /// Sends the message to the signalR hub and then the determined user
        /// </summary>
        /// <param name="message"></param>
        public async Task SendToUser(Message message)
        {

            try
            {

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
                    rsaAES, App.CurrentUser.UserId, receiverId);
            }
            catch (Exception ex)
            {
                App.Current.MainPage.DisplayAlert("Warning", "Error: " + ex, "Ok");
            }
        }

        public async Task GetUserID()
        {
            await _connection.InvokeAsync("GetUserID");
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
                myRSAAES = crypto.RSA.Encrypt(Convert.FromBase64String(conversation.Recipient.PublicKey.Split(",")[0]), Convert.FromBase64String(conversation.Recipient.PublicKey.Split(",")[1]), Convert.ToBase64String(myAesService.Key));
                SendRSAAES(Convert.ToBase64String(myRSAAES), conversation.Recipient.UserId);
                //string jsonConvo = JsonSerializer.Serialize(conversation);
                //App.Current.MainPage.DisplayAlert("Warning", jsonConvo, "Ok");

                await Shell.Current.GoToAsync($"MessagePage?recipient={conversation.Recipient.UserId}");
            }
            catch (Exception ex)
            {

                App.Current.MainPage.DisplayAlert("Warning", "Error: " + ex, "Ok");
            }


        }
        private void AddMessage(Message message)
        {
            CurrentConvo.Messages.Add(message);
        }


        /// <summary>
        /// sends the message allocated to the CurrentMessage property
        /// </summary>
        private void Send()
        {


            string ClearContent = CurrentMessage;
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
            byte[] encryptedBytes = EncryptStringToBytes_Aes(CurrentMessage, myAesService.Key, myAesService.IV);
            //format as base64 so that it can be sent as a string
            message.Content = Convert.ToBase64String(encryptedBytes);
            //debugging purposes
            App.Current.MainPage.DisplayAlert("Warning", Convert.ToBase64String(encryptedBytes), "Ok");
            //generate the hmac for hashing purposes
            message.HMAC = Hashing.GetHMAC(Convert.ToBase64String(Combine(myAesService.Key, encryptedBytes, myRSAAES)), Convert.ToBase64String(myAesService.Key));
            message.InitVector = Convert.ToBase64String(myAesService.IV);
  
            SendToUser(message);

            //set the messages content back to the plaintext content so that it shows in the UI as plaintext
            message.Content = ClearContent;



        }

        /// <summary>
        /// combines multiple byte arrays for cryptography purposes
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="third"></param>
        /// <returns></returns>
        static byte[] Combine(byte[] first, byte[] second, byte[] third)
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
