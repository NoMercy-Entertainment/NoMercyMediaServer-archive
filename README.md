

         \####################################################################################  
   \#                        ⚠️  WORK IN PROGRESS - USE WITH CAUTION ⚠️                        #  
 \#    This repository is currently under development and should not be considered stable or reliable.    #  
\####################################################################################

<img src="https://raw.githubusercontent.com/NoMercy-Entertainment/NoMercyMediaServer/master/NoMercy.Server/Assets/logo.png" style="width: auto;height: 240px;">

**"Empower Ownership: Overcome Licensing Barriers and Effortlessly Archive Your Media Collection with Privacy and Simplicity."**

## Features

- **Automatic Encoding**: Effortlessly convert your media files into various formats.
- **Comprehensive Media Management**: Organize and curate your library with ease.
- **Remote Streaming**: Access your media collection from anywhere, just like popular streaming platforms.
- **User-Friendly Interface**: Enjoy a sleek, intuitive design that’s easy to navigate.

## Server Switching

NoMercy MediaServer also features seamless server switching within the user interface, providing:

- **Separate Watch Histories**: Maintain distinct watch histories for each server, keeping your viewing data private and organized.
- **Enhanced Privacy**: Manage your media experiences independently across different servers without any data crossover.
- **Greater Flexibility**: Easily switch between servers to explore different content libraries or maintain separate user profiles.

This feature guarantees a tailored, private, and flexible media experience, perfectly suited to your needs.

## Secure Connection & Port Configuration

**NoMercy MediaServer** provides a fully trusted SSL certificate to ensure a secure connection for both internal and external access to your media server.

## Account Requirement

A user account is **mandatory** for using NoMercy MediaServer. This account is essential for:

- **Privacy and Security**: Protect your data and maintain privacy while enabling personalized features.
- **Custom Playlists**: Create, share, and enjoy personalized playlists with friends and the community.
- **Exclusive Content**: Access special offers, community plugins, and user-generated content.
- **Social Interaction**: Connect with others, share recommendations, and be part of the NoMercy community.

**Important:** While an account is required to access the full range of features, your media files will **never** be locked behind this account or any form of DRM. They will always remain freely available from the storage device on which they are stored. We are committed to ensuring that your files are accessible at all times, regardless of your account status.

We have many more exciting features planned for the future, enhancing your experience even further!

## Installation

To set up NoMercy MediaServer on your local machine:

1. Clone the repository:
   ```bash
   git clone https://github.com/NoMercy-Entertainment/NoMercyMediaServer.git
   ```
2. Navigate into the project directory:
   ```bash
   cd NoMercyMediaServer
   ```
3. Restore the necessary dependencies and build the project:
   ```bash
   dotnet restore
   dotnet build
   ```

## Usage

1. Start the server:
   ```bash
   dotnet run
   ```
2. If you are on a desktop, it will authenticate the server by logging in from the browser. If you are on a server, it will ask you for your credentials.
3. Open your browser and go to [https://app.nomercy.tv](https://app.nomercy.tv) to access the web interface.
4. Begin adding your media files and enjoy seamless access and management!

### External Access & Port Forwarding

To access your server from outside your home network, you need to forward the port `7626` on your router. This will enable remote connections while maintaining a secure environment.

### Custom Port Configuration

You can customize the internal and external ports by using the following options when launching the server:

- **Internal Port**: `--internal-port=<number>`
- **External Port**: `--external-port=<number>`

These settings allow for flexible networking configurations, ensuring that the NoMercy MediaServer fits seamlessly into your existing network setup.

## Contact

For further information or support, visit NoMercy.tv or contact our support team.

Made with ❤️ by [NoMercy Entertainment](https://nomercy.tv)
