using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KerbalKonstructs.Utilities;
using KerbalKonstructs.Core;

namespace KerbalKonstructs
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
        public string ShowDebugVectors = "false";

        private float touchDownOffset;
        private static float maxDist = 3000f;
        private GlideState currentState = GlideState.Right;
        private GlideState lastState;

        private double currentDistance;
        private double glideAngle;
        private Vector3 touchDownPoint;
        private Vector3 horizontalVector;

        private Animation animTooHigh;
        private Animation animHigh;
        private Animation animRight;
        private Animation animTooLow;

        private bool showDebug = false;
        private Vector3d vesselPosition;
        private Vector3 fromVesseltoPoint;

        // Yeh, it's a glide path of 3 degrees and a tolerance of 1.5


        void Start()
        {
            //Log.Normal("RunWay PAPI started: " + gameObject.name);
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


        //public override void StaticObjectUpdate()
        //{
        //    //Log.Normal("FlagDeclal: Set Texture Called");
        //    Log.NoSpam("RunWay PAPI StaticObjectUpdate: " + gameObject.name);
        //    this.enabled = true;
        //}

        public void Awake()
        {
            touchDownOffset = float.Parse(TouchDownOffset);
            if (!bool.TryParse(ShowDebugVectors, out showDebug))
            {
                Log.UserWarning("PAPI Module: could not parse ShowDebugVectors to bool: " + ShowDebugVectors);
            }
        }


        public void Update()
        {
            // dont do anything when its not in flight
            if (HighLogic.LoadedScene != GameScenes.FLIGHT)
            {
                return;
            }

            // hmm no vessel found.. thats bad because you are not flying... 
            if (FlightGlobals.ActiveVessel == null)
            {
                return; 
            }

            // the vessel is not active?!? we don't deal with such alien spacecraft. 
            if (FlightGlobals.ActiveVessel.state != Vessel.State.ACTIVE)
            {
                return;
            }

            // Only deal with flying things.
            if (FlightGlobals.ActiveVessel.situation != Vessel.Situations.FLYING)
            {
                return;
            }

            // from here it should be save to do acually some things.

            touchDownPoint = staticInstance.gameObject.transform.position + (staticInstance.gameObject.transform.forward.normalized * touchDownOffset);
            vesselPosition = FlightGlobals.ActiveVessel.GetWorldPos3D();
            fromVesseltoPoint = touchDownPoint - FlightGlobals.ActiveVessel.GetWorldPos3D();
            horizontalVector = Vector3.ProjectOnPlane(fromVesseltoPoint, staticInstance.gameObject.transform.up);

            if (Vector3d.Dot(horizontalVector, staticInstance.gameObject.transform.forward) < 0)
            {
                // we are behind the lights. no need to update them anymore.
                return;
            }

            currentDistance = horizontalVector.magnitude;

            // Do nothing if too far away
            if (currentDistance > maxDist)
            {
                return;
            }

            if (showDebug)
            {
                DebugDrawer.DebugVector(touchDownPoint, -horizontalVector, new Color(0.2f, 0.2f, 0.7f));
                DebugDrawer.DebugLine(touchDownPoint, vesselPosition, new Color(0.2f, 0.7f, 0.2f));
            }

            currentState = GetCurrentGlideState();

            if (lastState != currentState)
            {
                Log.Normal("PAPI: Switching State: " + lastState.ToString() + " to " + currentState.ToString());
                lastState = currentState;

                switch (currentState)
                {
                    case GlideState.TooHigh:
                        if (animTooHigh != null)
                            animTooHigh.Play();
                        break;
                    case GlideState.High:
                        if (animHigh != null)
                            animHigh.Play();
                        break;
                    case GlideState.Right:
                        if (animRight != null)
                            animRight.Play();
                        break;
                    case GlideState.TooLow:
                        if (animTooLow != null)
                            animTooLow.Play();
                        break;
                }
            }
        }

        internal GlideState GetCurrentGlideState()
        {
            glideAngle =  Mathf.Rad2Deg * Math.Acos(horizontalVector.magnitude/fromVesseltoPoint.magnitude);

            //Log.NoSpam("PAPI: Glide Angle: " + Math.Round(glideAngle,1));

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
