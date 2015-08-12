using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using System;
using System.Collections.Generic;
using KerbalKonstructs.API;
using KerbalKonstructs.Utilities;
using UnityEngine;

namespace KerbalKonstructs.UI
{
	public class SharedInterfaces
	{
		public static GUIStyle BoxInfo;

		public static void OpenCloseFacility(StaticObject selectedFacility)
		{
			BoxInfo = new GUIStyle(GUI.skin.box);
			BoxInfo.normal.textColor = Color.cyan;
			BoxInfo.fontSize = 13;
			BoxInfo.padding.top = 2;
			BoxInfo.padding.bottom = 1;
			BoxInfo.padding.left = 5;
			BoxInfo.padding.right = 5;
			BoxInfo.normal.background = null;

			float iFundsOpen2 = (float)selectedFacility.getSetting("OpenCost");
			float iFundsClose2 = (float)selectedFacility.getSetting("CloseValue");

			bool isAlwaysOpen2 = false;
			bool cannotBeClosed2 = false;

			// Career mode
			// If a launchsite is 0 to open it is always open
			if (iFundsOpen2 == 0)
				isAlwaysOpen2 = true;

			// If it is 0 to close you cannot close it
			if (iFundsClose2 == 0)
				cannotBeClosed2 = true;

			if (MiscUtils.isCareerGame())
			{
				bool isOpen2 = ((string)selectedFacility.getSetting("OpenCloseState") == "Open");

				GUILayout.BeginHorizontal();
				{
					if (!isAlwaysOpen2)
					{
						GUI.enabled = !isOpen2;
						if (GUILayout.Button("Open Facility for \n" + iFundsOpen2 + " funds"))
						{
							double currentfunds2 = Funding.Instance.Funds;

							if (iFundsOpen2 > currentfunds2)
							{
								ScreenMessages.PostScreenMessage("Insufficient funds to open this facility!", 10, ScreenMessageStyle.LOWER_CENTER);
							}
							else
							{
								// Open the site - save to instance
								selectedFacility.setSetting("OpenCloseState", "Open");

								// Charge some funds
								Funding.Instance.AddFunds(-iFundsOpen2, TransactionReasons.Cheating);

								// Save new state to persistence
								PersistenceUtils.saveStaticPersistence(selectedFacility);
							}
						}
						GUI.enabled = true;
					}
					else
					{
						// GUILayout.Box("This facility is always open.", BoxInfo);
					}
					
					if (!cannotBeClosed2)
					{
						GUI.enabled = isOpen2;
						if (GUILayout.Button("Close Facility for \n" + iFundsClose2 + " funds"))
						{
							// Close the site - save to instance
							// Pay back some funds
							Funding.Instance.AddFunds(iFundsClose2, TransactionReasons.Cheating);
							selectedFacility.setSetting("OpenCloseState", "Closed");

							// Save new state to persistence
							PersistenceUtils.saveStaticPersistence(selectedFacility);
						}
						GUI.enabled = true;
					}
					else
					{
						// GUILayout.Box("This facility cannot be closed.", BoxInfo);
					}					
				}
				GUILayout.EndHorizontal();
			}
		}
	}
}
