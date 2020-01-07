namespace GameCreator.Dialogue
{
    using System;
    using System.IO;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.AnimatedValues;
    using UnityEditor.SceneManagement;
    using UnityEditorInternal;
    using System.Linq;
    using System.Reflection;
    using GameCreator.Core;

    [CustomEditor(typeof(DatabaseDialogue))]
    public class DatabaseDialogueEditor : IDatabaseEditor
    {
        private const string DEFAULT_SKIN_PATH = "Assets/Plugins/GameCreator/Dialogue/Resources/GameCreator/DefaultDialogueSkin.prefab";
        private const string TITLE_CONFIG = "Default Data";

        private const string PROP_CONFIG = "defaultConfig";
        private const string PROP_CONFIG_SKIN = "dialogueSkin";

        // PROPERTIES: ----------------------------------------------------------------------------

        private bool stylesInitialized = false;
        private SerializedProperty spConfig;

        // INITIALIZE: ----------------------------------------------------------------------------

        private void OnEnable()
        {
            if (target == null || serializedObject == null) return;
            this.spConfig = serializedObject.FindProperty(PROP_CONFIG);

            SerializedProperty spDataSkin = this.spConfig.FindPropertyRelative(PROP_CONFIG_SKIN);
            if (spDataSkin.objectReferenceValue == null)
            {
                GameObject skin = AssetDatabase.LoadAssetAtPath<GameObject>(DEFAULT_SKIN_PATH);
                spDataSkin.objectReferenceValue = skin;

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.Update();
            }
        }

        // OVERRIDE METHODS: ----------------------------------------------------------------------

        public override string GetDocumentationURL()
        {
            return "https://docs.gamecreator.io/manual/dialogue.html";
        }

        public override string GetName()
        {
            return "Dialogue";
        }

        public override bool CanBeDecoupled()
        {
            return true;
        }

        // GUI METHODS: ---------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null) return;

            if (!this.stylesInitialized)
            {
                this.InitializeStyles();
                this.stylesInitialized = true;
            }

            this.serializedObject.Update();

            this.PaintSettings();

            this.serializedObject.ApplyModifiedProperties();
        }

        // PAINT SETTINGS: ------------------------------------------------------------------------

        private void PaintSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField(TITLE_CONFIG, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(this.spConfig);

            EditorGUILayout.EndVertical();
        }

        private void InitializeStyles()
        { }
    }
}