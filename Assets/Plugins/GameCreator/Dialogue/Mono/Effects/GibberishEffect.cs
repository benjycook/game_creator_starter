namespace GameCreator.Dialogue
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Audio;
    using GameCreator.Core;

    public class GibberishEffect
    {
        private const string GIBBERISH_RESOURCE = "GameCreator/DefaultGibberish";
        private const float MIN_SPACE = 0.005f;
        private const int MAX_BUFFERS = 5;

        private static int BUFFER_INDEX = 0;
        private static AudioBuffer[] BUFFERS = new AudioBuffer[MAX_BUFFERS];

        // PROPERTIES: ----------------------------------------------------------------------------

        private float lastGibberTime = -100f;
        private int lastVisibleCharacters = -1;

        private AudioClip audioClip;
        private float pitch = 1f;
        private float variation = 0f;

        // INITIALIZERS: --------------------------------------------------------------------------

        public GibberishEffect(AudioClip audioClip, float pitch, float variation)
        {
            this.audioClip = audioClip;
            if (this.audioClip == null)
            {
                this.audioClip = Resources.Load<AudioClip>(GIBBERISH_RESOURCE);
            }

            this.pitch = pitch;
            this.variation = variation;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        public void Gibber(int visibleCharacters)
        {
            if (Time.time < this.lastGibberTime + MIN_SPACE) return;
            if (this.lastVisibleCharacters == visibleCharacters) return;

            this.RequireAudio();

            float volume = AudioManager.Instance.GetGlobalVoiceVolume();
            AudioMixerGroup voiceMixer = DatabaseGeneral.Load().voiceAudioMixer;

            BUFFERS[BUFFER_INDEX].SetPitch(this.pitch + Random.Range(0.0f, this.variation));
            BUFFERS[BUFFER_INDEX].Play(this.audioClip, 0f, volume, voiceMixer);

            BUFFER_INDEX = (++BUFFER_INDEX >= MAX_BUFFERS ? 0 : BUFFER_INDEX);

            this.lastGibberTime = Time.time;
            this.lastVisibleCharacters = visibleCharacters;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void RequireAudio()
        {
            if (BUFFERS[BUFFER_INDEX] != null) return;

            GameObject source = new GameObject("gibberish_" + (BUFFER_INDEX + 1));
            source.transform.SetParent(AudioManager.Instance.transform);

            BUFFERS[BUFFER_INDEX] = new AudioBuffer(
                source.AddComponent<AudioSource>(),
                AudioManager.INDEX_VOLUME_VOICE
            );
        }
    }
}