namespace GameCreator.Dialogue
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(DatabaseDialogue.ConfigData))]
    public class ConfigDataPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty spDialogueUI = property.FindPropertyRelative("dialogueSkin");
            SerializedProperty spSkipKey = property.FindPropertyRelative("skipKey");
            SerializedProperty spRevisitChoice = property.FindPropertyRelative("revisitChoiceOpacity");

            SerializedProperty spTypewritterEffect = property.FindPropertyRelative("enableTypewriterEffect");
            SerializedProperty spTypewritterCPS = property.FindPropertyRelative("charactersPerSecond");

            EditorGUI.PropertyField(position, spDialogueUI);
            EditorGUILayout.PropertyField(spSkipKey);
            EditorGUILayout.PropertyField(spRevisitChoice);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Effects:", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(spTypewritterEffect);
            EditorGUI.indentLevel++;
            EditorGUI.BeginDisabledGroup(!spTypewritterEffect.boolValue);
            EditorGUILayout.PropertyField(spTypewritterCPS);
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();
        }
    }
}