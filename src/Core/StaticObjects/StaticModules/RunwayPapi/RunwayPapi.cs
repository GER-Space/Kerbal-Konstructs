using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KerbalKonstructs
{
    public class RunwayPapi : StaticModule
    {
        internal enum GlideState
        {
            TooHigh,
            High,
            Right,
            Low,
            TooLow,
            Off
        }


        public string AnimNameTooHigh;
        public string AnimNameHigh;
        public string AnimNameRight;
        public string AminNameTooLow;

        public string TouchDownOffset;

        public string ReverseDirections = "false";

        public string ShowDebugVectors = "false";

        private float touchDownOffset = 0f;
        private static float maxDist = 5000f;
        private GlideState currentState = GlideState.Off;
        private GlideState lastState = GlideState.Off;

        private double currentDistance;
        private double glideAngle;
        private Vector3 touchDownPoint;
        private Vector3 horizontalVector;

        private Animation animTooHigh;
        private Animation animHigh;
        private Animation animRight;
        private Animation animLow;

        private bool showDebug = false;
        private Vector3d vesselPosition;
        private Vector3 fromVesseltoPoint;

        private bool isreverse = false;
        private int directionsMult = 1;

        private Dictionary<string, bool> isWhite = new Dictionary<string, bool>();

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
                //    animTooHigh.wrapMode = WrapMode.Loop;
                isWhite.Add(AnimNameTooHigh, false);
            }

            if (animHigh == null)
            {
                animHigh = (from animationList in gameObject.GetComponentsInChildren<Animation>()
                            where animationList != null
                            from AnimationState animationState in animationList
                            where animationState.name == AnimNameHigh
                            select animationList).FirstOrDefault();
                //    animHigh.wrapMode = WrapMode.Loop;
                isWhite.Add(AnimNameHigh, false);
            }
            if (animRight == null)
            {
                animRight = (from animationList in gameObject.GetComponentsInChildren<Animation>()
                             where animationList != null
                             from AnimationState animationState in animationList
                             where animationState.name == AnimNameRight
                             select animationList).FirstOrDefault();
                //    animRight.wrapMode = WrapMode.Loop;
                isWhite.Add(AnimNameRight, false);
            }
            if (animLow == null)
            {
                animLow = (from animationList in gameObject.GetComponentsInChildren<Animation>()
                           where animationList != null
                           from AnimationState animationState in animationList
                           where animationState.name == AminNameTooLow
                           select animationList).FirstOrDefault();
                //    animLow.wrapMode = WrapMode.Loop;
                isWhite.Add(AminNameTooLow, false);
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
            try
            {
                touchDownOffset = float.Parse(TouchDownOffset);
            }
            catch
            {
                touchDownOffset = 0;
                Log.Normal("Could not Parse touchDownOffset");
            }
            if (!bool.TryParse(ShowDebugVectors, out showDebug))
            {
                Log.UserWarning("PAPI Module: could not parse ShowDebugVectors to bool: " + ShowDebugVectors);
            }

            if (!bool.TryParse(ReverseDirections, out isreverse))
            {
                Log.UserWarning("PAPI Module: could not parse ReverseDirections to bool: " + ShowDebugVectors);
            }
            if (isreverse)
            {
                directionsMult = -1;
                Log.Normal("Reverse is activated");
            }

        }


        public void Update()
        {
            // dont do anything when its not in flight
            if (HighLogic.LoadedScene != GameScenes.FLIGHT)
            {
                SetAllOff();
                return;
            }

            // hmm no vessel found.. thats bad because you are not flying... 
            if (FlightGlobals.ActiveVessel == null)
            {
                SetAllOff();
                return;
            }

            // the vessel is not active?!? we don't deal with such alien spacecraft. 
            if (FlightGlobals.ActiveVessel.state != Vessel.State.ACTIVE)
            {
                SetAllOff();
                return;
            }

            // Only deal with flying things.
            if (FlightGlobals.ActiveVessel.situation != Vessel.Situations.FLYING)
            {
                SetAllOff();
                return;
            }

            // from here it should be save to do acually some things.

            touchDownPoint = staticInstance.position + (directionsMult * staticInstance.transform.forward.normalized * touchDownOffset);
            vesselPosition = FlightGlobals.ActiveVessel.GetWorldPos3D();
            fromVesseltoPoint = (touchDownPoint - FlightGlobals.ActiveVessel.GetWorldPos3D());
            horizontalVector = Vector3.ProjectOnPlane(fromVesseltoPoint, staticInstance.transform.up);

            if (Vector3d.Dot(horizontalVector, directionsMult * staticInstance.transform.forward.normalized) < 0)
            {
                // we are behind the lights. no need to update them anymore.
                SetAllOff();
                return;
            }

            currentDistance = horizontalVector.magnitude;

            // Do nothing if too far away
            if (currentDistance > maxDist)
            {
                SetAllOff();
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
                //Log.Normal("PAPI: Switching State: " + lastState.ToString() + " to " + currentState.ToString());
                lastState = currentState;

                switch (currentState)
                {
                    case GlideState.TooHigh:
                        if (animTooHigh != null)
                        {
                            SetWhite(animLow, AminNameTooLow);
                            SetWhite(animRight, AnimNameRight);
                            SetWhite(animHigh, AnimNameHigh);
                            SetWhite(animTooHigh, AnimNameTooHigh);
                        }

                        break;
                    case GlideState.High:
                        if (animHigh != null)
                        {
                            SetWhite(animLow, AminNameTooLow);
                            SetWhite(animRight, AnimNameRight);
                            SetWhite(animHigh, AnimNameHigh);
                            SetRed(animTooHigh, AnimNameTooHigh);
                        }

                        break;
                    case GlideState.Right:
                        if (animRight != null)
                        {
                            SetWhite(animLow, AminNameTooLow);
                            SetWhite(animRight, AnimNameRight);
                            SetRed(animHigh, AnimNameHigh);
                            SetRed(animTooHigh, AnimNameTooHigh);
                        }

                        break;
                    case GlideState.Low:
                        if (animLow != null)
                        {
                            SetWhite(animLow, AminNameTooLow);
                            SetRed(animRight, AnimNameRight);
                            SetRed(animHigh, AnimNameHigh);
                            SetRed(animTooHigh, AnimNameTooHigh);

                        }
                        break;
                    case GlideState.TooLow:
                        if (animLow != null)
                        {
                            SetRed(animLow, AminNameTooLow);
                            SetRed(animRight, AnimNameRight);
                            SetRed(animHigh, AnimNameHigh);
                            SetRed(animTooHigh, AnimNameTooHigh);
                        }
                        break;
                }
            }
        }


        internal void SetAllOff()
        {
            currentState = GlideState.Off;
            if (lastState != currentState)
            {
                SetRed(animLow, AminNameTooLow);
                SetRed(animRight, AnimNameRight);
                SetRed(animHigh, AnimNameHigh);
                SetRed(animTooHigh, AnimNameTooHigh);
            }
            lastState = currentState;
        }


        internal void SetRed(Animation anim, string animationName)
        {
            if (isWhite[animationName])
            {
                anim[animationName].speed = -1f;
                anim[animationName].normalizedTime = 1f;
                isWhite[animationName] = false;
                anim.Play();
            }
        }

        internal void SetWhite(Animation anim, string animationName)
        {
            if (!isWhite[animationName])
            {
                anim[animationName].speed = 1f;
                anim[animationName].normalizedTime = 0f;
                isWhite[animationName] = true;
                anim.Play();
            }
        }



        internal GlideState GetCurrentGlideState()
        {
            glideAngle = Mathf.Rad2Deg * Math.Acos(horizontalVector.magnitude / fromVesseltoPoint.magnitude);

            //Log.NoSpam("PAPI: Glide Angle: " + Math.Round(glideAngle,1));

            if (glideAngle > 6f)
            {
                return GlideState.TooHigh;
            }
            if (glideAngle > 4f && glideAngle < 6f)
            {
                return GlideState.High;
            }
            if (glideAngle < 2.5f && glideAngle >= 2f)
            {
                return GlideState.Low;
            }
            if (glideAngle < 2f)
            {
                return GlideState.TooLow;
            }
            return GlideState.Right;
        }

    }
}
