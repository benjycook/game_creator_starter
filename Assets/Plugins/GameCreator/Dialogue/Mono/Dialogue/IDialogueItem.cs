namespace GameCreator.Dialogue
{
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Localization;
    using GameCreator.Variables;

    public class IDialogueItem : GlobalID
    {
        protected const float TIME_SAFE_OFFSET = 0.1f;
        protected static readonly Regex REGEX_GLOBAL = new Regex(@"global\[[a-zA-Z0-9_-]+\]");

        public enum ExecuteBehaviour
        {
            Simultaneous,
            DialogueBeforeActions,
            ActionsBeforeDialogue
        }

        public enum AfterRunBehaviour
        {
            Continue,
            Exit,
            Jump
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public Dialogue dialogue;
        public IDialogueItem parent;
        public List<IDialogueItem> children;

        [LocStringBigText]
        public LocString content = new LocString("");
        public AudioClip voice;
        public bool autoPlay = false;
        public float autoPlayTime = 3.0f;

        public Actor actor;
        public int actorSpriteIndex = 0;
        public TargetGameObject actorTransform = new TargetGameObject();

        public AfterRunBehaviour afterRun = AfterRunBehaviour.Continue;
        public IDialogueItem jumpTo;

        public ExecuteBehaviour executeBehavior = ExecuteBehaviour.Simultaneous;
        public IActionsList actionsList;
        public IConditionsList conditionsList;

        public bool overrideDefaultConfig = false;
        public DatabaseDialogue.ConfigData config = new DatabaseDialogue.ConfigData();

        // VIRTUAL METHODS: -----------------------------------------------------------------------

        public virtual string GetContent()
        {
            StringBuilder text = new StringBuilder(this.content.GetText());
            bool matchSuccess = true;
            while (matchSuccess)
            {
                Match match = REGEX_GLOBAL.Match(text.ToString());
                if (matchSuccess = match.Success)
                {
                    int sIndex = match.Value.IndexOf('[');
                    int eIndex = match.Value.IndexOf(']');
                    string variable = match.Value.Substring(sIndex + 1, eIndex - sIndex - 1);

                    object result = VariablesManager.GetGlobal(variable);
                    text.Remove(match.Index, match.Length);
                    text.Insert(match.Index, result == null ? "" : result.ToString());
                }
            }

            return text.ToString();
        }

        public virtual IDialogueItem[] GetNextItem()
        {
            return null;
        }

        protected virtual IEnumerator RunItem()
        {
            yield break;
        }

        public virtual bool CanHaveParent(IDialogueItem parent)
        {
            return true;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public List<int> GetChildrenIDs()
        {
            List<int> listIDs = new List<int>();
            if (this.children == null) return listIDs;

            for (int i = 0; i < this.children.Count; ++i)
            {
                listIDs.Add(this.children[i].GetInstanceID());
            }

            return listIDs;
        }

        public IEnumerator Run()
        {
            string gid = this.GetID();
            this.dialogue.information.revisits[gid] = true;

            switch (this.executeBehavior)
            {
                case ExecuteBehaviour.Simultaneous:
                    if (this.actionsList != null) this.actionsList.Execute(gameObject, null);
                    yield return this.RunItem();
                    break;

                case ExecuteBehaviour.ActionsBeforeDialogue:
                    if (this.actionsList != null) yield return this.actionsList.ExecuteCoroutine(gameObject, null);
                    yield return this.RunItem();
                    break;

                case ExecuteBehaviour.DialogueBeforeActions:
                    yield return this.RunItem();
                    if (this.actionsList != null) yield return this.actionsList.ExecuteCoroutine(gameObject, null);
                    break;
            }
        }

        public virtual bool CheckConditions()
        {
            if (this.conditionsList == null) return true;
            return this.conditionsList.Check();
        }

        public bool IsRevisit()
        {
            string gid = this.GetID();
            return (
                this.dialogue.information.revisits.ContainsKey(gid) &&
                this.dialogue.information.revisits[gid] == true
            );
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected DatabaseDialogue.ConfigData GetConfigData()
        {
            DatabaseDialogue.ConfigData defaultConfig = DatabaseDialogue.Load().defaultConfig;
            DatabaseDialogue.ConfigData result = new DatabaseDialogue.ConfigData(defaultConfig);

            if (this.dialogue.overrideConfig)
            {
                if (this.dialogue.config.dialogueSkin != null)
                {
                    result.dialogueSkin = this.dialogue.config.dialogueSkin;
                }

                result.skipKey = this.dialogue.config.skipKey;
                result.revisitChoiceOpacity = this.dialogue.config.revisitChoiceOpacity;

                result.enableTypewriterEffect = this.dialogue.config.enableTypewriterEffect;
                result.charactersPerSecond = this.dialogue.config.charactersPerSecond;
            }

            if (this.overrideDefaultConfig)
            {
                if (this.config.dialogueSkin != null) result.dialogueSkin = this.config.dialogueSkin;

                result.skipKey = this.config.skipKey;
                result.revisitChoiceOpacity = this.config.revisitChoiceOpacity;

                result.enableTypewriterEffect = this.config.enableTypewriterEffect;
                result.charactersPerSecond = this.config.charactersPerSecond;
            }

            return result;
        }

        protected IEnumerator RunShowText()
        {
            DatabaseDialogue.ConfigData configData = this.GetConfigData();
            DialogueUI.StartLine(this, configData);

            if (this.voice != null) AudioManager.Instance.PlayVoice(this.voice, 0f);
            float textInitTime = Time.time;

            WaitForSeconds waitForSeconds = new WaitForSeconds(TIME_SAFE_OFFSET);
            yield return waitForSeconds;

            yield return new WaitUntil(() => {
                if (Input.GetKeyUp(configData.skipKey))
                {
                    if (configData.enableTypewriterEffect && DialogueUI.IsTypeWriting())
                    {
                        DialogueUI.CompleteTypeWriting();
                        return false;
                    }

                    return true;
                }

                bool timeout = Time.time - textInitTime > this.autoPlayTime;
                if (this.autoPlay && timeout) return true;

                return false;
            });
        }
    }
}
 