using UnityEngine;
using UnityEditor;

namespace mOUND
{
    [System.Serializable]
    public class mOUNDSettings : ScriptableObject
    {
        [Header("Platform Configuration")]
        public string apiUrl = "https://mound.gllc.io";
        
        [Header("Build Settings")]
        public bool compressBuilds = true;
        public bool developmentBuild = false;
        public bool autoCleanup = true;
        
        [Header("Upload Defaults")]
        public string defaultAppName = "";
        public string defaultDescription = "";
        public bool defaultPublic = false;
        
        private static mOUNDSettings instance;
        
        public static mOUNDSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<mOUNDSettings>("mOUNDSettings");
                    if (instance == null)
                    {
                        instance = CreateInstance<mOUNDSettings>();
                        
                        // Create Resources folder if it doesn't exist
                        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                        {
                            AssetDatabase.CreateFolder("Assets", "Resources");
                        }
                        
                        AssetDatabase.CreateAsset(instance, "Assets/Resources/mOUNDSettings.asset");
                        AssetDatabase.SaveAssets();
                    }
                }
                return instance;
            }
        }
        
        [MenuItem("mOUND/Settings")]
        public static void OpenSettings()
        {
            Selection.activeObject = Instance;
            EditorGUIUtility.PingObject(Instance);
        }
    }
}
