namespace GameCreator.Dialogue
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Events;
    using GameCreator.Core;

    public class DialogueLogUIElement: MonoBehaviour
    {
        public Text text;

        public void Setup(DialogueLog.Log log)
        {
            this.text.text = log.text;
        }
    }
}