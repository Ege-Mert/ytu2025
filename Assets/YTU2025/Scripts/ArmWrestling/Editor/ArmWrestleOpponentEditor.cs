using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(ArmWrestleOpponentConfig))]
public class ArmWrestleOpponentEditor : Editor
{
    private bool showGameParameters = true;
    private bool showVNParameters = true;
    
    public override void OnInspectorGUI()
    {
        ArmWrestleOpponentConfig config = (ArmWrestleOpponentConfig)target;
        
        // Opponent Info
        EditorGUILayout.LabelField("Opponent Information", EditorStyles.boldLabel);
        config.opponentName = EditorGUILayout.TextField("Name", config.opponentName);


        EditorGUILayout.LabelField("Opponent Sprites", EditorStyles.boldLabel);
        config.normalSprite = (Sprite)EditorGUILayout.ObjectField("Normal Sprite", config.normalSprite, typeof(Sprite), false);
        config.nearWinSprite = (Sprite)EditorGUILayout.ObjectField("Near Loss Sprite", config.nearWinSprite, typeof(Sprite), false);
        config.lossSprite = (Sprite)EditorGUILayout.ObjectField("Loss Sprite", config.lossSprite, typeof(Sprite), false);
        config.nearLossSprite = (Sprite)EditorGUILayout.ObjectField("Near Win Sprite", config.nearLossSprite, typeof(Sprite), false);
        config.winSprite = (Sprite)EditorGUILayout.ObjectField("Win Sprite", config.winSprite, typeof(Sprite), false);
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Sprite Transition Thresholds", EditorStyles.boldLabel);
        config.nearWinThreshold = EditorGUILayout.Slider("Near Win Threshold", config.nearWinThreshold, 0f, 0.5f);
        config.nearLossThreshold = EditorGUILayout.Slider("Near Loss Threshold", config.nearLossThreshold, 0f, 0.5f);
        
        EditorGUILayout.Space();
        
        // Game Parameters
        showGameParameters = EditorGUILayout.Foldout(showGameParameters, "Game Parameters", true);
        if (showGameParameters)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.LabelField("Sweet Spot Parameters", EditorStyles.boldLabel);
            config.sweetSpotSize = EditorGUILayout.Slider("Sweet Spot Size", config.sweetSpotSize, 0.05f, 0.5f);
            config.driftSpeed = EditorGUILayout.Slider("Drift Speed", config.driftSpeed, 0.2f, 3f);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Stamina Parameters", EditorStyles.boldLabel);
            config.staminaDrainInside = EditorGUILayout.Slider("Stamina Drain Rate", config.staminaDrainInside, 0.05f, 1f);
            config.staminaRecoverOutside = EditorGUILayout.Slider("Stamina Recovery Rate", config.staminaRecoverOutside, 0.05f, 1f);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Player Control Parameters", EditorStyles.boldLabel);
            config.riseSpeed = EditorGUILayout.Slider("Rise Speed", config.riseSpeed, 0.5f, 5f);
            config.fallSpeed = EditorGUILayout.Slider("Fall Speed", config.fallSpeed, 0.5f, 5f);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Challenge Parameters", EditorStyles.boldLabel);
            config.timeLimit = EditorGUILayout.FloatField("Time Limit (0 = none)", config.timeLimit);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Visual Effects", EditorStyles.boldLabel);
            config.enableTwitching = EditorGUILayout.Toggle("Enable Twitching", config.enableTwitching);
            
            if (config.enableTwitching)
            {
                EditorGUI.indentLevel++;
                config.twitchInterval = EditorGUILayout.Slider("Twitch Interval", config.twitchInterval, 1f, 10f);
                config.twitchAmount = EditorGUILayout.Slider("Twitch Amount", config.twitchAmount, 0.05f, 0.3f);
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // Visual Novel Parameters
        showVNParameters = EditorGUILayout.Foldout(showVNParameters, "Visual Novel Parameters", true);
        if (showVNParameters)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.LabelField("Dialogue Lines", EditorStyles.boldLabel);
            
            if (config.dialogLines == null || config.dialogLines.Length == 0)
            {
                config.dialogLines = new string[3]; // Default to 3 lines
            }
            
            EditorGUI.indentLevel++;
            
            // Allow adding/removing dialogue lines
            int newSize = Mathf.Max(1, EditorGUILayout.IntField("Number of Lines", config.dialogLines.Length));
            if (newSize != config.dialogLines.Length)
            {
                string[] newArray = new string[newSize];
                for (int i = 0; i < newSize && i < config.dialogLines.Length; i++)
                {
                    newArray[i] = config.dialogLines[i];
                }
                config.dialogLines = newArray;
            }
            
            for (int i = 0; i < config.dialogLines.Length; i++)
            {
                config.dialogLines[i] = EditorGUILayout.TextArea(config.dialogLines[i], GUILayout.MinHeight(40));
                EditorGUILayout.Space(5);
            }
            
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Choice Options", EditorStyles.boldLabel);
            config.choiceA = EditorGUILayout.TextField("Choice A", config.choiceA);
            config.choiceB = EditorGUILayout.TextField("Choice B", config.choiceB);
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // Test buttons section
        EditorGUILayout.LabelField("Difficulty Presets", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Easy"))
        {
            SetEasyDifficulty(config);
        }
        
        if (GUILayout.Button("Medium"))
        {
            SetMediumDifficulty(config);
        }
        
        if (GUILayout.Button("Hard"))
        {
            SetHardDifficulty(config);
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
    
    private void SetEasyDifficulty(ArmWrestleOpponentConfig config)
    {
        config.sweetSpotSize = 0.3f;
        config.driftSpeed = 0.5f;
        config.staminaDrainInside = 0.4f;
        config.staminaRecoverOutside = 0.15f;
        config.riseSpeed = 2.5f;
        config.fallSpeed = 1.2f;
        config.timeLimit = 0;
        config.enableTwitching = false;
    }
    
    private void SetMediumDifficulty(ArmWrestleOpponentConfig config)
    {
        config.sweetSpotSize = 0.2f;
        config.driftSpeed = 1.0f;
        config.staminaDrainInside = 0.3f;
        config.staminaRecoverOutside = 0.25f;
        config.riseSpeed = 2.0f;
        config.fallSpeed = 1.5f;
        config.timeLimit = 45;
        config.enableTwitching = true;
        config.twitchInterval = 5f;
        config.twitchAmount = 0.1f;
    }
    
    private void SetHardDifficulty(ArmWrestleOpponentConfig config)
    {
        config.sweetSpotSize = 0.1f;
        config.driftSpeed = 1.8f;
        config.staminaDrainInside = 0.2f;
        config.staminaRecoverOutside = 0.4f;
        config.riseSpeed = 1.8f;
        config.fallSpeed = 2.0f;
        config.timeLimit = 30;
        config.enableTwitching = true;
        config.twitchInterval = 3f;
        config.twitchAmount = 0.2f;
    }
}
#endif
