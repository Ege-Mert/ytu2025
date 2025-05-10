using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR
public class CourtRoundsSetup : MonoBehaviour
{
    [MenuItem("Game/Create Court Rounds")]
    public static void CreateCourtRounds()
    {
        // Ensure directory exists
        Directory.CreateDirectory("Assets/YTU2025/ScriptableObjects");
        
        // Create Round 1
        var round1 = ScriptableObject.CreateInstance<CourtRoundConfig>();
        round1.words = new string[] { "ben", "yapmadım" };
        round1.timePerWord = 8f;
        round1.isFinalRound = false;
        AssetDatabase.CreateAsset(round1, "Assets/YTU2025/ScriptableObjects/CourtRound1.asset");
        
        // Create Round 2
        var round2 = ScriptableObject.CreateInstance<CourtRoundConfig>();
        round2.words = new string[] { "gerçekten", "ben", "yapmadım" };
        round2.timePerWord = 7f;
        round2.isFinalRound = false;
        AssetDatabase.CreateAsset(round2, "Assets/YTU2025/ScriptableObjects/CourtRound2.asset");
        
        // Create Round 3
        var round3 = ScriptableObject.CreateInstance<CourtRoundConfig>();
        round3.words = new string[] { "yemin", "ederim", "ben", "yapmadım" };
        round3.timePerWord = 5f;
        round3.isFinalRound = false;
        AssetDatabase.CreateAsset(round3, "Assets/YTU2025/ScriptableObjects/CourtRound3.asset");
        
        // Create Round 4
        var round4 = ScriptableObject.CreateInstance<CourtRoundConfig>();
        round4.words = new string[] { "lütfen", "inanın", "ben", "yapmadım" };
        round4.timePerWord = 4f;
        round4.isFinalRound = false;
        AssetDatabase.CreateAsset(round4, "Assets/YTU2025/ScriptableObjects/CourtRound4.asset");
        
        // Create Round 5 (Final with twist)
        var round5 = ScriptableObject.CreateInstance<CourtRoundConfig>();
        round5.words = new string[] { "Hakim", "bey", "cinayeti", "ben", "bilerek", "yapmadım" };
        round5.timePerWord = 3f;
        round5.isFinalRound = true;
        round5.overrideLastWord = "yaptım"; // The twist for the last word
        AssetDatabase.CreateAsset(round5, "Assets/YTU2025/ScriptableObjects/CourtRound5.asset");
        
        // Save all created assets
        AssetDatabase.SaveAssets();
        
        Debug.Log("Court Rounds created successfully!");
    }
}
#endif
