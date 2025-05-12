using UnityEngine;

[RequireComponent(typeof(FirstPersonController))]
public class FootstepPlayer : MonoBehaviour
{
    [Header("Footstep Settings")]
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.3f;
    [SerializeField] private float crouchStepInterval = 0.7f;
    [SerializeField] private float minimumVelocity = 0.1f;
    
    [Header("References")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip[] runFootstepSounds;
    [SerializeField] private string footstepSoundName = "footstep";
    
    private FirstPersonController fpsController;
    private Rigidbody rb;
    private float stepTimer = 0;
    private float currentStepInterval;
    
    void Start()
    {
        fpsController = GetComponent<FirstPersonController>();
        rb = GetComponent<Rigidbody>();
        
        if (fpsController == null)
        {
            Debug.LogError("No FirstPersonController found on this GameObject!");
            enabled = false;
            return;
        }
        
        if (rb == null)
        {
            Debug.LogError("No Rigidbody found on this GameObject!");
            enabled = false;
            return;
        }
    }
    
    void Update()
    {
        if (fpsController.playerCanMove && IsMoving() && IsGrounded())
        {
            // Determine step interval based on movement type
            if (IsSprinting())
            {
                currentStepInterval = runStepInterval;
            }
            else if (IsCrouched())
            {
                currentStepInterval = crouchStepInterval;
            }
            else
            {
                currentStepInterval = walkStepInterval;
            }
            
            // Increment timer
            stepTimer += Time.deltaTime;
            
            // Play footstep sound at interval
            if (stepTimer >= currentStepInterval)
            {
                PlayFootstepSound();
                stepTimer = 0; // Reset timer
            }
        }
        else
        {
            // Reset timer when not moving
            stepTimer = 0;
        }
    }
    
    private bool IsMoving()
    {
        // Check if player is moving horizontally
        Vector2 horizontalVelocity = new Vector2(rb.velocity.x, rb.velocity.z);
        return horizontalVelocity.magnitude > minimumVelocity;
    }
    
    private bool IsGrounded()
    {
        // Access the fpsController's isGrounded field using reflection
        // since the field is private in FirstPersonController
        var field = typeof(FirstPersonController).GetField("isGrounded",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return (bool)field.GetValue(fpsController);
        }
        
        // Fallback: Cast a ray downward to check for ground
        return Physics.Raycast(transform.position, Vector3.down, 0.2f);
    }
    
    private bool IsSprinting()
    {
        // Access the fpsController's isSprinting field using reflection
        var field = typeof(FirstPersonController).GetField("isSprinting",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return (bool)field.GetValue(fpsController);
        }
        
        return false;
    }
    
    private bool IsCrouched()
    {
        // Access the fpsController's isCrouched field using reflection
        var field = typeof(FirstPersonController).GetField("isCrouched",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return (bool)field.GetValue(fpsController);
        }
        
        return false;
    }
    
    private void PlayFootstepSound()
    {
        // Use SoundManager if available
        SoundManager soundManager = SoundManager.instance;
        if (soundManager != null)
        {
            soundManager.PlayFootstep();
        }
        else
        {
            // Fallback to direct audio playback if sound manager not available
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                // Select appropriate footstep sound array
                AudioClip[] currentSoundArray = IsSprinting() && runFootstepSounds.Length > 0 
                    ? runFootstepSounds 
                    : footstepSounds;
                
                // If we have footstep sounds, play a random one
                if (currentSoundArray != null && currentSoundArray.Length > 0)
                {
                    AudioClip clip = currentSoundArray[Random.Range(0, currentSoundArray.Length)];
                    if (clip != null)
                    {
                        audioSource.pitch = Random.Range(0.8f, 1.2f);
                        audioSource.volume = Random.Range(0.8f, 1.0f);
                        audioSource.PlayOneShot(clip);
                    }
                }
            }
        }
    }
}
