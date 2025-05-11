using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class DefaultOperationConfigCreator
{
    [MenuItem("Assets/Create/YTU2025/Default Operation Config")]
    public static void CreateDefaultOperationConfig()
    {
        // Create the scriptable object
        OperationConfig config = ScriptableObject.CreateInstance<OperationConfig>();
        
        // Set default values
        config.leadColors = new Color[] {
            new Color(0.8f, 0.2f, 0.2f, 1f),  // Red
            new Color(0.2f, 0.8f, 0.2f, 1f),  // Green
            new Color(0.8f, 0.8f, 0.2f, 1f),  // Yellow
            new Color(0.2f, 0.2f, 0.8f, 1f),  // Blue
            new Color(0.8f, 0.2f, 0.8f, 1f)   // Magenta
        };
        
        config.operationTime = 30f;
        config.warningThreshold = 0.2f;
        config.snapRadius = 50f;
        config.incorrectTimePenalty = 1f;
        config.screenShakeAmount = 0.1f;
        config.screenShakeDuration = 0.2f;
        
        // Create the asset file
        string path = "Assets/YTU2025/Configs/DefaultOperationConfig.asset";
        
        // Make sure the directory exists
        string directory = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        
        // Create the asset
        AssetDatabase.CreateAsset(config, path);
        AssetDatabase.SaveAssets();
        
        // Focus on the asset in the Project window
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = config;
        
        Debug.Log("Created Default Operation Config at " + path);
    }
}
#endif
