namespace GameCreator.Dialogue
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
    using UnityEngine.Serialization;
	using GameCreator.Core;
    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    public class DatabaseDialogue : IDatabase
	{
        private const string RESOURCE_DEFAULTSKIN_PATH = "GameCreator/DefaultDialogueSkin";

        [Serializable]
        public class ConfigData
        {
            public GameObject dialogueSkin;
            public KeyCode skipKey = KeyCode.Mouse0;

            [Range(0f, 1f)]
            public float revisitChoiceOpacity = 0.75f;

            [FormerlySerializedAs("enableTypewritterEffect")]
            public bool enableTypewriterEffect = true;
            public float charactersPerSecond = 30f;

            public ConfigData()
            { }

            public ConfigData(ConfigData config)
            {
                this.dialogueSkin = config.dialogueSkin;
                this.skipKey = config.skipKey;
                this.revisitChoiceOpacity = config.revisitChoiceOpacity;
                this.enableTypewriterEffect = config.enableTypewriterEffect;
                this.charactersPerSecond = config.charactersPerSecond;
            }
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public ConfigData defaultConfig;

        // PUBLIC STATIC METHODS: -----------------------------------------------------------------

        public static DatabaseDialogue Load()
        {
            DatabaseDialogue databaseDialogue = IDatabase.LoadDatabase<DatabaseDialogue>();
            if (databaseDialogue.defaultConfig.dialogueSkin == null)
            {
                GameObject skin = Resources.Load<GameObject>(RESOURCE_DEFAULTSKIN_PATH);
                databaseDialogue.defaultConfig.dialogueSkin = skin;
            }

            return databaseDialogue;
        }

        // OVERRIDE METHODS: ----------------------------------------------------------------------

        #if UNITY_EDITOR

        protected override string GetProjectPath()
        {
            return "Assets/Plugins/GameCreatorData/Dialogue/Resources";
        }

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            IDatabase.Setup<DatabaseDialogue>();
        }

        #endif
	}
}