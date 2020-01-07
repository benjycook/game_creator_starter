namespace GameCreator.Dialogue
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEditor.IMGUI.Controls;
    using GameCreator.Core;

    [CustomEditor(typeof(IDialogueItem), true)]
    public class IDialogueItemEditor : Editor
    {
        private const string HEADER_EVENTS = "Events";
        private const string HEADER_ACTIONS = "Actions";

        private const string PROP_DIALOGUE = "dialogue";
        private const string PROP_PARENT = "parent";
        private const string PROP_CHILDREN = "children";
        private const string PROP_CONFIG = "config";
        private const string PROP_OVERR_CONFIG = "overrideDefaultConfig";
        private const string PROP_EXECUTE_BEHAVIOR = "executeBehavior";
        private const string PROP_ACTIONS = "actionsList";
        private const string PROP_CONDITIONS = "conditionsList";

        private const string PROP_CONTENT = "content";
        private const string PROP_CONTENT_STR = "content";
        private const string PROP_SCENE_ACTIONS = "sceneActions";
        private const string PROP_VOICE = "voice";
        private const string PROP_AUTOPLAY = "autoPlay";
        private const string PROP_AUTOPLAY_TIME = "autoPlayTime";

        private const string PROP_ACTOR = "actor";
        private const string PROP_ACTOR_SPRITE = "actorSpriteIndex";
        private const string PROP_ACTOR_TRANSFORM = "actorTransform";

        private const string PROP_AFTERRUN = "afterRun";
        private const string PROP_AFTERRUN_JUMPTO = "jumpTo";

        private const string ICON_DEFAULT_PATH = "Assets/Plugins/GameCreator/Dialogue/Icons/Dialogue/NodeText.png";
        private static readonly int ICON_DEFAULT_HASH = ICON_DEFAULT_PATH.GetHashCode();
        private const int JUMP_MAX_TEXT = 30;

        private static readonly GUIContent GC_TRANSFORM = new GUIContent("Transform");

        private static Dictionary<int, Texture2D> ICON_TEXTURES = new Dictionary<int, Texture2D>();
        private static readonly List<System.Type> ALLOWED_JUMP_TO_TYPES = new List<System.Type>()
        {
            typeof(DialogueItemText),
            typeof(DialogueItemChoiceGroup)
        };

        // PROPERTIES: ----------------------------------------------------------------------------

        public IDialogueItem targetItem;

        public SerializedProperty spDialogue;
        public SerializedProperty spParent;
        public SerializedProperty spChildren;

        public SerializedProperty spContent;
        public SerializedProperty spContentString;
        public SerializedProperty spVoice;
        public SerializedProperty spAutoPlay;
        public SerializedProperty spAutoPlayTime;

        public SerializedProperty spActor;
        public SerializedProperty spActorSpriteIndex;
        public SerializedProperty spActorTransform;

        public SerializedProperty spOverrideConfig;
        public SerializedProperty spConfig;

        public SerializedProperty spAfterRun;
        public SerializedProperty spAfterRunJumpTo;

        public SerializedProperty spExecuteBehavior;
        public SerializedProperty spActionsList;
        public SerializedProperty spConditionList;

        public IActionsListEditor actionsListEditor;
        public IConditionsListEditor conditionListEditor;

        private int jumpOptionsIndex = -1;
        private string[] jumpOptions = new string[0];

        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            if (target == null || serializedObject == null) return;
            this.OnEnableBase();
        }

        protected void OnEnableBase()
        {
            this.targetItem = (IDialogueItem)target;

            this.spDialogue = this.serializedObject.FindProperty(PROP_DIALOGUE);
            this.spParent = this.serializedObject.FindProperty(PROP_PARENT);
            this.spChildren = serializedObject.FindProperty(PROP_CHILDREN);

            this.spConfig = this.serializedObject.FindProperty(PROP_CONFIG);
            this.spOverrideConfig = this.serializedObject.FindProperty(PROP_OVERR_CONFIG);

            this.spExecuteBehavior = serializedObject.FindProperty(PROP_EXECUTE_BEHAVIOR);

            this.spActionsList = serializedObject.FindProperty(PROP_ACTIONS);
            if (this.spActionsList.objectReferenceValue == null)
            {
                IActionsList actionsList = this.targetItem.gameObject.AddComponent<IActionsList>();
                actionsList.hideFlags = HideFlags.HideInInspector;

                this.spActionsList.objectReferenceValue = actionsList;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.Update();
            }

            this.spConditionList = serializedObject.FindProperty(PROP_CONDITIONS);
            if (this.spConditionList.objectReferenceValue == null)
            {
                IConditionsList conditionList = this.targetItem.gameObject.AddComponent<IConditionsList>();
                conditionList.hideFlags = HideFlags.HideInInspector;

                this.spConditionList.objectReferenceValue = conditionList;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.Update();
            }

            this.spContent = serializedObject.FindProperty(PROP_CONTENT);
            this.spContentString = this.spContent.FindPropertyRelative(PROP_CONTENT_STR);
            this.spVoice = this.serializedObject.FindProperty(PROP_VOICE);
            this.spAutoPlay = this.serializedObject.FindProperty(PROP_AUTOPLAY);
            this.spAutoPlayTime = this.serializedObject.FindProperty(PROP_AUTOPLAY_TIME);

            this.spActor = this.serializedObject.FindProperty(PROP_ACTOR);
            this.spActorSpriteIndex = this.serializedObject.FindProperty(PROP_ACTOR_SPRITE);
            this.spActorTransform = this.serializedObject.FindProperty(PROP_ACTOR_TRANSFORM);

            this.spAfterRun = this.serializedObject.FindProperty(PROP_AFTERRUN);
            this.spAfterRunJumpTo = this.serializedObject.FindProperty(PROP_AFTERRUN_JUMPTO);
            this.SetupJumpOptions();

            this.target.hideFlags = HideFlags.HideInInspector;
        }

        public void OnEnableBeforeGUI()
        {
            this.SetupJumpOptions();
        }

        private void SetupJumpOptions()
        {
            Dialogue dialogue = (Dialogue)this.spDialogue.objectReferenceValue;
            if (dialogue == null) return;

            this.jumpOptions = new string[dialogue.itemInstances.Length];
            this.jumpOptionsIndex = -1;

            for (int i = 0; i < this.jumpOptions.Length; ++i)
            {
                if (dialogue.itemInstances[i] == null) continue;
                if (!ALLOWED_JUMP_TO_TYPES.Contains(dialogue.itemInstances[i].GetType())) continue;

                int itemInstanceID = dialogue.itemInstances[i].GetInstanceID();
                if (dialogue.dialogue.GetInstanceID() == itemInstanceID) continue;
                if (itemInstanceID == this.target.GetInstanceID()) continue;

                if (this.spAfterRunJumpTo.objectReferenceValue != null &&
                    this.spAfterRunJumpTo.objectReferenceValue.GetInstanceID() == itemInstanceID)
                {
                    this.jumpOptionsIndex = i;
                }

                string itemContent = GetContent(dialogue.itemInstances[i]);
                if (itemContent.Length > JUMP_MAX_TEXT)
                {
                    this.jumpOptions[i] = itemContent.Substring(0, JUMP_MAX_TEXT);
                    this.jumpOptions[i] += "...";
                }
                else this.jumpOptions[i] = itemContent;
            }
        }

        // VIRTUAL METHODS: -----------------------------------------------------------------------

        public virtual string UpdateContent()
        {
            return "default content";
        }

        public virtual Texture2D UpdateIcon()
        {
            return IDialogueItemEditor.GetIcon();
        }

        public static Texture2D GetIcon()
        {
            return IDialogueItemEditor.GetOrLoadTexture(ICON_DEFAULT_PATH, ICON_DEFAULT_HASH);
        }

        public virtual void OnDestroyItem()
        {
            if (this.spActionsList.objectReferenceValue != null)
            {
                DestroyImmediate(this.spActionsList.objectReferenceValue, true);
            }
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public static string GetContent(Object item)
        {
            return GetContent((IDialogueItem)item);
        }

        public static string GetContent(IDialogueItem item)
        {
            string content = item.content.content;
            if (string.IsNullOrEmpty(content)) return "[ Empty Message ]";
            return content.Replace("\n", " ");
        }

        public static new IDialogueItemEditor CreateEditor(Object instance)
        {
            return (IDialogueItemEditor)Editor.CreateEditor(instance);
        }

        public void AddChild(IDialogueItem item, IDialogueItem parent, Dialogue dialogue)
        {
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.Update();

            if (parent == null)
            {
                Debug.LogError("Adding null parent");
                return;
            }

            SerializedProperty children = serializedObject.FindProperty(PROP_CHILDREN);
            int index = children.arraySize;

            SerializedObject childSerializedObject = new SerializedObject(item);
            childSerializedObject.FindProperty(PROP_DIALOGUE).objectReferenceValue = dialogue;
            childSerializedObject.FindProperty(PROP_PARENT).objectReferenceValue = parent;
            childSerializedObject.ApplyModifiedPropertiesWithoutUndo();
            childSerializedObject.Update();

            children.InsertArrayElementAtIndex(index);
            SerializedProperty child = children.GetArrayElementAtIndex(index);
            child.objectReferenceValue = item;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.Update();
        }

        public void AddSibling(IDialogueItem item, IDialogueItem sibling, Dialogue dialogue, int siblingID = -1)
        {
            if (sibling.parent == null)
            {
                Debug.LogError("Unable to add sibling. Unknown parent");
                return;
            }

            SerializedObject parentSerializedObject = new SerializedObject(sibling.parent);
            SerializedProperty spParentChildren = parentSerializedObject.FindProperty(PROP_CHILDREN);

            int index = spParentChildren.arraySize;
            if (siblingID != -1)
            {
                int childrenCount = this.targetItem.parent.children.Count;
                for (int i = 0; i < childrenCount; ++i)
                {
                    if (this.targetItem.parent.children[i].GetInstanceID() == siblingID)
                    {
                        index = i + 1;
                        break;
                    }
                }
            }

            spParentChildren.InsertArrayElementAtIndex(index);
            spParentChildren.GetArrayElementAtIndex(index).objectReferenceValue = item;

            parentSerializedObject.ApplyModifiedPropertiesWithoutUndo();
            parentSerializedObject.Update();

            SerializedObject itemSerializedObject = new SerializedObject(item);
            itemSerializedObject.FindProperty(PROP_DIALOGUE).objectReferenceValue = dialogue;
            itemSerializedObject.FindProperty(PROP_PARENT).objectReferenceValue = sibling.parent;
            itemSerializedObject.ApplyModifiedPropertiesWithoutUndo();
            itemSerializedObject.Update();
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected static Texture2D GetOrLoadTexture(string path, int pathHash)
        {
            if (!ICON_TEXTURES.ContainsKey(pathHash)) IDialogueItemEditor.LoadIconTexture(path);
            return ICON_TEXTURES[pathHash];
        }

        protected static void LoadIconTexture(string path)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null) Debug.LogError("Unable to load node texture at path: " + path);

            int hash = path.GetHashCode();
            if (!ICON_TEXTURES.ContainsKey(hash))
            {
                ICON_TEXTURES.Add(hash, texture);
            }
        }

        // PROTECTED PAINT METHODS: ---------------------------------------------------------------

        protected void PaintContentTab()
        {
            EditorGUILayout.PropertyField(this.spContent);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spVoice);
            EditorGUILayout.PropertyField(this.spAutoPlay);

            EditorGUI.BeginDisabledGroup(!this.spAutoPlay.boolValue);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(this.spAutoPlayTime);

            if (this.spVoice.objectReferenceValue != null)
            {
                AudioClip audioClip = (AudioClip)this.spVoice.objectReferenceValue;
                if (!Mathf.Approximately(audioClip.length, this.spAutoPlayTime.floatValue))
                {
                    Rect btnRect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.miniButton);
                    btnRect = new Rect(
                        btnRect.x + EditorGUIUtility.labelWidth,
                        btnRect.y,
                        btnRect.width - EditorGUIUtility.labelWidth,
                        btnRect.height
                    );

                    if (GUI.Button(btnRect, "Use voice length", EditorStyles.miniButton))
                    {
                        this.spAutoPlayTime.floatValue = audioClip.length;
                    }
                }
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(this.spActor);
            if (EditorGUI.EndChangeCheck())
            {
                Actor actor = this.spActor.objectReferenceValue as Actor;
                if (actor != null) ActorUtility.Add(actor);
            }

            if (this.spActor.objectReferenceValue == null)
            {
                for (int i = 0; i < ActorUtility.ACTORS.Count; ++i)
                {
                    Actor actor = ActorUtility.ACTORS[i];
                    if (actor != null)
                    {
                        Rect rect = GUILayoutUtility.GetRect(
                            GUIContent.none, 
                            EditorStyles.miniButton
                        );

                        rect = new Rect(
                            rect.x + EditorGUIUtility.labelWidth,
                            rect.y,
                            rect.width - EditorGUIUtility.labelWidth,
                            rect.height
                        );

                        if (GUI.Button(rect, actor.name, EditorStyles.miniButton))
                        {
                            if (actor != null) ActorUtility.Add(actor);
                            this.spActor.objectReferenceValue = actor;
                        }
                    }
                }
            }
            else
            {
                EditorGUI.indentLevel++;
                this.spActorSpriteIndex.intValue = EditorGUILayout.IntPopup(
                    "Portrait",
                    this.spActorSpriteIndex.intValue,
                    ((Actor)this.spActor.objectReferenceValue).GetPortraitNames(),
                    ((Actor)this.spActor.objectReferenceValue).GetPortraitValues()
                );
                EditorGUILayout.PropertyField(this.spActorTransform, GC_TRANSFORM);
                EditorGUI.indentLevel--;
            }
        }

        protected void PaintAfterRunTab()
        {
            EditorGUILayout.PropertyField(this.spAfterRun);
            if (this.spAfterRun.intValue == (int)IDialogueItem.AfterRunBehaviour.Jump)
            {
                int jmpCurrIndex = this.jumpOptionsIndex;
                this.jumpOptionsIndex = EditorGUILayout.Popup("Jump To", jmpCurrIndex, this.jumpOptions);
                if (jmpCurrIndex != this.jumpOptionsIndex)
                {
                    Dialogue dialogue = (Dialogue)this.spDialogue.objectReferenceValue;
                    if (dialogue != null)
                    {
                        IDialogueItem reference = dialogue.itemInstances[this.jumpOptionsIndex];
                        this.spAfterRunJumpTo.objectReferenceValue = reference;

                        serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        serializedObject.Update();
                    }
                }
            }
        }

        protected void PaintActionsTab()
        {
            EditorGUILayout.PropertyField(this.spExecuteBehavior);

            if (this.actionsListEditor == null)
            {
                this.actionsListEditor = (IActionsListEditor)IActionsListEditor.CreateEditor(
                    this.spActionsList.objectReferenceValue, typeof(IActionsListEditor)
                );
            }

            if (this.actionsListEditor != null)
            {
                this.actionsListEditor.OnInspectorGUI();
            }
        }

        protected void PaintConditionsTab()
        {
            if (this.conditionListEditor == null)
            {
                this.conditionListEditor = (IConditionsListEditor)IConditionsListEditor.CreateEditor(
                    this.spConditionList.objectReferenceValue, typeof(IConditionsListEditor)
                );
            }

            if (this.conditionListEditor != null)
            {
                this.conditionListEditor.OnInspectorGUI();
            }
        }

        protected void PaintConfigTab()
        {
            EditorGUILayout.PropertyField(this.spOverrideConfig);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(!this.spOverrideConfig.boolValue);
            EditorGUILayout.PropertyField(this.spConfig);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }
    }
}