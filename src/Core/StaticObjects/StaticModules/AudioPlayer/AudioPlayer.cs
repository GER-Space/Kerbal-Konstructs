using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;
using UnityEngine;


namespace KerbalKonstructs
{
    public class AudioPlayer : StaticModule
    {

        public string audioClip;
        public float minDistance = 1;
        public float maxDistance = 500;
        public bool loop = true;
        public float volume = 1;
        AudioSource audioPlayer = null;

        public void Start()
        {
            AudioClip soundFile = GameDatabase.Instance.GetAudioClip(audioClip);

            if (soundFile == null)
            {
                Log.UserError("No audiofile found at: " + audioClip);
                return;
            }

            float scale = InstanceUtil.GetStaticInstanceForGameObject(gameObject).ModelScale;


            audioPlayer = gameObject.AddComponent<AudioSource>();
            audioPlayer.clip = soundFile;
            audioPlayer.minDistance = minDistance * scale;
            audioPlayer.maxDistance = maxDistance * scale;
            audioPlayer.loop = loop;
            audioPlayer.volume = volume * KerbalKonstructs.soundMasterVolume;
            audioPlayer.playOnAwake = true;
            audioPlayer.spatialBlend = 1f;
            audioPlayer.rolloffMode = AudioRolloffMode.Linear;
            audioPlayer.Play();
        }

        public override void StaticObjectUpdate()
        {
            if (audioPlayer != null)
            {
                float scale = InstanceUtil.GetStaticInstanceForGameObject(gameObject).ModelScale;
                audioPlayer.minDistance = minDistance * scale;
                audioPlayer.maxDistance = maxDistance * scale;
            }
        }
    }
}