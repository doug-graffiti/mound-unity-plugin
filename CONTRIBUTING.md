# Contributing to mOUND Unity Plugin

Thank you for your interest in contributing to the mOUND Unity Plugin! This document provides guidelines for contributing to the project.

## 🤝 How to Contribute

### Reporting Issues
1. **Search existing issues** to avoid duplicates
2. **Use issue templates** when available
3. **Provide detailed information**:
   - Unity version
   - Plugin version
   - Steps to reproduce
   - Expected vs actual behavior
   - Console logs/error messages

### Suggesting Features
1. **Check existing feature requests** first
2. **Describe the use case** clearly
3. **Explain the benefit** to other users
4. **Consider implementation complexity**

### Code Contributions

#### Before You Start
1. **Fork the repository**
2. **Create a feature branch** from `main`
3. **Discuss major changes** in an issue first

#### Development Setup
1. Clone your fork locally
2. Open in Unity 2021.3 LTS or newer
3. Test with a sample Unity project
4. Follow the [Developer Guide](DEVELOPER.md)

#### Code Standards
- **Follow Unity C# conventions**
- **Use meaningful variable names**
- **Add XML documentation** for public methods
- **Include error handling** for network operations
- **Test thoroughly** before submitting

#### Example Code Style
```csharp
/// <summary>
/// Uploads a ZIP file to the mOUND Platform
/// </summary>
/// <param name="zipPath">Path to the ZIP file to upload</param>
/// <returns>Coroutine for the upload operation</returns>
private IEnumerator UploadZipFile(string zipPath)
{
    // Implementation with proper error handling
}
```

#### Commit Guidelines
- **Use conventional commits**: `feat:`, `fix:`, `docs:`, etc.
- **Write clear messages**: Explain what and why
- **Keep commits focused**: One logical change per commit

Example commit messages:
```
feat: add progress bar for large file uploads
fix: handle network timeout during authentication  
docs: update installation instructions for Unity 2022
```

#### Pull Request Process
1. **Update documentation** if needed
2. **Test your changes** thoroughly
3. **Update CHANGELOG.md** with your changes
4. **Submit pull request** with:
   - Clear description of changes
   - Reference to related issues
   - Testing performed
   - Screenshots if UI changes

## 🧪 Testing

### Manual Testing Checklist
- [ ] Plugin loads without errors in Unity
- [ ] Login works with valid credentials
- [ ] Organizations fetch correctly
- [ ] WebGL build completes successfully
- [ ] ZIP creation works properly
- [ ] Upload completes without errors
- [ ] Progress bars update correctly
- [ ] Cleanup removes temporary files
- [ ] Settings persist between sessions
- [ ] Error handling works as expected

### Test Scenarios
1. **Fresh Installation**: Test with clean Unity project
2. **Network Issues**: Test with poor/no internet connection
3. **Invalid Credentials**: Test error handling
4. **Large Projects**: Test with complex Unity projects
5. **Multiple Organizations**: Test organization switching

## 📝 Documentation

When contributing, please update relevant documentation:
- **README.md** - Main plugin information
- **SETUP.md** - Quick start guide
- **DEVELOPER.md** - Technical documentation
- **CHANGELOG.md** - Version history
- **Code comments** - Inline documentation

## 🐛 Bug Reports

Include this information in bug reports:
- **Unity Version**: e.g., "Unity 2021.3.15f1"
- **Plugin Version**: e.g., "v1.0.0"
- **Platform**: Windows/macOS/Linux
- **Steps to Reproduce**: Numbered list
- **Expected Behavior**: What should happen
- **Actual Behavior**: What actually happens
- **Console Logs**: Any relevant error messages
- **Screenshots**: If applicable

## 🎯 Feature Requests

Structure feature requests like this:
- **Problem**: What problem does this solve?
- **Solution**: Proposed solution
- **Alternatives**: Other approaches considered
- **Use Cases**: Who would benefit and how?
- **Implementation**: Any technical considerations

## 🔄 Release Process

For maintainers:
1. Update version in `package.json`
2. Update `CHANGELOG.md`
3. Test thoroughly
4. Create GitHub release
5. Upload distribution packages

## 📞 Getting Help

- **Documentation**: Check existing docs first
- **Issues**: Search existing issues
- **Discussions**: Use GitHub Discussions for questions
- **Email**: support@gllc.io for urgent issues

## 🙏 Recognition

Contributors will be:
- Listed in CHANGELOG.md
- Mentioned in release notes
- Added to contributors list

Thank you for making the mOUND Unity Plugin better! 🚀
