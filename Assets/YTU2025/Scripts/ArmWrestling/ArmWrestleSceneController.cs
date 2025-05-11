using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class ArmWrestleSceneController : MonoBehaviour
{
    [Header("Scene Setup")]
    [SerializeField] private AudioClip prisonDoorSound;
    [SerializeField] private float initialFadeInDuration = 1.5f;
    [SerializeField] private float doorSoundDelay = 0.5f;
    
    private UIManager uiManager;
    private SoundManager soundManager;
    
    private void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();
        soundManager = FindObjectOfType<SoundManager>();
    }
    
    private void Start()
    {
        // Start with black screen
        if (uiManager != null)
        {
            // Setup fade from black
            uiManager.FadeFromBlack(initialFadeInDuration, null);
        }
        
        // Play prison door sound after delay
        if (prisonDoorSound != null)
        {
            if (soundManager != null)
            {
                // Add sound to sound manager if needed
                SoundManager.Sound doorSound = System.Array.Find(soundManager.sounds, sound => sound.name == "PrisonDoor");
                
                if (doorSound == null)
                {
                    // Door sound not in sound manager, play it directly
                    AudioSource.PlayClipAtPoint(prisonDoorSound, Camera.main.transform.position, 1.0f);
                }
                else
                {
                    // Use sound manager with delay
                    DOVirtual.DelayedCall(doorSoundDelay, () => {
                        GameManager.Instance?.PlaySound("PrisonDoor");
                    });
                }
            }
            else
            {
                // No sound manager, play directly
                DOVirtual.DelayedCall(doorSoundDelay, () => {
                    AudioSource.PlayClipAtPoint(prisonDoorSound, Camera.main.transform.position, 1.0f);
                });
            }
        }
    }
}

// Helper class to add compatibility if the Sound class from SoundManager isn't available
[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    public bool loop = false;
    [Range(0f, 1f)]
    public float spatialBlend = 0f; // 0 = 2D, 1 = 3D
    
    [HideInInspector]
    public AudioSource source;
}
