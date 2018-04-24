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
        private AnimationTrigger trigger;

        void Start()
        {
            if (animationComponent == null)
            {
                animationComponent = (from animationList in gameObject.GetComponentsInChildren<Animation>()
                                      where animationList != null
                                      from AnimationState animationState in animationList
                                      where animationState.name == animationName
                                      select animationList).FirstOrDefault();

                foreach (var transform in gameObject.GetComponentsInChildren<Transform>(true).Where(t => t.gameObject != null && t.gameObject.name == collider))
                {
                    trigger = transform.gameObject.AddComponent<AnimationTrigger>();
                    trigger.animateOnClick = this;
                }

                if (!bool.TryParse(playSound, out useSound))
                {
                    Log.UserError("canot parse useSoundWithAnimation: " + playSound);
                    useSound = false;
                }
                if (!bool.TryParse(soundPlayAsLoopDuringAnimation, out soundAsLoop))
                {
                    Log.UserError("canot parse soundPlayAsLoopDuringAnimation: " + soundPlayAsLoopDuringAnimation);
                }

                if (!float.TryParse(soundMinDistance, out soundFallOff))
                {
                    Log.UserError("canot parse soundMinDistance: " + soundMinDistance);
                }

                if (!float.TryParse(soundMaxDistance, out soundMaxDist))
                {
                    Log.UserError("canot parse soundMaxDistance: " + soundMaxDistance);
                }

                if (useSound)
                {
                    AudioClip audioClip = GameDatabase.Instance.GetAudioClip(soundFile);
                    if (audioClip == null)
                    {
                        Log.UserError("Could not find sound file: " + soundFile);
                        useSound = false;
                    }
                    else
                    {
                        audioPlayer = gameObject.AddComponent<AudioSource>();

                        audioPlayer.clip = audioClip;
                        audioPlayer.minDistance = soundFallOff;
                        audioPlayer.maxDistance = soundMaxDist;
                        audioPlayer.loop = soundAsLoop;
                        audioPlayer.volume = 1;
                        audioPlayer.playOnAwake = false;
                        audioPlayer.spatialBlend = 1f;
                        audioPlayer.rolloffMode = AudioRolloffMode.Linear;
                    }
                }
            }
        }

        public void PlayAnimation()
        {
            if (!animationPlaying)
                StartCoroutine(playAnimation());
        }


        public IEnumerator playAnimation()
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




    public class AnimationTrigger : MonoBehaviour
    {
        public AnimateOnClick animateOnClick;

        void OnMouseEnter()
        {
            if (animateOnClick.HighlightOnHover)
            {
                gameObject.GetComponent<Renderer>().material.SetFloat("_RimFalloff", 2.5f);
                gameObject.GetComponent<Renderer>().material.SetColor("_RimColor", Color.green);
            }
        }

        void OnMouseExit()
        {
            if (animateOnClick.HighlightOnHover)
            {
                gameObject.GetComponent<Renderer>().material.SetFloat("_RimFalloff", 2.5f);
                gameObject.GetComponent<Renderer>().material.SetColor("_RimColor", Color.clear);
            }
        }

        void OnMouseDown()
        {
            animateOnClick.PlayAnimation();
        }

    }


}
