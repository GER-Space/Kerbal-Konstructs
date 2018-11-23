using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace KerbalKonstructs.Core
{
    class LaunchSiteChecks
    {
        internal static AsmUtils.Detour preFlightCheckDetour;
        internal static AsmUtils.Detour findVesselDetour;
        internal static AsmUtils.Detour findVesselDetour2;

        internal static void PrepareSystem()
        {

            MethodBase oldCheckFunction = typeof(EditorLogic).GetMethod("GetStockPreFlightCheck", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodBase newCheckFunction = typeof(LaunchSiteChecks).GetMethod("NewPreflightCheck", BindingFlags.Instance | BindingFlags.Public);

            preFlightCheckDetour  = new AsmUtils.Detour(oldCheckFunction, newCheckFunction);
            preFlightCheckDetour.Install();

            MethodBase oldFindVesselFunction = typeof(ShipConstruction).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(mi => mi.Name == "FindVesselsLandedAt" && mi.GetParameters().Length == 6).FirstOrDefault();
            MethodBase newFindVesselFunction = typeof(LaunchSiteChecks).GetMethod("FindVesselsLandedAtKK", BindingFlags.Static | BindingFlags.Public);

            findVesselDetour = new AsmUtils.Detour(oldFindVesselFunction, newFindVesselFunction);
            findVesselDetour.Install();

            MethodBase oldFindVesselFunction2 = typeof(ShipConstruction).GetMethods(BindingFlags.Static | BindingFlags.Public)
    .Where(mi => mi.Name == "FindVesselsLandedAt" && mi.GetParameters().Length == 2).FirstOrDefault();
            MethodBase newFindVesselFunction2 = typeof(LaunchSiteChecks).GetMethod("FindVesselsLandedAt2", BindingFlags.Static | BindingFlags.Public);

            findVesselDetour2 = new AsmUtils.Detour(oldFindVesselFunction2, newFindVesselFunction2);
            findVesselDetour2.Install();


        }

        /// <summary>
        /// Appends our custom checks to the prefilled checks
        /// </summary>
        /// <param name="launchSiteName"></param>
        /// <returns></returns>
        public  PreFlightCheck NewPreflightCheck(string launchSiteName)
        {
            Log.Normal("using injected call");
            PreFlightCheck check = (PreFlightCheck)preFlightCheckDetour.CallOriginal(EditorLogic.fetch, new object [] { launchSiteName } );
            check.AddTest(new KKPrelaunchSizeCheck(launchSiteName));
            check.AddTest(new KKPrelaunchMassCheck(launchSiteName));
            check.AddTest(new KKPrelaunchPartCheck(launchSiteName));

            return check;
        }


        /// <summary>
        ///  Checks the size of non-Squad LaunchSites
        /// </summary>
        public class KKPrelaunchSizeCheck :  PreFlightTests.IPreFlightTest
        {

            Vector3 shipSize;
            KKLaunchSite launchSite;

            bool allowLaunch = false;


            public KKPrelaunchSizeCheck(string launchSiteName)
            {
                if (HighLogic.LoadedScene == GameScenes.EDITOR)
                {
                    shipSize = ShipConstruction.CalculateCraftSize(EditorLogic.fetch.ship);
                    launchSite = LaunchSiteManager.GetLaunchSiteByName(launchSiteName);
                }
                else
                {
                    allowLaunch = true;
                }
            }

            public bool Test()
            {
                if (allowLaunch)
                {
                    return true;
                }

                if (launchSite == null)
                {
                    return true;
                }

                if (launchSite.isSquad)
                {
                    return true;
                }
                if (shipSize == Vector3.zero)
                {
                    return false;
                }

                bool retval = false;
                if (launchSite.LaunchSiteWidth == 0f || launchSite.LaunchSiteWidth > Math.Round(shipSize.x, 1))
                {
                    retval = true;
                }
                if (retval && (launchSite.LaunchSiteLength == 0f || (launchSite.LaunchSiteLength > Math.Round(shipSize.z, 1))))
                {
                    retval = true;
                }
                else
                {
                    retval = false;
                }
                if (retval && (launchSite.LaunchSiteHeight == 0f || (launchSite.LaunchSiteHeight > Math.Round(shipSize.y, 1))))
                {
                    retval = true;
                }
                else
                {
                    retval = false;
                }

                //Log.Normal("Ship dimensions: " + shipSize.ToString() );
                return retval;
            }



            public string GetWarningTitle()
            {
                return ("KK Launch Size Check");
            }


            public string GetWarningDescription()
            {
                return ("Max allowed size is " + launchSite.LaunchSiteWidth + "m x " + launchSite.LaunchSiteLength + "m x " + launchSite.LaunchSiteHeight + "\n"
                    + "Vessel size: " +  Math.Round(shipSize.x,1) + "m x " + Math.Round(shipSize.z, 1) + "m x " + Math.Round(shipSize.y, 1) + "m");
            }

            public string GetProceedOption()
            {
                return null;
            }

            public string GetAbortOption()
            {
                return "Craft is too big for: " + launchSite.LaunchSiteName ;
            }
        }



        /// <summary>
        ///  Checks the size of non-Squad LaunchSites
        /// </summary>
        public class KKPrelaunchMassCheck : PreFlightTests.IPreFlightTest
        {

            float shipMass;
            KKLaunchSite launchSite;

            bool allowLaunch = false;


            public KKPrelaunchMassCheck(string launchSiteName)
            {
                if (HighLogic.LoadedScene == GameScenes.EDITOR)
                {
                    shipMass = EditorLogic.fetch.ship.GetTotalMass();
                    launchSite = LaunchSiteManager.GetLaunchSiteByName(launchSiteName);
                }
                else
                {
                    allowLaunch = true;
                }
            }

            public bool Test()
            {
                if (allowLaunch)
                {
                    return true;
                }

                if (launchSite == null)
                {
                    return true;
                }

                if (launchSite.isSquad)
                {
                    return true;
                }
                if (shipMass == 0f)
                {
                    return false;
                }

                if (launchSite.MaxCraftMass == 0f || launchSite.MaxCraftMass > Math.Round(shipMass, 1))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                
            }



            public string GetWarningTitle()
            {
                return ("KK Vessel Mass Check");
            }


            public string GetWarningDescription()
            {
                return ("Max allowed Mass is " + launchSite.MaxCraftMass + "t\n"
                    + "Vessel Mass: " + Math.Round(shipMass,1) + "t");
            }

            public string GetProceedOption()
            {
                return null;
            }

            public string GetAbortOption()
            {
                return "Craft is too big for: " + launchSite.LaunchSiteName;
            }
        }


        /// <summary>
        ///  Checks the size of non-Squad LaunchSites
        /// </summary>
        public class KKPrelaunchPartCheck : PreFlightTests.IPreFlightTest
        {

            int shipParts;
            KKLaunchSite launchSite;

            bool allowLaunch = false;


            public KKPrelaunchPartCheck(string launchSiteName)
            {
                if (HighLogic.LoadedScene == GameScenes.EDITOR)
                {
                    shipParts = EditorLogic.fetch.ship.parts.Count;
                    launchSite = LaunchSiteManager.GetLaunchSiteByName(launchSiteName);
                }
                else
                {
                    allowLaunch = true;
                }
            }

            public bool Test()
            {
                if (allowLaunch)
                {
                    return true;
                }

                if (launchSite == null)
                {
                    return true;
                }

                if (launchSite.isSquad)
                {
                    return true;
                }
                if (shipParts == 0)
                {
                    return false;
                }

                if (launchSite.MaxCraftParts == 0 || launchSite.MaxCraftParts > shipParts)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }



            public string GetWarningTitle()
            {
                return ("KK Vessel Part Check");
            }


            public string GetWarningDescription()
            {
                return ("Max allowed parts are " + launchSite.MaxCraftParts + "\n"
                    + "Vessel part count: " + shipParts);
            }

            public string GetProceedOption()
            {
                return null;
            }

            public string GetAbortOption()
            {
                return "Craft is too complex for: " + launchSite.LaunchSiteName;
            }
        }




        /// <summary>
        /// ReplaceMent for finding vessels
        /// </summary>
        /// <param name="flightState"></param>
        /// <param name="landedAt"></param>
        /// <param name="count"></param>
        /// <param name="name"></param>
        /// <param name="idx"></param>
        /// <param name="vType"></param>
        public static void FindVesselsLandedAtKK(FlightState flightState, string landedAt, out int count, out string name, out int idx, out VesselType vType)
        {
           // Log.Normal("Called");
            float maxDistance = 100f;

            int vesselIndex = 0;
            KKLaunchSite launchSite = LaunchSiteManager.GetLaunchSiteByName(landedAt);
            count = 0;
            name = string.Empty;
            idx = 0;
            vType = VesselType.Debris;

            if (flightState != null)
            {
                foreach (ProtoVessel vessel in flightState.protoVessels)
                {
                    if (vessel == null)
                    {
                        continue;
                    }

                    if (launchSite == null || launchSite.isSquad)
                    {
                        if (vessel.landedAt.Contains(landedAt))
                        {
                            count++;
                            name = vessel.vesselName;
                            idx = vesselIndex;
                            vType = vessel.vesselType;
                            break;
                        }
                    }
                    else
                    {
                        //if (vessel.situation == Vessel.Situations.SPLASHED || vessel.situation == Vessel.Situations.LANDED || vessel.situation == Vessel.Situations.PRELAUNCH )
//                        {
////                            Log.Normal("Body: "+ FlightGlobals.Bodies[vessel.orbitSnapShot.ReferenceBodyIndex].name);
                            CelestialBody body = FlightGlobals.Bodies[vessel.orbitSnapShot.ReferenceBodyIndex];
                            Vector3 position = body.GetWorldSurfacePosition(vessel.latitude, vessel.longitude, vessel.altitude);
                            float distance = Vector3.Distance(position, launchSite.lsGameObject.transform.position);
                            Log.Normal("Vessel with distance: " + distance);
                            if (distance < Math.Min(launchSite.LaunchSiteWidth, maxDistance))
                            {
                                Log.Normal("Found Vessel at Launchsite with distance: " + distance);
                                count++;
                                name = vessel.vesselName;
                                idx = vesselIndex;
                                vType = vessel.vesselType;
                                break;
                            }
                        //}
                    }
                    vesselIndex++;   
                }
            }
        }




        public static List<ProtoVessel> FindVesselsLandedAt2(FlightState flightState, string landedAt)
        {
            float maxDistance = 100f;
            List<ProtoVessel> list = new List<ProtoVessel>();
            if (flightState != null)
            {
                KKLaunchSite launchSite = LaunchSiteManager.GetLaunchSiteByName(landedAt);

                foreach (ProtoVessel vessel in flightState.protoVessels)
                {
                    if (vessel == null)
                    {
                        continue;
                    }

                    if (launchSite == null || launchSite.isSquad)
                    {
                        if (vessel.landedAt.Contains(landedAt))
                        {
                            list.Add(vessel);
                            
                        }
                    }
                    else
                    {
                        //if (vessel.situation == Vessel.Situations.SPLASHED || vessel.situation == Vessel.Situations.LANDED || vessel.situation == Vessel.Situations.PRELAUNCH)
                        //{                          
                            CelestialBody body = FlightGlobals.Bodies[vessel.orbitSnapShot.ReferenceBodyIndex];
                            Vector3 position = body.GetWorldSurfacePosition(vessel.latitude, vessel.longitude, vessel.altitude);
                            float distance = Vector3.Distance(position, launchSite.lsGameObject.transform.position);

                            if (distance < Math.Min(launchSite.LaunchSiteWidth, maxDistance))
                            {
                                Log.Normal("Found Vessel at Launchsite with distance: " + distance);
                                list.Add(vessel);
                            }
                        //}
                    }

                }

            }
            return list;
        }


    }


}