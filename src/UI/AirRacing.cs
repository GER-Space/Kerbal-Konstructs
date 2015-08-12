using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using KerbalKonstructs.API;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using LibNoise.Unity.Operator;
using UnityEngine;
using System.Linq;
using System.IO;

namespace KerbalKonstructs.UI
{
	public class AirRacing
	{
		Rect racingRect = new Rect(40, 80, 300, 160);

		public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("medsouz/KerbalKonstructs/Assets/horizontalsep3", false);

		public static Boolean runningRace = false;
		public Boolean racing = false;
		public Boolean started = false;
		public Boolean finished = false;

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

		public StaticObject StartLine = null;
		public StaticObject FinishLine = null;

		GUIStyle raceStyle = new GUIStyle();
		GUIStyle BoxNoBorder;
		GUIStyle DeadButton;
		GUIStyle DeadButtonRed;

		public void drawRacing()
		{
			raceStyle.padding.top = 1;

			if (runningRace)
				racingRect = GUI.Window(0xC00C1E8, racingRect, drawRacingWindow, "", raceStyle);
		}

		public void drawRacingWindow(int windowID)
		{
			BoxNoBorder = new GUIStyle(GUI.skin.box);
			BoxNoBorder.normal.background = null;
			BoxNoBorder.normal.textColor = Color.white;

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

				GUILayout.Button("Air-Racing", DeadButton, GUILayout.Height(16));

				GUILayout.FlexibleSpace();

				GUI.enabled = true;

				if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(16)))
				{
					ResetRace();
					KerbalKonstructs.instance.showRacingApp = false;
					runningRace = false;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(1);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			if (!racing)
			{
				GUILayout.Box("Cross a start line to begin a race.");

				if (!started)
				{
					StartLine = NavUtils.GetNearestFacility(FlightGlobals.ActiveVessel.GetTransform().position, "RaceStart");
					if (StartLine != null)
					{
						fDistToStart = StartLine.GetDistanceToObject(FlightGlobals.ActiveVessel.GetTransform().position);

						if (fDistToStart < 150)
						{
							dStartTime = Planetarium.GetUniversalTime();
							ScreenMessages.PostScreenMessage("!!! GO GO GO !!!", 10, ScreenMessageStyle.UPPER_CENTER);
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
						StaticObject soNextGate = GetNextGate(StartLine, fNextGate);
						if (soNextGate != null)
						{
							GUILayout.Box("Next Gate: " + fNextGate);
							fDistToGate = GetDistanceToGate(soNextGate, FlightGlobals.ActiveVessel.GetTransform().position);

							fDistBetween = (GetGateWidth(soNextGate) / 2) + 10;

							if (fDistToGate > fDistBetween)
								GUILayout.Box("Distance to Next Gate: " + fDistToGate.ToString("#0.0") + " m");
							else
							{
								ScreenMessages.PostScreenMessage("!!! GATE  " + fNextGate.ToString() + "  CLEAR !!!", 10, ScreenMessageStyle.UPPER_CENTER);
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
						FinishLine = NavUtils.GetNearestFacility(FlightGlobals.ActiveVessel.GetTransform().position, "RaceFinish");
						if (FinishLine != null)
						{
							fDistToFinish = FinishLine.GetDistanceToObject(FlightGlobals.ActiveVessel.GetTransform().position);

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
						ScreenMessages.PostScreenMessage("!!! FINISHED !!!", 10, ScreenMessageStyle.UPPER_CENTER);
					}

					fTimeMins = (int)dFinishTime / 60;
					fTimeSecs = (float)(dFinishTime - (fTimeMins * 60));

					GUILayout.Box("Race Time: " + fTimeMins.ToString("#0") + " minutes " + fTimeSecs.ToString("#0.00") + " seconds ");

					if (GUILayout.Button("Save Race Results", GUILayout.Height(22)))
					{
						ScreenMessages.PostScreenMessage("Feature still WIP", 10, ScreenMessageStyle.LOWER_CENTER);
					}

					if (GUILayout.Button("Race Again!", GUILayout.Height(22))) ResetRace();
				}
			}

			GUILayout.Space(5);
			if (GUILayout.Button("I'm done racing!", GUILayout.Height(22)))
			{
				ResetRace();
				KerbalKonstructs.instance.showRacingApp = false;
				runningRace = false;
			}
			GUILayout.FlexibleSpace();
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
			GUILayout.Space(3);

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public StaticObject GetNextGate(StaticObject StartLine, float fNextGate)
		{
			StaticObject soNextGate = null;
			string sGate = "Gate" + fNextGate.ToString();
			string sGroup = (string)StartLine.getSetting("Group");

			soNextGate = NavUtils.GetNearestFacility(StartLine.gameObject.transform.position, sGate, sGroup);

			return soNextGate;
		}

		public float GetDistanceToGate(StaticObject soGate, Vector3 vPos)
		{
			float fDistance = 0f;
			Vector3 vCenter = new Vector3(0, 0, 0);
			StaticObject soNearestPole = null;

			string sGroup = (string)soGate.getSetting("Group");
			string sFacType = (string)soGate.getSetting("FacilityType");
			soNearestPole = NavUtils.GetNearestFacility(soGate.gameObject.transform.position, sFacType + "P", sGroup);

			vCenter = Vector3.Lerp(soGate.gameObject.transform.position, soNearestPole.gameObject.transform.position, 0.5f);

			fDistance = Vector3.Distance(vCenter, vPos);
			return fDistance;
		}

		public float GetGateWidth(StaticObject soGate)
		{
			float fDistance = 0f;
			StaticObject soNearestPole = null;

			string sGroup = (string)soGate.getSetting("Group");
			string sFacType = (string)soGate.getSetting("FacilityType");
			soNearestPole = NavUtils.GetNearestFacility(soGate.gameObject.transform.position, sFacType + "P", sGroup);

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
	}
}
