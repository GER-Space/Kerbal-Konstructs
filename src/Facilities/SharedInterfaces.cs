using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using UnityEngine;

namespace KerbalKonstructs.UI
{
    public class SharedInterfaces
    {

        public static GUIStyle BoxInfo;
        public static GUIStyle ButtonSmallText;


        public static void OpenCloseFacility(StaticInstance selectedFacility)
        {
            BoxInfo = new GUIStyle(GUI.skin.box);
            BoxInfo.normal.textColor = Color.white;
            BoxInfo.fontSize = 12;
            BoxInfo.padding.top = 2;
            BoxInfo.padding.bottom = 1;
            BoxInfo.padding.left = 5;
            BoxInfo.padding.right = 5;
            BoxInfo.normal.background = null;

            ButtonSmallText = new GUIStyle(GUI.skin.button);
            ButtonSmallText.fontSize = 12;
            ButtonSmallText.fontStyle = FontStyle.Normal;



            float iFundsOpen2 = selectedFacility.myFacilities[0].OpenCost;
            float iFundsClose2 = selectedFacility.myFacilities[0].CloseValue;
            float iFundsDefaultCost = selectedFacility.model.cost;

            bool isAlwaysOpen2 = false;
            bool cannotBeClosed2 = false;

            // Career mode
            // If a launchsite is 0 to open it is always open
            if (iFundsOpen2 == 0 && iFundsDefaultCost == 0)
                isAlwaysOpen2 = true;

            // If it is 0 to close you cannot close it
            if (iFundsClose2 == 0 && iFundsDefaultCost == 0)
                cannotBeClosed2 = true;

            if (MiscUtils.isCareerGame())
            {
                bool isOpen2 = selectedFacility.myFacilities[0].isOpen;

                GUILayout.BeginHorizontal();
                {
                    if (!isAlwaysOpen2)
                    {
                        GUI.enabled = !isOpen2;

                        if (iFundsOpen2 == 0) iFundsOpen2 = iFundsDefaultCost;

                        if (GUILayout.Button("Open for \n" + iFundsOpen2 + " funds", ButtonSmallText, GUILayout.Height(30)))
                        {
                            double currentfunds2 = Funding.Instance.Funds;

                            if (iFundsOpen2 > currentfunds2)
                            {
                                MiscUtils.HUDMessage("Insufficient funds to open this facility!", 10, 0);
                            }
                            else
                            {
                                // Open the site - save to instance
                                selectedFacility.myFacilities[0].SetOpen();

                                // Charge some funds
                                Funding.Instance.AddFunds(-iFundsOpen2, TransactionReasons.Structures);
                            }
                        }
                        GUI.enabled = true;
                    }

                    if (!cannotBeClosed2)
                    {
                        GUI.enabled = isOpen2;

                        if (iFundsClose2 == 0) iFundsClose2 = iFundsDefaultCost / 5;

                        if (GUILayout.Button("Close for \n" + iFundsClose2 + " refund", ButtonSmallText, GUILayout.Height(30)))
                        {
                            // Close the site - save to instance
                            // Pay back some funds
                            Funding.Instance.AddFunds(iFundsClose2, TransactionReasons.Structures);
                            selectedFacility.myFacilities[0].SetClosed();

                            // Callback to CommNet.
                            if ((selectedFacility.myFacilities[0].FacilityType) == "GroundStation")
                            {
                                Modules.ConnectionManager.DetachGroundStation(selectedFacility);
                            }

                        }

                        GUI.enabled = true;
                    }
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}
