namespace GameCreator.Dialogue
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;
    using GameCreator.Localization;
    using GameCreator.Variables;
    using GameCreator.Core.Hooks;

    [Serializable][CreateAssetMenu(fileName = "Actor", menuName = "Game Creator/Dialogue/Actor")]
    public class Actor : ScriptableObject, ISerializationCallbackReceiver
    {
        public enum Name
        {
            Constant,
            Variable
        }

        private Guid cacheGuid;
        [SerializeField] private byte[] serializedGuid;

        [SerializeField]
        private Name nameType = Name.Constant;

        [SerializeField][LocStringNoPostProcess]
        private LocString actorNameConstant = new LocString("");

        [SerializeField]
        private StringProperty actorNameVariable = new StringProperty("");

        public Color color = Color.white;
        public ActorSprites actorSprites = new ActorSprites();

        public bool useGibberish = false;
        public AudioClip gibberishAudio;
        public float gibberishPitch = 1.0f;
        public float gibberishVariation = 0.1f;

        // INITIALIZERS: --------------------------------------------------------------------------

        public Actor()
        {
            this.cacheGuid = Guid.NewGuid();
            this.serializedGuid = cacheGuid.ToByteArray();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public string GetID()
        {
            return this.cacheGuid.ToString();
        }

        public string GetName()
        {
            GameObject target = HookPlayer.Instance == null 
                ? null 
                : HookPlayer.Instance.gameObject;

            switch (this.nameType)
            {
                case Name.Constant: return this.actorNameConstant.GetText();
                case Name.Variable: return this.actorNameVariable.GetValue(target);
            }

            return "";
        }

        public string[] GetPortraitNames()
        {
            if (this.actorSprites.data == null) return new string[0];
            string[] names = new string[this.actorSprites.data.Length];

            for (int i = 0; i < this.actorSprites.data.Length; ++i)
            {
                names[i] = this.actorSprites.data[i].name;
            }

            return names;
        }

        public int[] GetPortraitValues()
        {
            int[] values = new int[this.actorSprites.data.Length];
            for (int i = 0; i < this.actorSprites.data.Length; ++i)
            {
                values[i] = i;
            }

            return values;
        }

        // SERIALIZATION: -------------------------------------------------------------------------

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (this.cacheGuid != Guid.Empty)
            {
                this.serializedGuid = this.cacheGuid.ToByteArray();
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (this.serializedGuid != null && this.serializedGuid.Length > 0)
            {
                this.cacheGuid = new Guid(serializedGuid);
            }
        }
    }
}