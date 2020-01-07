namespace GameCreator.Dialogue
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;

    [CustomEditor(typeof(DialogueItemRoot), true)]
    public class DialogueItemRootEditor : IDialogueItemEditor
    {
        private void OnEnable()
        {
            if (target == null || serializedObject == null) return;
            this.OnEnableBase();
        }

        public override string UpdateContent()
        {
            return "Dialogue Root";
        }
    }
}