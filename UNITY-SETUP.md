# Unity Configuration for mOUND Plugin

## 🔧 Required Unity Settings

### 1. **WebGL Platform Support**
- Go to `File > Build Settings`
- Ensure **WebGL** is listed in the platform list
- If not, install via `Unity Hub > Installs > [Your Unity Version] > Add Modules > WebGL Build Support`

### 2. **Project Settings for WebGL**
- Switch to **WebGL platform** in Build Settings
- Go to `Edit > Project Settings > Player > WebGL Settings`
- **Recommended settings**:
  - **Compression Format**: Brotli (for smaller builds)
  - **Memory Size**: 256 MB (or higher for complex projects)
  - **Exception Support**: None (for smaller builds)
  - **Code Optimization**: IL2CPP Code Generation

### 3. **Build Settings**
- Go to `File > Build Settings`
- **Add your scenes** to "Scenes In Build"
- **Select WebGL** as target platform
- Click **"Switch Platform"** if not already selected

### 4. **Unity 6 Specific Settings**

#### **Network Settings (Important for Unity 6)**
- Go to `Edit > Project Settings > XR Plug-in Management > Initialize XR on Startup` → **Disable** (if present)
- Go to `Edit > Project Settings > Player > WebGL Settings > Publishing Settings`
- Set **Compression Format** to **Brotli** or **Gzip**

#### **Security Settings**
- The plugin handles certificate validation automatically
- No additional security configuration needed

### 5. **Optional Optimizations**

#### **For Better Build Performance:**
```
Edit > Project Settings > Player > WebGL Settings:
- Compression Format: Brotli
- Code Optimization: Master  
- Managed Stripping Level: Medium
- Exception Support: None
```

#### **For Faster Development:**
```
Edit > Project Settings > Player > WebGL Settings:
- Compression Format: Disabled
- Code Optimization: Debug
- Development Build: ✅ (for testing)
```

## 🚫 **What You DON'T Need to Configure**

- ❌ **No proxy settings** required
- ❌ **No firewall configuration** (plugin uses browser auth)
- ❌ **No CORS settings** (handled by platform)
- ❌ **No certificate installation** (plugin handles this)
- ❌ **No Unity Cloud Build** setup needed

## 🔍 **Troubleshooting Unity Configuration**

### **"WebGL not available" Error:**
1. Install WebGL Build Support via Unity Hub
2. Restart Unity Editor
3. Switch platform to WebGL in Build Settings

### **"No scenes in build" Error:**
1. Go to `File > Build Settings`
2. Click **"Add Open Scenes"** or drag scenes from Project window
3. Ensure at least one scene is checked

### **Build Takes Forever:**
1. Set **Compression Format** to **Disabled** for testing
2. Enable **Development Build** for faster builds
3. Use **Debug** code optimization during development

### **Plugin Menu Missing:**
1. Check that plugin is in `Assets/mOUND/` folder
2. Look for compilation errors in Console
3. Restart Unity Editor
4. Re-import the plugin

## ✅ **Verification Checklist**

Before using the plugin:
- [ ] WebGL platform support installed
- [ ] Project switched to WebGL platform  
- [ ] At least one scene added to build
- [ ] Plugin appears in `mOUND` menu
- [ ] No compilation errors in Console
- [ ] Internet connection available

## 🎯 **Ready to Use**

Once these settings are configured:
1. Open `mOUND > Build and Upload`
2. Follow the 3-step authentication process
3. Configure your app details
4. Click "Build WebGL and Upload"

The plugin will handle everything else automatically! 🚀
