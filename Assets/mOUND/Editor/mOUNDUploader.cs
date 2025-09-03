using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System.IO.Compression;
using System.Collections;
using UnityEngine.Networking;
using System.Text;
using System;
using System.Collections.Generic;

namespace mOUND
{
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
        private Vector2 scrollPosition;
        
        private List<Organization> organizations = new List<Organization>();
        private int selectedOrgIndex = 0;
        
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
        
        [MenuItem("mOUND/Build and Upload")]
        public static void ShowWindow()
        {
            GetWindow<mOUNDUploader>("mOUND Uploader");
        }
        
        private void OnEnable()
        {
            LoadCredentials();
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
            
            GUILayout.Label("Username:");
            username = EditorGUILayout.TextField(username);
            
            GUILayout.Label("Password:");
            password = EditorGUILayout.PasswordField(password);
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Login"))
            {
                StartCoroutine(LoginCoroutine());
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
                selectedOrgIndex = EditorGUILayout.Popup(selectedOrgIndex, orgNames);
                organizationId = organizations[selectedOrgIndex].id;
            }
            else
            {
                GUILayout.Label("No organizations available");
                if (GUILayout.Button("Refresh Organizations"))
                {
                    StartCoroutine(FetchOrganizations());
                }
            }
            
            isPublic = EditorGUILayout.Toggle("Public Application", isPublic);
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("Build WebGL and Upload", GUILayout.Height(40)))
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
                
                BuildAndUpload();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Open mOUND Platform"))
            {
                Application.OpenURL(apiUrl);
            }
        }
        
        private void BuildAndUpload()
        {
            try
            {
                EditorUtility.DisplayProgressBar("mOUND Upload", "Building WebGL...", 0.1f);
                
                // Set WebGL build settings
                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
                buildPlayerOptions.scenes = GetEnabledScenes();
                buildPlayerOptions.locationPathName = "Builds/WebGL";
                buildPlayerOptions.target = BuildTarget.WebGL;
                buildPlayerOptions.options = BuildOptions.None;
                
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
            
            var loginData = new
            {
                username = this.username,
                password = this.password
            };
            
            string jsonData = JsonUtility.ToJson(loginData);
            Debug.Log($"🔐 mOUND: JSON payload: {jsonData}");
            
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            
            using (UnityWebRequest request = new UnityWebRequest(apiUrl + "/api/auth/login", "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                
                Debug.Log($"🔐 mOUND: Sending request to {request.url}");
                
                yield return request.SendWebRequest();
                
                Debug.Log($"🔐 mOUND: Response code: {request.responseCode}");
                Debug.Log($"🔐 mOUND: Response text: {request.downloadHandler.text}");
                Debug.Log($"🔐 mOUND: Request result: {request.result}");
                
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
                        StartCoroutine(FetchOrganizations());
                        
                        EditorUtility.DisplayDialog("Success", $"Logged in as {username}", "OK");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"🔐 mOUND: JSON parsing error: {e.Message}");
                        EditorUtility.DisplayDialog("Login Failed", "Invalid response from server: " + e.Message, "OK");
                    }
                }
                else
                {
                    string errorMsg = $"HTTP {request.responseCode}: {request.downloadHandler.text}";
                    Debug.LogError($"🔐 mOUND: Login failed - {errorMsg}");
                    EditorUtility.DisplayDialog("Login Failed", "Login failed: " + errorMsg, "OK");
                }
            }
        }
        
        private IEnumerator FetchOrganizations()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl + "/api/organizations"))
            {
                request.SetRequestHeader("Authorization", "Bearer " + authToken);
                
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
        
        private IEnumerator UploadZipFile(string zipPath)
        {
            byte[] zipData = File.ReadAllBytes(zipPath);
            
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("name", appName));
            formData.Add(new MultipartFormDataSection("description", appDescription));
            formData.Add(new MultipartFormDataSection("organizationId", organizationId));
            formData.Add(new MultipartFormDataSection("isPublic", isPublic.ToString().ToLower()));
            formData.Add(new MultipartFormFileSection("build", zipData, Path.GetFileName(zipPath), "application/zip"));
            
            using (UnityWebRequest request = UnityWebRequest.Post(apiUrl + "/api/applications", formData))
            {
                request.SetRequestHeader("Authorization", "Bearer " + authToken);
                
                var uploadProgress = request.SendWebRequest();
                
                while (!uploadProgress.isDone)
                {
                    EditorUtility.DisplayProgressBar("mOUND Upload", 
                        $"Uploading... {(request.uploadProgress * 100):F0}%", 
                        0.8f + (request.uploadProgress * 0.2f));
                    yield return null;
                }
                
                EditorUtility.ClearProgressBar();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    EditorUtility.DisplayDialog("Upload Successful", 
                        $"'{appName}' has been uploaded to mOUND Platform successfully!", "OK");
                    
                    // Clean up build files
                    try
                    {
                        if (Directory.Exists("Builds/WebGL"))
                            Directory.Delete("Builds/WebGL", true);
                        if (File.Exists(zipPath))
                            File.Delete(zipPath);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("Failed to clean up build files: " + e.Message);
                    }
                    
                    // Open the platform
                    if (EditorUtility.DisplayDialog("Success", 
                        "Upload complete! Would you like to open the mOUND Platform?", "Yes", "No"))
                    {
                        Application.OpenURL(apiUrl);
                    }
                }
                else
                {
                    string errorMsg = "Upload failed: " + request.error;
                    if (!string.IsNullOrEmpty(request.downloadHandler.text))
                    {
                        errorMsg += "\nServer response: " + request.downloadHandler.text;
                    }
                    EditorUtility.DisplayDialog("Upload Failed", errorMsg, "OK");
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
        
        private IEnumerator ValidateToken()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl + "/api/auth/me"))
            {
                request.SetRequestHeader("Authorization", "Bearer " + authToken);
                
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    StartCoroutine(FetchOrganizations());
                }
                else
                {
                    // Token is invalid
                    Logout();
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
