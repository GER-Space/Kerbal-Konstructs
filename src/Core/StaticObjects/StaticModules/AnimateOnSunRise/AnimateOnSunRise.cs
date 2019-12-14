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
    class AnimateOnSunRise : StaticModule
    {

        // Public .cfg fields
        public string animationName;
        public string reverseAnimation = "false";
        public string timeWarpAnimation = "true";
        // Very unused :
        public string delayLowTimeWarp = "2";
        public string delayHighTimeWarp = "0.1";


        private bool isActive = false;
        private bool shouldBeActive = false;

        private bool isRunning = false;
        private bool isInitialized = false;

        private bool revAnimation = false;
        private bool warpAnimation = true;


        private Animation animationComponent;
        private AnimationState targetAnimation;


        private float animLength;
        private float animationSpeed;

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
                    targetAnimation = anim[animationName];
                    animationComponent = anim;
                    
                    break;
                }
            }
            if (targetAnimation == null)
            {
                foreach (Animation anim in staticInstance.mesh.GetComponents<Animation>())
                {
                    if (anim[animationName] != null)
                    {
                        animationComponent = anim;
                        targetAnimation = anim[animationName];
                        break;
                    }
                }
            }
            if (targetAnimation == null)
            {
                Log.UserError("AnimateOnSunRise: no anim found: \"" + animationName + "\", on: " + staticInstance.model.name);
                GameObject.DestroyImmediate(this);
                return;
            }

            animLength = targetAnimation.length * targetAnimation.normalizedSpeed;
            animationSpeed = targetAnimation.speed;

            revAnimation = bool.Parse(reverseAnimation);
            warpAnimation = bool.Parse(timeWarpAnimation);

            StartCoroutine("ResetAnimationState");

            SetupCenter();

        }

        internal void SetupCenter()
        {

            if (staticInstance.groupCenter.gameObject.GetComponent<OnSunRiseMaster>() != null)
            {
                // Allready setup
                return;
            }

            OnSunRiseMaster master = staticInstance.groupCenter.gameObject.AddComponent<OnSunRiseMaster>();
            master.center = staticInstance.groupCenter;


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
            } 
            else
            {
                if (shouldBeActive)
                {
                    this.StartCoroutine("SetActive");
                }
                else
                {
                    this.StartCoroutine("SetInActive");
                }
            }
        }



        internal void OnSunriseActivate()
        {
            //Log.Normal("Called");
            shouldBeActive = true;
        }


        internal void OnSunriseDeActivate()
        {
            //Log.Normal("Called");
            shouldBeActive = false;
        }


        private IEnumerator ResetAnimationState()
        {
            isRunning = true;
            float waitTime = animLength;

            if (revAnimation)
            {
                targetAnimation.speed = animationSpeed;
                targetAnimation.time = 0;
            }
            else
            {
                targetAnimation.speed = -animationSpeed;
                targetAnimation.time = animLength;
            }

                targetAnimation.speed *= 1000f;
                waitTime /= 1000f;

            animationComponent.Play(animationName);
            isActive = false;
            yield return new WaitForSeconds(waitTime);
            isRunning = false;

        }



        private IEnumerator SetActive()
        {
            //Log.Normal("first");
            if (isRunning)
            {
                //Log.Normal("Still running");
                yield return null;
            }
            //Log.Normal("playing");
            isRunning = true;

            float waitTime = animLength;

            if (revAnimation)
            {
                targetAnimation.speed = -animationSpeed;
                targetAnimation.time = animLength;
            }
            else
            {
                targetAnimation.speed = animationSpeed;
                targetAnimation.time = 0;
            }

            if (warpAnimation)
            {
                targetAnimation.speed *= TimeWarp.CurrentRate;
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
                targetAnimation.speed = animationSpeed;
                targetAnimation.time = 0;
            }
            else
            {
                targetAnimation.speed = -animationSpeed;
                targetAnimation.time = animLength;
            }

            if (warpAnimation)
            {
                targetAnimation.speed *= TimeWarp.CurrentRate;
                waitTime /= TimeWarp.CurrentRate;
            }

            animationComponent.Play(animationName);
            isActive = false;
            yield return new WaitForSeconds(waitTime);
            isRunning = false;
        }

    }
}
