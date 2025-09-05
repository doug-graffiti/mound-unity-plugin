# Changelog

All notable changes to the mOUND Unity Plugin will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Comprehensive console logging for debugging chunked uploads
- Timeout protection for application creation requests
- Progress tracking with elapsed time display

### Fixed
- Variable naming conflicts in chunked upload method
- Application creation call after chunk upload completion
- Progress bar hanging at 80% during large file uploads

## [1.0.0] - 2025-01-05

### Added
- Initial release of mOUND Unity Plugin
- Chunked upload system for large files (>20MB)
- Version control and management
- Organization management features
- Modern UI design with icons and structured sections
- Support for Azure Blob, AWS S3, and Google Cloud Storage
- Comprehensive error handling and user feedback
- Direct login and authentication
- App selection for updates
- Changelog tracking for versions
- Automatic cleanup of temporary files

### Features
- **Chunked Upload**: Automatically splits large files into 20MB chunks
- **Version Control**: Create, manage, and rollback application versions
- **Organization Management**: Team collaboration and access control
- **Cloud Storage**: Multi-provider support with automatic processing
- **Modern UI**: Clean, intuitive interface with progress tracking
- **Error Handling**: Comprehensive error reporting and recovery

### Technical Details
- Unity 2022.3+ compatibility
- .NET 6 HttpClient integration for Unity 6
- Multipart form data handling
- Certificate validation bypass for development
- Progress bar integration with EditorUtility
- Coroutine-based asynchronous operations