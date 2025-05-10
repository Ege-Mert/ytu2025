using UnityEngine;

[CreateAssetMenu(fileName = "New Court Round", menuName = "Game/Court Round Config")]
public class CourtRoundConfig : ScriptableObject
{
    public string[] words; // words to type in order
    public float timePerWord; // seconds allowed per word

    // If this is the final round (for the twist)
    public bool isFinalRound;
    
    // For the final round twist, the actual word that will be displayed instead
    // of what the player actually types (only used for the last word of the final round)
    public string overrideLastWord = "yaptım";
}
