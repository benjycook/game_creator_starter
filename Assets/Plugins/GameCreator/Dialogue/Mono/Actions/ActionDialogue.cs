namespace GameCreator.Core
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;
    using GameCreator.Dialogue;

	#if UNITY_EDITOR
	using UnityEditor;
	#endif

	[AddComponentMenu("")]
	public class ActionDialogue : IAction
	{
        private const string ERR_DIALOGUE = "No Dialogue object provided";

        public Dialogue dialogue;
        public bool waitToComplete = false;

        // EXECUTABLE: ----------------------------------------------------------------------------

        public override bool InstantExecute(GameObject target, IAction[] actions, int index)
        {
            if (!this.waitToComplete)
            {
                if (this.dialogue == null) Debug.LogError(ERR_DIALOGUE);
                else CoroutinesManager.Instance.StartCoroutine(this.dialogue.Run());
                return true;
            }

            return false;
        }

        public override IEnumerator Execute(GameObject target, IAction[] actions, int index)
		{
            if (this.dialogue == null) Debug.LogError(ERR_DIALOGUE);
            else yield return this.dialogue.Run();

			yield return 0;
		}

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

	    public const string CUSTOM_ICON_PATH = "Assets/Plugins/GameCreator/Dialogue/Icons/Actions/";

		public static new string NAME = "Messages/Dialogue";
		private const string NODE_TITLE = "Dialogue {0}";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spDialogue;
        private SerializedProperty spWaitToComplete;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
            string dialogueName = (this.dialogue == null ? "null" : this.dialogue.name);
			return string.Format(NODE_TITLE, dialogueName);
		}

		protected override void OnEnableEditorChild ()
		{
            this.spDialogue = this.serializedObject.FindProperty("dialogue");
            this.spWaitToComplete = this.serializedObject.FindProperty("waitToComplete");
		}

		protected override void OnDisableEditorChild ()
		{
			this.spDialogue = null;
            this.spWaitToComplete = null;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this.spDialogue);
            EditorGUILayout.PropertyField(this.spWaitToComplete);

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
