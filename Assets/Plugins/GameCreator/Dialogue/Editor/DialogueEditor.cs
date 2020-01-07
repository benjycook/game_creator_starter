namespace GameCreator.Dialogue
{
    using System.IO;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEditorInternal;
    using GameCreator.Core;

    [CustomEditor(typeof(Dialogue))]
    public class DialogueEditor : Editor
    {
        private const float TREE_HEIGHT = 300f;
        private const float MIN_RESPONSIVE_WIDTH = 1200f;
        private const float INSPECTOR_DEF_HEIGHT = 50f;
        private const float INSPECTOR_MIN_HEIGHT = 50f;
        private const float INSPECTOR_MAX_HEIGHT = 500f;

        public const string PROP_DIALOGUE = "dialogue";
        public const string PROP_CHILDREN = "children";
        public const string PROP_PARENT = "parent";
        public const string PROP_ITEM_INSTANCES = "itemInstances";

        private const string GCTOOLBAR_ICON_PATH = "Assets/Plugins/GameCreator/Dialogue/Icons/GCToolbar/Dialogue.png";
        private const string TEXTURE_RESIZE_PATH = "Assets/Plugins/GameCreator/Dialogue/Icons/Dialogue/Resize.png";
        private const string MSG_UNSUPP_MULTISELECT = "Editing multiple selections is unsupported";

        // PROPERTIES: ----------------------------------------------------------------------------

        public DialogueTreeView dialogueTree = null;

        public Dialogue targetDialogue;
        public Dictionary<int, IDialogueItemEditor> itemsEditors;

        public SerializedProperty spDialogue;
        public SerializedProperty spItemInstances;
        public IDialogueItemEditor editorRoot;

        private SearchField searchField;
        private Vector2 scrollTree = Vector2.zero;

        private Texture2D textureResize;
        private bool inspectorResizing = false;

        private Dictionary<int, Object> itemInstances;

        private bool stylesInitialized = false;
        private GUIStyle btnToolbarOff;
        private GUIStyle btnToolbarOn;
        private GUIStyle boxDialogue;

        private bool isMaximized = false;

        private SerializedProperty spOverrideConfig;
        private SerializedProperty spConfig;

        // INITIALIZE METHODS: --------------------------------------------------------------------

        private void OnEnable()
        {
            if (target == null || serializedObject == null) return;
            this.targetDialogue = (Dialogue)target;

            this.spDialogue = serializedObject.FindProperty(PROP_DIALOGUE);
            this.spItemInstances = serializedObject.FindProperty(PROP_ITEM_INSTANCES);

            this.itemInstances = new Dictionary<int, Object>();
            if (this.spDialogue.objectReferenceValue == null)
            {
                DialogueItemRoot root = this.CreateDialogueItem<DialogueItemRoot>();
                this.spDialogue.objectReferenceValue = root;
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

                SerializedObject rootSO = new SerializedObject(root);
                rootSO.FindProperty(PROP_DIALOGUE).objectReferenceValue = this.targetDialogue;
                rootSO.ApplyModifiedProperties();
                rootSO.Update();
            }

            this.itemInstances.Add(DialogueTreeView.ROOT_ID, this.spDialogue.objectReferenceValue);
            this.itemsEditors = new Dictionary<int, IDialogueItemEditor>();

            Object[] items = this.targetDialogue.itemInstances;

            for (int i = 0; i < items.Length; ++i)
            {
                if (!this.itemInstances.ContainsKey(items[i].GetInstanceID()))
                {
                    this.itemInstances.Add(items[i].GetInstanceID(), items[i]);
                }

                IDialogueItemEditor editor = IDialogueItemEditor.CreateEditor(items[i]);
                this.itemsEditors.Add(items[i].GetInstanceID(), editor);

                if (items[i].GetInstanceID() == this.targetDialogue.dialogue.GetInstanceID())
                {
                    this.editorRoot = editor;
                }
            }

            this.dialogueTree = new DialogueTreeView(this.targetDialogue.dialogueTreeState, this);
            this.inspectorResizing = false;
            this.textureResize = AssetDatabase.LoadAssetAtPath<Texture2D>(TEXTURE_RESIZE_PATH);

            this.searchField = new SearchField();
            this.searchField.downOrUpArrowKeyPressed += this.dialogueTree.SetFocusAndEnsureSelectedItem;

            this.spOverrideConfig = this.serializedObject.FindProperty("overrideConfig");
            this.spConfig = this.serializedObject.FindProperty("config");
        }

        [InitializeOnLoadMethod]
        private static void RegisterDialogueToolbar()
        {
            GameCreatorToolbar.REGISTER_ITEMS.Push(new GameCreatorToolbar.Item(
                string.Format(GCTOOLBAR_ICON_PATH),
                "Create a Dialogue", 
                DialogueEditor.CreateDialogue,
                100
            ));
        }

        // GUI METHODS: ---------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            if (target == null || serializedObject == null) return;

            if (!this.stylesInitialized)
            {
                this.InitializeStyles();
                this.stylesInitialized = true;
            }

            serializedObject.Update();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(this.boxDialogue);

            this.PaintToolbar();
            this.PaintDialogueTree();
            this.PaintInspector();

            GUILayout.Space(1f);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spOverrideConfig);
            if (this.spOverrideConfig.boolValue)
            {
                EditorGUILayout.PropertyField(this.spConfig);
            }

            EditorGUILayout.Space();
            GlobalEditorID.Paint(this.targetDialogue);

            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
        }

        private void PaintToolbar()
        {
            bool hasSelection = this.dialogueTree.HasSelection();
            int selectionCount = (hasSelection ? this.dialogueTree.GetSelection().Count : 0);

            System.Type selectionType = null;
            if (selectionCount == 1)
            {
                int instanceID = this.dialogueTree.GetSelection()[0];
                Object instance = this.InstanceIDToObject(instanceID);
                selectionType = (instance != null ? instance.GetType() : null);
            }

            DialogueToolbarUtils.ContentStyle contentStyle = DialogueToolbarUtils.ContentStyle.IconOnly;
            GUIContent gcText = DialogueToolbarUtils.GetContent(DialogueToolbarUtils.ContentType.Text, contentStyle);
            GUIContent gcGroup = DialogueToolbarUtils.GetContent(DialogueToolbarUtils.ContentType.ChoiceGroup, contentStyle);
            GUIContent gcChoice = DialogueToolbarUtils.GetContent(DialogueToolbarUtils.ContentType.Choice, contentStyle);
            GUIContent gcDelete = DialogueToolbarUtils.GetContent(DialogueToolbarUtils.ContentType.Delete, contentStyle);

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUI.BeginDisabledGroup(!DialogueItemTextEditor.CanAddElement(selectionCount, selectionType));
            if (GUILayout.Button(gcText, EditorStyles.toolbarButton)) DialogueItemTextEditor.AddElement(this);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!DialogueItemChoiceGroupEditor.CanAddElement(selectionCount, selectionType));
            if (GUILayout.Button(gcGroup, EditorStyles.toolbarButton)) DialogueItemChoiceGroupEditor.AddElement(this);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!DialogueItemChoiceEditor.CanAddElement(selectionCount, selectionType));
            if (GUILayout.Button(gcChoice, EditorStyles.toolbarButton)) DialogueItemChoiceEditor.AddElement(this);
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(!hasSelection);
            if (GUILayout.Button(gcDelete, EditorStyles.toolbarButton) && this.dialogueTree.HasSelection())
            {
                List<int> items = new List<int>(this.dialogueTree.GetSelection());
                this.DeleteItems(items);
                this.dialogueTree.Reload();
            }
            EditorGUI.EndDisabledGroup();

            Rect searchRect = GUILayoutUtility.GetRect(
                0f, 10000f,
                EditorGUIUtility.singleLineHeight,
                EditorGUIUtility.singleLineHeight,
                EditorStyles.toolbarTextField
            );

            this.dialogueTree.searchString = this.searchField.OnToolbarGUI(
                searchRect, 
                this.dialogueTree.searchString
            );
            GUILayout.FlexibleSpace();

            GUIStyle flscrenStyle = (this.isMaximized ? this.btnToolbarOn : this.btnToolbarOff);
            GUIContent gcScreen = (this.isMaximized
                ? DialogueToolbarUtils.GetContent(DialogueToolbarUtils.ContentType.Minimize, contentStyle)
                : DialogueToolbarUtils.GetContent(DialogueToolbarUtils.ContentType.Maximize, contentStyle)
            );

            if (GUILayout.Button(gcScreen, flscrenStyle))
            {
                if (EditorWindow.mouseOverWindow != null)
                {
                    if (EditorWindow.mouseOverWindow.maximized)
                    {
                        EditorApplication.update += this.MinimizeWindow;
                    }
                    else
                    {
                        EditorApplication.update += this.MaximizeWindow;
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            UnityEngine.Event current = UnityEngine.Event.current;
            if (current.type != EventType.KeyDown) return;
            if (!current.command && !current.control) return;

            switch (current.keyCode)
            {
                case KeyCode.T:
                    if (DialogueItemTextEditor.CanAddElement(selectionCount, selectionType))
                    {
                        DialogueItemTextEditor.AddElement(this);
                        current.Use();
                    }
                    break;

                case KeyCode.G:
                    if (DialogueItemChoiceGroupEditor.CanAddElement(selectionCount, selectionType))
                    {
                        DialogueItemChoiceGroupEditor.AddElement(this);
                        current.Use();
                    }
                    break;

                case KeyCode.J:
                    if (DialogueItemChoiceEditor.CanAddElement(selectionCount, selectionType))
                    {
                        DialogueItemChoiceEditor.AddElement(this);
                        current.Use();
                    }
                    break;
            }
        }

        private void PaintDialogueTree()
        {
            this.scrollTree = EditorGUILayout.BeginScrollView(
                this.scrollTree,
                GUILayout.MinHeight(TREE_HEIGHT),
                GUILayout.MaxHeight(TREE_HEIGHT),
                GUILayout.ExpandHeight(false)
            );

            Rect treeViewRect = GUILayoutUtility.GetRect(
                0f, 10000f,
                TREE_HEIGHT,
                TREE_HEIGHT
            );

            this.dialogueTree.OnGUI(treeViewRect);

            EditorGUILayout.EndScrollView();
        }

        public override bool RequiresConstantRepaint()
        {
            bool repaint = base.RequiresConstantRepaint();
            return repaint || this.inspectorResizing;
        }

        private int inspectorSelectionID = -1;

        private void PaintInspector()
        {
            if (!this.dialogueTree.HasSelection()) return;
            List<int> selections = new List<int>(this.dialogueTree.GetSelection());
            for (int i = 0; i < selections.Count; ++i)
            {
                if (this.InstanceIDToObject(selections[i]) == null)
                {
                    this.dialogueTree.SetSelection(new List<int>());
                    return;
                }
            }

            Rect rect = EditorGUILayout.BeginVertical();

            Rect resizeRect = new Rect(rect.x, rect.y, rect.width, 16f);
            GUILayoutUtility.GetRect(resizeRect.width, resizeRect.height);
            EditorGUI.DrawPreviewTexture(resizeRect, this.textureResize);

            if (this.dialogueTree.GetSelection().Count == 1)
            {
                int instanceID = this.dialogueTree.GetSelection()[0];
                if (this.itemsEditors.ContainsKey(instanceID))
                {
                    if (this.inspectorSelectionID != instanceID)
                    {
                        this.itemsEditors[instanceID].OnEnableBeforeGUI();
                    }

                    this.itemsEditors[instanceID].OnInspectorGUI();
                    if (this.dialogueTree.treeItems.ContainsKey(instanceID))
                    {
                        this.dialogueTree.treeItems[instanceID].displayName = this.itemsEditors[instanceID].UpdateContent();
                        this.dialogueTree.treeItems[instanceID].icon = this.itemsEditors[instanceID].UpdateIcon();
                    }
                }

                this.inspectorSelectionID = instanceID;
            }
            else
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(MSG_UNSUPP_MULTISELECT, EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void MaximizeWindow()
        {
            if (EditorWindow.mouseOverWindow != null)
            {
                this.isMaximized = true;
                EditorWindow.mouseOverWindow.maximized = true;
            }

            EditorApplication.update -= this.MaximizeWindow;
        }

        private void MinimizeWindow()
        {
            if (EditorWindow.mouseOverWindow != null)
            {
                this.isMaximized = false;
                EditorWindow.mouseOverWindow.maximized = false;
            }

            EditorApplication.update -= this.MinimizeWindow;
        }

        private void DeleteItems(List<int> items)
        {
            if (items == null || items.Count == 0) return;

            for (int i = 0; i < items.Count; ++i)
            {
                int selectionID = items[i];

                UnityEngine.Object instance = this.InstanceIDToObject(selectionID);
                if (instance == null) continue;

                IDialogueItem instanceAsDialogue = (IDialogueItem)instance;
                this.DeleteItems(instanceAsDialogue.GetChildrenIDs());

                instanceAsDialogue.parent.children.Remove(instanceAsDialogue);

                this.itemInstances.Remove(instance.GetInstanceID());
                int spItemInstancesSize = this.spItemInstances.arraySize;
                for (int j = spItemInstancesSize - 1; j >= 0; --j)
                {
                    SerializedProperty property = spItemInstances.GetArrayElementAtIndex(j);
                    if (property.objectReferenceValue == null ||
                        property.objectReferenceValue.GetInstanceID() == instance.GetInstanceID())
                    {
                        this.spItemInstances.RemoveFromObjectArrayAt(j);
                    }
                }

                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

                if (this.itemsEditors.ContainsKey(selectionID) && 
                    this.itemsEditors[selectionID] != null)
                {
                    this.itemsEditors[selectionID].OnDestroyItem();
                }

                DestroyImmediate(instance, true);
            }
        }

        private void InitializeStyles()
        {
            this.btnToolbarOff = new GUIStyle(EditorStyles.toolbarButton);
            this.btnToolbarOn = new GUIStyle(EditorStyles.toolbarButton);
            this.btnToolbarOn.normal = this.btnToolbarOn.onNormal;
            this.btnToolbarOn.hover = this.btnToolbarOn.onHover;
            this.btnToolbarOn.active = this.btnToolbarOn.onActive;
            this.btnToolbarOn.focused = this.btnToolbarOn.onFocused;

            this.boxDialogue = new GUIStyle(GUI.skin.box);
            this.boxDialogue.margin = new RectOffset(0, 0, 0, 0);
            this.boxDialogue.padding = new RectOffset(1, 1, 1, 0);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public T CreateDialogueItem<T>() where T : IDialogueItem
        {
            T instance = this.targetDialogue.gameObject.AddComponent<T>();
            instance.children = new List<IDialogueItem>();
            this.itemInstances.Add(instance.GetInstanceID(), instance);

            int index = this.spItemInstances.arraySize;
            this.spItemInstances.InsertArrayElementAtIndex(index);
            this.spItemInstances.GetArrayElementAtIndex(index).objectReferenceValue = instance;

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            return instance;
        }

        public IDialogueItem InstanceIDToObject(int instanceID)
        {
            if (this.itemInstances.ContainsKey(instanceID))
            {
                return (IDialogueItem)this.itemInstances[instanceID];
            }

            return null;
        }

        public static bool IsWideScreen()
        {
            return (Screen.width > MIN_RESPONSIVE_WIDTH);
        }

        public void MoveItemTo(TreeViewItem draggedItem, TreeViewItem parent, int insertIndex)
        {
            this.editorRoot.spChildren.InsertArrayElementAtIndex(this.editorRoot.spChildren.arraySize);

            IDialogueItem draggedDialogueItem = this.InstanceIDToObject(draggedItem.id);
            if (draggedDialogueItem != null && draggedDialogueItem.parent != null)
            {
                int draggedItemParentID = draggedItem.parent.id;
                IDialogueItem draggedItemParent = this.InstanceIDToObject(draggedItemParentID);

                if (draggedItemParent != null)
                {
                    int rmIndex = draggedDialogueItem.parent.children.IndexOf(draggedDialogueItem);
                    SerializedObject spDraggedItemParent = new SerializedObject(draggedItemParent);
                    spDraggedItemParent.FindProperty(PROP_CHILDREN).RemoveFromObjectArrayAt(rmIndex);
                }
            }

            IDialogueItem parentDialogueItem = this.InstanceIDToObject(parent.id);
            if (parentDialogueItem != null)
            {
                SerializedObject spParentDialogueItem = new SerializedObject(parentDialogueItem);
                SerializedProperty spParentDialogueItemChildren = spParentDialogueItem.FindProperty(PROP_CHILDREN);

                insertIndex = Mathf.Min(spParentDialogueItemChildren.arraySize, insertIndex);

                spParentDialogueItemChildren.InsertArrayElementAtIndex(insertIndex);
                spParentDialogueItemChildren.GetArrayElementAtIndex(insertIndex).objectReferenceValue = draggedDialogueItem;

                spParentDialogueItemChildren.serializedObject.ApplyModifiedProperties();
                spParentDialogueItemChildren.serializedObject.Update();

                SerializedObject spDraggedDialogueItem = new SerializedObject(draggedDialogueItem);
                spDraggedDialogueItem.FindProperty(PROP_PARENT).objectReferenceValue = parentDialogueItem;

                spDraggedDialogueItem.ApplyModifiedProperties();
                spDraggedDialogueItem.Update();
            }
        }

        // HIERARCHY CONTEXT MENU: ----------------------------------------------------------------

        [MenuItem("GameObject/Game Creator/Other/Dialogue", false, 0)]
        public static void CreateDialogue()
        {
            GameObject dialogue = CreateSceneObject.Create("Dialogue");
            dialogue.AddComponent<Dialogue>();
        }
    }
}
