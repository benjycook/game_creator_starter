﻿namespace GameCreator.Inventory
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;

	#if UNITY_EDITOR
	using UnityEditor;
	#endif

	[AddComponentMenu("")]
	public class ActionInventoryRecipe : IAction 
	{
		public ItemHolder item1;
		public ItemHolder item2;

        // EXECUTABLE: -------------------------------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            InventoryManager.Instance.UseRecipe(this.item1.item.uuid, this.item2.item.uuid);
            return true;
        }

		// +-----------------------------------------------------------------------------------------------------------+
		// | EDITOR                                                                                                    |
		// +-----------------------------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

		public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Inventory/Icons/Actions/";

		public static new string NAME = "Inventory/Use Recipe";
		private const string NODE_TITLE = "Combine {0} with {1}";

		// PROPERTIES: -----------------------------------------------------------------------------------------------------

		private SerializedProperty spItem1;
		private SerializedProperty spItem2;

		// INSPECTOR METHODS: ----------------------------------------------------------------------------------------------

		public override string GetNodeTitle()
		{
			return string.Format(
				NODE_TITLE, 
				(this.item1.item == null ? "(none)" : this.item1.item.itemName.content),
				(this.item2.item == null ? "(none)" : this.item2.item.itemName.content)
			);
		}

		protected override void OnEnableEditorChild ()
		{
			this.spItem1 = this.serializedObject.FindProperty("item1");
			this.spItem2 = this.serializedObject.FindProperty("item2");
		}

		protected override void OnDisableEditorChild ()
		{
			this.spItem1 = null;
			this.spItem2 = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this.spItem1);
			EditorGUILayout.PropertyField(this.spItem2);

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}