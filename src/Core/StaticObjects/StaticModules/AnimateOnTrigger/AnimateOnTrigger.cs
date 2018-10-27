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
        internal enum VesselState
        {
            outside,
            inside
        }

        private Dictionary<Vessel, bool> vesselIsInside = new Dictionary<Vessel, bool>();

        public string ColliderNames = "";
        public string AnimOnEnter = "";
        public string AnimOnStay = "";
        public string AnimOnExit = "";


        private Animation enterAnim;
        private Animation stayAnim;
        private Animation exitAnim;

        private List<string> colNames = new List<string>();
        private List<Collider> colliders = new List<Collider>();

        private string[] seperators = new string[] { " ", ",", ";" };


        private bool isPlaying = false;

        private List<KKCallBackWorker> workers = new List<KKCallBackWorker>();


        internal void Start()
        {

            enterAnim = (from animationList in gameObject.GetComponentsInChildren<Animation>()
                         where animationList != null
                         from AnimationState animationState in animationList
                         where animationState.name == AnimOnEnter
                         select animationList).FirstOrDefault();

            stayAnim = (from animationList in gameObject.GetComponentsInChildren<Animation>()
                        where animationList != null
                        from AnimationState animationState in animationList
                        where animationState.name == AnimOnStay
                        select animationList).FirstOrDefault();

            exitAnim = (from animationList in gameObject.GetComponentsInChildren<Animation>()
                        where animationList != null
                        from AnimationState animationState in animationList
                        where animationState.name == AnimOnExit
                        select animationList).FirstOrDefault();


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
            Log.Normal("AniMateOnTrigger: Adding KKCallBack to GameObject");
            KKCallBack kkCall = gameObject.AddComponent<KKCallBack>();
            kkCall.ColliderNames = ColliderNames;

            if (!kkCall.isSetup)
            {
                kkCall.Start();
            }

            foreach (KKCallBackWorker worker in gameObject.GetComponentsInChildren<KKCallBackWorker>(true))
            {
                workers.Add(worker);
                worker.onEnterAction = PlayOnEnter;
                worker.onExitAction = PlayOnExit;
                worker.onStayAction = PlayOnStay;
            }
        }


        // called by OnTriggerEnter
        internal void PlayOnEnter(Part myPart)
        {
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

            if (!isPlaying)
            {
                vesselIsInside[myPart.vessel] = true;
                PlayAnim(enterAnim, AnimOnEnter);
            }
        }

        internal void PlayOnExit(Part myPart)
        {
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

            if (!isPlaying)
            {
                vesselIsInside[myPart.vessel] = true;
                PlayAnim(exitAnim, AnimOnExit);
            }
        }

        internal void PlayOnStay(Part myPart)
        {
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

            if (!isPlaying)
            {
                vesselIsInside[myPart.vessel] = true;
                PlayAnim(stayAnim, AnimOnStay);
            }
        }


        internal void PlayAnim(Animation anim, string animationName)
        {
            isPlaying = true;
            anim[animationName].speed = 1f;
            anim[animationName].normalizedTime = 0f;
            anim.Play();
            isPlaying = false;
        }
    }




}
