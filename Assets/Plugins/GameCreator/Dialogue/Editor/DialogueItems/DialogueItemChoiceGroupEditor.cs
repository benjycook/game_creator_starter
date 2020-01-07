namespace GameCreator.Dialogue
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;

    [CustomEditor(typeof(DialogueItemChoiceGroup), true)]
    public class DialogueItemChoiceGroupEditor : IDialogueItemEditor
    {
        private const string ICON_PATH = "Assets/Plugins/GameCreator/Dialogue/Icons/Dialogue/NodeChoiceGroup.png";
        private static readonly int ICON_HASH = ICON_PATH.GetHashCode();

        private const string PROP_SHUFFLE_CHOICES = "shuffleChoices";
        private const string PROP_TIMED_CHOICE = "timedChoice";
        private const string PROP_TIMEOUT = "timeout";
        private const string PROP_TIMEOUT_BEHAVIOR = "timeoutBehavior";

        private static readonly string[] OPTIONS = new string[]{
            "Message",
            "Choices",
            "Actions",
            "Conditions",
            "Settings"
        };

        // PROPERTIES: ----------------------------------------------------------------------------

        private int optionsIndex = 0;

        private SerializedProperty spShuffleChoices;
        private SerializedProperty spTimedChoice;
        private SerializedProperty spTimeout;
        private SerializedProperty spTimeoutBehavior;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            if (target == null || serializedObject == null) return;
            this.OnEnableBase();

            this.spShuffleChoices = this.serializedObject.FindProperty(PROP_SHUFFLE_CHOICES);
            this.spTimedChoice = this.serializedObject.FindProperty(PROP_TIMED_CHOICE);
            this.spTimeout = this.serializedObject.FindProperty(PROP_TIMEOUT);
            this.spTimeoutBehavior = this.serializedObject.FindProperty(PROP_TIMEOUT_BEHAVIOR);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string UpdateContent()
        {
            return this.spContentString.stringValue.Replace("\n", " ");
        }

        public override Texture2D UpdateIcon()
        {
            return DialogueItemChoiceGroupEditor.GetIcon();
        }

        public static new Texture2D GetIcon()
        {
            return IDialogueItemEditor.GetOrLoadTexture(ICON_PATH, ICON_HASH);
        }

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null) return;
            serializedObject.Update();

            this.PaintOptions();
            switch (this.optionsIndex)
            {
                case 0: this.PaintOptionsMessage(); break;
                case 1: this.PaintOptionChoices(); break;
                case 2: this.PaintOptionActions(); break;
                case 3: this.PaintOptionsConditions(); break;
                case 4: this.PaintOptionsSettings(); break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void PaintOptions()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            this.optionsIndex = GUILayout.Toolbar(this.optionsIndex, OPTIONS);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private void PaintOptionsMessage()
        {
            EditorGUILayout.BeginVertical();

            this.PaintContentTab();

            EditorGUILayout.EndVertical();
        }

        private void PaintOptionChoices()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.PropertyField(this.spTimedChoice);
            EditorGUI.BeginDisabledGroup(!this.spTimedChoice.boolValue);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(this.spTimeout);
            EditorGUILayout.PropertyField(this.spTimeoutBehavior);
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spShuffleChoices);

            EditorGUILayout.EndVertical();
        }

        private void PaintOptionActions()
        {
            EditorGUILayout.BeginVertical();

            this.PaintActionsTab();

            EditorGUILayout.EndVertical();
        }

        private void PaintOptionsConditions()
        {
            EditorGUILayout.BeginVertical();

            this.PaintConditionsTab();

            EditorGUILayout.EndVertical();
        }

        private void PaintOptionsSettings()
        {
            EditorGUILayout.BeginVertical();

            this.PaintConfigTab();

            EditorGUILayout.EndVertical();
        }

        // DIALOGUE EDITOR METHODS: ---------------------------------------------------------------

        public static bool CanAddElement(int selectionCount, Type selectionType)
        {
            bool acceptedType = (
                selectionType == typeof(DialogueItemRoot) ||
                selectionType == typeof(DialogueItemText) ||
                selectionType == typeof(DialogueItemChoice)
            );

            return selectionCount == 0 || (selectionCount == 1 && acceptedType);
        }

        public static void AddElement(DialogueEditor dialogueEditor)
        {
            List<int> selections = new List<int> { dialogueEditor.editorRoot.target.GetInstanceID() };
            List<int> nextSelections = new List<int>();
            if (dialogueEditor.dialogueTree.HasSelection())
            {
                selections = new List<int>(dialogueEditor.dialogueTree.GetSelection());
            }

            for (int i = 0; i < selections.Count; ++i)
            {
                int selectionID = selections[i];
                UnityEngine.Object instance = dialogueEditor.InstanceIDToObject(selectionID);

                IDialogueItem itemInstance = dialogueEditor.CreateDialogueItem<DialogueItemChoiceGroup>();
                nextSelections.Add(itemInstance.GetInstanceID());

                if (selectionID == dialogueEditor.editorRoot.target.GetInstanceID())
                {
                    dialogueEditor.editorRoot.AddChild(
                        itemInstance, 
                        (IDialogueItem)instance,
                        dialogueEditor.targetDialogue
                    );
                }
                else if (instance != null && instance.GetType() == typeof(DialogueItemText))
                {
                    dialogueEditor.itemsEditors[selectionID].AddSibling(
                        itemInstance, 
                        (IDialogueItem)instance, 
                        dialogueEditor.targetDialogue,
                        selectionID
                    );
                }
                else if (instance != null && instance.GetType() == typeof(DialogueItemChoice))
                {
                    dialogueEditor.itemsEditors[selectionID].AddChild(
                        itemInstance,
                        (IDialogueItem)instance,
                        dialogueEditor.targetDialogue
                    );
                }
                else
                {
                    Debug.LogError("Forbidden or Unknown type: " + instance.GetType());
                }

                dialogueEditor.itemsEditors.Add(
                    itemInstance.GetInstanceID(),
                    IDialogueItemEditor.CreateEditor(itemInstance)
                );

                dialogueEditor.dialogueTree.Reload();

                dialogueEditor.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                dialogueEditor.serializedObject.Update();
            }

            dialogueEditor.dialogueTree.SetFocusAndEnsureSelectedItem();
            dialogueEditor.dialogueTree.SetSelection(nextSelections, TreeViewSelectionOptions.RevealAndFrame);
        }
    }
}