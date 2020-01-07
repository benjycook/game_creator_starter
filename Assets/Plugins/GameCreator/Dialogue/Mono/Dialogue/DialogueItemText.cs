namespace GameCreator.Dialogue
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Localization;

    [AddComponentMenu("")]
    public class DialogueItemText : IDialogueItem
    {
        private bool continueSubitems = true;

        // OVERRIDE METHODS: ----------------------------------------------------------------------

        protected override IEnumerator RunItem()
        {
            this.continueSubitems = true;
            if (this.conditionsList != null && !this.conditionsList.Check())
            {
                this.continueSubitems = false;
                yield break;
            }

            yield return this.RunShowText();

            if (this.voice != null)
            {
                AudioManager.Instance.StopVoice(this.voice);
            }

            DialogueUI.CompleteLine(this);
            DialogueUI.HideText();

            yield break;
        }

        public override IDialogueItem[] GetNextItem()
        {
            if (!this.continueSubitems) return null;
            if (this.children == null || this.children.Count == 0) return null;

            return this.children.ToArray();
        }

        public override bool CanHaveParent(IDialogueItem parent)
        {
            if (parent.GetType() == typeof(DialogueItemChoiceGroup)) return false;
            return base.CanHaveParent(parent);
        }
    }
}