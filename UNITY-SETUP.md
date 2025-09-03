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

#### **API Compatibility Level (CRITICAL FOR UNITY 6)**
- Go to `Edit > Project Settings > Player > Configuration`
- Set **API Compatibility Level** to **`.NET 6.0`** (Unity 6 preferred)
- **Alternative**: `.NET Standard 2.1` (fallback option)
- **AVOID**: `.NET Framework` (causes networking issues in Unity 6)

#### **Network Settings (Important for Unity 6)**
- Go to `Edit > Project Settings > XR Plug-in Management > Initialize XR on Startup` → **Disable** (if present)
- Go to `Edit > Project Settings > Player > WebGL Settings > Publishing Settings`
- Set **Compression Format** to **Brotli** or **Gzip**

#### **Security Settings**
- The plugin handles certificate validation automatically
- No additional security configuration needed

#### **Scripting Backend**
- Go to `Edit > Project Settings > Player > Configuration`
- **Scripting Backend**: IL2CPP (recommended for WebGL)
- **Api Compatibility Level**: .NET 6.0 (Unity 6 preferred) or .NET Standard 2.1 (fallback)

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

### **"Check your internet connection" (Instant Failure):**
1. **Unity Editor Network Restrictions** - Most common in Unity 6
2. **Try the "🌐 Test Basic Connectivity" button first**
3. **Check Project Settings**:
   - `Edit > Project Settings > Player > Configuration`
   - **API Compatibility Level**: `.NET 6.0` (Unity 6 preferred) or `.NET Standard 2.1`
4. **Unity Editor Network Settings**:
   - `Unity > Preferences > External Tools`
   - Ensure no proxy settings are blocking requests
5. **Windows Firewall/Antivirus**: May block Unity Editor network requests
6. **Corporate Network**: May have restrictions on Unity Editor

#### **🔧 If All Connectivity Tests Fail:**
1. **Try Unity 2022.3 LTS** instead of Unity 6 (more stable networking)
2. **Check Windows Firewall**:
   - Allow Unity Editor through Windows Defender Firewall
   - Add exception for Unity Hub and Unity Editor
3. **Disable Antivirus temporarily** to test
4. **Try different network** (mobile hotspot, different WiFi)
5. **Run Unity as Administrator** (Windows only)
6. **Alternative**: Use the web platform directly for now

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
- [ ] **API Compatibility Level** set to **.NET 6.0** (Unity 6 preferred) or **.NET Standard 2.1**
- [ ] **Scripting Backend** set to **IL2CPP**
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
