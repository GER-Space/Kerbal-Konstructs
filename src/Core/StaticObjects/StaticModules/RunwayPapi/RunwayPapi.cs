using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    public class RunwayPapi : StaticModule
    {
        internal enum GlideState
        {
            TooHigh,
            High,
            Right,
            TooLow
        }


        public string AnimNameTooHigh;
        public string AnimNameHigh;
        public string AnimNameRight;
        public string AminNameTooLow;

        public string TouchDownOffset;


        private float touchDownOffset;
        private static float maxDist = 3000f;
        private GlideState currentState = GlideState.Right;
        private GlideState lastState ;

        private double currentDistance ;
        private Vector3 touchDownPoint;

        private Animation animTooHigh;
        private Animation animHigh;
        private Animation animRight;
        private Animation animTooLow;

        // Yeh, it's a glide path of 3 degrees and a tolerance of 1.5


        void Start()
        {
            if (animTooHigh == null)
            {
                animTooHigh = (from animationList in gameObject.GetComponentsInChildren<Animation>()
                                      where animationList != null
                                      from AnimationState animationState in animationList
                                      where animationState.name == AnimNameTooHigh
                               select animationList).FirstOrDefault();
            }
            if (animHigh == null)
            {
                animHigh = (from animationList in gameObject.GetComponentsInChildren<Animation>()
                               where animationList != null
                               from AnimationState animationState in animationList
                               where animationState.name == AnimNameHigh
                            select animationList).FirstOrDefault();
            }
            if (animRight == null)
            {
                animRight = (from animationList in gameObject.GetComponentsInChildren<Animation>()
                               where animationList != null
                               from AnimationState animationState in animationList
                               where animationState.name == AnimNameRight
                             select animationList).FirstOrDefault();
            }
            if (animTooLow == null)
            {
                animTooLow = (from animationList in gameObject.GetComponentsInChildren<Animation>()
                               where animationList != null
                               from AnimationState animationState in animationList
                               where animationState.name == AminNameTooLow
                              select animationList).FirstOrDefault();
            }

        }


        public void Awake()
        {
            touchDownOffset = float.Parse(TouchDownOffset);
        }


        public void Update()
        {
            if (FlightGlobals.ActiveVessel.state != Vessel.State.ACTIVE)
            {
                return;
            }

            if (FlightGlobals.ActiveVessel.situation != Vessel.Situations.FLYING)
            {
                return;
            }

            touchDownPoint = staticInstance.gameObject.transform.position + (staticInstance.gameObject.transform.forward.normalized * touchDownOffset);
            currentDistance = (touchDownPoint - FlightGlobals.ActiveVessel.GetWorldPos3D()).magnitude;

            // Do nothing if too far away
            if (currentDistance > maxDist)
            {
                return;
            }

            currentState = GetCurrentGlideState();

            if (lastState != currentState)
            {
                lastState = currentState;

                switch (currentState)
                {
                    case GlideState.TooHigh:
                        animTooHigh.Play();
                        break;
                    case GlideState.High:
                        animHigh.Play();
                        break;
                    case GlideState.Right:
                        animRight.Play();
                        break;
                    case GlideState.TooLow:
                        animTooLow.Play();
                        break;
                }
            }
        }

        internal GlideState GetCurrentGlideState()
        {

            double runwayAltitude = staticInstance.CelestialBody.pqsController.GetSurfaceHeight(staticInstance.RadialPosition) - staticInstance.CelestialBody.Radius;
            double relativeAlt = FlightGlobals.ActiveVessel.altitude - runwayAltitude;

            double glideAngle = Math.Atan2(relativeAlt, currentDistance);

            if (glideAngle > 6f)
            {
                return GlideState.TooHigh;
            }
            if (glideAngle > 4.5f)
            {
                return GlideState.High;
            }
            if (glideAngle < 1.5f)
            {
                return GlideState.TooLow;
            }
            return GlideState.Right;
        }



    }
}
