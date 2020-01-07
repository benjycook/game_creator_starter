namespace GameCreator.Dialogue
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;

    [AddComponentMenu("")]
    public class DialogueItemRoot : IDialogueItem
    {
        public override string GetContent()
        {
            return "root";
        }

        public override IDialogueItem[] GetNextItem()
        {
            if (this.children == null || this.children.Count == 0) return null;
            return this.children.ToArray();
        }
    }
}