using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

// This is a helper class that can be used to quickly set up a main menu scene
// It's mostly useful in the Unity Editor
public class MainMenuSetup : MonoBehaviour
{
    [Header("Scene Creation")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    [Header("UI Prefabs")]
    [SerializeField] private GameObject pauseMenuPrefab;
    
    // This method is only used in the editor to create a main menu scene
    #if UNITY_EDITOR
    [ContextMenu("Create Main Menu Scene")]
    public void CreateMainMenuScene()
    {
        // Create a new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // Create a camera and set it up
        var mainCamera = new GameObject("Main Camera");
        var camera = mainCamera.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        camera.transform.position = new Vector3(0, 1, -10);
        mainCamera.tag = "MainCamera";
        mainCamera.AddComponent<AudioListener>();
        
        // Create the UI
        var canvasObj = new GameObject("Main Menu Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Add other required components
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create panel background
        var panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        var panel = panelObj.AddComponent<Image>();
        panel.color = new Color(0f, 0f, 0f, 0.8f);
        var panelRT = panel.rectTransform;
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.sizeDelta = Vector2.zero;
        
        // Create logo placeholder
        var logoObj = new GameObject("Logo");
        logoObj.transform.SetParent(canvasObj.transform, false);
        var logo = logoObj.AddComponent<Image>();
        logo.color = new Color(1f, 1f, 1f, 1f);
        var logoRT = logo.rectTransform;
        logoRT.anchorMin = new Vector2(0.5f, 0.7f);
        logoRT.anchorMax = new Vector2(0.5f, 0.7f);
        logoRT.sizeDelta = new Vector2(300f, 150f);
        
        // Create game title
        var titleObj = new GameObject("Game Title");
        titleObj.transform.SetParent(canvasObj.transform, false);
        var title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "Without Intention";
        title.fontSize = 48;
        title.alignment = TextAlignmentOptions.Center;
        title.color = new Color(1f, 1f, 1f, 1f);
        var titleRT = title.rectTransform;
        titleRT.anchorMin = new Vector2(0.5f, 0.55f);
        titleRT.anchorMax = new Vector2(0.5f, 0.55f);
        titleRT.sizeDelta = new Vector2(500f, 100f);
        
        // Create a button container
        var buttonContainerObj = new GameObject("Button Container");
        buttonContainerObj.transform.SetParent(canvasObj.transform, false);
        var buttonContainerRT = buttonContainerObj.AddComponent<RectTransform>();
        buttonContainerRT.anchorMin = new Vector2(0.5f, 0.3f);
        buttonContainerRT.anchorMax = new Vector2(0.5f, 0.3f);
        buttonContainerRT.sizeDelta = new Vector2(300f, 250f);
        
        // Create buttons
        CreateButton("Start Button", buttonContainerObj.transform, "Start Game", 0);
        CreateButton("Options Button", buttonContainerObj.transform, "Options", 1);
        CreateButton("Exit Button", buttonContainerObj.transform, "Exit", 2);
        
        // Create a fade overlay
        var fadeObj = new GameObject("Fade Overlay");
        fadeObj.transform.SetParent(canvasObj.transform, false);
        var fadeImg = fadeObj.AddComponent<Image>();
        fadeImg.color = new Color(0f, 0f, 0f, 0f); // Start transparent
        var fadeRT = fadeImg.rectTransform;
        fadeRT.anchorMin = Vector2.zero;
        fadeRT.anchorMax = Vector2.one;
        fadeRT.sizeDelta = Vector2.zero;
        fadeObj.AddComponent<CanvasGroup>();
        
        // Add the MainMenu script to the canvas
        var mainMenu = canvasObj.AddComponent<MainMenu>();
        
        // Add a sound manager if one doesn't exist
        var soundManager = FindObjectOfType<SoundManager>();
        if (soundManager == null)
        {
            var soundObj = new GameObject("Sound Manager");
            soundObj.AddComponent<SoundManager>();
        }
        
        // Save the scene
        EditorSceneManager.SaveScene(scene, "Assets/YTU2025/Scenes/" + mainMenuSceneName + ".unity");
        
        Debug.Log("Main menu scene created at: Assets/YTU2025/Scenes/" + mainMenuSceneName + ".unity");
    }
    
    private void CreateButton(string name, Transform parent, string text, int index)
    {
        var buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        // Add button component
        var button = buttonObj.AddComponent<Button>();
        
        // Add image for the button background
        var image = buttonObj.AddComponent<Image>();
        button.targetGraphic = image;
        
        // Set up button colors
        var colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        colors.selectedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        button.colors = colors;
        
        // Position the button
        var buttonRT = button.GetComponent<RectTransform>();
        buttonRT.anchorMin = new Vector2(0.5f, 1f);
        buttonRT.anchorMax = new Vector2(0.5f, 1f);
        buttonRT.pivot = new Vector2(0.5f, 1f);
        buttonRT.sizeDelta = new Vector2(200f, 50f);
        buttonRT.anchoredPosition = new Vector2(0f, -index * 70f);
        
        // Add a text component for the button
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        var textComp = textObj.AddComponent<TextMeshProUGUI>();
        textComp.text = text;
        textComp.fontSize = 24;
        textComp.alignment = TextAlignmentOptions.Center;
        
        // Position the text
        var textRT = textComp.rectTransform;
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.sizeDelta = Vector2.zero;
        
        // Add a canvas group for fading
        buttonObj.AddComponent<CanvasGroup>();
    }
    #endif
}
