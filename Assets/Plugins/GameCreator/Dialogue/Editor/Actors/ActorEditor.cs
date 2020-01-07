namespace GameCreator.Dialogue
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEditor;
    using UnityEditorInternal;

    [CustomEditor(typeof(Actor), true)]
    public class ActorEditor : Editor
    {
        private static readonly GUIContent GC_NAME = new GUIContent("Name");
        private static readonly GUIContent GC_AUDIO = new GUIContent("Audio Clip");
        private static readonly GUIContent GC_PITCH = new GUIContent("Pitch");
        private static readonly GUIContent GC_VARIATION = new GUIContent("Variation");
        private const int SPRITE_SIZE = 80;

        // PROPERTIES: ----------------------------------------------------------------------------

        private SerializedProperty spNameType;
        private SerializedProperty spNameConstant;
        private SerializedProperty spNameVariable;

        private SerializedProperty spColor;

        private SerializedProperty spActorSprites;
        private SerializedProperty spActorSpritesData;

        private ReorderableList actorSprites;

        private SerializedProperty spUseGibberish;
        private SerializedProperty spGibberishAudio;
        private SerializedProperty spGibberishPitch;
        private SerializedProperty spGibberishVariation;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            this.spNameType = serializedObject.FindProperty("nameType");
            this.spNameConstant = serializedObject.FindProperty("actorNameConstant");
            this.spNameVariable = serializedObject.FindProperty("actorNameVariable");

            this.spColor = serializedObject.FindProperty("color");

            this.spActorSprites = serializedObject.FindProperty("actorSprites");
            this.spActorSpritesData = this.spActorSprites.FindPropertyRelative("data");

            this.spUseGibberish = serializedObject.FindProperty("useGibberish");
            this.spGibberishAudio = serializedObject.FindProperty("gibberishAudio");
            this.spGibberishPitch = serializedObject.FindProperty("gibberishPitch");
            this.spGibberishVariation = serializedObject.FindProperty("gibberishVariation");

            this.actorSprites = new ReorderableList(
                serializedObject,
                this.spActorSpritesData,
                true, false, true, true
            );

            this.actorSprites.elementHeightCallback = this.DrawActorSpritesHeight;
            this.actorSprites.drawElementCallback = this.DrawActorSpritesElement;
            this.actorSprites.onCanAddCallback = this.DrawActorSpritesCanAdd;
            this.actorSprites.onAddCallback = this.DrawActorSpritesAdd;
            this.actorSprites.onCanRemoveCallback = this.DrawActorSpritesCanRemove;
            this.actorSprites.onRemoveCallback = this.DrawActorSpritesRemove;
        }

        // PAINT METHODS: -------------------------------------------------------------------------

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(this.spNameType);
            switch (this.spNameType.intValue)
            {
                case (int)Actor.Name.Constant: 
                    EditorGUILayout.PropertyField(this.spNameConstant, GC_NAME); 
                    break;
                case (int)Actor.Name.Variable: 
                    EditorGUILayout.PropertyField(this.spNameVariable, GC_NAME); 
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spColor);

            EditorGUILayout.Space();
            this.actorSprites.DoLayoutList();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(this.spUseGibberish);
            EditorGUI.BeginDisabledGroup(!this.spUseGibberish.boolValue);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(this.spGibberishAudio, GC_AUDIO);
            EditorGUILayout.PropertyField(this.spGibberishPitch, GC_PITCH);
            EditorGUILayout.PropertyField(this.spGibberishVariation, GC_VARIATION);
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }

        // ACTOR SPRITES LIST METHODS: ------------------------------------------------------------

        private float DrawActorSpritesHeight(int index)
        {
            return (
                SPRITE_SIZE + 
                EditorGUIUtility.standardVerticalSpacing +
                EditorGUIUtility.standardVerticalSpacing
            );
        }

        private void DrawActorSpritesElement(Rect rect, int index, bool active, bool focus)
        {
            Rect objectRect = new Rect(
                rect.x,
                rect.y + EditorGUIUtility.standardVerticalSpacing,
                SPRITE_SIZE,
                SPRITE_SIZE
            );

            Rect typeRect = new Rect(
                objectRect.x + objectRect.width + EditorGUIUtility.standardVerticalSpacing,
                objectRect.y,
                rect.width - (objectRect.width + EditorGUIUtility.standardVerticalSpacing),
                EditorGUIUtility.singleLineHeight
            );

            Rect nameRect = new Rect(
                typeRect.x,
                typeRect.y + typeRect.height + EditorGUIUtility.standardVerticalSpacing,
                typeRect.width,
                typeRect.height
            );

            SerializedProperty spValue = this.spActorSpritesData.GetArrayElementAtIndex(index);
            SerializedProperty spName = spValue.FindPropertyRelative("name");
            SerializedProperty spType = spValue.FindPropertyRelative("type");
            
            SerializedProperty spSprite = spValue.FindPropertyRelative("sprite");
            SerializedProperty spTexture = spValue.FindPropertyRelative("texture");
            SerializedProperty spPrefab = spValue.FindPropertyRelative("prefab");

            EditorGUI.PropertyField(typeRect, spType);
            EditorGUI.PropertyField(nameRect, spName);

            switch (spType.intValue)
            {
                case (int)ActorSprites.DataType.Sprite:
                    EditorGUI.ObjectField(objectRect, spSprite, typeof(Sprite), GUIContent.none);
                    break;

                case (int)ActorSprites.DataType.Texture:
                    EditorGUI.ObjectField(objectRect, spTexture, typeof(Texture), GUIContent.none);
                    break;

                case (int)ActorSprites.DataType.Prefab:
                    EditorGUI.ObjectField(objectRect, spPrefab, typeof(GameObject), GUIContent.none);
                    break;
            }
        }

        private bool DrawActorSpritesCanAdd(ReorderableList list)
        {
            return true;
        }

        private void DrawActorSpritesAdd(ReorderableList list)
        {
            int index = (list.index > 0 ? list.index : list.count);
            list.serializedProperty.InsertArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
        }

        private bool DrawActorSpritesCanRemove(ReorderableList list)
        {
            return list.serializedProperty.arraySize > 0;
        }

        private void DrawActorSpritesRemove(ReorderableList list)
        {
            if (list.index < 0 || list.index >= list.count) return;
            list.serializedProperty.DeleteArrayElementAtIndex(list.index);
            serializedObject.ApplyModifiedProperties();
        }
    }
}