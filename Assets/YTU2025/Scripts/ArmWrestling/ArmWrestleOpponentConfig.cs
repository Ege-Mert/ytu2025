using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Opponent", menuName = "Game/Arm Wrestle Opponent")]
public class ArmWrestleOpponentConfig : ScriptableObject
{
    [Header("Opponent Info")]
    public string opponentName = "Prisoner";
    
    [Header("Opponent Sprites")]
    public Sprite normalSprite;   // When stamina is in the middle range
    public Sprite nearLossSprite; // When near losing
    public Sprite lossSprite;     // When loses
    public Sprite nearWinSprite;  // When near winning
    public Sprite winSprite;      // When wins
    
    [Header("Sprite Transition Thresholds")]
    [Range(0f, 0.5f)]
    public float nearLossThreshold = 0.2f;  // Show near-loss sprite when stamina > (1-threshold)
    [Range(0f, 0.5f)]
    public float nearWinThreshold = 0.2f;   // Show near-win sprite when stamina < threshold
    
    [Header("Opponent State Sounds")]
    public string nearLossSound;  // Sound to play when near losing
    public string nearWinSound;   // Sound to play when near winning
    public string lossSound;      // Sound to play when loses
    public string winSound;       // Sound to play when wins
    
    [Header("Game Parameters")]
    [Tooltip("Height of the sweet spot zone")]
    public float sweetSpotSize = 0.2f;
    
    [Tooltip("How fast the sweet spot moves")]
    public float driftSpeed = 1.0f;
    
    [Tooltip("How fast opponent's stamina drains when player is in the sweet spot")]
    public float staminaDrainInside = 0.3f;
    
    [Tooltip("How fast opponent's stamina recovers when player is outside the sweet spot")]
    public float staminaRecoverOutside = 0.2f;
    
    [Tooltip("How fast player bar rises when holding input")]
    public float riseSpeed = 2.0f;
    
    [Tooltip("How fast player bar falls when not holding input")]
    public float fallSpeed = 1.5f;
    
    [Tooltip("Maximum time before auto-loss (set to 0 for no time limit)")]
    public float timeLimit = 0f;
    
    [Tooltip("Whether the sweet spot can occasionally twitch/jerk")]
    public bool enableTwitching = false;
    
    [Tooltip("How often the sweet spot can twitch (in seconds)")]
    public float twitchInterval = 3f;
    
    [Tooltip("How far the sweet spot can twitch")]
    public float twitchAmount = 0.1f;
    
    [Header("Visual Novel Dialog")]
    [TextArea(3, 5)]
    public string[] dialogLines;
    
    [Header("Visual Novel Character Sprites")]
    [Tooltip("Player's default sprite shown during dialogue")]
    public Sprite playerNormalSprite;  // Player's default sprite
    [Tooltip("Player's sprite shown after making a choice - set this in the inspector!")]
    public Sprite playerChoiceSprite;  // Player's sprite after making a choice
    
    [Header("Visual Novel Choice")]
    public string choiceA = "Yes";
    public string choiceB = "No";
    [Tooltip("How long to show the response before transitioning (seconds)")]
    public float responseDuration = 1f;
    
#if UNITY_EDITOR
    // This will run in the Unity Editor to validate required fields
    private void OnValidate()
    {
        // Check for missing choice sprite
        if (playerChoiceSprite == null)
        {
            Debug.LogWarning($"[{name}] Player choice sprite is not set! The player's sprite won't change when making a choice.", this);
        }
    }
#endif
}
