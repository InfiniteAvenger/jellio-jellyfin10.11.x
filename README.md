# Jellio – Jellyfin to Stremio Plugin

**Jellio** is a Jellyfin plugin that bridges your media library to Stremio, enabling seamless streaming of your personal movies and TV shows through the Stremio interface.

---

## 🎯 Features

- **Native Jellyfin Integration**: Installs directly as a Jellyfin plugin—no external services required
- **Stremio-Compatible Manifest**: Exposes your Jellyfin libraries as Stremio catalogs
- **Robust Authentication**: Supports both token-based API access and session-based (cookie) authentication
- **Multi-Library Support**: Select which Jellyfin libraries to expose (Movies, TV Shows)
- **Modern Architecture**: Built for Jellyfin 10.11.x with .NET 9, featuring defensive error handling and user-specific library resolution
- **Web-Based Configuration**: Simple web UI for generating Stremio addon URLs with your credentials and library selections
- **Direct Streaming**: Media streams directly from your Jellyfin server to Stremio clients

---

## 📦 Installation

### Method 1: Install from Custom Repository (Recommended)

1. Open your Jellyfin Dashboard
2. Navigate to **Plugins → Repositories**
3. Click **Add** and paste the following URL:
   ```
   https://raw.githubusercontent.com/InfiniteAvenger/jellio/metadata/jellyfin-repo-manifest.json
   ```
4. Go to **Plugins → Catalog**
5. Find **Jellio** and click **Install**
6. Restart Jellyfin when prompted

### Method 2: Manual Installation

1. Download the latest `Jellio-vX.X.X.zip` from [Releases](https://github.com/InfiniteAvenger/jellio/releases)
2. Extract the ZIP to your Jellyfin plugins directory:
   - **Windows**: `C:\ProgramData\Jellyfin\Server\plugins\Jellio\`
   - **Linux**: `/var/lib/jellyfin/plugins/Jellio/`
   - **Docker**: `/config/plugins/Jellio/`
3. Restart Jellyfin

---

## 🚀 Configuration

### 1. Access the Configuration UI

After installing the plugin, navigate to:
```
https://<your-jellyfin-server>/jellio/configure
```

Or click the **Jellio** entry in your Jellyfin Dashboard under **Plugins → My Plugins**.

### 2. Generate Your Stremio Addon URL

1. **Server Name**: Enter a friendly name for your Jellyfin server (e.g., "My Home Server")
2. **Authentication Token**: 
   - Go to Jellyfin Dashboard → **Users → [Your User] → API Keys**
   - Click **New API Key**, give it a name, and copy the generated token
   - Paste the token into the configuration UI
3. **Select Libraries**: Choose which Jellyfin libraries (Movies/TV) to expose to Stremio
4. Click **Generate Addon URL**
5. Copy the generated URL

### 3. Add to Stremio

1. Open Stremio on any device
2. Click the **Addons** icon (puzzle piece) in the top-right
3. Click **Community Addons**
4. Paste your generated Jellio addon URL
5. Click **Install**

Your Jellyfin libraries will now appear as catalogs in Stremio!

---

## 🛠️ Requirements

- **Jellyfin**: Version 10.11.0 or higher
- **.NET Runtime**: .NET 9 (installed automatically with Jellyfin 10.11.x)
- **Network Access**: Jellyfin server must be accessible from devices running Stremio

---

## 🔧 Development

### Building from Source

**Prerequisites:**
- .NET 9 SDK
- Node.js 20+

**Build Steps:**
```bash
# Clone the repository
git clone https://github.com/InfiniteAvenger/jellio.git
cd jellio

# Build the frontend
cd jellio-web
npm ci
npm run build

# Copy frontend to plugin
cp dist/index.html ../Jellyfin.Plugin.Jellio/Web/index.html

# Build the plugin
cd ..
dotnet build Jellyfin.Plugin.Jellio/Jellyfin.Plugin.Jellio.csproj -c Release
```

The compiled DLL will be in:
```
Jellyfin.Plugin.Jellio/bin/Release/net9.0/Jellyfin.Plugin.Jellio.dll
```

---

## 📖 How It Works

1. **Plugin Endpoints**: Jellio exposes Stremio-compatible REST endpoints:
   - `/jellio/{config}/manifest.json` – Stremio addon manifest
   - `/jellio/{config}/catalog/{type}/{id}.json` – Library catalogs (with search/pagination)
   - `/jellio/{config}/meta/{type}/{id}.json` – Media metadata (episodes, descriptions, posters)
   - `/jellio/{config}/stream/{type}/{id}.json` – Direct stream URLs from Jellyfin

2. **Authentication**: Uses Jellyfin's native authentication:
   - API tokens for programmatic access
   - Session cookies for web UI access
   - User-specific library filtering ensures privacy and access control

3. **Configuration Encoding**: Your server name, auth token, and library selections are Base64-encoded into the addon URL, so no external database is needed

---

## 🤝 Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📝 License

This project is licensed under the **MIT License**. See [LICENSE](LICENSE) for details.

---

## 🙏 Acknowledgements

- Built for [Jellyfin](https://jellyfin.org/) – the free, open-source media server
- Designed to work with [Stremio](https://www.stremio.com/) – the modern media center
- Community-maintained fork with Jellyfin 10.11.x support

---

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/InfiniteAvenger/jellio/issues)
- **Discussions**: [GitHub Discussions](https://github.com/InfiniteAvenger/jellio/discussions)

---

**Enjoy seamless Jellyfin streaming in Stremio! 🎬**