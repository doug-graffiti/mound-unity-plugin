# mOUND Unity Plugin 🚀

[![Unity Version](https://img.shields.io/badge/Unity-6.0%2B%20%7C%20.NET%206-blue.svg)](https://unity3d.com/get-unity/download)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-WebGL-orange.svg)](https://docs.unity3d.com/Manual/webgl.html)

**The ultimate Unity WebGL deployment solution with advanced version control, chunked uploads, and seamless organization management.**

![mOUND Plugin Demo](https://via.placeholder.com/800x400/4F46E5/FFFFFF?text=mOUND+Unity+Plugin+Demo)

## ✨ Features

🚀 **One-Click Deployment** - Build WebGL and upload to mOUND Platform in seconds  
📦 **Chunked Upload System** - Upload files up to 100MB with automatic chunking  
🔄 **Version Control** - Create, update, and manage multiple app versions  
🔐 **Secure Authentication** - Multiple login methods with persistent credentials  
🏢 **Organization Management** - Upload to any organization you're a member of  
📊 **Real-Time Progress** - Visual progress tracking for builds and uploads  
🧹 **Smart Cleanup** - Automatic removal of temporary build files  
⚙️ **Persistent Settings** - Remembers your preferences between Unity sessions  
🌐 **Public/Private Apps** - Control application visibility with one checkbox  
🔧 **Unity 6 + .NET 6** - Modern async networking with HttpClient and UnityWebRequest fallbacks  
🎨 **Modern UI** - Beautiful, intuitive interface with emoji icons and organized sections  

## 🎯 Quick Start

### Installation

**Unity Package Manager (Recommended):**
```
1. Window > Package Manager
2. + > Add package from disk
3. Select package.json from this repository
```

**Manual Installation:**
```
1. Download and extract this repository
2. Copy Assets/mOUND/ to your Unity project's Assets folder
```

### Usage

1. **Access Plugin**: `mOUND > Build and Upload` in Unity menu
2. **Quick Login**: Enter your username/email and password for instant access
3. **Select Organization**: Choose which organization to deploy to
4. **Choose Mode**: Create new app or update existing app with version control
5. **Configure**: Set app name, description, changelog (for updates), and visibility
6. **Deploy**: Click "🚀 Build WebGL & Upload" for automatic chunked upload ✨

### Alternative Authentication

If direct login doesn't work:
1. Click "🌐 Open mOUND Platform in Browser"
2. Login and go to your Profile page
3. Copy your API Token
4. Paste it in the plugin and click "✅ Validate Token"

![Plugin Interface](https://via.placeholder.com/600x400/10B981/FFFFFF?text=Plugin+Interface+Screenshot)

## 📦 Chunked Upload System

The plugin automatically handles large file uploads using our advanced chunked upload system:

- **Automatic Detection**: Files > 20MB automatically use chunked upload
- **20MB Chunks**: Files are split into 20MB chunks for optimal upload
- **Progress Tracking**: Real-time progress bar shows chunk upload status
- **Error Recovery**: Individual chunk failures don't require full re-upload
- **100MB Limit**: Support for files up to 100MB (vs 32MB direct upload limit)

### Upload Methods

1. **Direct Upload** (≤20MB): Fast single-request upload
2. **Chunked Upload** (>20MB): Automatic chunking with progress tracking
3. **Fallback Support**: Multiple upload methods ensure reliability

## 📋 Requirements

- **Unity**: 6.0+ (optimized for Unity 6 with .NET 6)
- **API Compatibility**: .NET 6.0 (preferred) or .NET Standard 2.1 (fallback)
- **Platform**: WebGL build support installed
- **Account**: Active mOUND Platform account
- **Network**: Internet connection for uploads

## 🔧 Unity 6 + .NET 6 Networking

The plugin provides **dual networking methods** for maximum compatibility:

- **🔧 Try .NET 6 HttpClient Method** - Modern async networking (recommended for Unity 6)
- **🌐 Test Basic Connectivity** - Fallback UnityWebRequest method with comprehensive diagnostics

If UnityWebRequest fails (common in Unity 6), the .NET 6 HttpClient method often works better.

## 🛠️ Advanced Configuration

Access advanced settings via `mOUND > Settings`:
- Custom API URLs
- Build compression options  
- Default application settings
- Upload preferences

## 📚 Documentation

- **[Quick Setup Guide](SETUP.md)** - Get started in 2 minutes
- **[Unity Configuration](UNITY-SETUP.md)** - Required Unity settings and troubleshooting
- **[Developer Guide](DEVELOPER.md)** - Extend and customize the plugin
- **[Changelog](CHANGELOG.md)** - Version history and updates

## 🤝 Contributing

We welcome contributions! Please see our [Developer Guide](DEVELOPER.md) for:
- Development setup
- Code style guidelines
- Testing procedures
- Submission process

## 📞 Support

**Need Help?**
- 🌐 Visit: [mOUND Platform](https://mound.gllc.io)
- 📧 Email: support@gllc.io
- 📖 Docs: [Platform Documentation](https://mound.gllc.io/docs)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🏆 Why mOUND Platform?

- **Fast Deployment** - Upload and share Unity WebGL apps instantly
- **Team Collaboration** - Organization-based app management
- **Public Sharing** - Make your creations discoverable
- **Version Control** - Track and manage app updates
- **Mobile Optimized** - Apps work seamlessly on all devices

---

**Ready to revolutionize your Unity deployment workflow?** 🚀  
[Get started with mOUND Platform](https://mound.gllc.io) | [Download Plugin](../../releases/latest)
