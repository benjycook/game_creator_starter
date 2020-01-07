namespace GameCreator.Dialogue
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using GameCreator.Core;
    using UnityEditor.IMGUI.Controls;

    [CustomEditor(typeof(DialogueItemText), true)]
    public class DialogueItemTextEditor : IDialogueItemEditor
    {
        private const string ICON_PATH = "Assets/Plugins/GameCreator/Dialogue/Icons/Dialogue/NodeText.png";
        private static readonly int ICON_HASH = ICON_PATH.GetHashCode();

        private static readonly string[] OPTIONS = new string[]{
            "Message",
            "Actions",
            "Conditions",
            "Settings"
        };

        // PROPERTIES: ----------------------------------------------------------------------------

        private int optionsIndex;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            if (target == null || serializedObject == null) return;
            this.OnEnableBase();
        }

        // GUI METHODS: ---------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null) return;
            serializedObject.Update();

            this.PaintOptions();
            switch (this.optionsIndex)
            {
                case 0: this.PaintOptionMessage(); break;
                case 1: this.PaintOptionActions(); break;
                case 2: this.PaintOptionConditions(); break;
                case 3: this.PaintOptionSettings(); break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void PaintOptions()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            this.optionsIndex = GUILayout.Toolbar(this.optionsIndex, OPTIONS);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        private void PaintOptionMessage()
        {
            EditorGUILayout.BeginVertical();

            this.PaintContentTab();
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

        private void PaintOptionSettings()
        {
            EditorGUILayout.BeginVertical();

            this.PaintConfigTab();

            EditorGUILayout.EndVertical();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public override string UpdateContent()
        {
            return this.spContentString.stringValue.Replace("\n", " ");
        }

        public override Texture2D UpdateIcon()
        {
            return DialogueItemTextEditor.GetIcon();
        }

        public static new Texture2D GetIcon()
        {
            return IDialogueItemEditor.GetOrLoadTexture(ICON_PATH, ICON_HASH);
        }

        public static bool CanAddElement(int selectionCount, Type selectionType)
        {
            bool acceptedType = (
                selectionType == typeof(DialogueItemRoot)   ||
                selectionType == typeof(DialogueItemText)   ||
                selectionType == typeof(DialogueItemChoice) ||
                selectionType == typeof(DialogueItemChoiceGroup)
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

                IDialogueItem itemInstance = dialogueEditor.CreateDialogueItem<DialogueItemText>();
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
                else if (instance != null && instance.GetType() == typeof(DialogueItemChoiceGroup))
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
                    Debug.LogError("Unknown type: " + instance.GetType());
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