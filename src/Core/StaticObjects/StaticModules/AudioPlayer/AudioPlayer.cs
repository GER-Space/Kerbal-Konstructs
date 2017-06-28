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

        public void Start()
        {
            AudioClip soundFile = GameDatabase.Instance.GetAudioClip(audioClip);

            if (soundFile == null)
            {
                Log.UserError("No audiofile found at: " + audioClip);
                return;
            }

            AudioSource audioPlayer = gameObject.AddComponent<AudioSource>();
            audioPlayer.clip = soundFile;
            audioPlayer.minDistance = minDistance;
            audioPlayer.maxDistance = maxDistance;
            audioPlayer.loop = loop;
            audioPlayer.volume = volume * KerbalKonstructs.soundMasterVolume;
            audioPlayer.playOnAwake = true;
            audioPlayer.Play();
        }
    }

}