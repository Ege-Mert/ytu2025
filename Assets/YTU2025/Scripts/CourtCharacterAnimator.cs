using UnityEngine;
using DG.Tweening;

public class CourtCharacterAnimator : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private float bounceAmplitude = 0.2f;
    [SerializeField] private float bounceInterval = 0.5f;
    
    [Header("Sprite Settings")]
    [SerializeField] private SpriteRenderer characterSprite;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite shockedSprite;
    
    private Tween bounceTween;
    private Vector3 startPosition;
    
    private void Start()
    {
        // Store initial position
        startPosition = transform.position;
        
        // Ensure we have a sprite renderer reference
        if (characterSprite == null)
        {
            characterSprite = GetComponent<SpriteRenderer>();
        }
        
        // Set initial sprite
        if (characterSprite != null && normalSprite != null)
        {
            characterSprite.sprite = normalSprite;
        }
    }
    
    public void StartBouncing()
    {
        // Cancel any existing bounce
        StopBouncing();
        
        // Create the bounce tween
        bounceTween = transform.DOMoveY(
            startPosition.y + bounceAmplitude, 
            bounceInterval / 2)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetId("CourtBounce");
    }
    
    public void StopBouncing()
    {
        if (bounceTween != null && bounceTween.IsActive())
        {
            bounceTween.Kill();
        }
        
        // Reset position
        transform.position = startPosition;
    }
    
    public void ShowShockedExpression()
    {
        if (characterSprite != null && shockedSprite != null)
        {
            characterSprite.sprite = shockedSprite;
        }
    }
    
    public void ShowNormalExpression()
    {
        if (characterSprite != null && normalSprite != null)
        {
            characterSprite.sprite = normalSprite;
        }
    }
    
    private void OnDestroy()
    {
        // Clean up tweens
        if (bounceTween != null && bounceTween.IsActive())
        {
            bounceTween.Kill();
        }
    }
}
