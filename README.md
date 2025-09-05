# mOUND Unity Plugin

The ultimate Unity WebGL deployment solution with advanced version control, chunked uploads, and seamless organization management.

## Installation

### Via Unity Package Manager (Recommended)

1. Open Unity and go to **Window > Package Manager**
2. Click the **+** button and select **Add package from git URL**
3. Enter: `https://github.com/doug-graffiti/mound-unity-plugin.git`
4. Click **Add**

### Manual Installation

1. Download the latest release from [GitHub Releases](https://github.com/doug-graffiti/mound-unity-plugin/releases)
2. Extract the files to your Unity project's `Assets` folder
3. Import the package

## Usage

1. Open **Window > mOUND Platform Uploader**
2. Log in with your mOUND account credentials
3. Select your organization
4. Configure your app settings (name, description, public/private)
5. Build your WebGL application
6. Upload directly from Unity

## Features

### 🚀 **Chunked Upload System**
- Automatically handles large files (>20MB) with intelligent chunking
- 20MB chunks for optimal performance
- Real-time progress tracking
- Automatic retry on network issues

### 📋 **Version Control**
- Create and manage multiple versions of your applications
- Easy rollback to previous versions
- Changelog tracking for each version
- Stable user-facing URLs

### 🏢 **Organization Management**
- Team collaboration features
- Organization-based access control
- Member role management
- Shared application libraries

### ☁️ **Cloud Storage Support**
- Azure Blob Storage
- AWS S3
- Google Cloud Storage
- Automatic file processing and extraction

### 🎨 **Modern UI**
- Clean, intuitive interface
- Real-time status updates
- Comprehensive error handling
- Progress tracking with detailed feedback

## Requirements

- Unity 2022.3 or later
- Internet connection for uploads
- mOUND Platform account

## Getting Started

1. **Create Account**: Sign up at [mound.gllc.io](https://mound.gllc.io)
2. **Create Organization**: Set up your team organization
3. **Configure Storage**: Set up cloud storage (Azure, AWS, or GCP)
4. **Install Plugin**: Use the Unity Package Manager installation method above
5. **Upload Your First App**: Follow the usage steps above

## Support

- **Documentation**: [mound.gllc.io/docs](https://mound.gllc.io/docs)
- **Support**: [support@mound.gllc.io](mailto:support@mound.gllc.io)
- **Issues**: [GitHub Issues](https://github.com/doug-graffiti/mound-unity-plugin/issues)

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Changelog

### Version 1.0.0
- Initial release
- Chunked upload system
- Version control
- Organization management
- Modern UI design
- Comprehensive error handling