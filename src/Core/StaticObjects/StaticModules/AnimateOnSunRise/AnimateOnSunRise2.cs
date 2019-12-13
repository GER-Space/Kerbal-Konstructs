using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KerbalKonstructs.Core;

namespace KerbalKonstructs
{ 
    class AnimateOnSunRise2 : StaticModule
    {

        // Public .cfg fields
        public string animationName;
        public string reverseAnimation = "false";
        public string timeWarpAnimation = "true";
        // Very optional :
        public string delayLowTimeWarp = "2";
        public string delayHighTimeWarp = "0.1";


        private bool isActive = false;
        private bool shouldBeActive = false;

        private bool isRunning = false;
        private bool isInitialized = false;

        private bool revAnimation = false;
        private bool warpAnimation = true;
        private float lowWarpDelay = 2f;
        private float highWarpDelay = 0.1f;

        private Animation animationComponent;

        private float animLength;
        private float animationSpeed;

        private float lastStartTime = 0;

        private void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;

            foreach (Animation anim in staticInstance.mesh.GetComponentsInChildren<Animation>(true))
            {
                if (anim[animationName] != null)
                {
                    animationComponent = anim;
                    break;
                }
            }
            if (animationComponent == null)
            {
                foreach (Animation anim in staticInstance.mesh.GetComponents<Animation>())
                {
                    if (anim[animationName] != null)
                    {
                        animationComponent = anim;
                        break;
                    }
                }
            }
            if (animationComponent == null)
            {
                Log.UserError("AnimateOnSunRise: no anim found: \"" + animationName + "\", on: " + staticInstance.model.name);
                GameObject.DestroyImmediate(this);
                return;
            }

            animLength = animationComponent[animationName].length * animationComponent[animationName].normalizedSpeed;
            animationSpeed = animationComponent[animationName].speed;

            revAnimation = bool.Parse(reverseAnimation);
            warpAnimation = bool.Parse(timeWarpAnimation);
            lowWarpDelay = float.Parse(delayLowTimeWarp);
            highWarpDelay = float.Parse(delayHighTimeWarp);
            ResetAnimationState();
        }



        public void Update()
        {
            if (!isInitialized)
            {
                Initialize();
            }

            if (isActive == shouldBeActive)
            {
                return;
            } else
            {
                if (shouldBeActive)
                {
                    SetActive();
                }
                else
                {
                    SetInActive();
                }
            }
        }



        internal void OnSunriseActivate()
        {
            shouldBeActive = true;
        }


        internal void OnSunriseDeActivate()
        {
            shouldBeActive = false;
        }



        private void ResetAnimationState()
        {
            isRunning = false;
            isActive = false;

            if (revAnimation)
            {
                animationComponent[animationName].normalizedTime = 1f;
            }
            else
            {
                animationComponent[animationName].time = 0;
            }
        }



        private IEnumerator SetActive()
        {
            if (isRunning)
            {
                yield return null;
            }

            isRunning = true;

            float waitTime = animLength;

            if (revAnimation)
            {
                animationComponent[animationName].speed = -animationSpeed;
                animationComponent[animationName].normalizedTime = 1f;
            }
            else
            {
                animationComponent[animationName].speed = animationSpeed;
                animationComponent[animationName].time = 0;
            }

            if (warpAnimation)
            {
                animationComponent[animationName].speed *= TimeWarp.CurrentRate;
                waitTime /= TimeWarp.CurrentRate;
            }

            animationComponent.Play(animationName);
            isActive = true;
            yield return new WaitForSeconds(waitTime);
            isRunning = false;
        }

        private IEnumerator SetInActive()
        {
            if (isRunning)
            {
                yield return null;
            }
            isRunning = true;

            float waitTime = animLength;

            if (revAnimation)
            {
                animationComponent[animationName].speed = animationSpeed;
                animationComponent[animationName].time = 0;
            }
            else
            {
                animationComponent[animationName].speed = -animationSpeed;
                animationComponent[animationName].normalizedTime = 1f;
            }

            if (warpAnimation)
            {
                animationComponent[animationName].speed *= TimeWarp.CurrentRate;
                waitTime /= TimeWarp.CurrentRate;
            }

            animationComponent.Play(animationName);
            isActive = false;
            yield return new WaitForSeconds(waitTime);
            isRunning = false;
        }



    }
}
