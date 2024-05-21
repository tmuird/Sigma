# Sigma Encrypted Messaging App

![Logo](path_to_logo_image) <!-- Add a logo if available -->

Sigma is a secure and encrypted messaging application built using .NET MAUI. This app provides end-to-end encryption for private conversations, ensuring the security and privacy of your messages.

## Features

- **End-to-End Encryption**: Messages are encrypted using AES and RSA algorithms to ensure privacy and security.
- **User Authentication**: Secure user authentication and key management.
- **Real-Time Messaging**: Instant messaging with real-time updates using SignalR.
- **Cross-Platform Support**: Available on Android, iOS, macOS, and Windows.

## Screenshots

<!-- Add screenshots of your app here -->
![Screenshot1](path_to_screenshot1)
![Screenshot2](path_to_screenshot2)
![Screenshot3](path_to_screenshot3)

## Getting Started

### Prerequisites

- .NET 6.0 SDK or later
- Visual Studio 2022 or later with .NET MAUI workload installed
- SQLite database

### Installation

1. **Clone the repository**

   ```sh
   git clone https://github.com/your_username/sigma-encrypted-messaging-app.git
   cd sigma-encrypted-messaging-app
   ```

2. **Restore NuGet packages**

   ```sh
   dotnet restore
   ```

3. **Update the configuration**

   Update the configuration settings in `appsettings.json` or relevant configuration files.

4. **Run the application**

   Open the solution in Visual Studio and run the project on your preferred platform.

### Usage

1. **Register a New User**

   - Open the app and register with a unique username.
   - The app generates RSA key pairs and stores them securely.

2. **Add Contacts**

   - Navigate to the "Add Contact" page.
   - Enter the username of the contact you want to add.

3. **Start a Conversation**

   - Select a contact from your contacts list.
   - Start a new conversation and send encrypted messages.

## Project Structure

- **Models**: Contains data models such as `User`, `Message`, and `Conversation`.
- **ViewModels**: Contains view models to manage the UI logic.
- **Views**: Contains XAML pages and their code-behind files.
- **Services**: Contains services for encryption, decryption, and API interactions.
- **Converters**: Contains value converters for data binding in XAML.

## Encryption Details

### AES Encryption

- **Key Size**: 256-bit
- **Mode**: CBC (Cipher Block Chaining)
- **Padding**: PKCS7

### RSA Encryption

- **Key Size**: 2048-bit or higher
- **Usage**: Encrypting AES keys for secure transmission

### HMAC

- **Algorithm**: HMAC-SHA256
- **Usage**: Ensuring message integrity and authenticity

## Contributing

Contributions are welcome! Please fork the repository and submit pull requests for any enhancements, bug fixes, or new features.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

Distributed under the MIT License. See `LICENSE` for more information.

## Contact

**Tom Muir**
- **GitHub**: [tmuird](https://github.com/tmuird)

## Acknowledgements

- [Prism](https://github.com/PrismLibrary/Prism)
- [SignalR](https://github.com/dotnet/aspnetcore/tree/main/src/SignalR)
- [Refit](https://github.com/reactiveui/refit)
- [crypto](https://github.com/your_username/crypto-library)

---

<!-- Optional: Add badges for build status, license, etc. -->

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue)

```

### Instructions to Customize the README

1. **Logo and Screenshots**: Add paths to your logo and screenshots in the `README.md` file.
2. **Configuration Settings**: Specify how to update configuration settings (e.g., `appsettings.json`).
3. **Contact Information**: Update your contact information and GitHub profile link.
4. **Acknowledgements**: Add any additional libraries or resources used in your project.
