#!/bin/bash

# mOUND Unity Plugin Package Builder
# This script creates a Unity package (.unitypackage) for distribution

PLUGIN_NAME="mOUND-Platform-Uploader"
VERSION="1.0.0"
OUTPUT_DIR="dist"

echo "🔧 Building mOUND Unity Plugin Package..."

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Create a zip file for manual installation
echo "📦 Creating ZIP package for manual installation..."
zip -r "$OUTPUT_DIR/${PLUGIN_NAME}-v${VERSION}.zip" Assets/ package.json README.md -x "*.meta"

# Create Unity package (requires Unity command line)
if command -v unity >/dev/null 2>&1; then
    echo "🎯 Creating Unity package..."
    unity -batchmode -quit -projectPath . -exportPackage Assets/mOUND "$OUTPUT_DIR/${PLUGIN_NAME}-v${VERSION}.unitypackage"
else
    echo "⚠️  Unity command line tools not found. Skipping .unitypackage creation."
    echo "   You can create a .unitypackage manually from Unity Editor:"
    echo "   Assets > Export Package > Select mOUND folder > Export"
fi

echo "✅ Package build complete!"
echo "📁 Output files:"
ls -la "$OUTPUT_DIR/"

echo ""
echo "🚀 Installation Instructions:"
echo "1. For Unity Package Manager: Use the package.json file"
echo "2. For manual installation: Extract the ZIP file to your project's Assets folder"
echo "3. For Unity Package: Import the .unitypackage file through Assets > Import Package"
