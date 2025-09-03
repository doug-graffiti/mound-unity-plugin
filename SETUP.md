# mOUND Unity Plugin - Quick Setup Guide

## 🚀 Quick Start (2 minutes)

### 1. Install the Plugin
**Option A - Unity Package Manager (Recommended):**
1. Open Unity Package Manager (`Window > Package Manager`)
2. Click `+` → `Add package from disk`
3. Select the `package.json` file from this folder

**Option B - Manual Installation:**
1. Copy the `Assets/mOUND` folder into your Unity project's `Assets` folder

### 2. Configure Your Project
1. Go to `File > Build Settings`
2. Add your scenes to the build
3. Switch platform to `WebGL`
4. Close Build Settings

### 3. Use the Plugin
1. Open the plugin: `mOUND > Build and Upload`
2. Login with your mOUND Platform username and password
3. Fill in your app details:
   - **Name**: Your application name
   - **Description**: Brief description of your app
   - **Organization**: Select target organization
   - **Public**: Check if app should be publicly accessible
4. Click **"Build WebGL and Upload"**
5. Wait for build and upload to complete (progress bar will show status)

## ✨ Features

- **🏗️ Automated WebGL Build**: Handles all WebGL build configuration
- **📦 Auto-Zipping**: Creates properly formatted ZIP files for upload
- **🔐 Secure Authentication**: Saves credentials safely in Unity Editor
- **🏢 Organization Support**: Upload to any organization you're a member of
- **📊 Progress Tracking**: Real-time build and upload progress
- **🧹 Auto-Cleanup**: Removes temporary files after successful upload
- **⚙️ Settings Persistence**: Remembers your preferences between sessions

## 🔧 Advanced Configuration

Access advanced settings via `mOUND > Settings`:
- API URL (for custom deployments)
- Build compression settings
- Default application settings

## ❓ Troubleshooting

**Build Fails?**
- Ensure scenes are added to Build Settings
- Check for compilation errors in Console
- Verify WebGL platform support is installed

**Upload Fails?**
- Check internet connection
- Verify mOUND Platform credentials
- Ensure organization permissions

**Can't See Plugin Menu?**
- Check that plugin is in `Assets/mOUND/` folder
- Restart Unity Editor
- Check Console for import errors

## 📱 Supported Unity Versions

- Unity 2021.3 LTS or newer
- WebGL build support required

## 🌐 Platform Support

This plugin uploads to: **https://mound.gllc.io**

---

**Need Help?** Visit the mOUND Platform for support and documentation.
