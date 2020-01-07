namespace GameCreator.Dialogue
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;

    public class DialogueTreeView : TreeView
    {
        public const int ROOT_ID = 0;
        public const string ROOT_NAME = "Dialogue";

        private const float ROW_ICON_WIDTH = 20f;
        private static Texture2D ICON_CONTENT_EXIT;
        private static Texture2D ICON_CONTENT_JUMP;
        private static Texture2D ICON_CONTENT_ACTIONS;
        private static Texture2D ICON_CONTENT_CONDITIONS;
        private static Texture2D ICON_CONTENT_NO_ACTIONS;
        private static Texture2D ICON_CONTENT_NO_CONDITIONS;

        // PROPERTIES: -----------------------------------------------------------------------------

        private DialogueEditor dialogueEditor;
        public Dictionary<int, TreeViewItem> treeItems;

        // CONSTRUCTORS: ---------------------------------------------------------------------------

        public DialogueTreeView(TreeViewState state, DialogueEditor dialogueEditor) : base(state)
        {
            this.dialogueEditor = dialogueEditor;
            this.showAlternatingRowBackgrounds = true;
            this.showBorder = false;

            this.Reload();
        }

        private void BuildTree(ref TreeViewItem parentTree, IDialogueItem parentAsset)
        {
            this.treeItems.Add(parentAsset.GetInstanceID(), parentTree);

            IDialogueItemEditor editor = this.dialogueEditor.itemsEditors[parentAsset.GetInstanceID()];
            parentTree.displayName = editor.UpdateContent();
            parentTree.icon = editor.UpdateIcon();

            List<IDialogueItem> childrenAssets = parentAsset.children;
            int childrenAssetsCount = childrenAssets.Count;
            for (int i = 0; i < childrenAssetsCount; ++i)
            {
                IDialogueItem childAsset = childrenAssets[i];
                if (childAsset == null) continue;

                int childAssetID = childAsset.GetInstanceID();
                int depth = parentTree.depth + 1;
                TreeViewItem childTree = new TreeViewItem(childAssetID, depth, "Loading...");

                if (!this.dialogueEditor.itemsEditors.ContainsKey(childAssetID))
                {
                    Debug.LogError("No IDialogueItem Editor found with instanceID: " + childAssetID);
                    continue;
                }

                this.BuildTree(ref childTree, childAsset);
                parentTree.AddChild(childTree);
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            this.treeItems = new Dictionary<int, TreeViewItem>();
            TreeViewItem root = new TreeViewItem(ROOT_ID, -1, ROOT_NAME);

            this.BuildTree(ref root, this.dialogueEditor.targetDialogue.dialogue);

            if (root.hasChildren) SetupDepthsFromParentsAndChildren(root);
            else SetupParentsAndChildrenFromDepths(root, new List<TreeViewItem>());

            return root;
        }

        protected override void RowGUI(TreeView.RowGUIArgs args)
        {
            IDialogueItem item = this.dialogueEditor.InstanceIDToObject(args.item.id);

            Rect altRect = new Rect(
                args.rowRect.x,
                args.rowRect.y,
                args.rowRect.width - (EditorGUIUtility.singleLineHeight * 3f),
                args.rowRect.height
            );

            Rect icon1Rect = new Rect(
                args.rowRect.x + args.rowRect.width - EditorGUIUtility.singleLineHeight,
                args.rowRect.y,
                EditorGUIUtility.singleLineHeight,
                EditorGUIUtility.singleLineHeight
            );

            Rect icon2Rect = new Rect(
                icon1Rect.x - icon1Rect.width,
                icon1Rect.y,
                icon1Rect.width,
                icon1Rect.height
            );

            Rect icon3Rect = new Rect(
                icon2Rect.x - icon2Rect.width,
                icon2Rect.y,
                icon2Rect.width,
                icon2Rect.height
            );

            if (item != null)
            {
                args.rowRect = altRect;
                base.RowGUI(args);

                switch (item.afterRun)
                {
                    case IDialogueItem.AfterRunBehaviour.Exit:
                        GUI.DrawTexture(icon1Rect, GetIconExit());
                        break;

                    case IDialogueItem.AfterRunBehaviour.Jump:
                        GUI.DrawTexture(icon1Rect, GetIconJump());
                        break;
                }

                Texture actions = (item.actionsList.actions.Length > 0
                    ? GetIconActions()
                    : GetIconNoActions()
                );

                GUI.DrawTexture(icon3Rect, actions);

                Texture conditions = (item.conditionsList.conditions.Length > 0
                    ? GetIconConditions()
                    : GetIconNoConditions()
                );

                GUI.DrawTexture(icon2Rect, conditions);
            }
        }

        // ROW ICON: ------------------------------------------------------------------------------

        private static Texture2D GetIconExit()
        {
            if (ICON_CONTENT_EXIT == null) ICON_CONTENT_EXIT = GetIcon("Exit.png");
            return ICON_CONTENT_EXIT;
        }

        private static Texture2D GetIconJump()
        {
            if (ICON_CONTENT_JUMP == null) ICON_CONTENT_JUMP = GetIcon("Jump.png");
            return ICON_CONTENT_JUMP;
        }

        private static Texture2D GetIconActions()
        {
            if (ICON_CONTENT_ACTIONS == null) ICON_CONTENT_ACTIONS = GetIcon("Actions.png");
            return ICON_CONTENT_ACTIONS;
        }

        private static Texture2D GetIconConditions()
        {
            if (ICON_CONTENT_CONDITIONS == null) ICON_CONTENT_CONDITIONS = GetIcon("Conditions.png");
            return ICON_CONTENT_CONDITIONS;
        }

        private static Texture2D GetIconNoActions()
        {
            if (ICON_CONTENT_NO_ACTIONS == null) ICON_CONTENT_NO_ACTIONS = GetIcon("NoActions.png");
            return ICON_CONTENT_NO_ACTIONS;
        }

        private static Texture2D GetIconNoConditions()
        {
            if (ICON_CONTENT_NO_CONDITIONS == null) ICON_CONTENT_NO_CONDITIONS = GetIcon("NoConditions.png");
            return ICON_CONTENT_NO_CONDITIONS;
        }

        private static Texture2D GetIcon(string iconName)
        {
            string path = string.Format("Assets/Plugins/GameCreator/Dialogue/Icons/Rows/{0}", iconName);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // DRAG AND DROP: -------------------------------------------------------------------------

        private const string KEY_DRAG = "gamecreator-dialogue-drag";

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return args.draggedItemIDs.Count == 1;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            if (this.hasSearch) return;

            DragAndDrop.PrepareStartDrag();

            int itemID = args.draggedItemIDs[0];
            if (!this.treeItems.ContainsKey(itemID)) return;
            TreeViewItem item = this.treeItems[itemID];

            DragAndDrop.SetGenericData(KEY_DRAG, item);

            DragAndDrop.objectReferences = new UnityEngine.Object[]{};
            DragAndDrop.StartDrag(item.displayName);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            TreeViewItem draggedItem = DragAndDrop.GetGenericData(KEY_DRAG) as TreeViewItem;
            if (draggedItem == null) return DragAndDropVisualMode.None;

            if (args.dragAndDropPosition == DragAndDropPosition.UponItem ||
                args.dragAndDropPosition == DragAndDropPosition.BetweenItems)
            {
                bool validDrag = ValidDrag(args.parentItem, draggedItem);
                if (args.performDrop && validDrag)
                {
                    this.OnDropItemAtIndex(
                        draggedItem, 
                        args.parentItem, 
                        args.insertAtIndex == -1 ? 0 : args.insertAtIndex
                    );
                }

                return validDrag ? DragAndDropVisualMode.Move : DragAndDropVisualMode.None;
            }
            else if (args.dragAndDropPosition == DragAndDropPosition.OutsideItems)
            {
                bool validDrag = ValidDrag(this.rootItem, draggedItem);
                if (args.performDrop && validDrag)
                {
                    this.OnDropItemAtIndex(
                        draggedItem,
                        this.rootItem,
                        this.rootItem.children.Count
                    );
                }

                return DragAndDropVisualMode.Move;
            }

            return DragAndDropVisualMode.None;
        }

        private void OnDropItemAtIndex(TreeViewItem draggedItem, TreeViewItem parent, int insertIndex)
        {
            this.dialogueEditor.MoveItemTo(draggedItem, parent, insertIndex);
            this.Reload();

            this.SetFocusAndEnsureSelectedItem();
            this.SetSelection(new List<int> { draggedItem.id }, TreeViewSelectionOptions.RevealAndFrame);
        }

        private bool ValidDrag(TreeViewItem parentItem, TreeViewItem draggedItem)
        {
            IDialogueItem draggedItemInstance = this.dialogueEditor.InstanceIDToObject(draggedItem.id);
            IDialogueItem parentItemInstance = this.dialogueEditor.InstanceIDToObject(parentItem.id);

            if (draggedItemInstance == null || parentItemInstance == null) return false;
            if (!draggedItemInstance.CanHaveParent(parentItemInstance)) return false;

            TreeViewItem currentParent = parentItem;
            while (currentParent != null)
            {
                if (draggedItem == currentParent) return false;
                currentParent = currentParent.parent;
            }

            return true;
        }
    }
}