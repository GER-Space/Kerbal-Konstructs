using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

namespace KerbalKonstructs.UI
{
	class AirRacing : KKWindow
    {
        private static AirRacing _instance = null;
        internal static AirRacing instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AirRacing();

                }
                return _instance;
            }
        }

        Rect racingRect = new Rect(40, 80, 320, 170);
		Rect orbitalRect = new Rect(40, 80, 420, 80);

		public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep3", false);

		public static Boolean runningRace = false;
		public Boolean racing = false;
		public Boolean started = false;
		public Boolean finished = false;
		public static Boolean basicorbitalhud = false;

		public float fNextGate = 1;
		public float fTimeMins = 0;
		public float fTimeSecs = 0;
		public float fDistToStart = 0f;
		public float fDistToFinish = 0f;
		public float fDistToGate = 0f;
		public float fDistBetween = 0;

		public double dStartTime = 0;
		public double dTimeSinceStart = 0;
		public double dFinishTime = 0;

		public StaticInstance StartLine = null;
		public StaticInstance FinishLine = null;

		GUIStyle raceStyle = new GUIStyle();
		GUIStyle BoxNoBorder;
		GUIStyle BoxNoBorderS;
		GUIStyle DeadButton;
		GUIStyle DeadButtonRed;

		public override void Draw()
		{
            if (MapView.MapIsEnabled)
            {
                return;
            }
			raceStyle.padding.top = 1;

			if (runningRace)
				racingRect = GUI.Window(0xC00C1E8, racingRect, drawRacingWindow, "", raceStyle);

			if (basicorbitalhud)
				orbitalRect = GUI.Window(0xC00C1E8, orbitalRect, drawRacingWindow, "", raceStyle);
		}

		public void drawRacingWindow(int windowID)
		{
			BoxNoBorder = new GUIStyle(GUI.skin.box);
			BoxNoBorder.normal.background = null;
			BoxNoBorder.normal.textColor = Color.white;

			BoxNoBorderS = new GUIStyle(GUI.skin.box);
			BoxNoBorderS.normal.background = null;
			BoxNoBorderS.normal.textColor = Color.white;
			BoxNoBorderS.fontSize = 13;

			DeadButton = new GUIStyle(GUI.skin.button);
			DeadButton.normal.background = null;
			DeadButton.hover.background = null;
			DeadButton.active.background = null;
			DeadButton.focused.background = null;
			DeadButton.normal.textColor = Color.white;
			DeadButton.hover.textColor = Color.white;
			DeadButton.active.textColor = Color.white;
			DeadButton.focused.textColor = Color.white;
			DeadButton.fontSize = 14;
			DeadButton.fontStyle = FontStyle.Bold;

			DeadButtonRed = new GUIStyle(GUI.skin.button);
			DeadButtonRed.normal.background = null;
			DeadButtonRed.hover.background = null;
			DeadButtonRed.active.background = null;
			DeadButtonRed.focused.background = null;
			DeadButtonRed.normal.textColor = Color.red;
			DeadButtonRed.hover.textColor = Color.yellow;
			DeadButtonRed.active.textColor = Color.red;
			DeadButtonRed.focused.textColor = Color.red;
			DeadButtonRed.fontSize = 12;
			DeadButtonRed.fontStyle = FontStyle.Bold;

			GUILayout.BeginHorizontal();
			{
				GUI.enabled = false;
				GUILayout.Button("-KK-", DeadButton, GUILayout.Height(16));

				GUILayout.FlexibleSpace();
				if (basicorbitalhud)
					GUILayout.Button("Orbital Data", DeadButton, GUILayout.Height(16));
				else
					GUILayout.Button("Air-Racing", DeadButton, GUILayout.Height(16));

				GUILayout.FlexibleSpace();

				GUI.enabled = true;

				if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(16)))
				{
					ResetRace();
                    this.Close();
                    runningRace = false;
					basicorbitalhud = false;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(1);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			if (basicorbitalhud)
			{
				string sBody = FlightGlobals.ActiveVessel.mainBody.name;
				double dPeriapsis = FlightGlobals.ActiveVessel.orbit.PeA;
				double dApoapsis = FlightGlobals.ActiveVessel.orbit.ApA;
				double dInclination = FlightGlobals.ActiveVessel.orbit.inclination;
				GUILayout.BeginHorizontal();
				{
					GUILayout.Box("SOI: " + sBody, BoxNoBorder, GUILayout.Width(75));
					GUILayout.FlexibleSpace();
					GUILayout.Box("Periapsis " + (dPeriapsis/1000).ToString("#0.0") + " km", BoxNoBorder, GUILayout.Width(90));
					GUILayout.FlexibleSpace();
					GUILayout.Box("Apoapsis " + (dApoapsis/1000).ToString("#0.0") + " km", BoxNoBorder, GUILayout.Width(90));
					GUILayout.FlexibleSpace();
					GUILayout.Box("Inclination " + dInclination.ToString("#0.00") + "°", BoxNoBorder, GUILayout.Width(80));
				}
				GUILayout.EndHorizontal();
			}
			else
			{

				if (!racing)
				{
					GUILayout.Box("Cross a start line to begin a race.");

					if (!started)
					{
						StartLine = GetNearestFacility(FlightGlobals.ActiveVessel.GetTransform().position, "RaceStart");
						if (StartLine != null)
						{
							fDistToStart = Vector3.Distance(StartLine.gameObject.transform.position, FlightGlobals.ActiveVessel.GetTransform().position);

							if (fDistToStart < 150)
							{
								dStartTime = Planetarium.GetUniversalTime();
								MiscUtils.HUDMessage("!!! GO GO GO !!!", 10, 0);
								started = true;
								racing = true;
							}

							if (fDistToStart < 200)
								GUILayout.Box("!!!!!! RACE ON! GO GO GO !!!!!!");

							if (fDistToStart >= 200)
								GUILayout.Box("Distance to Start Line: " + fDistToStart.ToString("#0.00") + " m");
						}
						else
						{
							GUILayout.Box("Cannot find any start lines.");
						}
					}
				}

				if (racing)
				{
					if (!finished && started)
					{
						dTimeSinceStart = Planetarium.GetUniversalTime() - dStartTime;
						fTimeMins = (int)dTimeSinceStart / 60;
						fTimeSecs = (float)(dTimeSinceStart - (fTimeMins * 60));

						GUILayout.Box("Race Time: " + fTimeMins.ToString("#0") + " minutes " + fTimeSecs.ToString("#0.00") + " seconds ");

						if (fNextGate > 0)
						{
							StaticInstance soNextGate = GetNextGate(StartLine, fNextGate);
							if (soNextGate != null)
							{
								GUILayout.Box("Next Gate: " + fNextGate);
								fDistToGate = GetDistanceToGate(soNextGate, FlightGlobals.ActiveVessel.GetTransform().position);

								fDistBetween = (GetGateWidth(soNextGate) / 2) + 10;

								if (fDistToGate > fDistBetween)
									GUILayout.Box("Distance to Next Gate: " + fDistToGate.ToString("#0.0") + " m");
								else
								{
									MiscUtils.HUDMessage("!!! GATE  " + fNextGate.ToString() + "  CLEAR !!!", 10, 0);
									fNextGate = fNextGate + 1;
								}
							}
							else fNextGate = 0;
						}
						else
						{
							GUILayout.Box("Final Stretch!");
						}

						if (fNextGate == 0)
						{
							FinishLine = GetNearestFacility(FlightGlobals.ActiveVessel.GetTransform().position, "RaceFinish");
							if (FinishLine != null)
							{
								fDistToFinish = Vector3.Distance(FinishLine.gameObject.transform.position,FlightGlobals.ActiveVessel.GetTransform().position);

								GUILayout.Box("Distance to Finish Line: " + fDistToFinish.ToString("#0.0") + " m");

								if (fDistToFinish < 150)
								{
									finished = true;
									started = false;
								}
							}
							else
							{
								GUILayout.Box("Cannot find any finish lines.");
							}
						}
					}

					if (finished)
					{
						GUILayout.Box("!!!!! FINISH LINE !!!!!");

						if (dFinishTime == 0)
						{
							dFinishTime = Planetarium.GetUniversalTime() - dStartTime;
							MiscUtils.HUDMessage("!!! FINISHED !!!", 10, 0);
						}

						fTimeMins = (int)dFinishTime / 60;
						fTimeSecs = (float)(dFinishTime - (fTimeMins * 60));

						GUILayout.Box("Race Time: " + fTimeMins.ToString("#0") + " minutes " + fTimeSecs.ToString("#0.00") + " seconds ");

						if (GUILayout.Button("Save Race Results", GUILayout.Height(22)))
						{
							MiscUtils.HUDMessage("Feature still WIP", 10, 3);
						}

						if (GUILayout.Button("Race Again!", GUILayout.Height(22))) ResetRace();
					}
				}

				GUILayout.Space(5);
				if (GUILayout.Button("I'm done racing!", GUILayout.Height(22)))
				{
					ResetRace();
                    this.Close();
                    runningRace = false;
                    return;
				}
			}

			GUILayout.FlexibleSpace();
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
			GUILayout.Space(3);

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public StaticInstance GetNextGate(StaticInstance StartLine, float fNextGate)
		{
			StaticInstance soNextGate = null;
			string sGate = "Gate" + fNextGate.ToString();
			string sGroup = StartLine.Group;

			soNextGate = GetNearestFacility(StartLine.gameObject.transform.position, sGate, sGroup);

			return soNextGate;
		}

		public float GetDistanceToGate(StaticInstance soGate, Vector3 vPos)
		{
			float fDistance = 0f;
			Vector3 vCenter = new Vector3(0, 0, 0);
			StaticInstance soNearestPole = null;

			string sGroup = soGate.Group;
			string sFacType = soGate.legacyfacilityID;
			soNearestPole = GetNearestFacility(soGate.gameObject.transform.position, sFacType + "P", sGroup);

			vCenter = Vector3.Lerp(soGate.gameObject.transform.position, soNearestPole.gameObject.transform.position, 0.5f);

			fDistance = Vector3.Distance(vCenter, vPos);
			return fDistance;
		}

		public float GetGateWidth(StaticInstance soGate)
		{
			float fDistance = 0f;
			StaticInstance soNearestPole = null;

			string sGroup = soGate.Group;
			string sFacType = soGate.legacyfacilityID;
			soNearestPole = GetNearestFacility(soGate.gameObject.transform.position, sFacType + "P", sGroup);

			fDistance = Vector3.Distance(soGate.gameObject.transform.position, soNearestPole.gameObject.transform.position);
			return fDistance;
		}

		public void ResetRace()
		{
			racing = false;
			fNextGate = 1;
			fTimeMins = 0;
			fTimeSecs = 0;
			dStartTime = 0;
			dTimeSinceStart = 0;
			dFinishTime = 0;
			finished = false;
			started = false;
		}

        public static StaticInstance GetNearestFacility(Vector3 vPosition, string sFacilityType, string sGroup = "None")
        {
            StaticInstance soFacility = null;

            float fLastDist = 100000000f;
            float fDistance = 0f;
            float fNearest = 0f;

            foreach (StaticInstance instance in StaticDatabase.allStaticInstances)
            {
                if (sGroup != "None")
                {
                    if (instance.Group != sGroup) continue;
                }

                if (instance.legacyfacilityID == sFacilityType)
                {
                    fDistance = Vector3.Distance(instance.gameObject.transform.position, vPosition);

                    if (fDistance < fLastDist)
                    {
                        fNearest = fDistance;
                        soFacility = instance;
                    }

                    fLastDist = fDistance;
                }
                else continue;
            }

            return soFacility;
        }

    }
}
