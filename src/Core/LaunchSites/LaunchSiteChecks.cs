using CustomPreLaunchChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace KerbalKonstructs.Core
{
    internal class LaunchSiteChecks
    {
        internal static AsmUtils.Detour findVesselDetour;
        internal static AsmUtils.Detour findVesselDetour2;

        internal static void PrepareSystem()
        {

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


            CPLC.RegisterCheck(GetSizeCheck);
            CPLC.RegisterCheck(GetMassCheck);
            CPLC.RegisterCheck(GetPartCheck);

        }


        public static PreFlightTests.IPreFlightTest GetSizeCheck(string launchSiteName)
        {
            return new KKPrelaunchSizeCheck(launchSiteName);
        }
        public static PreFlightTests.IPreFlightTest GetMassCheck(string launchSiteName)
        {
            return new KKPrelaunchMassCheck(launchSiteName);
        }
        public static PreFlightTests.IPreFlightTest GetPartCheck(string launchSiteName)
        {
            return new KKPrelaunchPartCheck(launchSiteName);
        }


        /// <summary>
        ///  Checks the size of non-Squad LaunchSites
        /// </summary>
        public class KKPrelaunchSizeCheck : PreFlightTests.IPreFlightTest
        {

            private Vector3 shipSize;
            private KKLaunchSite launchSite;

            private bool allowLaunch = false;


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

                return retval;
            }

            public string GetWarningTitle()
            {
                return ("KK Launch Size Check");
            }


            public string GetWarningDescription()
            {
                return ("Max allowed size is " + launchSite.LaunchSiteWidth + "m x " + launchSite.LaunchSiteLength + "m x " + launchSite.LaunchSiteHeight + "\n"
                    + "Vessel size: " + Math.Round(shipSize.x, 1) + "m x " + Math.Round(shipSize.z, 1) + "m x " + Math.Round(shipSize.y, 1) + "m");
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
        public class KKPrelaunchMassCheck : PreFlightTests.IPreFlightTest
        {

            private float shipMass;
            private KKLaunchSite launchSite;

            private bool allowLaunch = false;


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
                    + "Vessel Mass: " + Math.Round(shipMass, 1) + "t");
            }

            public string GetProceedOption()
            {
                return null;
            }

            public string GetAbortOption()
            {
                return "Craft is too heavy for: " + launchSite.LaunchSiteName;
            }
        }


        /// <summary>
        ///  Checks the size of non-Squad LaunchSites
        /// </summary>
        public class KKPrelaunchPartCheck : PreFlightTests.IPreFlightTest
        {

            private int shipParts;
            private KKLaunchSite launchSite;

            private bool allowLaunch = false;


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
                if (allowLaunch || launchSite == null || launchSite.isSquad)
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

                    if (VesselAtLaunchSite(vessel, launchSite, maxDistance, landedAt))
                    {
                        count++;
                        name = vessel.vesselName;
                        idx = vesselIndex;
                        vType = vessel.vesselType;
                        break;
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

                    if (VesselAtLaunchSite(vessel, launchSite, maxDistance, landedAt))
                    {
                        list.Add(vessel);
                    }
                }
            }
            return list;
        }

        public static bool VesselAtLaunchSite(ProtoVessel vessel, KKLaunchSite launchSite, float maxDistance, string landedAt)
        {
            if (launchSite == null || launchSite.isSquad)
            {
                if (vessel.landedAt.Contains(landedAt))
                {
                    return true;
                }
                else if (launchSite == null)
                {
                    Log.Normal($"Launchsite is null, did not find vessel ({vessel.vesselName}) at Squad launchsite");
                    return false;
                }
            }

            if (vessel.orbitSnapShot.ReferenceBodyIndex == launchSite.body.flightGlobalsIndex)
            {
                if (vessel.situation == Vessel.Situations.SPLASHED || vessel.situation == Vessel.Situations.LANDED || vessel.situation == Vessel.Situations.PRELAUNCH)
                {
                    CelestialBody body = FlightGlobals.Bodies[vessel.orbitSnapShot.ReferenceBodyIndex];
                    Vector3 position = body.GetWorldSurfacePosition(vessel.latitude, vessel.longitude, vessel.altitude);
                    float distance = Vector3.Distance(position, launchSite.staticInstance.gameObject.transform.position);

                    if (body == null)
                    {
                        Log.Normal($"Could not find body for vessel ({vessel.vesselName})");
                        return false;
                    }

                    if (body != launchSite.body)
                    {
                        Log.Normal($"Vessel ({vessel.vesselName}) is on different body than Launchsite");
                        return false;
                    }

                    if (distance == 0)
                    {
                        Log.Normal($"Vessel ({vessel.vesselName}) has same position as Launchsite, probably floating point error. Using gps checks instead.");

                        Vector2d tol = GetLatLonTolerance(launchSite.staticInstance.groupCenter.RefLatitude, launchSite.staticInstance.groupCenter.RefLongitude, maxDistance, body.Radius + launchSite.staticInstance.groupCenter.RadiusOffset);

                        if (Math.Abs(vessel.latitude - launchSite.staticInstance.groupCenter.RefLatitude) > tol.y || Math.Abs(vessel.longitude - launchSite.staticInstance.groupCenter.RefLongitude) > tol.x)
                        {
                            Log.Normal($"Vessel ({vessel.vesselName}) is outside lat/lon tolerance");
                            Log.Debug($"Lat/Lon tolerance: {tol.y}, {tol.x}");
                            Log.Debug($"Vessel Lat/Lon: {vessel.latitude}, {vessel.longitude}");
                            Log.Debug($"Launchsite Lat/Lon: {launchSite.staticInstance.groupCenter.RefLatitude}, {launchSite.staticInstance.groupCenter.RefLongitude}");
                            return false;
                        }
                        else
                        {
                            Log.Normal($"Found Vessel ({vessel.vesselName}) at Launchsite inside lat/lon tolerance");
                            Log.Debug($"Lat/Lon tolerance: {tol.y}, {tol.x}");
                            Log.Debug($"Vessel Lat/Lon: {vessel.latitude}, {vessel.longitude}");
                            Log.Debug($"Launchsite Lat/Lon: {launchSite.staticInstance.groupCenter.RefLatitude}, {launchSite.staticInstance.groupCenter.RefLongitude}");
                            return true;
                        }
                    }
                    else if (distance < maxDistance)
                    {
                        Log.Normal($"Found Vessel ({vessel.vesselName}) at Launchsite with distance: " + distance);
                        return true;
                    }
                }
            }
            return false;
        }

        public static Vector2d GetLatLonTolerance(double lat, double lon, double squareSize, double bodySize)
        {
            Vector2d dest(double latRad, double lonRad, double bearingRad, double angDist)
            {
                Vector2d res = new Vector2d(0, 0);

                // lon = x, lat = y
                res.x = Math.Asin(Math.Sin(latRad) * Math.Cos(angDist) + Math.Cos(latRad) * Math.Sin(angDist) * Math.Cos(bearingRad));
                res.y = lonRad + Math.Atan2(Math.Sin(bearingRad) * Math.Sin(angDist) * Math.Cos(latRad), Math.Cos(angDist) - Math.Sin(latRad) * Math.Sin(res.x));

                res.y = (res.y + Math.PI) % (2 * Math.PI) - Math.PI;

                return res;
            }

            double s = squareSize / 2d;

            double latRads = lat * Math.PI / 180d;
            double lonRads = lon * Math.PI / 180d;
            double delta = s / bodySize;

            Vector2d north = dest(latRads, lonRads, 0d, delta);
            double deltaLat = Math.Abs(north.x - latRads) * 180d / Math.PI;

            Vector2d east = dest(latRads, lonRads, Math.PI / 2d, delta);
            double deltaLon = Math.Abs(east.y - lonRads) * 180d / Math.PI;

            deltaLat = Math.Min(0.2, deltaLat);
            deltaLon = Math.Min(0.2, deltaLon);

            return new Vector2d(deltaLon, deltaLat);
        }
    }
}