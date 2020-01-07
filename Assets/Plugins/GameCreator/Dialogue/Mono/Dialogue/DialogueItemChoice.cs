namespace GameCreator.Dialogue
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Localization;

    [AddComponentMenu("")]
    public class DialogueItemChoice : IDialogueItem
    {
        public enum FailChoiceCondition
        {
            HideChoice,
            DisableChoice
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public bool showOnce = false;

        [Tooltip("What to do if Conditions of Choice are not met")]
        public FailChoiceCondition onFailChoice = FailChoiceCondition.HideChoice;

        // OVERRIDE METHODS: ----------------------------------------------------------------------

        public override IDialogueItem[] GetNextItem()
        {
            if (this.children == null || this.children.Count == 0) return null;
            return this.children.ToArray();
        }

        public override bool CanHaveParent(IDialogueItem parent)
        {
            if (parent.GetType() == typeof(DialogueItemChoiceGroup)) return true;
            return false;
        }

        /*
        public override bool CheckConditions()
        {
            if (this.showOnce && this.IsRevisit()) return false;
            return base.CheckConditions();
        }*/
    }
}
