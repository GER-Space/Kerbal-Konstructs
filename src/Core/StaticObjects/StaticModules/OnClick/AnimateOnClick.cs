using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using KerbalKonstructs.Core;

namespace KerbalKonstructs
{
    public class AnimateOnClick : StaticModule
    {
        public string collider;
        public string animationName;
        //optional
        public bool HighlightOnHover = true;
        public float animationSpeed = 1f;

        public string playSound = "false";
        public string soundFile = "";
        public string soundMinDistance = "100";
        public string soundMaxDistance = "300";
        public string soundPlayAsLoopDuringAnimation = "false";


        private AudioSource audioPlayer;
        private bool useSound = false;
        private float soundFallOff = 100f;
        private float soundMaxDist = 300f;
        private bool soundAsLoop = false;

        private bool animationPlaying = false;
        private bool playAnimationForward = true;
        private Animation animationComponent;

        void Start()
        {
            if (animationComponent == null)
            {
                animationComponent = (from animationList in gameObject.GetComponentsInChildren<Animation>()
                                      where animationList != null
                                      from AnimationState animationState in animationList
                                      where animationState.name == animationName
                                      select animationList).FirstOrDefault();

                GameObject obj = (from t in gameObject.GetComponentsInChildren<Transform>()
                                  where t.gameObject != null && t.gameObject.name == collider
                                  select t.gameObject).FirstOrDefault();

                AnimateOnClick colliderObject = obj.AddComponent<AnimateOnClick>();
                colliderObject.collider = collider;
                colliderObject.animationName = animationName;
                colliderObject.HighlightOnHover = HighlightOnHover;
                colliderObject.animationSpeed = animationSpeed;
                colliderObject.animationComponent = animationComponent;

                if (!bool.TryParse(playSound, out colliderObject.useSound))
                {
                    Log.UserError("canot parse useSoundWithAnimation: " + playSound);
                }
                if (!bool.TryParse(soundPlayAsLoopDuringAnimation, out colliderObject.soundAsLoop))
                {
                    Log.UserError("canot parse soundAsLoop: " + soundPlayAsLoopDuringAnimation);
                }

                if (!float.TryParse(soundMinDistance, out colliderObject.soundFallOff))
                {
                    Log.UserError("canot parse soundAsLoop: " + soundPlayAsLoopDuringAnimation);
                }

                if (!float.TryParse(soundMaxDistance, out colliderObject.soundMaxDist))
                {
                    Log.UserError("canot parse soundAsLoop: " + soundPlayAsLoopDuringAnimation);
                }

                if (colliderObject.useSound)
                {
                    AudioClip audioClip = GameDatabase.Instance.GetAudioClip(soundFile);
                    if (audioClip == null)
                    {
                        Log.UserError("Could not find sound file: " + soundFile);
                    }
                    else
                    {
                        colliderObject.audioPlayer = gameObject.AddComponent<AudioSource>();

                        colliderObject.audioPlayer.clip = audioClip;
                        colliderObject.audioPlayer.minDistance = colliderObject.soundFallOff;
                        colliderObject.audioPlayer.maxDistance = colliderObject.soundMaxDist;
                        colliderObject.audioPlayer.loop = colliderObject.soundAsLoop;
                        colliderObject.audioPlayer.volume = 1;
                        colliderObject.audioPlayer.playOnAwake = false;
                        colliderObject.audioPlayer.spatialBlend = 1f;
                        colliderObject.audioPlayer.rolloffMode = AudioRolloffMode.Linear;
                    }
                }

                Destroy(this);
            }
        }

        void OnMouseDown()
        {
            if (!animationPlaying)
                StartCoroutine(playAnimation());
        }

        void OnMouseEnter()
        {
            if (HighlightOnHover)
            {
                gameObject.GetComponent<Renderer>().material.SetFloat("_RimFalloff", 2.5f);
                gameObject.GetComponent<Renderer>().material.SetColor("_RimColor", Color.green);
            }
        }

        void OnMouseExit()
        {
            if (HighlightOnHover)
            {
                gameObject.GetComponent<Renderer>().material.SetFloat("_RimFalloff", 2.5f);
                gameObject.GetComponent<Renderer>().material.SetColor("_RimColor", Color.clear);
            }
        }

        IEnumerator playAnimation()
        {
            if (useSound)
            {
                audioPlayer.Play();
            }
            animationPlaying = true;
            animationComponent[animationName].speed = playAnimationForward ? animationSpeed : -animationSpeed;
            animationComponent[animationName].normalizedTime = playAnimationForward ? 0 : 1;
            animationComponent.Play(animationName);
            yield return new WaitForSeconds(animationComponent[animationName].length / animationSpeed);
            playAnimationForward = !playAnimationForward;
            animationPlaying = false;
            if (useSound && soundAsLoop)
            {
                audioPlayer.Stop();
            }
        }
    }
}
