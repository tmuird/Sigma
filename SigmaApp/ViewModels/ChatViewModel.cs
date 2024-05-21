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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SigmaApp.ViewModels
{
    public class ChatViewModel : BaseViewModel
    {
        public string CurrentMessage { get; set; }
        private readonly HubConnection _connection;
        public IUserInfo Api { get; private set; }
        private Conversation _currentConvo;
        public Conversation CurrentConvo
        {
            get => _currentConvo;
            set
            {
                _currentConvo = value;
                OnPropertyChanged();
            }
        }
        public User Sender { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        // Make the set accessors public
        public ObservableCollection<User> Contacts { get; set; }
        public ObservableCollection<Conversation> Conversations { get; set; } = new ObservableCollection<Conversation>();


        private const string _url = "https://sigma.neonnet.uk";
        public ICommand GoToChatCommand { get; private set; }
        public ICommand StartChatCommand { get; private set; }
        public ICommand DeleteChatCommand { get; private set; }
        public ICommand DeleteContactCommand { get; private set; }
        public ICommand GoToAddContactCommand { get; private set; }
        public ICommand SendMessageCommand { get; private set; }
        public ICommand LogoutCommand { get; private set; }
        public Dictionary<string, byte[]> UserKeys { get; private set; }
        private byte[] clientKeyCipher;
        public Aes AesService { get; private set; }

        public ChatViewModel()
        {
            AesService = Aes.Create();
            Api = RestService.For<IUserInfo>(_url);
            Sender = App.CurrentUser;
            _connection = new HubConnectionBuilder()
                .WithUrl($"{_url}/chatHub", options =>
                {
                    options.HttpMessageHandlerFactory = handler =>
                    {
                        var newHandler = new HttpClientHandler();
                        newHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, sslPolicyErrors) => true;
                        return newHandler;
                    };
                })
                .Build();

            InitializeCommands();
            InitializeCollections();
            InitializeSignalREvents();
        }

        private void InitializeCommands()
        {
            StartChatCommand = new DelegateCommand<User>(StartChat);
            GoToChatCommand = new DelegateCommand<Conversation>(GoToChat);
            DeleteChatCommand = new DelegateCommand<Conversation>(DeleteChat);
            DeleteContactCommand = new DelegateCommand<User>(DeleteContact);
            GoToAddContactCommand = new Command(GoToAddContact);
            SendMessageCommand = new Command(Send);
            LogoutCommand = new Command(Logout);
        }

        private void InitializeCollections()
        {
            Contacts = new ObservableCollection<User>();
            Conversations = new ObservableCollection<Conversation>();
            UserKeys = new Dictionary<string, byte[]>();
        }

        private void InitializeSignalREvents()
        {
            _connection.On<Message>("Receive", OnMessageReceived);
            _connection.On<string[]>("ReceiveRSA", OnReceiveRSAAESKey);
        }
        public void InitCrypt()
        {
            AesService = Aes.Create();
            AesService.Padding = PaddingMode.PKCS7;
            AesService.GenerateKey();
            AesService.GenerateIV();
        }
        private async void OnMessageReceived(Message message)
        {
            try
            {
                if (string.IsNullOrEmpty(message.Content))
                {
                    await App.Current.MainPage.DisplayAlert("Warning", "Received message content is empty", "Ok");
                    return;
                }

                Console.WriteLine("Step 1: Received Message Content");
                Console.WriteLine($"Received Message: {message.Content}");

                byte[] contentBytes;
                try
                {
                    contentBytes = Convert.FromBase64String(message.Content);
                    Console.WriteLine("Step 2: Converted Message Content from Base64 to Byte Array");
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Error converting message content from Base64: {ex.Message}");
                    await App.Current.MainPage.DisplayAlert("Warning", "Message content is not a valid Base64 string", "Ok");
                    return;
                }

                if (UserKeys.TryGetValue(message.Sender.UserID, out byte[] aesKey))
                {
                    Console.WriteLine($"AES Key: {Convert.ToBase64String(aesKey)}");

                    string localHMAC;
                    try
                    {
                        localHMAC = Hashing.GetHMAC(Convert.ToBase64String(Combine(aesKey, contentBytes, Convert.FromBase64String(message.InitVector))), Convert.ToBase64String(aesKey));
                        Console.WriteLine($"Step 4: Generated Local HMAC: {localHMAC}");
                        Console.WriteLine($"Received HMAC: {message.HMAC}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error generating HMAC: {ex.Message}");
                        await App.Current.MainPage.DisplayAlert("Warning", "Error generating HMAC", "Ok");
                        return;
                    }

                    if (message.HMAC != localHMAC)
                    {
                        await App.Current.MainPage.DisplayAlert("Warning", "Invalid hash", "Ok");
                        return;
                    }
                    Console.WriteLine("Step 5: HMAC Verification Successful");

                    try
                    {
                        message.Content = DecryptStringFromBytes_Aes(contentBytes, aesKey, Convert.FromBase64String(message.InitVector));
                        Console.WriteLine($"Decrypted Content: {message.Content}");
                    }
                    catch (Exception decryptionEx)
                    {
                        Console.WriteLine($"Decryption Error: {decryptionEx.Message}");
                        if (decryptionEx.InnerException != null)
                        {
                            Console.WriteLine($"Inner Decryption Error: {decryptionEx.InnerException.Message}");
                        }
                        await App.Current.MainPage.DisplayAlert("Warning", "Decryption failed", "Ok");
                        return;
                    }

                    AddOrUpdateConversation(message);
                }
                else
                {
                    Console.WriteLine($"AES Key for user {message.Sender.UserID} not found.");
                    await App.Current.MainPage.DisplayAlert("Warning", $"AES Key for user {message.Sender.UserID} not found.", "Ok");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                await App.Current.MainPage.DisplayAlert("Warning", ex.Message, "Ok");
            }
        }








        private void AddOrUpdateConversation(Message message)
        {
            try
            {
            
              
                    var existingConvo = Conversations.FirstOrDefault(c => c.Recipient.UserID == message.Sender.UserID);

                    if (existingConvo != null)
                    {
                        existingConvo.Messages.Add(message);
                        existingConvo.RecentMessage = message.Content;
                        CurrentConvo = existingConvo;
                    }
                    else
                    {
                        var newConvo = new Conversation
                        {
                            Recipient = message.Sender,
                            RecentMessage = message.Content
                        };
                        newConvo.Messages.Add(message);
                        Conversations.Add(newConvo);
                        CurrentConvo = newConvo;
                    }
              
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddOrUpdateConversation Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                App.Current.MainPage.DisplayAlert("Warning", "Error: " + ex.Message, "Ok");
            }
        }




        private async void OnReceiveRSAAESKey(string[] key)
        {
            try
            {
                byte[] RSAAES = Convert.FromBase64String(key[1]);
                if (!UserKeys.ContainsKey(key[0]))
                {
                    byte[] decryptedAES = Convert.FromBase64String(crypto.RSA.Decrypt(App.KeyChain['d'], App.KeyChain['N'], RSAAES));
                    UserKeys.Add(key[0], decryptedAES);
                    Console.WriteLine($"Decrypted AES Key: {BitConverter.ToString(decryptedAES)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RSA Key Reception Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                await App.Current.MainPage.DisplayAlert("Warning", "Error: " + ex, "Ok");
            }
        }



        private async void DeleteContact(User contact)
        {
            try
            {
                var conversation = Conversations.FirstOrDefault(s => s.Recipient == contact);
                if (conversation != null)
                {
                    DeleteChat(conversation);
                }

                Contacts.Remove(contact);
                using (var context = new LocalContext())
                {
                    context.Users.Remove(contact);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async void DeleteChat(Conversation convo)
        {
            try
            {
                Conversations.Remove(convo);
                using (var context = new LocalContext())
                {
                    context.Conversations.Remove(convo);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async void GoToAddContact()
        {
            try
            {
                await Shell.Current.GoToAsync($"AddContactPage");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Warning", "Error: " + ex, "Ok");
            }
        }

        private async void StartChat(User receiver)
        {
            try
            {
                var existingConvo = Conversations.FirstOrDefault(convo => convo.Recipient.UserID == receiver.UserID);
                if (existingConvo != null)
                {
                    GoToChat(existingConvo);
                    return;
                }

                var newConvo = new Conversation { Recipient = receiver };
                Conversations.Add(newConvo);
                CurrentConvo = newConvo;

                // Save the conversation to the database
                using (var context = new LocalContext())
                {
                    context.Conversations.Add(newConvo);
                    await context.SaveChangesAsync();
                }

                // Encrypt AES key and send it
                clientKeyCipher = crypto.RSA.Encrypt(Convert.FromBase64String(receiver.PublicKey.Split(",")[0]), Convert.FromBase64String(receiver.PublicKey.Split(",")[1]), Convert.ToBase64String(AesService.Key));
                SendRSAAES(Convert.ToBase64String(clientKeyCipher), receiver.UserID);

                await Shell.Current.GoToAsync($"MessagePage?recipient={receiver}");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Warning", ex.Message, "Ok");
            }
        }


        private async Task SendToUser(Message message)
        {
            try
            {
                message.IsMine = true;

                Console.WriteLine($"Sending message: {message.Content}");
                await _connection.InvokeAsync("SendToUser", message);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Warning", "Error: " + ex, "Ok");
            }
        }

        public async void SendRSAAES(string rsaAES, string receiverId)
        {
            try
            {
                await _connection.InvokeAsync("SendRSAAES", rsaAES, App.CurrentUser.UserID, receiverId);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Warning", "Error: " + ex, "Ok");
            }
        }

        public async Task Connect()
        {
            try
            {
                await _connection.StartAsync();
                await _connection.InvokeAsync("UserConnected", App.CurrentUser);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

        private async void GoToChat(Conversation conversation)
        {
            try
            {
                CurrentConvo = conversation;
                clientKeyCipher = crypto.RSA.Encrypt(Convert.FromBase64String(conversation.Recipient.PublicKey.Split(",")[0]), Convert.FromBase64String(conversation.Recipient.PublicKey.Split(",")[1]), Convert.ToBase64String(AesService.Key));
                SendRSAAES(Convert.ToBase64String(clientKeyCipher), conversation.Recipient.UserID);
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
            await Disconnect();
            await Shell.Current.GoToAsync($"LoginPage");
        }

        public void AddMessage(Message message)
        {
            try
            {
                var conversation = Conversations.SingleOrDefault(p => p.Recipient == CurrentConvo.Recipient);
                if (conversation != null)
                {
                    message.ConversationID = conversation.ConversationID;
                    conversation.RecentMessage = message.Content;
                    conversation.Messages.Add(message);

                    using (var context = new LocalContext())
                    {
                        context.Messages.Add(message);
                        context.SaveChanges();
                    }
                }
                else
                {
                    App.Current.MainPage.DisplayAlert("Warning", "Conversation not found", "Ok");
                }
            }
            catch (Exception ex)
            {
                App.Current.MainPage.DisplayAlert("Warning", "Error: " + ex.Message, "Ok");
            }
        }

        private void Send()
        {
            if (!string.IsNullOrEmpty(CurrentMessage))
            {
                Console.WriteLine($"Original Message: {CurrentMessage}");

                string clearContent = CurrentMessage;
                var message = new Message
                {
                    IsMine = true,
                    Sender = App.CurrentUser,
                    Receiver = CurrentConvo.Recipient,
                    Creation = DateTime.Now,
                    Content = CurrentMessage,
                };

                AddMessage(message);

                // Encrypt the message content using AES
                byte[] encryptedBytes = EncryptStringToBytes_Aes(CurrentMessage, AesService.Key, AesService.IV);
                Console.WriteLine($"Encrypted Message Bytes: {BitConverter.ToString(encryptedBytes)}");

                message.Content = Convert.ToBase64String(encryptedBytes);
                message.HMAC = Hashing.GetHMAC(Convert.ToBase64String(Combine(AesService.Key, encryptedBytes, AesService.IV)), Convert.ToBase64String(AesService.Key));
                message.InitVector = Convert.ToBase64String(AesService.IV);

                Console.WriteLine($"Encrypted Message: {message.Content}");

                SendToUser(message);

                message.Content = clearContent;
                CurrentMessage = string.Empty;
            }
            else
            {
                App.Current.MainPage.DisplayAlert("Warning", "Message cannot be empty", "Ok");
            }
        }




        private static byte[] Combine(byte[] first, byte[] second, byte[] third)
{
    byte[] combined = new byte[first.Length + second.Length + third.Length];
    Buffer.BlockCopy(first, 0, combined, 0, first.Length);
    Buffer.BlockCopy(second, 0, combined, first.Length, second.Length);
    Buffer.BlockCopy(third, 0, combined, first.Length + second.Length, third.Length);

    Console.WriteLine($"Combined Byte Array: {BitConverter.ToString(combined)}");
    return combined;
}


        public byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException(nameof(Key));
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException(nameof(IV));

            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            Console.WriteLine($"EncryptStringToBytes_Aes - Encrypted Bytes: {BitConverter.ToString(encrypted)}");
            return encrypted;
        }


        public string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.Key = Key;
                    aesAlg.IV = IV;

                    using (var msDecrypt = new MemoryStream(cipherText))
                    using (var csDecrypt = new CryptoStream(msDecrypt, aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV), CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        string plaintext = srDecrypt.ReadToEnd();
                        Console.WriteLine($"Decrypted Text: {plaintext}");
                        return plaintext;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Decryption Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                App.Current.MainPage.DisplayAlert("Warning", "Error: " + ex.Message, "Ok");
                return null;
            }
        }



    }
}
