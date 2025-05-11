using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class RabbitController : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform waypoint1;
    public Transform waypoint2;
    [SerializeField] private float moveSpeed = 2f;
    
    [Header("References")]
    [SerializeField] private GameObject rabbitModel; // Separate reference for the visual model
    [SerializeField] private SpriteRenderer rabbitSprite; // Reference to the sprite renderer
    [SerializeField] private Sprite hitSprite; // Sprite to show when hit
    [SerializeField] private float hitSpriteDisplayTime = 1f; // How long to show the hit sprite
    
    [Header("Special Rabbit Settings")]
    [SerializeField] private bool isSpecialRabbit = false;
    [SerializeField] private GameObject humanModel; // Referenced separately
    
    [Header("Twist Settings")]
    [SerializeField] private float slowMotionTimeScale = 0.3f;
    [SerializeField] private float slowMotionDuration = 2.0f;
    [SerializeField] private string nextSceneName = "TransitionScene";
    
    private Transform currentTarget;
    private bool isDead = false;
    private Sprite originalSprite; // Store the original sprite
    private GameManager gameManager;
    private UIManager uiManager;
    
    void Start()
    {
        // Get references
        gameManager = FindObjectOfType<GameManager>();
        uiManager = FindObjectOfType<UIManager>();
        
        // Set initial target waypoint
        if (waypoint1 != null)
        {
            currentTarget = waypoint1;
        }
        else
        {
            Debug.LogError("Waypoints not assigned for rabbit: " + gameObject.name);
        }
        
        // If rabbitModel isn't set, use this GameObject
        if (rabbitModel == null)
        {
            // Try to find a child with a renderer
            Renderer renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                rabbitModel = renderer.gameObject;
            }
            else
            {
                // Fall back to using this object
                rabbitModel = gameObject;
                Debug.LogWarning("No rabbit model assigned - using controller GameObject");
            }
        }
        
        // If rabbitSprite isn't set, try to find one
        if (rabbitSprite == null)
        {
            rabbitSprite = GetComponentInChildren<SpriteRenderer>();
        }
        
        // Store the original sprite if we have a sprite renderer
        if (rabbitSprite != null && rabbitSprite.sprite != null)
        {
            originalSprite = rabbitSprite.sprite;
        }
        
        // Make sure the human model is disabled at start
        if (humanModel != null)
        {
            humanModel.SetActive(false);
        }
        else if (isSpecialRabbit)
        {
            Debug.LogError("This is marked as a special rabbit but no human model is assigned!");
        }
    }
    
    void Update()
    {
        if (isDead || currentTarget == null) return;
        
        // Move towards current waypoint
        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget.position,
            moveSpeed * Time.deltaTime
        );
        
        // Check if reached waypoint
        if (Vector3.Distance(transform.position, currentTarget.position) < 0.1f)
        {
            // Switch target
            currentTarget = (currentTarget == waypoint1) ? waypoint2 : waypoint1;
        }
    }
    
    public void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        // Show hit sprite if available
        bool canShowHitSprite = ShowHitSprite();
        
        // Notify game manager to count the kill
        if (gameManager != null)
        {
            gameManager.RegisterKill();
        }
        
        if (isSpecialRabbit)
        {
            // If we're showing a hit sprite first, delay the reveal
            if (canShowHitSprite)
            {
                DOVirtual.DelayedCall(hitSpriteDisplayTime, RevealHuman);
            }
            else
            {
                // Otherwise reveal immediately
                RevealHuman();
            }
        }
        else
        {
            // For regular rabbits, destroy after showing hit sprite
            if (canShowHitSprite)
            {
                DOVirtual.DelayedCall(hitSpriteDisplayTime, () => {
                    Destroy(gameObject);
                });
            }
            else
            {
                // No hit sprite, destroy immediately
                Destroy(gameObject);
            }
        }
    }
    
    private bool ShowHitSprite()
    {
        // If we have both a sprite renderer and a hit sprite, show it
        if (rabbitSprite != null && hitSprite != null)
        {
            rabbitSprite.sprite = hitSprite;
            return true;
        }
        return false;
    }
    
    private void RevealHuman()
    {
        // Play the shatter sound
        if (gameManager != null)
        {
            gameManager.PlaySound("shatter");
        }
        
        // Disable just the rabbit MODEL, not the controller GameObject
        if (rabbitModel != null && rabbitModel != gameObject)
        {
            rabbitModel.SetActive(false);
        }
        else
        {
            // If rabbitModel is this gameObject, we need to handle differently
            // Hide renderers but keep the GameObject active
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.enabled = false;
            }
        }
        
        // Activate the human model
        if (humanModel != null)
        {
            humanModel.SetActive(true);
            
            // Ensure the human appears at the right position
            humanModel.transform.position = transform.position;
        }
        
        // Slow down time for dramatic effect
        Time.timeScale = slowMotionTimeScale;
        
        // Play scream sound
        if (gameManager != null)
        {
            gameManager.PlaySound("scream");
        }
        
        // Disable player controls
        FirstPersonController fpc = FindObjectOfType<FirstPersonController>();
        if (fpc != null)
        {
            fpc.playerCanMove = false;
            fpc.cameraCanMove = false;
        }
        
        // Hide the gun UI
        if (uiManager != null)
        {
            uiManager.SetGunVisible(false);
        }
        
        // After slow motion duration, transition to next scene
        DOVirtual.DelayedCall(slowMotionDuration * (1/slowMotionTimeScale), () => {
            // Reset time scale before scene transition
            Time.timeScale = 1.0f;
            
            // Load the transition scene
            SceneManager.LoadScene(nextSceneName);
        }).SetUpdate(true); // Important: Make it time-scale independent
    }
}
