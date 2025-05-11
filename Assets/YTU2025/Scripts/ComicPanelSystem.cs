using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class ComicPanelSystem : MonoBehaviour
{
    [System.Serializable]
    public class ComicPanel
    {
        public RectTransform panelRect;
        public Image panelImage;
        public string soundEffectName; // Optional sound for this panel
        [TextArea(2, 4)]
        public string caption; // Optional caption text for the panel
    }
    
    [Header("Comic Panel Settings")]
    [SerializeField] private List<ComicPanel> comicPanels = new List<ComicPanel>();
    [SerializeField] private string nextSceneName; // Scene to load after the last panel
    [SerializeField] private float panelFadeDuration = 0.5f;
    [SerializeField] private float inactivityTimeForHint = 3f; // Seconds before showing hint
    [SerializeField] private string pageFlipSoundName = "PageFlip"; // Default sound for page flip
    
    [Header("UI References")]
    [SerializeField] private Canvas comicCanvas;
    [SerializeField] private TextMeshProUGUI clickToContinueText;
    [SerializeField] private TextMeshProUGUI captionText; // Optional caption display
    [SerializeField] private float captionFadeDuration = 0.3f;
    [SerializeField] private CanvasGroup fadeToBlackGroup; // For final transition
    
    private int currentPanelIndex = -1;
    private float inactivityTimer = 0f;
    private bool isComicActive = false;
    private Coroutine inactivityCoroutine;
    
    void Awake()
    {
        // Set up initial state
        if (comicCanvas != null)
        {
            comicCanvas.gameObject.SetActive(true);
        }
        
        // Hide all panels initially
        foreach (ComicPanel panel in comicPanels)
        {
            if (panel.panelImage != null)
            {
                Color c = panel.panelImage.color;
                c.a = 0f;
                panel.panelImage.color = c;
            }
        }
        
        // Hide click to continue text initially
        if (clickToContinueText != null)
        {
            clickToContinueText.gameObject.SetActive(false);
        }
        
        // Hide caption text initially
        if (captionText != null)
        {
            captionText.gameObject.SetActive(false);
        }
        
        // Set up the fade to black group
        if (fadeToBlackGroup != null)
        {
            fadeToBlackGroup.alpha = 0f;
        }
    }
    
    void Start()
    {
        // Start the comic sequence after a short delay
        Invoke("StartComic", 0.5f);
    }
    
    void Update()
    {
        // Check for input to advance to the next panel
        if (isComicActive && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            ShowNextPanel();
        }
        
        // Track inactivity
        if (isComicActive)
        {
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                // Reset the inactivity timer
                inactivityTimer = 0f;
                
                // Hide the hint if it's showing
                if (clickToContinueText != null && clickToContinueText.gameObject.activeInHierarchy)
                {
                    clickToContinueText.gameObject.SetActive(false);
                    DOTween.Kill(clickToContinueText.transform);
                }
            }
            else
            {
                // Increment the inactivity timer
                inactivityTimer += Time.deltaTime;
                
                // Show the hint if inactive for too long
                if (inactivityTimer >= inactivityTimeForHint && clickToContinueText != null)
                {
                    if (!clickToContinueText.gameObject.activeInHierarchy)
                    {
                        clickToContinueText.gameObject.SetActive(true);
                        // Animate the hint
                        clickToContinueText.transform.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
                    }
                }
            }
        }
    }
    
    public void StartComic()
    {
        if (comicPanels.Count == 0) return;
        
        isComicActive = true;
        inactivityTimer = 0f;
        currentPanelIndex = -1;
        
        // Show the first panel
        ShowNextPanel();
    }
    
    public void ShowNextPanel()
    {
        // Reset inactivity timer
        inactivityTimer = 0f;
        
        // Hide the hint if it's showing
        if (clickToContinueText != null && clickToContinueText.gameObject.activeInHierarchy)
        {
            clickToContinueText.gameObject.SetActive(false);
            DOTween.Kill(clickToContinueText.transform);
        }
        
        // Increment panel index
        currentPanelIndex++;
        
        // Check if we've shown all panels
        if (currentPanelIndex >= comicPanels.Count)
        {
            FinalizeComic();
            return;
        }
        
        ComicPanel currentPanel = comicPanels[currentPanelIndex];
        
        // Play the page flip sound (default or panel-specific)
        string soundToPlay = !string.IsNullOrEmpty(currentPanel.soundEffectName) ? 
            currentPanel.soundEffectName : pageFlipSoundName;
            
        if (!string.IsNullOrEmpty(soundToPlay) && SoundManager.instance != null)
        {
            SoundManager.instance.Play(soundToPlay);
        }
        
        // Show the current panel with fade in
        if (currentPanel.panelImage != null)
        {
            // Set up starting position slightly offset
            currentPanel.panelRect.anchoredPosition = new Vector2(currentPanel.panelRect.anchoredPosition.x + 20f, 
                currentPanel.panelRect.anchoredPosition.y);
            
            // Fade in the panel
            currentPanel.panelImage.DOFade(1f, panelFadeDuration);
            
            // Move to final position
            currentPanel.panelRect.DOAnchorPos(
                new Vector2(currentPanel.panelRect.anchoredPosition.x - 20f, currentPanel.panelRect.anchoredPosition.y), 
                panelFadeDuration * 1.2f).SetEase(Ease.OutQuad);
        }
        
        // Show caption if available
        if (captionText != null && !string.IsNullOrEmpty(currentPanel.caption))
        {
            captionText.text = currentPanel.caption;
            captionText.gameObject.SetActive(true);
            
            // Fade in caption
            CanvasGroup captionGroup = captionText.GetComponent<CanvasGroup>();
            if (captionGroup == null)
            {
                captionGroup = captionText.gameObject.AddComponent<CanvasGroup>();
            }
            
            captionGroup.alpha = 0f;
            captionGroup.DOFade(1f, captionFadeDuration);
        }
        else if (captionText != null)
        {
            captionText.gameObject.SetActive(false);
        }
    }
    
    private void FinalizeComic()
    {
        isComicActive = false;
        
        // Fade out caption if showing
        if (captionText != null && captionText.gameObject.activeInHierarchy)
        {
            CanvasGroup captionGroup = captionText.GetComponent<CanvasGroup>();
            if (captionGroup != null)
            {
                captionGroup.DOFade(0f, captionFadeDuration);
            }
        }
        
        // Fade to black
        if (fadeToBlackGroup != null)
        {
            fadeToBlackGroup.gameObject.SetActive(true);
            fadeToBlackGroup.DOFade(1f, 1f).OnComplete(() => {
                // Load the next scene
                if (!string.IsNullOrEmpty(nextSceneName))
                {
                    SceneManager.LoadScene(nextSceneName);
                }
            });
        }
        else
        {
            // No fade group, just load the next scene
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
    
    // Method to manually set the next scene name
    public void SetNextScene(string sceneName)
    {
        nextSceneName = sceneName;
    }
    
    // Method to add a panel programmatically (useful for dynamic comics)
    public void AddPanel(RectTransform panelRect, Image panelImage, string soundEffect = "", string caption = "")
    {
        ComicPanel newPanel = new ComicPanel
        {
            panelRect = panelRect,
            panelImage = panelImage,
            soundEffectName = soundEffect,
            caption = caption
        };
        
        comicPanels.Add(newPanel);
    }
}
