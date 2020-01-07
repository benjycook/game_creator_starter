namespace GameCreator.Dialogue
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using GameCreator.Core;

    [Serializable]
    public class ActorSprites
    {
        public enum DataType
        {
            Sprite,
            Texture,
            Prefab
        }

        [Serializable]
        public class Data
        {
            public string name = "";
            public DataType type = DataType.Sprite;

            public Sprite sprite;
            public Texture texture;
            public GameObject prefab;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public Data[] data;
    }
}