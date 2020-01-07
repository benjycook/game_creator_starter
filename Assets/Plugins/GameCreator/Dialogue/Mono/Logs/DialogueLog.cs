namespace GameCreator.Dialogue
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using GameCreator.Core;

    public class DialogueLog
    {
        public class Log
        {
            public string text;
            public bool isChoice;

            public Actor actor;
            public int actorSpriteIndex;

            public Log(string text, bool isChoice, Actor actor, int actorSpriteIndex)
            {
                this.text = text;
                this.isChoice = isChoice;
                this.actor = actor;
                this.actorSpriteIndex = actorSpriteIndex;
            }
        }

        private class EventAdd : UnityEvent<Log> { }
        private class EventReset : UnityEvent { }

        // PROPERTIES: ----------------------------------------------------------------------------

        private List<Log> logs = new List<Log>();

        private readonly EventAdd eventAdd = new EventAdd();
        private readonly EventReset eventReset = new EventReset();

        // INITIALIZERS: --------------------------------------------------------------------------

        public DialogueLog()
        {
            this.logs = new List<Log>();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Add(IDialogueItem item, bool isChoice)
        {
            Log log = new Log(
                item.GetContent(),
                isChoice,
                item.actor,
                item.actorSpriteIndex
            );

            this.logs.Add(log);
            this.eventAdd.Invoke(log);
        }

        public void Reset()
        {
            this.logs = new List<Log>();
            this.eventReset.Invoke();
        }

        // EVENT METHODS: -------------------------------------------------------------------------

        public void AddListenerOnAdd(UnityAction<Log> callback)
        {
            this.eventAdd.AddListener(callback);
        }

        public void RemoveListenerOnAdd(UnityAction<Log> callback)
        {
            this.eventAdd.RemoveListener(callback);
        }

        public void AddListenerOnReset(UnityAction callback)
        {
            this.eventReset.AddListener(callback);
        }

        public void RemoveListenerOnReset(UnityAction callback)
        {
            this.eventReset.RemoveListener(callback);
        }
    }
}