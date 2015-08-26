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
	public class DownlinkGUI
	{
		Rect targetSelectorRect = new Rect(640, 120, 190, 400);
		Rect DownlinkRect = new Rect(300, 30, 175, 720);

		public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep", false);
		public Texture tSpeaker = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/speaker", false);

		public Texture tSpeaker2 = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/speaker2", false);
		public Texture tSpeaker3 = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/speaker3", false);

		public AudioClip aStatic = GameDatabase.Instance.GetAudioClip("KerbalKonstructs/Sounds/radiostatic");
		public AudioClip aBeep1 = GameDatabase.Instance.GetAudioClip("KerbalKonstructs/Sounds/beep1");
		public AudioClip aBeep2 = GameDatabase.Instance.GetAudioClip("KerbalKonstructs/Sounds/beep2");


		public static GameObject Dis;
		public static AudioSource DisAudio;

		public Texture tStatic = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/static", false);
		public Texture tStatic2 = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/static2", false);
		public Texture tKerbal = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kerbcam1", false);
		public Texture tKerbal1 = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kerbcam1", false);
		public Texture tKerbal2 = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kerbcam2", false);
		public Texture tKerbal3 = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kerbcam3", false);
		public Texture tKerbal4 = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kerbcam4", false);
		public Texture tKerbal5 = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kerbcam5", false);
		public Texture tKerbal6 = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kerbcam6", false);
		public Texture tKerbal7 = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/kerbcam7", false);

		public Texture2D tIndicatorRed = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/indicatorred", false);
		public Texture2D tIndicatorGreen = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/indicatorgreen", false);
		public Texture2D tGreyButton = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/indicatorgrey", false);


		public Texture2D tIndicatorBad = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/indicatorbad", false);
		public Texture2D tIndicatorGood = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/indicatorgood", false);

		public Texture2D tBigOlButton = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/bigolbutton", false);

		public Texture2D tSpeed = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/speed", false);
		public Texture2D tStrength = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/strength", false);

		public Texture2D tButtonRed = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/buttonred", false);
		public Texture2D tButtonGreen = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/buttongreen", false);
		public Texture2D tButtonBlue = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/buttonblue", false);
		public Texture2D tTitleBox = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/titlebox", false);


		public Texture tFoldOut = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldin", false);
		public Texture tFoldIn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);
		public Texture tFolded = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);

		public static string sAntennaInterval = "0";
		public static string sAntennaPacket = "0";
		public static string sAntennaResourceCost = "0";

		public double dUpdater = 0;
		public double dUpdater2 = 0;
		public double dUpdater3 = 0;
		public double dUpdater4 = 0;
		public double dUpdater5 = 0;

		public static string sWarning1 = "None";
		public static string sWarning2 = "None";

		public string sConversation = "None";
		public string sConversation2 = "";
		public static string sConversation3 = "";
		public string sConversation4 = "";
		public static string sConversation5 = "";
		public static string sConversation6 = "";

		public static string sTargetType = "None";
		public static string sTarget = "None";
		public static string sRelayName = "None";
		public static string sIsRelay = "No";

		public string sVesselName = "";

		public static StaticObject soTargetStation = null;
		public static string sStationRadial = "None";

		public static float fMaxAngle = 0f;
		public static float StationLOS = 0f;

		public static Vessel vTargetVessel = null;
		public Vessel vRelayVessel = null;

		public Vessel vSelectedVessel = null;

		GUIStyle DeadButton;
		GUIStyle DeadButtonRed;
		GUIStyle BoxNoBorder;
		GUIStyle navStyle = new GUIStyle();

		GUIStyle ButtonSmallText;
		GUIStyle ButtonGreen;
		GUIStyle ButtonRed;
		GUIStyle ButtonBlue;
		GUIStyle ButtonBig;
		GUIStyle ButtonGrey;
		GUIStyle IndicatorGreen;
		GUIStyle IndicatorRed;
		GUIStyle BoxInfo;
		GUIStyle BoxTitle;

		public static Boolean bVideo = false;
		public static Boolean bAudio = false;
		public Boolean bTextlink = false;
		public static Boolean bDatalink = false;
		public Boolean bAutoMode = false;
		public Boolean bDiagMode = false;
		public static Boolean bBoost = false;
		public Boolean bClean = false;
		public Boolean bPowered = true;
		public static Boolean bScanning = false;

		public static Boolean bCheckedRegistry = true;
		public static Boolean bUnregistered = false;

		public static Boolean bChangeTarget = false;

		public Boolean bShowDevicePanel = true;
		public Boolean bShowTargetPanel = true;
		public Boolean bShowSignalPanel = true;
		public Boolean bShowPowerPanel = true;

		public double dRefresh = 15;

		Vector2 scrollPos;
		Vector2 scrollPos2;
		//Vector2 scrollPos3;
		//Vector2 scrollPos4;

		public float fSignalDelay = 0f;
		public float fSignalStrength = 0f;

		public float fPowerDraw = 0f;
		public double dAvailableEC = 0;

		public static Part pAntenna = null;

		void drawTargetSelector(int windowID)
		{
			TrackingStationGUI.TargetSelector(sTargetType, null);
			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public DownlinkGUI()
		{
			navStyle.padding.left = 0;
			navStyle.padding.right = 0;
			navStyle.padding.top = 1;
			navStyle.padding.bottom = 3;
			navStyle.normal.background = null;

			Dis = new GameObject();
			DisAudio = Dis.AddComponent<AudioSource>();
			DisAudio.clip = aStatic;
			DisAudio.loop = false;
			DisAudio.playOnAwake = false;
			DisAudio.rolloffMode = AudioRolloffMode.Logarithmic;
			DisAudio.dopplerLevel = 1f;
			DisAudio.panLevel = 1f;
			DisAudio.volume = GameSettings.SHIP_VOLUME / 2f;
			DisAudio.bypassEffects = true;
			Dis.transform.SetParent(FlightCamera.fetch.mainCamera.transform);
			Dis.SetActive(true);
		}

		public void PlayBeep1()
		{
			if (DisAudio == null) return;
			DisAudio.Stop();
			//DisAudio.loop = false;
			//DisAudio.clip = aBeep1;
			DisAudio.PlayOneShot(aBeep1);
		}

		public void PlayBeep2()
		{
			if (DisAudio == null) return;
			DisAudio.Stop();
			//DisAudio.loop = false;
			//DisAudio.clip = aBeep2;
			DisAudio.PlayOneShot(aBeep2);
		}

		public void PlayStatic()
		{
			if (DisAudio == null) return;
			if (DisAudio.isPlaying) return;

			//DisAudio.loop = true;
			DisAudio.clip = aStatic;
			DisAudio.Play();
		}

		public void drawDownlink()
		{
			if (KerbalKonstructs.instance.showDownlink)
			{
				DownlinkRect = GUI.Window(0xE05B9C9, DownlinkRect, drawDownlinkWindow, "", navStyle);

				if (bChangeTarget)
					targetSelectorRect = GUI.Window(0xB71B2A1, targetSelectorRect, drawTargetSelector, "Select Target");
			}
		}

		void InitialiseBoard()
		{
			sWarning1 = "None";
			sWarning2 = "None";
			sConversation = "None";
			sConversation2 = "";
			sConversation3 = "";
			sConversation4 = "";
			sConversation5 = "";
			sConversation6 = "";
			bVideo = false;
			bAudio = false;
			bTextlink = false;
			bDatalink = false;
			bAutoMode = false;
			bDiagMode = false;
			bBoost = false;
			bClean = false;
			sTarget = "None";
			sTargetType = "None";
			vTargetVessel = null;
			soTargetStation = null;
			fSignalDelay = 0f;
			fSignalStrength = 0f;
			bChangeTarget = false;
			dUpdater = 0;
			dUpdater3 = 0;
			dUpdater4 = 0;
			dUpdater5 = 0;
			bScanning = false;
		}

		void drawDownlinkWindow(int windowID)
		{
			ButtonGreen = new GUIStyle(GUI.skin.button);
			ButtonGreen.normal.background = tButtonGreen;
			ButtonGreen.hover.background = tButtonGreen;
			ButtonGreen.active.background = tButtonGreen;
			ButtonGreen.focused.background = tButtonGreen;
			ButtonGreen.normal.textColor = Color.black;
			ButtonGreen.hover.textColor = Color.black;
			ButtonGreen.active.textColor = Color.black;
			ButtonGreen.focused.textColor = Color.black;
			ButtonGreen.fontSize = 12;
			ButtonGreen.fontStyle = FontStyle.Normal;

			ButtonBlue = new GUIStyle(GUI.skin.button);
			ButtonBlue.normal.background = tButtonBlue;
			ButtonBlue.hover.background = tButtonBlue;
			ButtonBlue.active.background = tButtonBlue;
			ButtonBlue.focused.background = tButtonBlue;
			ButtonBlue.normal.textColor = Color.white;
			ButtonBlue.hover.textColor = Color.black;
			ButtonBlue.active.textColor = Color.grey;
			ButtonBlue.focused.textColor = Color.black;
			ButtonBlue.fontSize = 12;
			ButtonBlue.fontStyle = FontStyle.Normal;

			ButtonBig = new GUIStyle(GUI.skin.button);
			ButtonBig.normal.background = tBigOlButton;
			ButtonBig.hover.background = tBigOlButton;
			ButtonBig.active.background = tBigOlButton;
			ButtonBig.focused.background = tBigOlButton;
			ButtonBig.normal.textColor = Color.white;
			ButtonBig.hover.textColor = Color.black;
			ButtonBig.active.textColor = Color.grey;
			ButtonBig.focused.textColor = Color.black;
			ButtonBig.fontSize = 12;
			ButtonBig.fontStyle = FontStyle.Normal;

			ButtonSmallText = new GUIStyle(GUI.skin.button);
			ButtonSmallText.fontSize = 12;
			ButtonSmallText.fontStyle = FontStyle.Normal;

			ButtonGrey = new GUIStyle(GUI.skin.button);
			ButtonGrey.fontSize = 12;
			ButtonGrey.fontStyle = FontStyle.Normal;
			ButtonGrey.normal.background = tGreyButton;
			ButtonGrey.hover.background = tGreyButton;
			ButtonGrey.active.background = tGreyButton;
			ButtonGrey.focused.background = tGreyButton;
			ButtonGrey.normal.textColor = Color.white;
			ButtonGrey.hover.textColor = Color.white;
			ButtonGrey.active.textColor = Color.white;
			ButtonGrey.focused.textColor = Color.white;

			ButtonRed = new GUIStyle(GUI.skin.button);
			ButtonRed.normal.background = tButtonRed;
			ButtonRed.hover.background = tButtonRed;
			ButtonRed.active.background = tButtonRed;
			ButtonRed.focused.background = tButtonRed;
			ButtonRed.normal.textColor = Color.black;
			ButtonRed.hover.textColor = Color.black;
			ButtonRed.active.textColor = Color.black;
			ButtonRed.focused.textColor = Color.black;
			ButtonRed.fontSize = 12;
			ButtonRed.fontStyle = FontStyle.Normal;

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

			BoxNoBorder = new GUIStyle(GUI.skin.box);
			BoxNoBorder.normal.background = null;
			BoxNoBorder.normal.textColor = Color.white;
			BoxNoBorder.fontSize = 13;

			BoxTitle = new GUIStyle(GUI.skin.button);
			BoxTitle.normal.background = tTitleBox;
			BoxTitle.hover.background = tTitleBox;
			BoxTitle.active.background = tTitleBox;
			BoxTitle.focused.background = tTitleBox;
			BoxTitle.normal.textColor = Color.white;
			BoxTitle.hover.textColor = Color.white;
			BoxTitle.active.textColor = Color.white;
			BoxTitle.focused.textColor = Color.white;
			BoxTitle.fontSize = 13;

			BoxInfo = new GUIStyle(GUI.skin.box);
			BoxInfo.normal.background = null;
			BoxInfo.normal.textColor = Color.yellow;
			BoxInfo.fontSize = 13;

			IndicatorGreen = new GUIStyle(GUI.skin.box);
			IndicatorGreen.normal.background = tIndicatorGreen;
			IndicatorGreen.normal.textColor = Color.black;
			IndicatorGreen.fontSize = 12;
			IndicatorGreen.fontStyle = FontStyle.Normal;

			IndicatorRed = new GUIStyle(GUI.skin.box);
			IndicatorRed.normal.background = tIndicatorRed;
			IndicatorRed.normal.textColor = Color.black;
			IndicatorRed.fontSize = 12;
			IndicatorRed.fontStyle = FontStyle.Normal;

			if (FlightGlobals.ActiveVessel != vSelectedVessel)
			{
				InitialiseBoard();
				vRelayVessel = null;
				sRelayName = "None";
				sIsRelay = "No";
				pAntenna = null;

				if (Dis != null)
				{
					Dis.transform.SetParent(FlightCamera.fetch.mainCamera.transform);
					Dis.SetActive(true);
				}
				
				if (DisAudio != null)
					DisAudio.Stop();
			}

			vSelectedVessel = FlightGlobals.ActiveVessel;

			GUILayout.BeginHorizontal();
			{
				GUI.enabled = false;
				GUILayout.Button("-KK-", DeadButton, GUILayout.Height(21));

				GUILayout.FlexibleSpace();

				GUILayout.Button("Downlink", DeadButton, GUILayout.Height(21));

				GUILayout.FlexibleSpace();

				GUI.enabled = true;

				if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(21)))
				{
					KerbalKonstructs.instance.enableDownlink = false;
					KerbalKonstructs.instance.showDownlink = false;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(1);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			GUILayout.BeginHorizontal();
			if (bShowDevicePanel)
			{
				if (GUILayout.Button(tFoldIn, BoxTitle, GUILayout.Height(20), GUILayout.Width(20)))
				{
					bShowDevicePanel = false;
				}
			}
			else
			{
				if (GUILayout.Button(tFoldOut, BoxTitle, GUILayout.Height(20), GUILayout.Width(20)))
				{
					bShowDevicePanel = true;
				}
			}

			if (GUILayout.Button("Comms Devices", BoxTitle, GUILayout.Height(20)))
			{
				if (bShowDevicePanel)
					bShowDevicePanel = false;
				else
					bShowDevicePanel = true;
			}

			if (bShowDevicePanel)
			{
				if (GUILayout.Button(tFoldIn, BoxTitle, GUILayout.Height(20), GUILayout.Width(20)))
				{
					bShowDevicePanel = false;
				}
			}
			else
			{
				if (GUILayout.Button(tFoldOut, BoxTitle, GUILayout.Height(20), GUILayout.Width(20)))
				{
					bShowDevicePanel = true;
				}
			}

			GUILayout.EndHorizontal();

			if (bShowDevicePanel)
			{
				scrollPos2 = GUILayout.BeginScrollView(scrollPos2);
				{
					foreach (Part pPart in FlightGlobals.ActiveVessel.Parts)
					{
						foreach (PartModule pModule in pPart.Modules)
						{
							if (pModule.moduleName == "ModuleDataTransmitter")
							{
								GUI.enabled = !(pAntenna == pPart);
								if (GUILayout.Button(pPart.partInfo.title, ButtonBig, GUILayout.Height(20)))
								{
									InitialiseBoard();
									vRelayVessel = null;
									sRelayName = "None";
									sIsRelay = "No";

									pAntenna = pPart;

									LoadCommsPersistence(pPart);

									ConfigNode node = pAntenna.partInfo.partConfig;

									if (node != null)
									{
										var moduleNodes = from nodes in node.GetNodes("MODULE")
														  where nodes.GetValue("name") == "ModuleDataTransmitter"
														  select nodes;

										foreach (ConfigNode moduleNode in moduleNodes)
										{
											if (moduleNode == null)
												continue;

											if (moduleNode.HasValue("packetInterval"))
											{
												sAntennaInterval = (string)moduleNode.GetValue("packetInterval");
											}
											if (moduleNode.HasValue("packetSize"))
											{
												sAntennaPacket = (string)moduleNode.GetValue("packetSize");
											}
											if (moduleNode.HasValue("packetResourceCost"))
											{
												sAntennaResourceCost = (string)moduleNode.GetValue("packetResourceCost");
											}
										}

									}

								}
								GUI.enabled = true;
							}
						}
					}
				}
				GUILayout.EndScrollView();

				GUILayout.FlexibleSpace();
			}

			GUILayout.BeginHorizontal();
			if (bShowTargetPanel)
			{
				if (GUILayout.Button(tFoldIn, BoxTitle, GUILayout.Height(20), GUILayout.Width(20)))
				{
					bShowTargetPanel = false;
				}
			}
			else
			{
				if (GUILayout.Button(tFoldOut, BoxTitle, GUILayout.Height(20), GUILayout.Width(20)))
				{
					bShowTargetPanel = true;
				}
			}

			if (GUILayout.Button("Target", BoxTitle, GUILayout.Height(20)))
			{
				if (bShowTargetPanel)
					bShowTargetPanel = false;
				else
					bShowTargetPanel = true;
			}

			if (bShowTargetPanel)
			{
				if (GUILayout.Button(tFoldIn, BoxTitle, GUILayout.Height(20), GUILayout.Width(20)))
				{
					bShowTargetPanel = false;
				}
			}
			else
			{
				if (GUILayout.Button(tFoldOut, BoxTitle, GUILayout.Height(20), GUILayout.Width(20)))
				{
					bShowTargetPanel = true;
				}
			}

			GUILayout.EndHorizontal();

			bool bRelay = false;
			if (sIsRelay == "Yes") bRelay = true;

			if (bShowTargetPanel)
			{
				GUILayout.BeginHorizontal();
				{
					if (!bScanning)
						GUI.enabled = (pAntenna != null);
					else
						GUI.enabled = false;

					if (GUILayout.Button("Vessel", ButtonBlue, GUILayout.Height(20)))
					{
						InitialiseBoard();
						sTargetType = "Craft";
						PersistenceUtils.saveCommsPersistence(FlightGlobals.ActiveVessel, pAntenna,
							sTargetType, sTarget, bRelay, sRelayName, sStationRadial);
						bChangeTarget = true;
						bCheckedRegistry = false;
					}
					GUI.enabled = true;

					if (!bScanning)
						GUI.enabled = (pAntenna != null);
					else
						GUI.enabled = false;

					if (GUILayout.Button("Station", ButtonBlue, GUILayout.Height(20)))
					{
						InitialiseBoard();
						sTargetType = "Station";
						PersistenceUtils.saveCommsPersistence(FlightGlobals.ActiveVessel, pAntenna,
							sTargetType, sTarget, bRelay, sRelayName, sStationRadial);

						foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
						{
							if ((string)obj.getSetting("FacilityType") == "TrackingStation")
								PersistenceUtils.loadStaticPersistence(obj);
						}

						bChangeTarget = true;
					}
					GUI.enabled = true;
				}
				GUILayout.EndHorizontal();

				if (pAntenna == null)
					GUI.enabled = false;
				else
					if (!bScanning)
						GUI.enabled = (bPowered);
					else
						GUI.enabled = false;

				GUILayout.BeginHorizontal();
				{
					if (GUILayout.Button("SCAN", ButtonBlue, GUILayout.Height(20)))
					{
						InitialiseBoard();
						dUpdater5 = Planetarium.GetUniversalTime();
						bScanning = true;
						sTargetType = "Craft";
						ScanForVessels();
					}
					if (GUILayout.Button("SCAN", ButtonBlue, GUILayout.Height(20)))
					{
						InitialiseBoard();
						dUpdater5 = Planetarium.GetUniversalTime();
						bScanning = true;
						sTargetType = "Station";
						ScanForStations();
					}
				}
				GUILayout.EndHorizontal();
			}

			GUI.enabled = true;

			if (bScanning)
			{
				double dTicker5 = Planetarium.GetUniversalTime();
				if ((dTicker5 - dUpdater5) > 9)
				{
					bScanning = false;
				}
			}

			string sDisplayTarget = "None";
			vTargetVessel = null;

			if (sTargetType == "Craft" && sTarget != "None")
			{
				vTargetVessel = TrackingStationGUI.GetTargetVessel(sTarget);

				if (vTargetVessel != null)
				{
					int iU = vTargetVessel.name.IndexOf("(");
					if (iU < 2) iU = vTargetVessel.name.Length + 1;
					sDisplayTarget = vTargetVessel.name.Substring(0, iU - 1);
				}
				else
					sDisplayTarget = "None";

				if (bShowTargetPanel)
					GUILayout.Box(sDisplayTarget, BoxInfo, GUILayout.Height(20));
			}
			else
				if (sTargetType == "Station" && soTargetStation != null)
				{
					float fAlt = (float)soTargetStation.getSetting("RadiusOffset");

					CelestialBody cPlanetoid = (CelestialBody)soTargetStation.getSetting("CelestialBody");

					Vector3 ObjectPos = cPlanetoid.transform.InverseTransformPoint(soTargetStation.gameObject.transform.position);
					Double dObjectLat = NavUtils.GetLatitude(ObjectPos);
					Double dObjectLon = NavUtils.GetLongitude(ObjectPos);
					Double disObjectLat = dObjectLat * 180 / Math.PI;
					Double disObjectLon = dObjectLon * 180 / Math.PI;

					if (disObjectLon < 0) disObjectLon = disObjectLon + 360;

					sDisplayTarget = cPlanetoid.name + " Station\nAltitude: " + fAlt.ToString("#0") + "m\nLat. "
						+ disObjectLat.ToString("#0.000") + " Lon. " + disObjectLon.ToString("#0.000");

					if (bShowTargetPanel)
						GUILayout.Box(sDisplayTarget, BoxInfo, GUILayout.Height(60));
				}
				else
				{
					if (bShowTargetPanel)
						GUILayout.Box(sDisplayTarget, BoxInfo, GUILayout.Height(20));
				}

			if (bShowTargetPanel)
			{
				if (bScanning)
				{
					GUILayout.BeginHorizontal();

					double dTicker4 = Planetarium.GetUniversalTime();

					if ((dTicker4 - dUpdater4) > 4)
					{
						GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
						GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
						GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
						GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
						GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
						GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
						dUpdater4 = Planetarium.GetUniversalTime();
					}
					else
						if ((dTicker4 - dUpdater4) > 3.5)
						{
							GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
							GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
							GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
							GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
							GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
							GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
						}
						else
							if ((dTicker4 - dUpdater4) > 3)
							{
								GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
								GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
								GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
								GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
								GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
								GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
							}
							else
								if ((dTicker4 - dUpdater4) > 2.5)
								{
									GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
									GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
									GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
									GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
									GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
									GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
								}
								else
									if ((dTicker4 - dUpdater4) > 2)
									{
										GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
										GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
										GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
										GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
										GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
										GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
									}
									else
										if ((dTicker4 - dUpdater4) > 1.5)
										{
											GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
											GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
											GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
											GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
											GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
											GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
										}
										else
											if ((dTicker4 - dUpdater4) > 1)
											{
												GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
												GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
												GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
												GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
												GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
												GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
											}
											else
											{
												GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
												GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
												GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
												GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
												GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
												GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
											}

					GUILayout.EndHorizontal();
				}
				else
				{
					if (sDisplayTarget == "None" || fSignalStrength <= 0f)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
						GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
						GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
						GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
						GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
						GUILayout.Box(" ", IndicatorRed, GUILayout.Height(5));
						GUILayout.EndHorizontal();
					}
					else
					{
						GUILayout.BeginHorizontal();
						GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
						GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
						GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
						GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
						GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
						GUILayout.Box(" ", IndicatorGreen, GUILayout.Height(5));
						GUILayout.EndHorizontal();
					}
				}

				GUI.enabled = (sDisplayTarget != "None");
				if (GUILayout.Button("CUT CONNECTION", ButtonBig, GUILayout.Height(20)))
				{
					sTarget = "None";
					sTargetType = "None";
					soTargetStation = null;
					vTargetVessel = null;
					bChangeTarget = false;
					PersistenceUtils.saveCommsPersistence(FlightGlobals.ActiveVessel, pAntenna,
						"None", "None", bRelay, sRelayName, "None");
				}
				GUI.enabled = true;

				string sRelayButton = "Set as Relay";

				if (sIsRelay == "No")
				{
					sRelayButton = "SET AS RELAY";
					GUI.enabled = (sTarget != "None" && sTargetType != "Station");
				}
				else
				{
					sRelayButton = "DISCONNECT RELAY";
					GUI.enabled = true;
				}

				if (GUILayout.Button(sRelayButton, ButtonBig, GUILayout.Height(20)))
				{
					if (sIsRelay == "Yes")
					{
						sIsRelay = "No";
						vRelayVessel = null;
						sRelayName = "None";
						PersistenceUtils.saveCommsPersistence(FlightGlobals.ActiveVessel, pAntenna,
							sTargetType, sTarget, false, "None", sStationRadial);
					}
					else
					{
						sIsRelay = "Yes";
						vRelayVessel = vTargetVessel;
						sRelayName = vTargetVessel.name + "_" + vTargetVessel.id.ToString();
						PersistenceUtils.saveCommsPersistence(FlightGlobals.ActiveVessel, pAntenna,
							sTargetType, sTarget, true, sRelayName, sStationRadial);
					}
				}
			}

			string sDisplayRelay = "None";

			if (sRelayName != "None")
			{
				vRelayVessel = TrackingStationGUI.GetTargetVessel(sRelayName);

				if (vRelayVessel != null)
				{
					int iU2 = vRelayVessel.name.IndexOf("(");
					if (iU2 < 2) iU2 = vRelayVessel.name.Length + 1;
					sDisplayRelay = vRelayVessel.name.Substring(0, iU2 - 1);
					sWarning1 = "None";
				}
				else
				{
					sDisplayRelay = "None";
					sWarning1 = "Could not find relay vessel...";
				}
			}

			if (bShowTargetPanel)
				GUILayout.Box(sDisplayRelay, BoxInfo);

			GUILayout.FlexibleSpace();

			GUILayout.Space(1);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
			
			GUI.enabled = true;

			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			{
				if (!bBoost)
				{
					if (GUILayout.Button("BOOST", ButtonRed, GUILayout.Height(20)))
					{
						bBoost = true;
					}
				}
				else
				{
					if (GUILayout.Button("BOOST", ButtonGreen, GUILayout.Height(20)))
					{
						bBoost = false;
					}

					if (!bPowered) bBoost = false;
				}

				if (!bClean)
				{
					if (GUILayout.Button("CLEAN", ButtonRed, GUILayout.Height(20)))
					{
						bClean = true;
					}
				}
				else
				{
					if (GUILayout.Button("CLEAN", ButtonGreen, GUILayout.Height(20)))
					{
						bClean = false;
					}

					if (!bPowered) bClean = false;
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(15);
				if (bVideo)
				{
					System.Random rnd = new System.Random();
					int irand = rnd.Next(((int)fSignalStrength + 8) - (int)fSignalDelay);

					if (sTargetType == "Station")
					{
						if (irand > 1)
							GUILayout.Box(tKerbal, GUILayout.Width(64), GUILayout.Height(64));
					}
					else
					{
						if (irand > 1)
							GUILayout.Box(tKerbal7, GUILayout.Width(64), GUILayout.Height(64));
					}

					if (irand == 1)
						GUILayout.Box(tStatic, GUILayout.Width(64));
					if (irand == 0)
						GUILayout.Box(tStatic2, GUILayout.Width(64));
				}
				else
				{
					GUILayout.Box("", GUILayout.Width(64), GUILayout.Height(64));
				}

				GUILayout.FlexibleSpace();
				if (!bAudio)
					GUILayout.Box(tSpeaker, BoxNoBorder);
				else
				{
					if (DisAudio != null)
					{
						if (DisAudio.isPlaying)
						{
							System.Random rnd5 = new System.Random();
							int irand5 = rnd5.Next(((int)fSignalStrength + 8) - (int)fSignalDelay);

							if (irand5 > 1)
								GUILayout.Box(tSpeaker, BoxNoBorder);
							if (irand5 == 1)
								GUILayout.Box(tSpeaker2, BoxNoBorder);
							if (irand5 == 0)
								GUILayout.Box(tSpeaker3, BoxNoBorder);
						}
						else
							GUILayout.Box(tSpeaker, BoxNoBorder);
					}
					else
						GUILayout.Box(tSpeaker, BoxNoBorder);
				}
				GUILayout.Space(15);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{
				if (pAntenna == null || sDisplayTarget == "None")
				{
					GUI.enabled = false;
					bVideo = false;
					bAudio = false;
					bDatalink = false;
					bTextlink = false;
				}
				else
				{
					GUI.enabled = true;
				}

				if (bVideo)
				{
					if (GUILayout.Button("Video", ButtonGreen, GUILayout.Height(20)))
						bVideo = false;

					if (fSignalStrength < 0.8f) bVideo = false;

					if (sTargetType == "Craft")
					{
						if (vTargetVessel != null)
						{
							if (VesselIsUncrewed(vTargetVessel)) bVideo = false;
						}
					}

					if (VesselIsUncrewed(FlightGlobals.ActiveVessel)) bVideo = false;

					if (!bPowered) bVideo = false;
				}
				else
				{
					if (GUILayout.Button("Video", ButtonRed, GUILayout.Height(20)))
						bVideo = true;
				}

				if (bAudio)
				{
					if (GUILayout.Button("Audio", ButtonGreen, GUILayout.Height(20)))
					{
						// Stop static						
						bAudio = false;
						DisAudio.Stop();
						//Dis.gameObject.SetActive(false);
					}

					if (fSignalStrength < 0.4f)
					{
						// Stop static
						bAudio = false;
						DisAudio.Stop();
						//Dis.gameObject.SetActive(false);
					}

					if (!bPowered)
					{
						bAudio = false;
						DisAudio.Stop();
					}
				}
				else
				{
					if (GUILayout.Button("Audio", ButtonRed, GUILayout.Height(20)))
						bAudio = true;
				}
			}
			GUILayout.EndHorizontal();

			if (!bAudio)
			{	
				if (Dis != null)
					if (DisAudio != null)
						DisAudio.Stop();
			}
			else
			{
				//if (!DisAudio.isPlaying)
					//DisAudio.Play();
				//else
				{
					double dTicker2 = Planetarium.GetUniversalTime();
					if ((dTicker2 - dUpdater2) > 5)
					{
						System.Random rnd9 = new System.Random();
						int irand9 = rnd9.Next(5);

						if (irand9 == 1)
						{
							DisAudio.Stop();
							PlayBeep1();
						}
						else
						if (irand9 == 2)
						{
							DisAudio.Stop();
							PlayBeep2();
						}
						else
							if (irand9 < ((int)fSignalDelay + 2)) PlayStatic();

						dUpdater2 = Planetarium.GetUniversalTime();
					}

					// PlayStatic();
				}
			}

			GUILayout.Space(1);

			GUILayout.BeginHorizontal();
			{
				if (bDatalink)
				{
					if (GUILayout.Button("Data", ButtonGreen, GUILayout.Height(20)))
					{
						bDatalink = false;
						dUpdater = 0;
						sConversation = "None";
					}
				}
				else
				{
					if (GUILayout.Button("Data", ButtonRed, GUILayout.Height(20)))
					{
						bDatalink = true;
						dUpdater = 0;
						sConversation = "None";
					}
				}

				if (bDatalink)
				{
					double dTicker = Planetarium.GetUniversalTime();
					if ((dTicker - dUpdater) > dRefresh)
					{
						dUpdater = Planetarium.GetUniversalTime();

						if (dRefresh == 1) dRefresh = 15;

						string sStrength = fSignalStrength.ToString("#0.0");
						string sDelay = fSignalDelay.ToString("#0.0");
						if (fSignalStrength < 0.1f) sStrength = "No signal!";
						if (fSignalDelay > 4.9f || fSignalDelay == 0f) sDelay = "All packets lost!";

						string sDatastream = "Signal strength:\n" + sStrength +
							"\nSignal Delay:\n" + sDelay;

						sConversation = "" + sDatastream + sConversation2 + sConversation3 + sConversation4 + sConversation5 + sConversation6;
						sConversation5 = "";
						sConversation6 = "";
					}
				}

				if (bTextlink)
				{
					if (GUILayout.Button("Text", ButtonGreen, GUILayout.Height(20)))
						bTextlink = false;
				}
				else
				{
					if (GUILayout.Button("Text", ButtonRed, GUILayout.Height(20)))
						bTextlink = true;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.FlexibleSpace();

			GUI.enabled = true;

			scrollPos = GUILayout.BeginScrollView(scrollPos);
			{
				if (pAntenna != null && bDatalink)
				{
					GUILayout.Box("DataLink", BoxNoBorder, GUILayout.Height(20));
					GUILayout.Box("Interval " + sAntennaInterval + "s | Size " + sAntennaPacket, BoxNoBorder, GUILayout.Height(20));
				}

				if (pAntenna == null)
					GUILayout.Box("No device", BoxNoBorder, GUILayout.Height(20));

				if (!bPowered)
					GUILayout.Box("No power", BoxNoBorder, GUILayout.Height(20));

				if (!bDatalink)
					GUILayout.Box("No datalink", BoxNoBorder, GUILayout.Height(20));

				if (!bTextlink)
					GUILayout.Box("No textlink", BoxNoBorder, GUILayout.Height(20));

				GUI.enabled = false;

				if (bTextlink || bDatalink)
				{
					bool bHasInfo = false;

					if (sConversation != "None")
					{
						GUILayout.Box(sConversation, BoxNoBorder);
						bHasInfo = true;
					}

					if (sIsRelay == "Yes")
					{
						GUILayout.Box("...RELAY SET...", BoxNoBorder);
						bHasInfo = true;
					}

					if (sWarning1 != "None")
					{
						GUILayout.Box(sWarning1, BoxNoBorder);
						bHasInfo = true;
					}

					if (sWarning2 != "None")
					{
						GUILayout.Box(sWarning2, BoxNoBorder);
						bHasInfo = true;
					}

					if (!bHasInfo)
						GUILayout.Box("Connected", BoxNoBorder);
				}

				GUI.enabled = true;
			}
			GUILayout.EndScrollView();

			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button(" Refresh ", ButtonBig, GUILayout.Height(20)))
				{
					dRefresh = 1;
				}

				if (GUILayout.Button("►", ButtonBlue, GUILayout.Height(20), GUILayout.Width(25)))
				{
					if (dRefresh <= 2) { }
					else
						dRefresh = dRefresh - 1;
				}

				GUI.enabled = false;
				GUILayout.Button("" + dRefresh.ToString() + "s", ButtonGrey, GUILayout.Height(20));
				GUI.enabled = true;

				if (GUILayout.Button("◄", ButtonBlue, GUILayout.Height(20), GUILayout.Width(25)))
				{
					if (dRefresh >= 30) { }
					else
						dRefresh = dRefresh + 1;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(1);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();

			if (bShowSignalPanel)
			{
				if (GUILayout.Button(tFoldIn, BoxTitle, GUILayout.Height(20), GUILayout.Width(20)))
				{
					bShowSignalPanel = false;
				}
			}
			else
			{
				if (GUILayout.Button(tFoldOut, BoxTitle, GUILayout.Height(20), GUILayout.Width(20)))
				{
					bShowSignalPanel = true;
				}
			}

			if (GUILayout.Button("Signal", BoxTitle, GUILayout.Height(20)))
			{
				if (bShowSignalPanel)
					bShowSignalPanel = false;
				else
					bShowSignalPanel = true;
			}

			if (bShowSignalPanel)
			{
				if (GUILayout.Button(tFoldIn, BoxTitle, GUILayout.Height(20), GUILayout.Width(20)))
				{
					bShowSignalPanel = false;
				}
			}
			else
			{
				if (GUILayout.Button(tFoldOut, BoxTitle, GUILayout.Height(20), GUILayout.Width(20)))
				{
					bShowSignalPanel = true;
				}
			}

			GUILayout.EndHorizontal();

			fSignalDelay = DetermineSignalDelay(FlightGlobals.ActiveVessel, pAntenna, vTargetVessel,
				soTargetStation, vRelayVessel);
			fSignalStrength = DetermineSignalStrength(FlightGlobals.ActiveVessel, pAntenna, vTargetVessel,
				soTargetStation, vRelayVessel);

			if (!bPowered) fSignalDelay = 0f;
			if (!bPowered) fSignalStrength = 0f;

			if (bShowSignalPanel)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Box(tStrength, BoxNoBorder, GUILayout.Height(20), GUILayout.Width(20));

				GUILayout.Space(5);

				if (fSignalStrength > 5f) fSignalStrength = 5f;
				else
					if (fSignalStrength < 0f) fSignalStrength = 0f;

				int fStrengthGoodCount = (int)fSignalStrength * 2;
				int fStrengthBadCount = 10 - fStrengthGoodCount;

				if (fSignalStrength > 5f)
				{
					fStrengthGoodCount = 10;
					fStrengthBadCount = 0;
				}

				if (fSignalStrength == 0f)
				{
					fStrengthGoodCount = 0;
					fStrengthBadCount = 10;
				}

				while (fStrengthGoodCount > 0)
				{
					GUILayout.Box(tIndicatorGood, BoxNoBorder, GUILayout.Height(20), GUILayout.Width(8));
					fStrengthGoodCount = fStrengthGoodCount - 1;
				}
				while (fStrengthBadCount > 0)
				{
					GUILayout.Box(tIndicatorBad, BoxNoBorder, GUILayout.Height(20), GUILayout.Width(8));
					fStrengthBadCount = fStrengthBadCount - 1;
				}

				GUILayout.EndHorizontal();

				GUILayout.Space(2);

				GUILayout.BeginHorizontal();
				GUILayout.Box(tSpeed, BoxNoBorder, GUILayout.Height(20), GUILayout.Width(20));

				GUILayout.Space(5);

				if (fSignalDelay > 5f) fSignalDelay = 5f;
				else
					if (fSignalDelay < 0f) fSignalDelay = 0f;

				int fDelayBadCount = (int)fSignalDelay * 2;
				int fDelayGoodCount = 10 - fDelayBadCount;

				if (fSignalDelay == 0f)
				{
					fDelayGoodCount = 0;
					fDelayBadCount = 10;
				}

				while (fDelayBadCount > 0)
				{
					GUILayout.Box(tIndicatorBad, BoxNoBorder, GUILayout.Height(20), GUILayout.Width(8));
					fDelayBadCount = fDelayBadCount - 1;
				}

				while (fDelayGoodCount > 0)
				{
					GUILayout.Box(tIndicatorGood, BoxNoBorder, GUILayout.Height(20), GUILayout.Width(8));
					fDelayGoodCount = fDelayGoodCount - 1;
				}

				GUILayout.EndHorizontal();
			}

			GUILayout.Space(1);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			GUILayout.FlexibleSpace();

			fPowerDraw = 0f;

			if (sDisplayTarget != "None" || sRelayName != "None")
			{
				if (fSignalStrength > 0)
				{
					fPowerDraw = float.Parse(sAntennaResourceCost) / 2f;

					if (sDisplayTarget != "None" && sRelayName == "None")
						fPowerDraw = fPowerDraw / 2f;

					if (sDisplayTarget == "None" && sRelayName != "None")
						fPowerDraw = fPowerDraw / 2f;

					if (bVideo) fPowerDraw = fPowerDraw * 1.2f;
					if (bBoost) fPowerDraw = fPowerDraw * 1.35f;
					if (bClean) fPowerDraw = fPowerDraw * 1.5f;
				}
			}

			dAvailableEC = 0;
			foreach (Part fBatt in FlightGlobals.ActiveVessel.parts)
			{
				foreach (PartResource rResource in fBatt.Resources)
				{
					if (rResource.resourceName == "ElectricCharge")
					{
						dAvailableEC = dAvailableEC + rResource.amount;
					}
					else continue;
				}
			}

			if (dAvailableEC <= 0) bPowered = false;
			else
				bPowered = true;

			if (bPowered)
			{
				if (fPowerDraw > dAvailableEC) bPowered = false;
			}

			GUILayout.BeginHorizontal();

			if (bShowPowerPanel)
			{
				if (GUILayout.Button(tFoldIn, BoxTitle, GUILayout.Height(20), GUILayout.Width(20)))
				{
					bShowPowerPanel = false;
				}
			}
			else
			{
				if (GUILayout.Button(tFoldOut, BoxTitle, GUILayout.Height(20), GUILayout.Width(20)))
				{
					bShowPowerPanel = true;
				}
			}
			
			if (GUILayout.Button("Power Draw", BoxTitle, GUILayout.Height(20)))
			{
				if (bShowPowerPanel)
					bShowPowerPanel = false;
				else
					bShowPowerPanel = true;
			}

			if (bShowPowerPanel)
			{
				if (GUILayout.Button(tFoldIn, BoxTitle, GUILayout.Height(20), GUILayout.Width(20)))
				{
					bShowPowerPanel = false;
				}
			}
			else
			{
				if (GUILayout.Button(tFoldOut, BoxTitle, GUILayout.Height(20), GUILayout.Width(20)))
				{
					bShowPowerPanel = true;
				}
			}

			GUILayout.EndHorizontal();

			if (bShowPowerPanel)
			{
				GUI.enabled = false;
				GUILayout.Button("" + fPowerDraw.ToString("#0.0") + " EC/s of " + dAvailableEC.ToString("#0.0") + " EC", ButtonGrey, GUILayout.Height(20));
				GUI.enabled = true;
			}

			if (fPowerDraw > 0.0f && bPowered)
			{
				double dTicker3 = Planetarium.GetUniversalTime();
				if ((dTicker3 - dUpdater3) > 1)
				{
					pAntenna.RequestResource("ElectricCharge", fPowerDraw);
					dUpdater3 = Planetarium.GetUniversalTime();
				}
			}

			GUILayout.Space(1);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

			GUILayout.FlexibleSpace();

			if (FlightGlobals.ActiveVessel.GetCrewCount() < 1)
			{
				bAutoMode = true;
				sConversation2 = "\n\nNo crew\nAutomatic mode enabled";
			}
			else
			{
				if (sRelayName != "None" && sDisplayTarget == "None")
				{
					bAutoMode = true;
					sConversation2 = "\n\nDedicated relay set\nAutomatic mode enabled";
				}
				else
				{
					bAutoMode = false;
					sConversation2 = "";
				}
			}

			if (!bAutoMode)
			{
				GUILayout.Box("Automatic Mode", IndicatorRed, GUILayout.Height(20));
			}
			else
			{
				bVideo = false;
				bAudio = false;
				GUILayout.Box("Automatic Mode", IndicatorGreen, GUILayout.Height(20));
			}

			if (pAntenna == null) bDiagMode = true;
			else
				if (sDisplayTarget == "None" && sRelayName == "None") bDiagMode = true;
				else
					if (fSignalStrength < 0.6f)
						bDiagMode = true;
					else
						if (fSignalDelay > 4.4f || fSignalDelay == 0f)
							bDiagMode = true;
						else
							if (!bPowered) bDiagMode = true;
							else
								bDiagMode = false;

			if (!bDiagMode)
			{
				GUILayout.Box("Diagnostic Mode", IndicatorRed, GUILayout.Height(20));
			}
			else
			{
				GUILayout.Box("Diagnostic Mode", IndicatorGreen, GUILayout.Height(20));
			}

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("REBOOT BOARD", ButtonBig, GUILayout.Height(20)))
			{
				if (pAntenna != null)
					PersistenceUtils.saveCommsPersistence(FlightGlobals.ActiveVessel, pAntenna,
						"None", "None", false, "None", "None");

				pAntenna = null;
				InitialiseBoard();
				vRelayVessel = null;
				sRelayName = "None";
				sIsRelay = "No";
			}

			GUILayout.Space(1);
			GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
			GUILayout.Space(1);

			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public static void ScanForVessels()
		{
			Vessel vBest = null;
			float fBestSignal = 0f;
			float fSignal = 0f;

			foreach (Vessel vVessel in FlightGlobals.Vessels)
			{
				if (vVessel.vesselType == VesselType.SpaceObject) continue;
				if (vVessel.vesselType == VesselType.Debris) continue;
				if (vVessel.vesselType == VesselType.EVA) continue;
				if (vVessel.vesselType == VesselType.Flag) continue;
				if (vVessel.vesselType == VesselType.Unknown) continue;
				if (vVessel == FlightGlobals.ActiveVessel) continue;

				fSignal = DetermineSignalStrength(FlightGlobals.ActiveVessel, pAntenna, vVessel, null, null, true);

				if (fSignal > fBestSignal)
				{
					fBestSignal = fSignal;
					vBest = vVessel;
					Debug.Log("KK: Scan found a better signal to a vessel");
				}
			}

			if (vBest != null)
			{
				vTargetVessel = vBest;
				sTarget = vBest.name + "_" + vBest.id.ToString();
				SaveCommsState();
				Debug.Log("KK: Best signal is " + sTarget);
			}
			else
			{
				sTarget = "None";
				vTargetVessel = null;
				Debug.Log("KK: Scan found no signal to any other vessel");
			}

			// bScanning = false;
		}

		public static void ScanForStations()
		{
			StaticObject soBest = null;
			float fBestSignal = 0f;
			float fSignal = 0f;

			foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
			{
				if ((string)obj.getSetting("FacilityType") == "TrackingStation")
				{
					//Debug.Log("KK: Found a station");
					PersistenceUtils.loadStaticPersistence(obj);
					if ((string)obj.getSetting("OpenCloseState") == "Closed")
					{
						//Debug.Log("KK: Station is closed");
						continue;
					}

					//Debug.Log("KK: Checking signal to an open station");

					if (FlightGlobals.ActiveVessel == null || pAntenna == null || obj == null)
					{
						//Debug.Log("KK: WTF?");
						bScanning = false;
						return;
					}

					fSignal = DetermineSignalStrength(FlightGlobals.ActiveVessel, pAntenna, null, obj, null, true);

					//Debug.Log("KK: fSignal is " + fSignal.ToString());

					if (fSignal > fBestSignal)
					{
						//Debug.Log("KK: Better signal " + fSignal.ToString());
						soBest = obj;
						fBestSignal = fSignal;
					}
				}
				else continue;
			}

			if (soBest != null)
			{
				soTargetStation = soBest;
				var FacilityKey = soBest.getSetting("RadialPosition");
				sStationRadial = FacilityKey.ToString();
				SaveCommsState();
				//Debug.Log("KK: Scan found best signal to station " + sStationRadial);
			}
			else
			{
				sTarget = "None";
				soTargetStation = null;
				//Debug.Log("KK: Scan found no signal to any station");
			}

			// bScanning = false;
		}

		public static float DetermineSignalStrength(Vessel vVessel, Part pPart, Vessel tTargetVessel, StaticObject tTargetStation, Vessel tRelayVessel, bool bScanner = false)
		{
			bool bRelayUsed = false;

			float fReturnStrength = 4f;

			if (pAntenna == null) return 0f;

			if (!bScanner)
			{
				if (tTargetStation == null)
				{
					if (sTarget == "None" && sRelayName == "None") return 0f;
				}
			}

			// Channels
			if (bVideo) fReturnStrength = fReturnStrength - 1f;
			if (bAudio) fReturnStrength = fReturnStrength - 0.5f;
			if (bDatalink) fReturnStrength = fReturnStrength - 0.1f;

			// Equipment
			float fInterval = float.Parse(sAntennaInterval);
			fReturnStrength = fReturnStrength - (fInterval * 3f);

			float fPacket = float.Parse(sAntennaPacket);
			fReturnStrength = fReturnStrength + (fPacket / 4f);

			// Occlusion? Relayed?
			float fReturnRange = 0f;
			
			if (tTargetVessel != null)
			{
				fReturnRange = Vector3.Distance(tTargetVessel.gameObject.transform.position, FlightGlobals.ActiveVessel.gameObject.transform.position);

				if (bUnregistered)
				{
					sWarning2 = "Target comms not registered";
					return 0f;
				}

				if (VesselAOccludedFromVesselB(FlightGlobals.ActiveVessel, tTargetVessel) ||
					VesselAOccludedFromVesselB(tTargetVessel, FlightGlobals.ActiveVessel))
				{
					if (tRelayVessel != null)
					{
						if (VesselAOccludedFromVesselB(FlightGlobals.ActiveVessel, tRelayVessel) ||
							VesselAOccludedFromVesselB(tRelayVessel, FlightGlobals.ActiveVessel))
						{
							sWarning2 = "Connection to relay is blocked";
							return 0f;
						}
						else
						{
							if (VesselAOccludedFromVesselB(tTargetVessel, tRelayVessel) ||
								VesselAOccludedFromVesselB(tRelayVessel, tTargetVessel))
							{
								sWarning2 = "Connection between target and relay is blocked";
								return 0f;
							}
							else
								bRelayUsed = true;
						}
					}
					else
					{
						sWarning2 = "Connection with target is blocked";
						return 0f;
					}
				}

				sWarning2 = "None";

				if (bRelayUsed)
				{
					fReturnRange = Vector3.Distance(tTargetVessel.gameObject.transform.position,
						tRelayVessel.gameObject.transform.position)
						+ Vector3.Distance(tRelayVessel.gameObject.transform.position,
						FlightGlobals.ActiveVessel.gameObject.transform.position);
					sWarning2 = "Connection is relayed";
				}
			}

			if (tTargetStation != null)
			{
				fReturnRange = Vector3.Distance(tTargetStation.gameObject.transform.position, FlightGlobals.ActiveVessel.gameObject.transform.position);

				if (VesselOccludedFromStation(FlightGlobals.ActiveVessel, tTargetStation) ||
					StationOccludedFromVessel(tTargetStation, FlightGlobals.ActiveVessel))
				{
					if (tRelayVessel != null)
					{
						if (VesselAOccludedFromVesselB(tRelayVessel, FlightGlobals.ActiveVessel) ||
							VesselAOccludedFromVesselB(FlightGlobals.ActiveVessel, tRelayVessel))
						{
							sWarning2 = "Connection to relay is blocked";
							return 0f;
						}
						else
						{
						if (VesselOccludedFromStation(tRelayVessel, tTargetStation) ||
							StationOccludedFromVessel(tTargetStation, tRelayVessel))
							{
								sWarning2 = "No connection between station and relay";
								return 0f;
							}
							else
								bRelayUsed = true;
						}
					}
					else
					{
						sWarning2 = "No connection to station";
						return 0f;
					}
				}

				sWarning2 = "None";

				if (bRelayUsed)
				{
					fReturnRange = Vector3.Distance(tTargetStation.gameObject.transform.position,
						tRelayVessel.gameObject.transform.position)
						+ Vector3.Distance(tRelayVessel.gameObject.transform.position,
						FlightGlobals.ActiveVessel.gameObject.transform.position);
					sWarning2 = "Connection is relayed";
				}
			}

			if (fReturnRange < 600000f)
			{
				fReturnStrength = fReturnStrength + 2.5f;
			}
			else
				if (fReturnRange < 20000000f)
				{
					fReturnStrength = fReturnStrength + 1.5f;
				}
				else
					if (fReturnRange < 100000000f)
					{
						fReturnStrength = fReturnStrength - (fReturnRange / 300000000f);
					}
					else
						if (fReturnRange < 2500000000f)
						{
							fReturnStrength = (fReturnStrength - 0.25f) - (fReturnRange / 500000000f);
						}
						else
						{
							fReturnStrength = (fReturnStrength - 0.5f) - (fReturnRange / 900000000f);
						}

			if (fReturnStrength > 0f)
			{
				if (tTargetVessel == tRelayVessel) fReturnStrength = fReturnStrength * 2f;
			}
			else
				fReturnStrength = fReturnStrength + 1.0f;

			if (bDatalink)
				sConversation3 = "\nRange to target:\n" + (fReturnRange / 1000f).ToString("#0") + " km\n"
					+ "Uncapped signal strength:\n" + fReturnStrength.ToString("#0.00");
			else
				sConversation3 = "";

			if (fReturnStrength < 0.6f) fReturnStrength = 0.5f;

			System.Random rnd = new System.Random();
			int irand = rnd.Next(960);

			if (!bScanner)
			{
				// Random signal noise
				if (irand < 21)
					fReturnStrength = fReturnStrength - (float)irand / 40f;
			}

			if (bBoost)
			{
				irand = rnd.Next(11);
				fReturnStrength = fReturnStrength + (float)irand / 5f;
			}

			return fReturnStrength;
		}

		float DetermineSignalDelay(Vessel vVessel, Part pPart, Vessel tTargetVessel, StaticObject tTargetStation, Vessel tRelayVessel)
		{
			bool bRelayUsed = false;
			
			float fReturnDelay = 2f;

			if (pAntenna == null) return 0f;

			if (tTargetStation == null)
			{
				if (sTarget == "None" && sRelayName == "None") return 0f;
			}

			// Equipment
			float fInterval = float.Parse(sAntennaInterval);
			fReturnDelay = fReturnDelay + (fInterval * 2f);

			float fPacket = float.Parse(sAntennaPacket);
			fReturnDelay = fReturnDelay - (fPacket / 4f);

			// Distance to target. Relayed?
			float fReturnRange = 0f;
			if (tTargetVessel != null)
			{
				fReturnRange = Vector3.Distance(tTargetVessel.gameObject.transform.position, FlightGlobals.ActiveVessel.gameObject.transform.position);

				if (bUnregistered) return 0f;

				if (VesselAOccludedFromVesselB(FlightGlobals.ActiveVessel, tTargetVessel) ||
					VesselAOccludedFromVesselB(tTargetVessel, FlightGlobals.ActiveVessel))
				{
					if (tRelayVessel != null)
					{
						if (VesselAOccludedFromVesselB(FlightGlobals.ActiveVessel, tRelayVessel) ||
							VesselAOccludedFromVesselB(tRelayVessel, FlightGlobals.ActiveVessel))
						{
							sConversation4 = "\nActive vessel occluded from relay vessel";
							return 0f;
						}
						else
						{
							if (VesselAOccludedFromVesselB(tTargetVessel, tRelayVessel) ||
								VesselAOccludedFromVesselB(tRelayVessel, tTargetVessel))
							{
								sConversation4 = "\nTarget vessel occluded from relay vessel";
								return 0f;
							}
							else
								bRelayUsed = true;
						}
					}
					else
					{
						sConversation4 = "\nTarget vessel occluded from active vessel";
						return 0f;
					}

					sConversation4 = "";
				}

				if (bRelayUsed)
				{
					sConversation4 = "\nConnection is relayed";
					fReturnRange = Vector3.Distance(tTargetVessel.gameObject.transform.position,
						tRelayVessel.gameObject.transform.position)
						+ Vector3.Distance(tRelayVessel.gameObject.transform.position,
						FlightGlobals.ActiveVessel.gameObject.transform.position);
				}
				else
					sConversation4 = "";
			}

			if (tTargetStation != null)
			{
				fReturnRange = Vector3.Distance(tTargetStation.gameObject.transform.position, FlightGlobals.ActiveVessel.gameObject.transform.position);

				if (VesselOccludedFromStation(FlightGlobals.ActiveVessel, tTargetStation) ||
					StationOccludedFromVessel(tTargetStation, FlightGlobals.ActiveVessel))
				{
					if (tRelayVessel != null)
					{
						if (VesselAOccludedFromVesselB(tRelayVessel, FlightGlobals.ActiveVessel) ||
							VesselAOccludedFromVesselB(FlightGlobals.ActiveVessel, tRelayVessel))
						{
							sConversation4 = "\nRelay vessel occluded from active vessel";
							return 0f;
						}
						else
						{
							if (VesselOccludedFromStation(tRelayVessel, tTargetStation) ||
								StationOccludedFromVessel(tTargetStation, tRelayVessel))
							{
								sConversation4 = "\nRelay vessel is occluded from station";
								return 0f;
							}
							else
								bRelayUsed = true;
						}
					}
					else
					{
						sConversation4 = "\nActive vessel occluded from station";
						return 0f;
					}

					sConversation4 = "";
				}

				if (bRelayUsed)
				{
					sConversation4 = "\nConnection is relayed";
					fReturnRange = Vector3.Distance(tTargetStation.gameObject.transform.position,
						tRelayVessel.gameObject.transform.position)
						+ Vector3.Distance(tRelayVessel.gameObject.transform.position,
						FlightGlobals.ActiveVessel.gameObject.transform.position);
				}
				else
					sConversation4 = "";
			}

			if (fReturnRange < 600000f)
			{
				fReturnDelay = 0.5f;
			}
			else
				if (fReturnRange < 20000000f)
				{
					fReturnDelay = fReturnDelay - 0.5f;
				}
				else
					if (fReturnRange < 100000000f)
					{
						fReturnDelay = fReturnDelay + 0.2f + (fReturnRange / 100000000f);
					}
					else
						if (fReturnRange < 2500000000f)
						{
							fReturnDelay = fReturnDelay + 0.5f + (fReturnRange / 200000000f);
						}
						else
						{
							fReturnDelay = fReturnDelay + 1.0f + (fReturnRange / 300000000f);
						}

			if (tTargetVessel == tRelayVessel) fReturnDelay = fReturnDelay - 0.25f;

			if (bRelayUsed) fReturnDelay = fReturnDelay / 1.5f;

			if (fReturnDelay < 0.5f) fReturnDelay = 0.5f;

			if (fReturnDelay > 4.5f) fReturnDelay = 4.5f;

			// Random signal delay
			System.Random rnd = new System.Random();
			int irand = rnd.Next(480);

			if (irand < 21)
				fReturnDelay = fReturnDelay + (float)irand / 40f;

			if (bClean)
				fReturnDelay = fReturnDelay / 2f;

			return fReturnDelay;
		}

		public static Boolean VesselAOccludedFromVesselB (Vessel vVesselA, Vessel vVesselB)
		{
			CelestialBody cActive = vVesselA.mainBody;
			CelestialBody cTarget = vVesselB.mainBody;

			Vector3d VesselPos = vVesselA.gameObject.transform.position;
			Vector3d PlanetPos = cActive.gameObject.transform.position;
			Vector3d TargetPos = vVesselB.gameObject.transform.position;

			float fPlanetRange = 0f;
			float fVesselRange = 0f;

			Vector3d vDirectionToPlanet = (PlanetPos - VesselPos);
			float fAngleToPlanet = Vector3.Angle(vVesselA.gameObject.transform.up, vDirectionToPlanet);

			Vector3d vDirectionToTarget = (TargetPos - VesselPos);
			float fAngleToTarget = Vector3.Angle(vVesselA.gameObject.transform.up, vDirectionToTarget);
			
			float fAngleDiff = 0f;
			
			if (fAngleToPlanet > fAngleToTarget) fAngleDiff = fAngleToPlanet - fAngleToTarget;
			if (fAngleToTarget > fAngleToPlanet) fAngleDiff = fAngleToTarget - fAngleToPlanet;

			fPlanetRange = Vector3.Distance(TargetPos, PlanetPos);
			fVesselRange = Vector3.Distance(TargetPos, VesselPos);

			if (cActive == cTarget)
			{
				if (fAngleDiff > 75f)
				{
					sConversation5 = "\nNo occlusion from\n" + cActive.name
						+ "\n" + fAngleDiff.ToString("#0") + "°";
					return false;
				}
			}
			else
				if (fAngleDiff > 5f)
				{
					sConversation5 = "\nNo occlusion from\n" + cActive.name
						+ "\n" + fAngleDiff.ToString("#0") + "°";
					return false;
				}

			if (fVesselRange > fPlanetRange)
			{
				sConversation6 = "\nOccluded by\n" + cActive.name + "\n" + fAngleDiff.ToString("#0") + "°";
				return true;
			}

			sConversation5 = "\nNo occlusion from\n" + cActive.name + "\n" + fAngleDiff.ToString("#0") + "°";
			return false;
		}

		public static Boolean VesselOccludedFromStation (Vessel vVesselSource, StaticObject soStationTarget)
		{
			CelestialBody cActive = vVesselSource.mainBody;
			CelestialBody cTarget = (CelestialBody)soStationTarget.getSetting("CelestialBody");

			Vector3d VesselPos = vVesselSource.gameObject.transform.position;
			Vector3d PlanetPos = cActive.gameObject.transform.position;
			Vector3d TargetPos = soStationTarget.gameObject.transform.position;

			float fPlanetRange = 0f;
			float fVesselRange = 0f;

			Vector3d vDirectionToSource = (VesselPos - PlanetPos);
			float fAngleToSource = Vector3.Angle(cActive.gameObject.transform.up, vDirectionToSource);

			Vector3d vDirectionToTarget = (TargetPos - PlanetPos);
			float fAngleToTarget = Vector3.Angle(cActive.gameObject.transform.up, vDirectionToTarget);

			float fAngleDiff = 0f;
			if (fAngleToSource > fAngleToTarget) fAngleDiff = fAngleToSource - fAngleToTarget;
			if (fAngleToTarget > fAngleToSource) fAngleDiff = fAngleToTarget - fAngleToSource;

			fPlanetRange = Vector3.Distance(TargetPos, PlanetPos);
			fVesselRange = Vector3.Distance(TargetPos, VesselPos);

			StationLOS = TrackingStationGUI.StationHasLOS(soStationTarget, vVesselSource);

			fMaxAngle = (float)soStationTarget.getSetting("TrackingAngle");

			if (StationLOS > fMaxAngle) return true;

			if (fAngleDiff > 5f) return false;

			if (fVesselRange > fPlanetRange) return true;

			return false;
		}

		public static Boolean StationOccludedFromVessel(StaticObject soStationSource, Vessel vVesselTarget)
		{
			CelestialBody cActive = (CelestialBody)soStationSource.getSetting("CelestialBody");;
			CelestialBody cTarget = vVesselTarget.mainBody;

			Vector3d VesselPos = soStationSource.gameObject.transform.position;
			Vector3d PlanetPos = cActive.gameObject.transform.position;
			Vector3d TargetPos = vVesselTarget.gameObject.transform.position;

			float fPlanetRange = 0f;
			float fVesselRange = 0f;

			Vector3d vDirectionToSource = (VesselPos - PlanetPos);
			float fAngleToSource = Vector3.Angle(cActive.gameObject.transform.up, vDirectionToSource);

			Vector3d vDirectionToTarget = (TargetPos - PlanetPos);
			float fAngleToTarget = Vector3.Angle(cActive.gameObject.transform.up, vDirectionToTarget);

			float fAngleDiff = 0f;
			if (fAngleToSource > fAngleToTarget) fAngleDiff = fAngleToSource - fAngleToTarget;
			if (fAngleToTarget > fAngleToSource) fAngleDiff = fAngleToTarget - fAngleToSource;

			fPlanetRange = Vector3.Distance(TargetPos, PlanetPos);
			fVesselRange = Vector3.Distance(TargetPos, VesselPos);

			StationLOS = TrackingStationGUI.StationHasLOS(soStationSource, vVesselTarget);

			fMaxAngle = (float)soStationSource.getSetting("TrackingAngle");

			if (StationLOS > fMaxAngle) return true;

			if (fAngleDiff > 5f) return false;

			if (fVesselRange > fPlanetRange) return true;

			return false;
		}

		public static void changeTarget(Boolean bChange)
		{
			bChangeTarget = bChange;
		}

		public static void SaveCommsState()
		{
			bool bRelay = false;
			if (sIsRelay == "Yes") bRelay = true;
			PersistenceUtils.saveCommsPersistence(FlightGlobals.ActiveVessel, pAntenna, sTargetType, sTarget, bRelay, sRelayName, sStationRadial);
		
			if (sTargetType == "Craft")
			{
				bool bRegistered = true;
				Vessel vTargetVessel = TrackingStationGUI.GetTargetVessel(sTarget);
				if (vTargetVessel != null)
					bRegistered = CheckCommsRegistered(vTargetVessel);
				else
					bRegistered = false;

				bUnregistered = !bRegistered;

				bCheckedRegistry = true;				
			}
		}

		public static bool CheckCommsRegistered(Vessel vVessel)
		{
			bool bRegistered = false;
			string sVesselID = vVessel.id.ToString();

			string saveConfigPath = string.Format("{0}saves/{1}/KKComms.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);

			ConfigNode rootNode = new ConfigNode();

			if (!File.Exists(saveConfigPath))
			{
				ConfigNode GameNode = rootNode.AddNode("GAME");
				ConfigNode ScenarioNode = GameNode.AddNode("SCENARIO");
				ScenarioNode.AddValue("Name", "KKComms");
				rootNode.Save(saveConfigPath);
			}

			rootNode = ConfigNode.Load(saveConfigPath);
			ConfigNode rootrootNode = rootNode.GetNode("GAME");

			foreach (ConfigNode ins in rootrootNode.GetNodes("SCENARIO"))
			{
				if (bRegistered) break;
				
				if (ins.GetValue("Name") == "KKComms")
				{
					foreach (ConfigNode insins in ins.GetNodes("KKComm"))
					{
						if (insins.GetValue("VesselID") == sVesselID)
						{
							bRegistered = true;
							break;
						}
					}
				}
			}

			return bRegistered;
		}

		public bool VesselIsUncrewed(Vessel vVessel)
		{
			int iCrew = 0;

			if (vVessel == null) return false;

			if (vVessel.loaded)
				iCrew = vVessel.GetCrewCount();
			else
				iCrew = vVessel.protoVessel.protoPartSnapshots.Sum(pps => pps.protoModuleCrew.Count);

			if (iCrew < 1) return true;

			return false;
		}

		public void LoadCommsPersistence(Part pPart)
		{
			string sPartID = pPart.flightID.ToString();

			string saveConfigPath = string.Format("{0}saves/{1}/KKComms.cfg", KSPUtil.ApplicationRootPath, HighLogic.SaveFolder);

			ConfigNode rootNode = new ConfigNode();

			if (!File.Exists(saveConfigPath))
			{
				ConfigNode GameNode = rootNode.AddNode("GAME");
				ConfigNode ScenarioNode = GameNode.AddNode("SCENARIO");
				ScenarioNode.AddValue("Name", "KKComms");
				rootNode.Save(saveConfigPath);
			}

			rootNode = ConfigNode.Load(saveConfigPath);
			ConfigNode rootrootNode = rootNode.GetNode("GAME");

			Boolean bMatch = false;

			foreach (ConfigNode ins in rootrootNode.GetNodes("SCENARIO"))
			{
				if (ins.GetValue("Name") == "KKComms")
				{
					foreach (ConfigNode insins in ins.GetNodes("KKComm"))
					{
						string sPartKey = insins.GetValue("PartID");

						if (sPartKey == sPartID)
						{
							sTarget = insins.GetValue("TargetID");
							sTargetType = insins.GetValue("TargetType");
							sIsRelay = insins.GetValue("isRelay");
							sVesselName = insins.GetValue("VesselID");
							sRelayName = insins.GetValue("RelayTarget");
							sStationRadial = insins.GetValue("StationKey");

							if (sTargetType == "Craft" && sStationRadial != "None")
							{
								Debug.Log("KK: Antenna persistence has both craft and station as a target. Probably old test-data. Ignoring target station.");
								sStationRadial = "None";
							}

							soTargetStation = null;

							if (sStationRadial != "None")
							{
								foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
								{
									if ((string)obj.getSetting("FacilityType") == "TrackingStation")
									{
										if ((string)obj.getSetting("OpenCloseState") == "Closed") continue;

										var FacilityKey = obj.getSetting("RadialPosition");

										if (sStationRadial == FacilityKey.ToString())
										{
											soTargetStation = obj;
											break;
										}

									}
								}
							}

							if (sTargetType == "Craft")
							{
								bool bRegistered = true;
								vTargetVessel = TrackingStationGUI.GetTargetVessel(sTarget);
								if (vTargetVessel != null)
									bRegistered = CheckCommsRegistered(vTargetVessel);
								else
									bRegistered = false;

								bUnregistered = !bRegistered;

								bCheckedRegistry = true;
							}

							bMatch = true;
							break;
						}
					}
					break;
				}
			}

			if (!bMatch)
			{
				bool bRelay = false;
				if (sIsRelay == "Yes") bRelay = true;

				PersistenceUtils.saveCommsPersistence(FlightGlobals.ActiveVessel, pPart, sTargetType, sTarget, bRelay, sRelayName, sStationRadial);
			}
		}

	}
}
