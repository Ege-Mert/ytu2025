using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class TransitionSceneController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float riseHeight = 5f;
    [SerializeField] private float riseTime = 6f;
    [SerializeField] private float rotationAngle = 180f;
    [SerializeField] private float rotationTime = 6f;
    [SerializeField] private Ease cameraEase = Ease.InOutSine;
    
    [Header("Sound Settings")]
    [SerializeField] private AudioClip policeSound;
    [SerializeField] private AudioClip ambienceSound;
    [SerializeField] private float volumeFadeTime = 2f;
    
    [Header("Scene Transition")]
    [SerializeField] private float sceneTime = 8f;
    [SerializeField] private float fadeOutTime = 2f;
    [SerializeField] private string courtSceneName = "CourtScene";
    
    private AudioSource audioSource;
    private UIManager uiManager;
    
    void Start()
    {
        // Get references
        uiManager = FindObjectOfType<UIManager>();
        
        // If camera transform is not set, use the main camera
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main?.transform;
            
            if (cameraTransform == null)
            {
                Debug.LogError("No camera found for TransitionSceneController!");
                return;
            }
        }
        
        // Setup audio
        SetupAudio();
        
        // Start camera movement sequence
        StartCameraSequence();
    }
    
    private void SetupAudio()
    {
        // Create audio source if needed
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Setup audio properties
        audioSource.loop = true;
        audioSource.volume = 1f;
        
        // Start with police sounds
        if (policeSound != null)
        {
            audioSource.clip = policeSound;
            audioSource.Play();
        }
    }
    
    private void StartCameraSequence()
    {
        // Store initial position and rotation
        Vector3 initialPosition = cameraTransform.position;
        Quaternion initialRotation = cameraTransform.rotation;
        
        // Setup the rise animation
        cameraTransform.DOMoveY(initialPosition.y + riseHeight, riseTime)
            .SetEase(cameraEase);
        
        // Setup the rotation animation
        cameraTransform.DORotate(new Vector3(
            initialRotation.eulerAngles.x,
            initialRotation.eulerAngles.y + rotationAngle,
            initialRotation.eulerAngles.z
        ), rotationTime).SetEase(cameraEase);
        
        // Schedule the scene transition
        DOVirtual.DelayedCall(sceneTime - fadeOutTime, () => {
            if (uiManager != null)
            {
                // Fade out to black
                uiManager.FadeToBlack(fadeOutTime, () => {
                    // Load the court scene
                    SceneManager.LoadScene(courtSceneName);
                });
            }
            else
            {
                // No UI manager, just delay and load
                DOVirtual.DelayedCall(fadeOutTime, () => {
                    SceneManager.LoadScene(courtSceneName);
                });
            }
            
            // Fade out audio
            if (audioSource != null)
            {
                audioSource.DOFade(0, fadeOutTime);
            }
        });
    }
}
