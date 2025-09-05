# mOUND Unity Plugin - Developer Guide

## 🛠️ Development Setup

This guide is for developers who want to modify or extend the mOUND Unity Plugin.

### Plugin Architecture

```
Assets/mOUND/
├── Editor/
│   ├── mOUNDUploader.cs      # Main plugin window and functionality
│   ├── mOUNDSettings.cs      # Settings management
│   └── mOUNDIcon.png         # Plugin icon
├── mOUND.asmdef              # Assembly definition
└── Resources/
    └── mOUNDSettings.asset   # Settings asset (created automatically)
```

### Core Components

#### 1. mOUNDUploader.cs
- **Main Plugin Window**: The primary UI and functionality
- **Authentication**: Handles login/logout with mOUND Platform
- **Build Automation**: Manages WebGL builds using Unity's BuildPipeline
- **Upload Management**: Handles ZIP creation and HTTP uploads
- **Organization Management**: Fetches and manages user organizations

#### 2. mOUNDSettings.cs
- **Configuration Management**: Stores plugin settings as ScriptableObject
- **Persistence**: Uses Unity's asset system for settings storage
- **Default Values**: Provides sensible defaults for new installations

### API Integration

The plugin integrates with these mOUND Platform endpoints:

```
POST /api/auth/login          # User authentication
GET  /api/auth/me            # Token validation
GET  /api/organizations      # Fetch user organizations
POST /api/applications       # Upload new applications
```

### Build Process Flow

1. **Validation**: Check project setup and user input
2. **WebGL Build**: Use Unity BuildPipeline to create WebGL build
3. **ZIP Creation**: Package build output into ZIP file
4. **Upload**: Send ZIP to mOUND Platform via HTTP POST
5. **Cleanup**: Remove temporary files
6. **Feedback**: Show success/error messages to user

### Extending the Plugin

#### Adding New Features

1. **New Settings**: Add fields to `mOUNDSettings.cs`
2. **UI Elements**: Extend `DrawLoggedInSection()` in `mOUNDUploader.cs`
3. **API Calls**: Add new coroutines for additional endpoints
4. **Build Options**: Modify `BuildPlayerOptions` in `BuildAndUpload()`

#### Custom Build Configurations

```csharp
// Example: Add custom build options
buildPlayerOptions.options = BuildOptions.Development | BuildOptions.ConnectWithProfiler;

// Example: Custom WebGL settings
PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
PlayerSettings.WebGL.memorySize = 512;
```

#### Adding New API Endpoints

```csharp
private IEnumerator CustomAPICall()
{
    using (UnityWebRequest request = UnityWebRequest.Get(apiUrl + "/api/custom"))
    {
        request.SetRequestHeader("Authorization", "Bearer " + authToken);
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            // Handle response
        }
    }
}
```

### Testing

#### Manual Testing Checklist

- [ ] Plugin appears in Unity menu
- [ ] Login works with valid credentials
- [ ] Organizations load correctly
- [ ] WebGL build completes successfully
- [ ] ZIP file is created properly
- [ ] Upload completes without errors
- [ ] Cleanup removes temporary files
- [ ] Settings persist between sessions

#### Common Issues

**"Coroutine couldn't be started"**
- Ensure coroutines are started from OnGUI thread
- Use `StartCoroutine()` wrapper method

**"Build failed" errors**
- Check Build Settings has scenes added
- Verify WebGL platform is installed
- Look for compilation errors in Console

**Upload timeouts**
- Large builds may exceed default timeout
- Consider adding upload progress feedback
- Implement retry logic for failed uploads

### Code Style Guidelines

- Use `mOUND` namespace for all plugin code
- Follow Unity C# coding conventions
- Add XML documentation for public methods
- Use descriptive variable names
- Include error handling for all network operations

### Building for Distribution

1. **Update Version**: Modify version in `package.json`
2. **Test Thoroughly**: Run through complete workflow
3. **Build Package**: Run `./build-package.sh`
4. **Create Release**: Tag and release on version control

### Contributing

1. Fork the repository
2. Create feature branch
3. Make changes with tests
4. Submit pull request with description

---

## 📚 Unity API References

- [BuildPipeline](https://docs.unity3d.com/ScriptReference/BuildPipeline.html)
- [UnityWebRequest](https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.html)
- [EditorWindow](https://docs.unity3d.com/ScriptReference/EditorWindow.html)
- [EditorPrefs](https://docs.unity3d.com/ScriptReference/EditorPrefs.html)

## 🌐 mOUND Platform API

See the main platform documentation for detailed API specifications.

---

*Happy coding! 🚀*

