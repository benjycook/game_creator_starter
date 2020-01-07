namespace GameCreator.Dialogue
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;

    [CustomEditor(typeof(DialogueItemChoice), true)]
    public class DialogueItemChoiceEditor : IDialogueItemEditor
    {
        private const string ICON_PATH = "Assets/Plugins/GameCreator/Dialogue/Icons/Dialogue/NodeChoice.png";
        private static readonly int ICON_HASH = ICON_PATH.GetHashCode();

        private static readonly string[] OPTIONS = new string[]{
            "Answer",
            "Actions",
            "Conditions"
        };

        // PROPERTIES: ----------------------------------------------------------------------------

        private int optionsIndex = 0;

        private SerializedProperty spShowOnce;
        private SerializedProperty spOnFailChoice;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            if (target == null || serializedObject == null) return;
            this.OnEnableBase();
            this.spShowOnce = serializedObject.FindProperty("showOnce");
            this.spOnFailChoice = serializedObject.FindProperty("onFailChoice");
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string UpdateContent()
        {
            return this.spContentString.stringValue.Replace("\n", " ");
        }

        public override Texture2D UpdateIcon()
        {
            return DialogueItemChoiceEditor.GetIcon();
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
                case 0: this.PaintOptionAnswer(); break;
                case 1: this.PaintOptionActions(); break;
                case 2: this.PaintOptionConditions(); break;
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

        private void PaintOptionAnswer()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.PropertyField(this.spContent);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(this.spShowOnce);
            EditorGUILayout.PropertyField(this.spOnFailChoice);

            EditorGUILayout.Space();
            this.PaintAfterRunTab();

            EditorGUILayout.EndVertical();
        }

        private void PaintOptionActions()
        {
            EditorGUILayout.BeginVertical();

            this.PaintActionsTab();

            EditorGUILayout.EndVertical();
        }

        private void PaintOptionConditions()
        {
            EditorGUILayout.BeginVertical();

            this.PaintConditionsTab();

            EditorGUILayout.EndVertical();
        }

        // DIALOGUE EDITOR METHODS: ---------------------------------------------------------------

        public static bool CanAddElement(int selectionCount, Type selectionType)
        {
            bool acceptedType = (
                selectionType == typeof(DialogueItemChoiceGroup) ||
                selectionType == typeof(DialogueItemChoice)
            );

            return selectionCount == 1 && acceptedType;
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
                dialogueEditor.dialogueTree.SetExpandedRecursive(selectionID, true);

                IDialogueItem itemInstance = dialogueEditor.CreateDialogueItem<DialogueItemChoice>();
                nextSelections.Add(itemInstance.GetInstanceID());

                if (instance != null && instance.GetType() == typeof(DialogueItemChoiceGroup))
                {
                    dialogueEditor.itemsEditors[selectionID].AddChild(
                        itemInstance, 
                        (IDialogueItem)instance,
                        dialogueEditor.targetDialogue
                    );
                }
                else if (instance != null && instance.GetType() == typeof(DialogueItemChoice))
                {
                    dialogueEditor.itemsEditors[selectionID].AddSibling(
                        itemInstance,
                        (IDialogueItem)instance, 
                        dialogueEditor.targetDialogue,
                        selectionID
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