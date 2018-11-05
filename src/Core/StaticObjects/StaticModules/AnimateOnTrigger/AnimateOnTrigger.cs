using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;
using UnityEngine;

namespace KerbalKonstructs
{
    class AnimateOnTrigger : StaticModule
    {

        public string ColliderNames = "";
        public string AnimOnEnter = "";
        public string AnimOnStay = "";
        public string AnimOnExit = "";

        private Dictionary<Vessel, bool> vesselIsInside = new Dictionary<Vessel, bool>();

        private Animation enterAnim;
        private Animation stayAnim;
        private Animation exitAnim;

        private List<string> colNames = new List<string>();

        private string[] seperators = new string[] { " ", ",", ";" };


        internal void Start()
        {
            if (!String.IsNullOrEmpty(AnimOnEnter))
            {
                enterAnim = (from animationList in gameObject.GetComponentsInChildren<Animation>()
                             where animationList != null
                             from AnimationState animationState in animationList
                             where animationState.name == AnimOnEnter
                             select animationList).FirstOrDefault();
            }
            if (!String.IsNullOrEmpty(AnimOnStay))
            {
                stayAnim = (from animationList in gameObject.GetComponentsInChildren<Animation>()
                            where animationList != null
                            from AnimationState animationState in animationList
                            where animationState.name == AnimOnStay
                            select animationList).FirstOrDefault();
            }
            if (!String.IsNullOrEmpty(AnimOnExit))
            {
                exitAnim = (from animationList in gameObject.GetComponentsInChildren<Animation>()
                            where animationList != null
                            from AnimationState animationState in animationList
                            where animationState.name == AnimOnExit
                            select animationList).FirstOrDefault();
            }

            colNames = ColliderNames.Split(seperators, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (colNames.Count == 0)
            {
                Log.UserError("No trigger colliders found");
                return;
            }

            AddColliderTriggers();
        }


        internal void AddColliderTriggers()
        {

            //KKCallBack kkCall = gameObject.GetComponentInChildren<KKCallBack>(true);
            KKCallBack kkCall = gameObject.AddComponent<KKCallBack>();
            kkCall.ColliderNames = ColliderNames;

            if (!kkCall.isSetup)
            {
                kkCall.Start();
            }

            foreach (KKCallBackWorker worker in gameObject.GetComponentsInChildren<KKCallBackWorker>(true))
            {
                worker.onEnterAction = PlayOnEnter;
                worker.onExitAction = PlayOnExit;
                if (stayAnim != null)
                {
                    worker.onStayAction = PlayOnStay;
                }
            }
        }


        // called by OnTriggerEnter
        internal void PlayOnEnter(Part myPart)
        {
            if (myPart == null)
            {
                return;
            }

            if (myPart != myPart.vessel.rootPart)
            {
                return;
            }


            if (!vesselIsInside.ContainsKey(myPart.vessel))
            {
                vesselIsInside.Add(myPart.vessel, false);
            }

            if (vesselIsInside[myPart.vessel])
            {
                return;
            }

            vesselIsInside[myPart.vessel] = true;
            PlayAnim(enterAnim, AnimOnEnter);
        }

        internal void PlayOnExit(Part myPart)
        {
            if (myPart == null)
            {
                return;
            }
            if (myPart != myPart.vessel.rootPart)
            {
                return;
            }
            if (!vesselIsInside.ContainsKey(myPart.vessel))
            {
                return;
            }

            if (!vesselIsInside[myPart.vessel])
            {
                return;
            }


            vesselIsInside[myPart.vessel] = false;
            PlayAnim(exitAnim, AnimOnExit);

        }

        internal void PlayOnStay(Part myPart)
        {
            if (myPart == null)
            {
                return;
            }
            if (myPart != myPart.vessel.rootPart)
            {
                return;
            }
            if (!vesselIsInside.ContainsKey(myPart.vessel))
            {
                return;
            }

            if (!vesselIsInside[myPart.vessel])
            {
                return;
            }


            vesselIsInside[myPart.vessel] = true;
            PlayAnim(stayAnim, AnimOnStay);

        }


        internal void PlayAnim(Animation anim, string animationName)
        {
            if (anim == null || String.IsNullOrEmpty(animationName))
            {
                return;
            }

            if (!anim.IsPlaying(animationName))
            {
                anim.Rewind(animationName);
                anim[animationName].speed = 1f;
                anim[animationName].normalizedTime = 0f;
                anim.Play();
            }
            else
            {
                //Log.Normal("Animation still playing: " + animationName);
            }
        }
    }




}
