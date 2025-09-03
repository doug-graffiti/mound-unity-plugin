using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System.IO.Compression;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Security;
using System;
using System.Collections.Generic;

namespace mOUND
{
    // Unity 6 certificate handler for Editor HTTPS requests
    public class AcceptAllCertificatesSignedWithASpecificKeyPublicKey : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            // Accept all certificates in Unity Editor for development
            return true;
        }
    }
    public class mOUNDUploader : EditorWindow
    {
        private string apiUrl = "https://mound.gllc.io";
        private string username = "";
        private string password = "";
        private string appName = "";
        private string appDescription = "";
        private string organizationId = "";
        private bool isPublic = false;
        private string authToken = "";
        private bool isLoggedIn = false;
        private bool isValidatingToken = false;
        private Vector2 scrollPosition;
        
        // Modern .NET 6 HttpClient for better Unity 6 compatibility
        private static readonly HttpClient httpClient = new HttpClient();
        
        private List<Organization> organizations = new List<Organization>();
        private int selectedOrgIndex = 0;
        private List<ApplicationData> existingApps = new List<ApplicationData>();
        private int selectedAppIndex = -1; // -1 means "Create New"
        private bool isUpdateMode = false;
        private string changelogText = "";
        
        [System.Serializable]
        public class LoginResponse
        {
            public string token;
            public User user;
        }
        
        [System.Serializable]
        public class User
        {
            public string id;
            public string username;
            public string email;
        }
        
        [System.Serializable]
        public class Organization
        {
            public string id;
            public string name;
        }
        
        [System.Serializable]
        public class OrganizationsResponse
        {
            public Organization[] organizations;
        }
        
        [System.Serializable]
        public class OrganizationData
        {
            public string _id;
            public string name;
            public string description;
        }
        
        [System.Serializable]
        public class OrganizationWrapper
        {
            public OrganizationData[] items;
        }
        
        [System.Serializable]
        public class ApplicationData
        {
            public string _id;
            public string name;
            public string description;
            public string version;
        }
        
        [System.Serializable]
        public class ApplicationWrapper
        {
            public ApplicationData[] items;
        }
        
        [System.Serializable]
        public class LoginRequest
        {
            public string username;
            public string password;
        }
        
        [MenuItem("mOUND/Build and Upload")]
        public static void ShowWindow()
        {
            GetWindow<mOUNDUploader>("mOUND Uploader");
        }
        
        private void OnEnable()
        {
            LoadCredentials();
            
            // If we have a saved token, validate it and fetch organizations
            if (isLoggedIn && !string.IsNullOrEmpty(authToken))
            {
                Debug.Log($"🔄 mOUND: Found saved token, validating and fetching organizations...");
                _ = ValidateTokenAndFetchDataAsync();
            }
        }
        
        private async Task ValidateTokenAndFetchDataAsync()
        {
            try
            {
                // Validate the saved token first
                await ValidateTokenAsync();
                
                // If validation was successful and we don't have organizations, fetch them
                if (isLoggedIn && organizations.Count == 0)
                {
                    Debug.Log($"🔄 mOUND: Token valid, fetching organizations...");
                    await FetchOrganizationsAsync();
                    
                    // If we have an organization selected, fetch its apps too
                    if (!string.IsNullOrEmpty(organizationId))
                    {
                        Debug.Log($"🔄 mOUND: Fetching apps for saved organization: {organizationId}");
                        await FetchApplicationsAsync(organizationId);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ mOUND: Error during startup validation: {e.Message}");
            }
        }
        
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            GUILayout.Label("mOUND Platform Uploader", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (!isLoggedIn)
            {
                DrawLoginSection();
            }
            else
            {
                DrawLoggedInSection();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawLoginSection()
        {
            GUILayout.Label("Login to mOUND Platform", EditorStyles.boldLabel);
            
            GUILayout.Label("API URL:");
            apiUrl = EditorGUILayout.TextField(apiUrl);
            
            EditorGUILayout.Space(5);
            
            // Direct username/password login using .NET 6 HttpClient
            EditorGUILayout.HelpBox(".NET 6 HttpClient method works! Using direct login:", MessageType.Info);
            
            GUILayout.Label("Username:");
            username = EditorGUILayout.TextField(username);
            
            GUILayout.Label("Password:");
            password = EditorGUILayout.PasswordField(password);
            
            EditorGUILayout.Space(5);
            
            EditorGUI.BeginDisabledGroup(isValidatingToken);
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && 
                GUILayout.Button(isValidatingToken ? "Logging in..." : "Login (.NET 6)", GUILayout.Height(30)))
            {
                _ = LoginAsync(); // Use .NET 6 HttpClient method that works
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space(10);
            
            GUILayout.Label("Alternative Methods:", EditorStyles.boldLabel);
            
            if (GUILayout.Button("1. Open mOUND Platform Login", GUILayout.Height(30)))
            {
                string loginUrl = apiUrl + "/login";
                Application.OpenURL(loginUrl);
                EditorUtility.DisplayDialog("Browser Login Instructions", 
                    "1. Login in the browser that just opened\n" +
                    "2. Go to your Profile page\n" +
                    "3. Copy the API Token\n" +
                    "4. Paste it in the field below\n" +
                    "5. Click 'Validate Token'", "OK");
            }
            
            EditorGUILayout.Space(5);
            
            GUILayout.Label("2. Paste API Token from Profile Page:");
            string manualToken = EditorGUILayout.TextField(authToken, GUILayout.Height(20));
            
            if (manualToken != authToken)
            {
                authToken = manualToken;
            }
            
            EditorGUILayout.Space(5);
            
            EditorGUI.BeginDisabledGroup(isValidatingToken);
            if (!string.IsNullOrEmpty(authToken) && GUILayout.Button(isValidatingToken ? "Validating..." : "3. Validate Token", GUILayout.Height(30)))
            {
                StartCoroutine(ValidateToken());
            }
            EditorGUI.EndDisabledGroup();
            
            if (GUILayout.Button("Need Help? Open Profile Page", GUILayout.Height(25)))
            {
                Application.OpenURL(apiUrl + "/profile");
            }
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("🌐 Test Basic Connectivity", GUILayout.Height(25)))
            {
                StartCoroutine(TestConnectivity());
            }
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("🔧 Try .NET 6 HttpClient Method", GUILayout.Height(25)))
            {
                _ = ValidateTokenAsync(); // Fire and forget async
            }
        }
        
        private void DrawLoggedInSection()
        {
            GUILayout.Label($"Logged in as: {username}", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Logout"))
            {
                Logout();
                return;
            }
            
            GUILayout.Space(10);
            
            GUILayout.Label("Application Details", EditorStyles.boldLabel);
            
            GUILayout.Label("Application Name:");
            appName = EditorGUILayout.TextField(appName);
            
            GUILayout.Label("Description:");
            appDescription = EditorGUILayout.TextArea(appDescription, GUILayout.Height(60));
            
            GUILayout.Label("Organization:");
            if (organizations.Count > 0)
            {
                string[] orgNames = new string[organizations.Count];
                for (int i = 0; i < organizations.Count; i++)
                {
                    orgNames[i] = organizations[i].name;
                }
                int newOrgIndex = EditorGUILayout.Popup(selectedOrgIndex, orgNames);
                if (newOrgIndex != selectedOrgIndex)
                {
                    selectedOrgIndex = newOrgIndex;
                    organizationId = organizations[selectedOrgIndex].id;
                    // Fetch apps for this organization
                    _ = FetchApplicationsAsync(organizationId);
                }
            }
            else
            {
                GUILayout.Label("No organizations available");
                if (GUILayout.Button("Refresh Organizations"))
                {
                    Debug.Log($"🔄 mOUND: Manual organization refresh requested");
                    _ = FetchOrganizationsAsync(); // Use async method
                }
            }
            
            // Always show refresh button for debugging
            if (organizations.Count > 0 && GUILayout.Button("🔄 Refresh Organizations", GUILayout.Height(20)))
            {
                Debug.Log($"🔄 mOUND: Manual organization refresh (debug)");
                _ = FetchOrganizationsAsync();
            }
            
            GUILayout.Space(10);
            
            // Update mode toggle
            bool newUpdateMode = EditorGUILayout.Toggle("Update Existing App", isUpdateMode);
            if (newUpdateMode != isUpdateMode)
            {
                isUpdateMode = newUpdateMode;
                if (isUpdateMode)
                {
                    // When switching to update mode, fetch apps if we have an org selected
                    if (!string.IsNullOrEmpty(organizationId) && existingApps.Count == 0)
                    {
                        Debug.Log($"🔄 mOUND: Switching to update mode, fetching apps for org: {organizationId}");
                        _ = FetchApplicationsAsync(organizationId);
                    }
                    selectedAppIndex = -1; // Reset selection
                }
                else
                {
                    // When switching to create mode, clear app details
                    appName = "";
                    appDescription = "";
                    changelogText = "";
                    selectedAppIndex = -1;
                }
            }
            
            if (isUpdateMode && existingApps.Count > 0)
            {
                GUILayout.Label("Select App to Update:");
                string[] appOptions = new string[existingApps.Count + 1];
                appOptions[0] = "Create New App";
                for (int i = 0; i < existingApps.Count; i++)
                {
                    appOptions[i + 1] = $"{existingApps[i].name} (v{existingApps[i].version ?? "1.0.0"})";
                }
                
                int newAppIndex = EditorGUILayout.Popup(selectedAppIndex + 1, appOptions) - 1;
                if (newAppIndex != selectedAppIndex)
                {
                    selectedAppIndex = newAppIndex;
                    if (selectedAppIndex >= 0)
                    {
                        // Auto-populate with existing app data
                        var selectedApp = existingApps[selectedAppIndex];
                        appName = selectedApp.name;
                        appDescription = selectedApp.description;
                        Debug.Log($"🔄 mOUND: Auto-populated app details for update: {selectedApp.name}");
                    }
                    else
                    {
                        // Reset to empty for new app
                        appName = "";
                        appDescription = "";
                        changelogText = "";
                    }
                }
                
                // Show changelog field for updates
                if (selectedAppIndex >= 0)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.HelpBox("Changelog is required for app updates", MessageType.Info);
                    GUILayout.Label("What's New in This Version: *");
                    changelogText = EditorGUILayout.TextArea(changelogText, GUILayout.Height(80));
                    
                    if (string.IsNullOrEmpty(changelogText.Trim()))
                    {
                        EditorGUILayout.HelpBox("Please describe what's new in this update", MessageType.Warning);
                    }
                }
            }
            else if (isUpdateMode)
            {
                GUILayout.Label("No existing apps found for this organization");
                if (GUILayout.Button("Refresh Apps"))
                {
                    if (!string.IsNullOrEmpty(organizationId))
                    {
                        _ = FetchApplicationsAsync(organizationId);
                    }
                }
            }
            
            isPublic = EditorGUILayout.Toggle("Public Application", isPublic);
            
            GUILayout.Space(20);
            
            string buttonText = isUpdateMode && selectedAppIndex >= 0 
                ? $"Build WebGL and Update {existingApps[selectedAppIndex].name}" 
                : "Build WebGL and Upload";
                
            if (GUILayout.Button(buttonText, GUILayout.Height(40)))
            {
                if (string.IsNullOrEmpty(appName))
                {
                    EditorUtility.DisplayDialog("Error", "Please enter an application name.", "OK");
                    return;
                }
                
                if (string.IsNullOrEmpty(organizationId))
                {
                    EditorUtility.DisplayDialog("Error", "Please select an organization.", "OK");
                    return;
                }
                
                // Validate changelog for updates
                if (isUpdateMode && selectedAppIndex >= 0)
                {
                    if (string.IsNullOrEmpty(changelogText.Trim()))
                    {
                        EditorUtility.DisplayDialog("Error", "Please enter a changelog describing what's new in this update.", "OK");
                        return;
                    }
                    BuildAndUpdate();
                }
                else
                {
                    BuildAndUpload();
                }
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Open mOUND Platform"))
            {
                Application.OpenURL(apiUrl);
            }
        }
        
        private void BuildAndUpdate()
        {
            if (selectedAppIndex < 0 || selectedAppIndex >= existingApps.Count)
            {
                EditorUtility.DisplayDialog("Error", "Please select a valid app to update.", "OK");
                return;
            }
            
            var appToUpdate = existingApps[selectedAppIndex];
            Debug.Log($"🔄 mOUND: Updating app: {appToUpdate.name} (ID: {appToUpdate._id})");
            
            try
            {
                EditorUtility.DisplayProgressBar("mOUND Update", $"Building WebGL for {appToUpdate.name}...", 0.1f);
                
                // Set WebGL build settings with compression for updates
                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
                buildPlayerOptions.scenes = GetEnabledScenes();
                buildPlayerOptions.locationPathName = "Builds/WebGL";
                buildPlayerOptions.target = BuildTarget.WebGL;
                buildPlayerOptions.options = BuildOptions.CompressWithLz4; // Enable compression
                
                // Set WebGL compression in player settings
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
                PlayerSettings.WebGL.decompressionFallback = true;
                Debug.Log($"🔧 mOUND: Set WebGL compression to Brotli for smaller update builds");
                
                // Clear previous build
                if (Directory.Exists("Builds/WebGL"))
                {
                    Directory.Delete("Builds/WebGL", true);
                }
                
                EditorUtility.DisplayProgressBar("mOUND Update", "Building WebGL...", 0.3f);
                
                // Build the project
                BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                BuildSummary summary = report.summary;
                
                if (summary.result != BuildResult.Succeeded)
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("Build Failed", "WebGL build failed. Check console for details.", "OK");
                    return;
                }
                
                EditorUtility.DisplayProgressBar("mOUND Update", "Creating ZIP file...", 0.6f);
                
                // Create ZIP file
                string zipPath = $"Builds/{appName}_update.zip";
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
                
                ZipFile.CreateFromDirectory("Builds/WebGL", zipPath);
                
                EditorUtility.DisplayProgressBar("mOUND Update", "Uploading update...", 0.8f);
                
                // Upload update
                StartCoroutine(UpdateAppCoroutine(appToUpdate._id, zipPath));
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"Error during build and update: {e.Message}");
                EditorUtility.DisplayDialog("Error", $"Build and update failed: {e.Message}", "OK");
            }
        }
        
        private void BuildAndUpload()
        {
            try
            {
                EditorUtility.DisplayProgressBar("mOUND Upload", "Building WebGL...", 0.1f);
                
                // Set WebGL build settings with compression
                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
                buildPlayerOptions.scenes = GetEnabledScenes();
                buildPlayerOptions.locationPathName = "Builds/WebGL";
                buildPlayerOptions.target = BuildTarget.WebGL;
                buildPlayerOptions.options = BuildOptions.CompressWithLz4; // Enable compression
                
                // Set WebGL compression in player settings
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
                PlayerSettings.WebGL.decompressionFallback = true;
                Debug.Log($"🔧 mOUND: Set WebGL compression to Brotli for smaller builds");
                
                // Clear previous build
                if (Directory.Exists("Builds/WebGL"))
                {
                    Directory.Delete("Builds/WebGL", true);
                }
                
                EditorUtility.DisplayProgressBar("mOUND Upload", "Building WebGL...", 0.3f);
                
                // Build the project
                BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                
                if (report.summary.result != BuildResult.Succeeded)
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("Build Failed", "WebGL build failed. Check the console for errors.", "OK");
                    return;
                }
                
                EditorUtility.DisplayProgressBar("mOUND Upload", "Creating ZIP archive...", 0.6f);
                
                // Create ZIP file
                string zipPath = "Builds/" + appName.Replace(" ", "_") + "_WebGL.zip";
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
                
                ZipFile.CreateFromDirectory("Builds/WebGL", zipPath);
                
                EditorUtility.DisplayProgressBar("mOUND Upload", "Uploading to mOUND Platform...", 0.8f);
                
                // Upload the ZIP file
                StartCoroutine(UploadZipFile(zipPath));
                
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Error", "Build failed: " + e.Message, "OK");
                Debug.LogError("mOUND Build Error: " + e.Message);
            }
        }
        
        private string[] GetEnabledScenes()
        {
            List<string> scenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }
            return scenes.ToArray();
        }
        
        private IEnumerator LoginCoroutine()
        {
            Debug.Log($"🔐 mOUND: Attempting login to {apiUrl}/api/auth/login");
            Debug.Log($"🔐 mOUND: Username: {username}");
            Debug.Log($"🔐 mOUND: Password length: {password.Length}");
            
            var loginData = new LoginRequest
            {
                username = this.username,
                password = this.password
            };
            
            string jsonData = JsonUtility.ToJson(loginData);
            Debug.Log($"🔐 mOUND: JSON payload: {jsonData}");
            
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            
            // Unity 6 compatible networking approach
            using (UnityWebRequest request = new UnityWebRequest(apiUrl + "/api/auth/login", "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");
                request.SetRequestHeader("User-Agent", "Unity-mOUND-Plugin/1.0.0");
                request.SetRequestHeader("Cache-Control", "no-cache");
                request.timeout = 30; // 30 second timeout
                
                // Unity 6 security bypass for Editor
                request.certificateHandler = new AcceptAllCertificatesSignedWithASpecificKeyPublicKey();
                request.disposeCertificateHandlerOnDispose = true;
                
                Debug.Log($"🔐 mOUND: Sending POST request to {request.url}");
                Debug.Log($"🔐 mOUND: Content-Type: application/json, User-Agent: Unity-mOUND-Plugin/1.0.0");
                
                yield return request.SendWebRequest();
                
                Debug.Log($"🔐 mOUND: Response code: {request.responseCode}");
                Debug.Log($"🔐 mOUND: Response text: {request.downloadHandler.text}");
                Debug.Log($"🔐 mOUND: Request result: {request.result}");
                Debug.Log($"🔐 mOUND: Error (if any): {request.error}");
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                        authToken = response.token;
                        username = response.user.username;
                        isLoggedIn = true;
                        
                        Debug.Log($"🔐 mOUND: Login successful, token received");
                        
                        SaveCredentials();
                        _ = FetchOrganizationsAsync(); // Use the working async method
                        
                        EditorUtility.DisplayDialog("Success", $"Logged in as {username}", "OK");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"🔐 mOUND: JSON parsing error: {e.Message}");
                        Debug.LogError($"🔐 mOUND: Response was: {request.downloadHandler.text}");
                        EditorUtility.DisplayDialog("Login Failed", "Invalid response from server: " + e.Message, "OK");
                    }
                }
                else
                {
                    string errorMsg;
                    if (request.responseCode == 0)
                    {
                        errorMsg = $"Network error: {request.error}. Check your internet connection and firewall settings.";
                    }
                    else
                    {
                        errorMsg = $"HTTP {request.responseCode}: {request.downloadHandler.text}";
                    }
                    
                    Debug.LogError($"🔐 mOUND: Login failed - {errorMsg}");
                    EditorUtility.DisplayDialog("Login Failed", errorMsg, "OK");
                }
            }
        }
        
        private IEnumerator FetchOrganizations()
        {
            Debug.Log($"🏢 mOUND: Fetching organizations...");
            
            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl + "/api/organizations"))
            {
                request.SetRequestHeader("Authorization", "Bearer " + authToken);
                request.SetRequestHeader("User-Agent", "Unity-mOUND-Plugin/1.0.0");
                request.timeout = 30;
                
                // Unity 6 certificate handler
                request.certificateHandler = new AcceptAllCertificatesSignedWithASpecificKeyPublicKey();
                request.disposeCertificateHandlerOnDispose = true;
                
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        // Parse the array response
                        string jsonResponse = request.downloadHandler.text;
                        // Unity's JsonUtility doesn't handle arrays directly, so we need to wrap it
                        string wrappedJson = "{\"organizations\":" + jsonResponse + "}";
                        OrganizationsResponse response = JsonUtility.FromJson<OrganizationsResponse>(wrappedJson);
                        
                        organizations.Clear();
                        organizations.AddRange(response.organizations);
                        
                        if (organizations.Count > 0)
                        {
                            selectedOrgIndex = 0;
                            organizationId = organizations[0].id;
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Failed to parse organizations: " + e.Message);
                    }
                }
                else
                {
                    Debug.LogError("Failed to fetch organizations: " + request.error);
                }
            }
        }
        
        private void SaveCredentials()
        {
            EditorPrefs.SetString("mOUND_ApiUrl", apiUrl);
            EditorPrefs.SetString("mOUND_Username", username);
            EditorPrefs.SetString("mOUND_AuthToken", authToken);
            EditorPrefs.SetBool("mOUND_IsLoggedIn", isLoggedIn);
        }
        
        private void LoadCredentials()
        {
            apiUrl = EditorPrefs.GetString("mOUND_ApiUrl", "https://mound.gllc.io");
            username = EditorPrefs.GetString("mOUND_Username", "");
            authToken = EditorPrefs.GetString("mOUND_AuthToken", "");
            isLoggedIn = EditorPrefs.GetBool("mOUND_IsLoggedIn", false);
            
            if (isLoggedIn && !string.IsNullOrEmpty(authToken))
            {
                StartCoroutine(ValidateToken());
            }
        }
        
        // Modern .NET 6 async login method
        private async Task LoginAsync()
        {
            try
            {
                isValidatingToken = true;
                Debug.Log($"🔧 mOUND: === .NET 6 LOGIN START ===");
                Debug.Log($"🔧 mOUND: Username: {username}");
                Debug.Log($"🔧 mOUND: Using HttpClient for login");
                
                // Configure HttpClient
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Unity-mOUND-Plugin-NET6/1.0.0");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                
                string url = $"{apiUrl}/api/auth/login";
                Debug.Log($"🔧 mOUND: Requesting: {url}");
                
                // Create login payload
                var loginData = new LoginRequest
                {
                    username = this.username,
                    password = this.password
                };
                
                string jsonData = JsonUtility.ToJson(loginData);
                Debug.Log($"🔧 mOUND: Login payload: {jsonData}");
                
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(url, content);
                
                Debug.Log($"🔧 mOUND: Login Response Status: {response.StatusCode}");
                
                string responseText = await response.Content.ReadAsStringAsync();
                Debug.Log($"🔧 mOUND: Login Response Text: {responseText}");
                
                if (response.IsSuccessStatusCode)
                {
                    Debug.Log($"✅ mOUND: .NET 6 HttpClient login SUCCESS!");
                    
                    var loginResponse = JsonUtility.FromJson<LoginResponse>(responseText);
                    authToken = loginResponse.token;
                    username = loginResponse.user.username;
                    isLoggedIn = true;
                    
                    SaveCredentials();
                    EditorUtility.DisplayDialog("Success", 
                        "✅ Login successful with .NET 6 HttpClient!\nFetching organizations...", "OK");
                    
                    // Fetch organizations
                    await FetchOrganizationsAsync();
                }
                else
                {
                    string errorMsg = $".NET 6 HttpClient login failed:\nStatus: {response.StatusCode}\nResponse: {responseText}";
                    Debug.LogError($"❌ mOUND: {errorMsg}");
                    EditorUtility.DisplayDialog("Login Failed", errorMsg, "OK");
                }
            }
            catch (HttpRequestException e)
            {
                string errorMsg = $"Network error during login:\n{e.Message}";
                Debug.LogError($"🌐 mOUND: {errorMsg}");
                EditorUtility.DisplayDialog("Network Error", errorMsg, "OK");
            }
            catch (Exception e)
            {
                string errorMsg = $"Login error:\n{e.Message}";
                Debug.LogError($"❌ mOUND: {errorMsg}");
                EditorUtility.DisplayDialog("Error", errorMsg, "OK");
            }
            finally
            {
                isValidatingToken = false;
            }
        }
        
        // Modern .NET 6 async method for Unity 6 compatibility
        private async Task ValidateTokenAsync()
        {
            try
            {
                isValidatingToken = true;
                Debug.Log($"🔧 mOUND: === .NET 6 TOKEN VALIDATION START ===");
                Debug.Log($"🔧 mOUND: Unity Version: {Application.unityVersion}");
                Debug.Log($"🔧 mOUND: Using HttpClient instead of UnityWebRequest");
                Debug.Log($"🔧 mOUND: Token length: {authToken?.Length ?? 0}");
                
                // Configure HttpClient for Unity 6
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Unity-mOUND-Plugin-NET6/1.0.0");
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                
                string url = $"{apiUrl}/api/auth/me";
                Debug.Log($"🔧 mOUND: Requesting: {url}");
                
                var response = await httpClient.GetAsync(url);
                
                Debug.Log($"🔧 mOUND: Response Status: {response.StatusCode}");
                Debug.Log($"🔧 mOUND: Response Headers: {response.Headers}");
                
                string responseText = await response.Content.ReadAsStringAsync();
                Debug.Log($"🔧 mOUND: Response Text: {responseText}");
                
                if (response.IsSuccessStatusCode)
                {
                    Debug.Log($"✅ mOUND: .NET 6 HttpClient validation SUCCESS!");
                    isLoggedIn = true;
                    
                    // Parse response
                    try
                    {
                        var loginResponse = JsonUtility.FromJson<LoginResponse>(responseText);
                        username = loginResponse.user.username;
                        Debug.Log($"🔧 mOUND: Username: {username}");
                        
                        EditorUtility.DisplayDialog("Success", 
                            "✅ .NET 6 HttpClient validation successful!\nThis method works better in Unity 6.", "OK");
                        
                        // Fetch organizations using async method too
                        _ = FetchOrganizationsAsync();
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"🔧 mOUND: JSON parsing error: {e.Message}");
                        username = "Unity User";
                        EditorUtility.DisplayDialog("Partial Success", 
                            "Token validated but couldn't parse user info.\nYou can still proceed.", "OK");
                    }
                }
                else
                {
                    string errorMsg = $".NET 6 HttpClient validation failed:\nStatus: {response.StatusCode}\nResponse: {responseText}";
                    Debug.LogError($"❌ mOUND: {errorMsg}");
                    EditorUtility.DisplayDialog("Validation Failed", errorMsg, "OK");
                }
            }
            catch (HttpRequestException e)
            {
                string errorMsg = $"Network error with .NET 6 HttpClient:\n{e.Message}";
                Debug.LogError($"🌐 mOUND: {errorMsg}");
                EditorUtility.DisplayDialog("Network Error", errorMsg, "OK");
            }
            catch (TaskCanceledException e)
            {
                string errorMsg = "Request timed out (30 seconds)";
                Debug.LogError($"⏰ mOUND: {errorMsg}");
                EditorUtility.DisplayDialog("Timeout", errorMsg, "OK");
            }
            catch (Exception e)
            {
                string errorMsg = $"Unexpected error:\n{e.Message}";
                Debug.LogError($"❌ mOUND: {errorMsg}");
                EditorUtility.DisplayDialog("Error", errorMsg, "OK");
            }
            finally
            {
                isValidatingToken = false;
            }
        }
        
        private async Task FetchOrganizationsAsync()
        {
            try
            {
                Debug.Log($"🔧 mOUND: Fetching organizations with .NET 6 HttpClient...");
                
                string url = $"{apiUrl}/api/organizations";
                var response = await httpClient.GetAsync(url);
                string responseText = await response.Content.ReadAsStringAsync();
                
                Debug.Log($"🔧 mOUND: Organizations response: {responseText}");
                
                if (response.IsSuccessStatusCode)
                {
                    // The response is a direct array, not an object with organizations property
                    // Need to wrap it for JsonUtility
                    string wrappedJson = "{\"items\":" + responseText + "}";
                    Debug.Log($"🔧 mOUND: Wrapped JSON: {wrappedJson}");
                    
                    var orgWrapper = JsonUtility.FromJson<OrganizationWrapper>(wrappedJson);
                    organizations.Clear();
                    
                    // Convert OrganizationData to Organization
                    foreach (var orgData in orgWrapper.items)
                    {
                        organizations.Add(new Organization
                        {
                            id = orgData._id,
                            name = orgData.name
                        });
                    }
                    
                    Debug.Log($"🔧 mOUND: Loaded {organizations.Count} organizations");
                    
                    // Reset organization selection
                    selectedOrgIndex = 0;
                    if (organizations.Count > 0)
                    {
                        organizationId = organizations[0].id;
                        Debug.Log($"🔧 mOUND: Selected first organization: {organizations[0].name}");
                    }
                }
                else
                {
                    Debug.LogError($"❌ mOUND: Failed to fetch organizations: {response.StatusCode}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ mOUND: Error fetching organizations: {e.Message}");
            }
        }
        
        private async Task FetchApplicationsAsync(string orgId)
        {
            try
            {
                Debug.Log($"🔧 mOUND: Fetching applications for organization: {orgId}");
                
                string url = $"{apiUrl}/api/applications?organizationId={orgId}";
                var response = await httpClient.GetAsync(url);
                string responseText = await response.Content.ReadAsStringAsync();
                
                Debug.Log($"🔧 mOUND: Applications response: {responseText}");
                
                if (response.IsSuccessStatusCode)
                {
                    // Parse applications array
                    string wrappedJson = "{\"items\":" + responseText + "}";
                    var appWrapper = JsonUtility.FromJson<ApplicationWrapper>(wrappedJson);
                    existingApps.Clear();
                    existingApps.AddRange(appWrapper.items);
                    
                    Debug.Log($"🔧 mOUND: Loaded {existingApps.Count} existing applications");
                    selectedAppIndex = -1; // Reset to "Create New"
                }
                else
                {
                    Debug.LogError($"❌ mOUND: Failed to fetch applications: {response.StatusCode}");
                    existingApps.Clear();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ mOUND: Error fetching applications: {e.Message}");
                existingApps.Clear();
            }
        }
        
        private IEnumerator TestConnectivity()
        {
            Debug.Log($"🌐 mOUND: === CONNECTIVITY TEST START ===");
            Debug.Log($"🌐 mOUND: Unity Version: {Application.unityVersion}");
            Debug.Log($"🌐 mOUND: Is Editor: {Application.isEditor}");
            Debug.Log($"🌐 mOUND: Internet Reachability: {Application.internetReachability}");
            
            bool anySuccess = false;
            string testResults = "";
            
            // Test 1: HTTP (non-SSL) request to Google
            Debug.Log($"🌐 mOUND: Test 1 - HTTP request to Google...");
            using (UnityWebRequest request = UnityWebRequest.Get("http://www.google.com"))
            {
                request.timeout = 10;
                yield return request.SendWebRequest();
                
                Debug.Log($"🌐 mOUND: Google HTTP - Result: {request.result}, Code: {request.responseCode}");
                if (request.result == UnityWebRequest.Result.Success)
                {
                    anySuccess = true;
                    testResults += "✅ Google HTTP: SUCCESS\n";
                }
                else
                {
                    testResults += $"❌ Google HTTP: {request.result} ({request.responseCode})\n";
                }
            }
            
            // Test 2: HTTPS request to Google
            Debug.Log($"🌐 mOUND: Test 2 - HTTPS request to Google...");
            using (UnityWebRequest request = UnityWebRequest.Get("https://www.google.com"))
            {
                request.timeout = 10;
                request.certificateHandler = new AcceptAllCertificatesSignedWithASpecificKeyPublicKey();
                request.disposeCertificateHandlerOnDispose = true;
                yield return request.SendWebRequest();
                
                Debug.Log($"🌐 mOUND: Google HTTPS - Result: {request.result}, Code: {request.responseCode}");
                if (request.result == UnityWebRequest.Result.Success)
                {
                    anySuccess = true;
                    testResults += "✅ Google HTTPS: SUCCESS\n";
                }
                else
                {
                    testResults += $"❌ Google HTTPS: {request.result} ({request.responseCode})\n";
                }
            }
            
            // Test 3: Our API domain
            Debug.Log($"🌐 mOUND: Test 3 - mOUND domain...");
            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                request.timeout = 10;
                request.certificateHandler = new AcceptAllCertificatesSignedWithASpecificKeyPublicKey();
                request.disposeCertificateHandlerOnDispose = true;
                yield return request.SendWebRequest();
                
                Debug.Log($"🌐 mOUND: mOUND domain - Result: {request.result}, Code: {request.responseCode}");
                if (request.result == UnityWebRequest.Result.Success)
                {
                    anySuccess = true;
                    testResults += "✅ mOUND Domain: SUCCESS\n";
                }
                else
                {
                    testResults += $"❌ mOUND Domain: {request.result} ({request.responseCode})\n";
                }
            }
            
            // Show comprehensive results
            string title = anySuccess ? "Partial Connectivity" : "No Connectivity";
            string message = $"Connectivity Test Results:\n\n{testResults}";
            
            if (!anySuccess)
            {
                message += "\n🚨 UNITY EDITOR NETWORK BLOCKED\n";
                message += "This is a Unity Editor restriction, not our server.\n\n";
                message += "Solutions:\n";
                message += "1. Check API Compatibility Level (.NET Standard 2.1)\n";
                message += "2. Check Windows Firewall/Antivirus\n";
                message += "3. Try different Unity version\n";
                message += "4. Use Unity Hub's Unity 2022.3 LTS";
            }
            
            Debug.Log($"🌐 mOUND: === CONNECTIVITY TEST COMPLETE ===");
            Debug.Log($"🌐 mOUND: {message}");
            EditorUtility.DisplayDialog(title, message, "OK");
        }
        
        private IEnumerator ValidateToken()
        {
            isValidatingToken = true;
            Debug.Log($"🔐 mOUND: === TOKEN VALIDATION START ===");
            Debug.Log($"🔐 mOUND: Unity Version: {Application.unityVersion}");
            Debug.Log($"🔐 mOUND: Platform: {Application.platform}");
            Debug.Log($"🔐 mOUND: API URL: {apiUrl}");
            Debug.Log($"🔐 mOUND: Full validation URL: {apiUrl}/api/auth/me");
            Debug.Log($"🔐 mOUND: Token length: {authToken.Length}");
            Debug.Log($"🔐 mOUND: Token preview: {authToken.Substring(0, Math.Min(20, authToken.Length))}...");
            
            // First test basic connectivity
            Debug.Log($"🌐 mOUND: Testing basic connectivity to {apiUrl}");
            
            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl + "/api/auth/me"))
            {
                request.SetRequestHeader("Authorization", "Bearer " + authToken);
                request.SetRequestHeader("User-Agent", "Unity-mOUND-Plugin/1.0.0");
                request.SetRequestHeader("Accept", "application/json");
                request.timeout = 30;
                
                // Unity 6 certificate handler
                request.certificateHandler = new AcceptAllCertificatesSignedWithASpecificKeyPublicKey();
                request.disposeCertificateHandlerOnDispose = true;
                
                Debug.Log($"🔐 mOUND: Sending validation request to {request.url}");
                
                yield return request.SendWebRequest();
                
                Debug.Log($"🔐 mOUND: === REQUEST COMPLETED ===");
                Debug.Log($"🔐 mOUND: Response Code: {request.responseCode}");
                Debug.Log($"🔐 mOUND: Request Result: {request.result}");
                Debug.Log($"🔐 mOUND: Error: {request.error ?? "None"}");
                Debug.Log($"🔐 mOUND: Response Text Length: {request.downloadHandler?.text?.Length ?? 0}");
                Debug.Log($"🔐 mOUND: Response Text: {request.downloadHandler?.text ?? "NULL"}");
                Debug.Log($"🔐 mOUND: Request URL: {request.url}");
                Debug.Log($"🔐 mOUND: Request Method: {request.method}");
                
                // Check for immediate network failures
                if (request.result == UnityWebRequest.Result.ConnectionError || 
                    request.result == UnityWebRequest.Result.ProtocolError ||
                    request.responseCode == 0)
                {
                    Debug.LogError($"🌐 mOUND: NETWORK FAILURE DETECTED");
                    Debug.LogError($"🌐 mOUND: This is likely a Unity Editor networking issue");
                    Debug.LogError($"🌐 mOUND: Response Code: {request.responseCode}");
                    Debug.LogError($"🌐 mOUND: Error: {request.error}");
                    Debug.LogError($"🌐 mOUND: Result: {request.result}");
                }
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"🔐 mOUND: Token is valid");
                    isLoggedIn = true;
                    isValidatingToken = false;
                    
                    // Try to get username from response
                    try
                    {
                        var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                        username = response.user.username;
                        Debug.Log($"🔐 mOUND: Username set to: {username}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"🔐 mOUND: Could not parse username from response: {e.Message}");
                        // If parsing fails, use a default username
                        username = "Unity User";
                    }
                    
                    SaveCredentials();
                    EditorUtility.DisplayDialog("Success", "Token validated successfully!\nFetching organizations...", "OK");
                    _ = FetchOrganizationsAsync(); // Use the working async method
                }
                else
                {
                    string errorMsg;
                    if (request.responseCode == 0)
                    {
                        errorMsg = $"Network error: {request.error}. Check your internet connection.";
                    }
                    else if (request.responseCode == 401)
                    {
                        errorMsg = "Invalid or expired token. Please generate a new token from the Profile page.";
                    }
                    else
                    {
                        errorMsg = $"HTTP {request.responseCode}: {request.downloadHandler.text}";
                    }
                    
                    Debug.LogError($"🔐 mOUND: Token validation failed: {errorMsg}");
                    EditorUtility.DisplayDialog("Token Validation Failed", errorMsg, "OK");
                    isValidatingToken = false;
                    
                    // Don't auto-logout on network errors, only on auth errors
                    if (request.responseCode == 401 || request.responseCode == 403)
                    {
                        Logout();
                    }
                }
            }
        }
        
        private void Logout()
        {
            authToken = "";
            isLoggedIn = false;
            organizations.Clear();
            selectedOrgIndex = 0;
            organizationId = "";
            
            EditorPrefs.DeleteKey("mOUND_AuthToken");
            EditorPrefs.SetBool("mOUND_IsLoggedIn", false);
        }
        
        private IEnumerator UploadZipFile(string zipPath)
        {
            Debug.Log($"📤 mOUND: === UPLOAD START ===");
            Debug.Log($"📤 mOUND: ZIP Path: {zipPath}");
            Debug.Log($"📤 mOUND: App Name: {appName}");
            Debug.Log($"📤 mOUND: Description: {appDescription}");
            Debug.Log($"📤 mOUND: Organization ID: {organizationId}");
            Debug.Log($"📤 mOUND: Is Public: {isPublic}");
            Debug.Log($"📤 mOUND: Auth Token Length: {authToken?.Length ?? 0}");
            
            if (!File.Exists(zipPath))
            {
                Debug.LogError($"❌ mOUND: ZIP file not found: {zipPath}");
                EditorUtility.DisplayDialog("Error", $"ZIP file not found: {zipPath}", "OK");
                yield break;
            }
            
            byte[] zipData = File.ReadAllBytes(zipPath);
            long fileSizeMB = zipData.Length / (1024 * 1024);
            Debug.Log($"📤 mOUND: ZIP file size: {zipData.Length} bytes ({fileSizeMB} MB)");
            
            // Check file size limits (common server limits are 50-100MB)
            if (zipData.Length > 100 * 1024 * 1024) // 100MB
            {
                EditorUtility.ClearProgressBar();
                string sizeWarning = $"ZIP file is too large: {fileSizeMB} MB\n\n" +
                                   "Server limit is typically 100MB.\n\n" +
                                   "Solutions:\n" +
                                   "• Enable WebGL compression in Build Settings\n" +
                                   "• Remove large assets from build\n" +
                                   "• Use Development Build for smaller size\n" +
                                   "• Optimize textures and audio";
                
                Debug.LogError($"❌ mOUND: {sizeWarning}");
                EditorUtility.DisplayDialog("File Too Large", sizeWarning, "OK");
                yield break;
            }
            else if (zipData.Length > 50 * 1024 * 1024) // 50MB warning
            {
                string sizeWarning = $"Large file warning: {fileSizeMB} MB\n\n" +
                                   "This may fail on some servers.\n" +
                                   "Consider optimizing your build.";
                
                Debug.LogWarning($"⚠️ mOUND: {sizeWarning}");
                
                if (!EditorUtility.DisplayDialog("Large File Warning", 
                    sizeWarning + "\n\nContinue upload anyway?", "Yes", "Cancel"))
                {
                    EditorUtility.ClearProgressBar();
                    yield break;
                }
            }
            
            using (UnityWebRequest request = new UnityWebRequest(apiUrl + "/api/applications", "POST"))
            {
                // Create form data
                List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
                formData.Add(new MultipartFormDataSection("name", appName));
                formData.Add(new MultipartFormDataSection("description", appDescription));
                formData.Add(new MultipartFormDataSection("organizationId", organizationId));
                formData.Add(new MultipartFormDataSection("isPublic", isPublic.ToString().ToLower()));
                formData.Add(new MultipartFormFileSection("build", zipData, appName + ".zip", "application/zip"));
                
                byte[] boundary = UnityWebRequest.GenerateBoundary();
                byte[] formSections = UnityWebRequest.SerializeFormSections(formData, boundary);
                
                request.uploadHandler = new UploadHandlerRaw(formSections);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "multipart/form-data; boundary=" + System.Text.Encoding.UTF8.GetString(boundary));
                request.SetRequestHeader("Authorization", "Bearer " + authToken);
                request.SetRequestHeader("User-Agent", "Unity-mOUND-Plugin/1.0.0");
                request.timeout = 300; // 5 minute timeout for uploads
                
                // Unity 6 certificate handler
                request.certificateHandler = new AcceptAllCertificatesSignedWithASpecificKeyPublicKey();
                request.disposeCertificateHandlerOnDispose = true;
                
                Debug.Log($"📤 mOUND: Sending upload request to {request.url}");
                Debug.Log($"📤 mOUND: Request method: {request.method}");
                
                yield return request.SendWebRequest();
                
                // Ensure request is fully completed
                while (!request.isDone)
                {
                    yield return null;
                }
                
                Debug.Log($"📤 mOUND: === UPLOAD RESPONSE ===");
                Debug.Log($"📤 mOUND: Request fully completed, isDone: {request.isDone}");
                Debug.Log($"📤 mOUND: Response Code: {request.responseCode}");
                Debug.Log($"📤 mOUND: Result: {request.result}");
                Debug.Log($"📤 mOUND: Error: {request.error ?? "None"}");
                Debug.Log($"📤 mOUND: Response Text: {request.downloadHandler?.text ?? "None"}");
                
                EditorUtility.ClearProgressBar();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"✅ mOUND: Upload successful!");
                    EditorUtility.DisplayDialog("Success", "Application uploaded successfully!", "OK");
                    
                    // Clean up
                    if (File.Exists(zipPath))
                    {
                        File.Delete(zipPath);
                    }
                    if (Directory.Exists("Builds/WebGL"))
                    {
                        Directory.Delete("Builds/WebGL", true);
                    }
                }
                else
                {
                    string errorDetails = $"Upload failed:\n" +
                                        $"Result: {request.result}\n" +
                                        $"Response Code: {request.responseCode}\n" +
                                        $"Error: {request.error ?? "None"}\n" +
                                        $"Response: {request.downloadHandler?.text ?? "None"}";
                    
                    Debug.LogError($"❌ mOUND: {errorDetails}");
                    EditorUtility.DisplayDialog("Upload Failed", errorDetails, "OK");
                }
            }
        }
        
        private IEnumerator UpdateAppCoroutine(string appId, string zipPath)
        {
            Debug.Log($"🔄 mOUND: Starting update of app {appId} with {zipPath}");
            
            if (!File.Exists(zipPath))
            {
                Debug.LogError($"❌ mOUND: ZIP file not found: {zipPath}");
                EditorUtility.DisplayDialog("Error", $"ZIP file not found: {zipPath}", "OK");
                yield break;
            }
            
            byte[] zipData = File.ReadAllBytes(zipPath);
            long fileSizeMB = zipData.Length / (1024 * 1024);
            Debug.Log($"🔄 mOUND: Update ZIP file size: {zipData.Length} bytes ({fileSizeMB} MB)");
            
            // Check file size limits for updates too
            if (zipData.Length > 100 * 1024 * 1024) // 100MB
            {
                EditorUtility.ClearProgressBar();
                string sizeWarning = $"Update ZIP file is too large: {fileSizeMB} MB\n\n" +
                                   "Server limit is typically 100MB.\n\n" +
                                   "Solutions:\n" +
                                   "• Enable WebGL compression in Build Settings\n" +
                                   "• Remove large assets from build\n" +
                                   "• Use Development Build for smaller size\n" +
                                   "• Optimize textures and audio";
                
                Debug.LogError($"❌ mOUND: {sizeWarning}");
                EditorUtility.DisplayDialog("Update File Too Large", sizeWarning, "OK");
                yield break;
            }
            else if (zipData.Length > 50 * 1024 * 1024) // 50MB warning
            {
                string sizeWarning = $"Large update file warning: {fileSizeMB} MB\n\n" +
                                   "This may fail on some servers.\n" +
                                   "Consider optimizing your build.";
                
                Debug.LogWarning($"⚠️ mOUND: {sizeWarning}");
                
                if (!EditorUtility.DisplayDialog("Large Update Warning", 
                    sizeWarning + "\n\nContinue update anyway?", "Yes", "Cancel"))
                {
                    EditorUtility.ClearProgressBar();
                    yield break;
                }
            }
            
            using (UnityWebRequest request = new UnityWebRequest(apiUrl + "/api/applications/" + appId + "/versions", "POST"))
            {
                // Create form data for update
                List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
                formData.Add(new MultipartFormDataSection("name", appName));
                formData.Add(new MultipartFormDataSection("description", appDescription));
                formData.Add(new MultipartFormDataSection("isPublic", isPublic.ToString().ToLower()));
                formData.Add(new MultipartFormDataSection("changelog", changelogText));
                formData.Add(new MultipartFormFileSection("build", zipData, appName + "_update.zip", "application/zip"));
                
                byte[] boundary = UnityWebRequest.GenerateBoundary();
                byte[] formSections = UnityWebRequest.SerializeFormSections(formData, boundary);
                
                request.uploadHandler = new UploadHandlerRaw(formSections);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "multipart/form-data; boundary=" + System.Text.Encoding.UTF8.GetString(boundary));
                request.SetRequestHeader("Authorization", "Bearer " + authToken);
                request.SetRequestHeader("User-Agent", "Unity-mOUND-Plugin/1.0.0");
                request.timeout = 300; // 5 minute timeout for uploads
                
                // Unity 6 certificate handler
                request.certificateHandler = new AcceptAllCertificatesSignedWithASpecificKeyPublicKey();
                request.disposeCertificateHandlerOnDispose = true;
                
                yield return request.SendWebRequest();
                
                // Ensure request is fully completed
                while (!request.isDone)
                {
                    yield return null;
                }
                
                EditorUtility.ClearProgressBar();
                
                Debug.Log($"📤 mOUND: Request fully completed, isDone: {request.isDone}");
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"✅ mOUND: App update successful!");
                    EditorUtility.DisplayDialog("Success", $"Application '{appName}' updated successfully!", "OK");
                    
                    // Refresh the apps list
                    _ = FetchApplicationsAsync(organizationId);
                    
                    // Clean up
                    if (File.Exists(zipPath))
                    {
                        File.Delete(zipPath);
                    }
                    if (Directory.Exists("Builds/WebGL"))
                    {
                        Directory.Delete("Builds/WebGL", true);
                    }
                }
                else
                {
                    string errorDetails = $"App update failed:\n" +
                                        $"Result: {request.result}\n" +
                                        $"Response Code: {request.responseCode}\n" +
                                        $"Error: {request.error ?? "None"}\n" +
                                        $"Response: {request.downloadHandler?.text ?? "None"}";
                    
                    Debug.LogError($"❌ mOUND: {errorDetails}");
                    EditorUtility.DisplayDialog("Update Failed", errorDetails, "OK");
                }
            }
        }
        
        private void StartCoroutine(IEnumerator coroutine)
        {
            EditorApplication.update += () => UpdateCoroutine(coroutine);
        }
        
        private void UpdateCoroutine(IEnumerator coroutine)
        {
            try
            {
                if (!coroutine.MoveNext())
                {
                    EditorApplication.update -= () => UpdateCoroutine(coroutine);
                }
            }
            catch (System.Exception e)
            {
                EditorApplication.update -= () => UpdateCoroutine(coroutine);
                Debug.LogError("Coroutine error: " + e.Message);
            }
        }
    }
}
