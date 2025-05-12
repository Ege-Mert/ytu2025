using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(ArmWrestleOpponentConfig))]
public class ArmWrestleOpponentEditor : Editor
{
    private bool showBasicInfo = true;
    private bool showVisualNovelInfo = true;
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        ArmWrestleOpponentConfig config = (ArmWrestleOpponentConfig)target;

        // Basic Info Section
        showBasicInfo = EditorGUILayout.Foldout(showBasicInfo, "Game Settings", true);
        if (showBasicInfo)
        {
            // Draw the default inspector section by section
            DrawPropertiesExcluding(serializedObject, 
                "m_Script", 
                "dialogLines", 
                "playerNormalSprite", 
                "playerChoiceSprite",
                "choiceA", 
                "choiceB", 
                "responseDuration");
        }

        // Visual Novel Section
        EditorGUILayout.Space(10);
        showVisualNovelInfo = EditorGUILayout.Foldout(showVisualNovelInfo, "Visual Novel Settings", true);
        if (showVisualNovelInfo)
        {
            EditorGUILayout.LabelField("Visual Novel Dialog", EditorStyles.boldLabel);
            SerializedProperty dialogLinesProperty = serializedObject.FindProperty("dialogLines");
            EditorGUILayout.PropertyField(dialogLinesProperty, true);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Character Sprites", EditorStyles.boldLabel);
            
            // Show normal sprite field
            SerializedProperty normalSpriteProperty = serializedObject.FindProperty("playerNormalSprite");
            EditorGUILayout.PropertyField(normalSpriteProperty);
            
            // Show choice sprite with warning if not set
            SerializedProperty choiceSpriteProperty = serializedObject.FindProperty("playerChoiceSprite");
            EditorGUILayout.PropertyField(choiceSpriteProperty);
            
            if (config.playerChoiceSprite == null)
            {
                EditorGUILayout.HelpBox("Player Choice Sprite is not set! The player's sprite won't change when making a choice.", MessageType.Warning);
            }
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Choice Options", EditorStyles.boldLabel);
            
            SerializedProperty choiceAProperty = serializedObject.FindProperty("choiceA");
            SerializedProperty choiceBProperty = serializedObject.FindProperty("choiceB");
            SerializedProperty responseDurationProperty = serializedObject.FindProperty("responseDuration");
            
            EditorGUILayout.PropertyField(choiceAProperty);
            EditorGUILayout.PropertyField(choiceBProperty);
            EditorGUILayout.PropertyField(responseDurationProperty);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
