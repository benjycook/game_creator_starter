namespace GameCreator.Dialogue
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using GameCreator.Core;

    [AddComponentMenu("")]
    public class DialogueUI : MonoBehaviour
    {
        private static DialogueUI Instance = null;
        public static DialogueLog LOG = new DialogueLog();

        private static int PREFAB_INSTANCE_ID = 0;
        private static readonly int ANIM_TIMER = Animator.StringToHash("Start");

        private const string ERR_NO_DIALOGUE_UI = "No Dialogue UI prefab specified";
        private const string ERR_NO_BUTTON = "No Button component found in Choice object";

        // PROPERTIES: ----------------------------------------------------------------------------

        [Header("Prefabs")]
        public GameObject prefabChoice;

        [Header("Wrappers")]
        public GameObject wrapGraphic;
        public GameObject wrapMessage;
        public GameObject wrapChoices;
        public GameObject wrapTimer;
        public GameObject wrapLogs;

        [Header("Messages")]
        public Text textTitle;
        public Text textMessage;

        [Header("Choices")]
        public RectTransform choiceContainer;

        [Header("Timer")]
        public Animator animatorTimer;

        [Header("Actors")]
        public Graphic actorColor;
        public Image actorImage;
        public RawImage actorRawImage;
        public RectTransform actorPrefabContainer;

        private string currentMessage = "";

        private GibberishEffect gibberish;
        private TypeWriterEffect typewriter;
        private bool isTypewriting = false;

        private bool typewriterEnabled = false;
        private float typewriterCharsPerSec = 1.0f;
        private float typewriterStartTime = 0.0f;

        // INITIALIZE METHODS: --------------------------------------------------------------------

        private void Awake()
        {
            DialogueUI.Instance = this;
            EventSystemManager.Instance.Wakeup();

            if (this.wrapLogs != null) this.wrapLogs.SetActive(false);
            if (this.wrapGraphic != null) this.wrapGraphic.SetActive(false);
            if (this.wrapMessage != null) this.wrapMessage.SetActive(false);
            if (this.wrapChoices != null) this.wrapChoices.SetActive(false);
            if (this.wrapTimer != null) this.wrapTimer.SetActive(false);
        }

        // UPDATE METHODS: ------------------------------------------------------------------------

        private void Update()
        {
            this.UpdateTypewriterEffect();
        }

        protected void UpdateTypewriterEffect()
        {
            this.isTypewriting = false;

            if (!this.wrapMessage.activeInHierarchy) return;
            if (string.IsNullOrEmpty(this.currentMessage)) return;
            if (!this.typewriterEnabled)
            {
                this.textMessage.text = this.currentMessage;
                return;
            }

            float elapsedTime = Time.time - this.typewriterStartTime;
            int messageLength = Mathf.Min(
                Mathf.FloorToInt(elapsedTime * this.typewriterCharsPerSec),
                this.currentMessage.Length
            );

            string message = this.typewriter.GetText(messageLength);
            this.textMessage.text = message;

            if (this.gibberish != null)
            {
                this.gibberish.Gibber(messageLength);
            }

            if (messageLength < this.typewriter.CountVisibleCharacters()) this.isTypewriting = true;
            else this.isTypewriting = false;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static void BeginDialogue()
        {
            LOG.Reset();
        }

        public static void EndDialogue()
        {

        }

        public static void StartLine(IDialogueItem item, DatabaseDialogue.ConfigData config)
        {
            DialogueUI.RequireInstance(config);
            DialogueUI dialogue = DialogueUI.Instance;

            dialogue.currentMessage = item.GetContent();
            dialogue.typewriterEnabled = config.enableTypewriterEffect;
            dialogue.typewriterCharsPerSec = config.charactersPerSecond;
            dialogue.typewriterStartTime = Time.time;
            dialogue.gibberish = null;

            string msg = (config.enableTypewriterEffect ? "" : dialogue.currentMessage);
            dialogue.textMessage.text = msg;

            if (dialogue.wrapLogs != null) dialogue.wrapLogs.SetActive(true);
            if (dialogue.wrapGraphic != null) dialogue.wrapGraphic.SetActive(true);
            if (dialogue.wrapMessage != null) dialogue.wrapMessage.SetActive(true);

            if (dialogue.textTitle != null) dialogue.textTitle.gameObject.SetActive(false);
            if (dialogue.actorColor != null) dialogue.actorColor.gameObject.SetActive(false);
            if (dialogue.actorImage != null) dialogue.actorImage.gameObject.SetActive(false);
            if (dialogue.actorRawImage != null) dialogue.actorRawImage.gameObject.SetActive(false);
            if (dialogue.actorPrefabContainer != null) dialogue.actorPrefabContainer.gameObject.SetActive(false);

            if (item.actor != null)
            {
                if (dialogue.textTitle != null)
                {
                    dialogue.textTitle.gameObject.SetActive(true);
                    dialogue.textTitle.text = item.actor.GetName();
                }

                if (dialogue.actorColor != null)
                {
                    dialogue.actorColor.gameObject.SetActive(true);
                    dialogue.actorColor.color = item.actor.color;
                }

                if (item.actorSpriteIndex < item.actor.actorSprites.data.Length)
                {
                    ActorSprites.Data spriteData = item.actor.actorSprites.data[item.actorSpriteIndex];
                    switch (spriteData.type)
                    {
                        case ActorSprites.DataType.Sprite:
                            if (dialogue.actorImage != null)
                            {
                                dialogue.actorImage.gameObject.SetActive(true);
                                dialogue.actorImage.sprite = spriteData.sprite;
                            }
                            break;

                        case ActorSprites.DataType.Texture:
                            if (dialogue.actorRawImage != null)
                            {
                                dialogue.actorRawImage.gameObject.SetActive(true);
                                dialogue.actorRawImage.texture = spriteData.texture;
                            }
                            break;

                        case ActorSprites.DataType.Prefab:
                            if (dialogue.actorPrefabContainer != null)
                            {
                                dialogue.actorPrefabContainer.gameObject.SetActive(true);
                                for (int i = dialogue.actorPrefabContainer.childCount - 1; i >= 0; --i)
                                {
                                    Destroy(dialogue.actorPrefabContainer.GetChild(i).gameObject);
                                }

                                GameObject instance = Instantiate(
                                    spriteData.prefab,
                                    dialogue.actorPrefabContainer
                                );
                                instance.transform.localScale = Vector3.one;
                            }
                            break;
                    }
                }

                if (item.actor.useGibberish)
                {
                    dialogue.gibberish = new GibberishEffect(
                        item.actor.gibberishAudio,
                        item.actor.gibberishPitch,
                        item.actor.gibberishVariation
                    );
                }
            }

            if (config.enableTypewriterEffect)
            {
                DialogueUI.Instance.typewriter = new TypeWriterEffect(
                    DialogueUI.Instance.currentMessage
                );
            }
        }

        public static bool StartChoices(DialogueItemChoiceGroup item, DatabaseDialogue.ConfigData config)
        {
            DialogueUI.RequireInstance(config);
            DialogueUI.Instance.wrapChoices.SetActive(true);

            bool choicesAvailable = DialogueUI.Instance.SetupChoices(item, config);
            if (choicesAvailable && item.timedChoice)
            {
                DialogueUI.Instance.StartTimer(item.timeout.GetValue(item.gameObject));
            }

            return choicesAvailable;
        }

        public static void CompleteLine(IDialogueItem item)
        {
            LOG.Add(item, false);
        }

        public static void CompleteChoice(IDialogueItem item)
        {
            LOG.Add(item, true);
        }

        public static void HideText()
        {
            if (Instance.wrapMessage != null) Instance.wrapMessage.SetActive(false);
            if (Instance.wrapGraphic != null) Instance.wrapGraphic.SetActive(false);
            if (Instance.wrapLogs != null) Instance.wrapLogs.SetActive(false);
        }

        public static void HideChoices()
        {
            if (Instance.wrapChoices != null) Instance.wrapChoices.SetActive(false);
            if (Instance.wrapTimer != null) Instance.wrapTimer.SetActive(false);
        }

        public static bool IsTypeWriting()
        {
            return DialogueUI.Instance.isTypewriting;
        }

        public static void CompleteTypeWriting()
        {
            DialogueUI.Instance.typewriterEnabled = false;
        }

        public void SkipLine()
        {

        }

        // PROTECTED STATIC METHODS: ----------------------------------------------------------------

        protected static void RequireInstance(DatabaseDialogue.ConfigData config)
        {
            if (config.dialogueSkin == null) Debug.LogError(ERR_NO_DIALOGUE_UI);
            if (DialogueUI.Instance != null)
            {
                if (config.dialogueSkin.GetInstanceID() == PREFAB_INSTANCE_ID) return;
                Destroy(DialogueUI.Instance.gameObject);
            }

            PREFAB_INSTANCE_ID = config.dialogueSkin.GetInstanceID();
            Instantiate(config.dialogueSkin, Vector3.zero, Quaternion.identity);
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected void StartTimer(float timeout)
        {
            this.wrapTimer.SetActive(true);
            this.animatorTimer.SetTrigger(ANIM_TIMER);
            this.animatorTimer.speed = 1f / timeout;
        }

        protected bool SetupChoices(DialogueItemChoiceGroup item, DatabaseDialogue.ConfigData config)
        {
            this.CleanChoices();
            int choicesSetup = 0;

            for (int i = 0; i < item.children.Count; ++i)
            {
                DialogueItemChoice choice = item.children[i] as DialogueItemChoice;
                if (!choice || (choice.showOnce && choice.IsRevisit())) continue;

                bool conditions = choice.CheckConditions();
                if (!conditions && choice.onFailChoice == DialogueItemChoice.FailChoiceCondition.HideChoice) continue;

                GameObject choiceInstance = Instantiate<GameObject>(this.prefabChoice);
                choiceInstance.GetComponent<RectTransform>().SetParent(this.choiceContainer, false);

                DialogueChoiceUI choiceUI = choiceInstance.GetComponent<DialogueChoiceUI>();
                bool disabled = (
                    !conditions &&
                    choice.onFailChoice == DialogueItemChoice.FailChoiceCondition.DisableChoice
                );

                if (choiceUI != null) choiceUI.Setup(config, item, i, disabled);
                choicesSetup++;
            }

            if (item.shuffleChoices) this.ShuffleChoices();
            if (this.choiceContainer.childCount > 0)
            {
                Transform selection = this.choiceContainer.GetChild(0);
                EventSystem.current.SetSelectedGameObject(selection.gameObject);
            }

            return choicesSetup != 0;
        }

        protected void CleanChoices()
        {
            for (int i = this.choiceContainer.childCount - 1; i >= 0; --i)
            {
                Destroy(this.choiceContainer.GetChild(i).gameObject);
            }
        }

        protected void ShuffleChoices()
        {
            int childCount = this.choiceContainer.childCount;
            for (int i = 0; i < childCount; ++i)
            {
                int randomIndex = Random.Range(0, childCount);
                this.choiceContainer.GetChild(i).SetSiblingIndex(randomIndex);
            }
        }
    }
}