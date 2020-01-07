namespace GameCreator.Dialogue
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using GameCreator.Core;

    [RequireComponent(typeof(DialogueUI))]
    public class DialogueLogUI : MonoBehaviour
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        public RectTransform logsContainer;

        [Header("Prefabs")]
        public GameObject logPrefabText;
        public GameObject logPrefabChoice;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            DialogueUI.LOG.AddListenerOnReset(this.OnReset);
            DialogueUI.LOG.AddListenerOnAdd(this.OnAddEntry);
        }

        private void OnDestroy()
        {
            DialogueUI.LOG.RemoveListenerOnReset(this.OnReset);
            DialogueUI.LOG.RemoveListenerOnAdd(this.OnAddEntry);
        }

        // EVENT METHODS: -------------------------------------------------------------------------

        private void OnReset()
        {
            if (this.logsContainer == null) return;
            int count = this.logsContainer.childCount;

            for (int i = count - 1; i >= 0; --i)
            {
                Destroy(this.logsContainer.GetChild(i).gameObject);
            }
        }

        private void OnAddEntry(DialogueLog.Log log)
        {
            GameObject prefab = log.isChoice ? this.logPrefabChoice : this.logPrefabText;
            GameObject instance = Instantiate(prefab, this.logsContainer);

            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            DialogueLogUIElement ui = instance.GetComponent<DialogueLogUIElement>();
            if (ui != null) ui.Setup(log);
        }
    }
}