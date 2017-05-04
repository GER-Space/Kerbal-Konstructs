using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using KerbalKonstructs.Core;
using UnityEngine;

namespace KerbalKonstructs.Modules
{
    internal static class CareerUtils
    {

        private static List<string> stringAttributes = new List<string>{
            "FacilityType", "InStorage", "TargetID", "TargetType" , "Producing", "OpenCloseState", "missionlog" , "openclosestate" , "favouritesite" };

        private static List<string> floatAttributes = new List<string>{
            "FacilityLengthUsed", "FacilityWidthUsed", "FacilityHeightUsed", "FacilityMassUsed",
            "FacilityXP",
            "StaffMax", "StaffCurrent", "LqFCurrent", "OxFCurrent", "MoFCurrent",
            "ECCurrent", "OreCurrent", "PrOreCurrent", "ScienceOMax", "ScienceOCurrent",
            "RepOMax", "RepOCurrent", "FundsOMax", "FundsOCurrent", "LastCheck",
            "ProductionRateMax", "ProductionRateCurrent", "MissionCount",
            "missioncount" };

        private static List<string> vector3Attributes = new List<string> { "RadialPosition" };

        private static List<string> boolAttributes = new List<string>();


        internal static bool isCareerGame
        {
            get
            {
                return ((HighLogic.CurrentGame.Mode == Game.Modes.CAREER) & (!KerbalKonstructs.instance.disableCareerStrategyLayer));
            }
        }

        internal static bool isSandboxGame
        {
            get
            {
                return ((HighLogic.CurrentGame.Mode != Game.Modes.CAREER) || (KerbalKonstructs.instance.disableCareerStrategyLayer));
            }
        }

        /// <summary>
        /// Returns a list with the neccessary parameters for a facility
        /// </summary>
        /// <param name="facType"></param>
        /// <returns></returns>
        internal static List<string> ParametersForFacility(string facType)
        {
            List<string> paramList = new List<string> { "OpenCloseState" };


            switch (facType)
            {
                case "Barracks":
                    paramList.Add("StaffCurrent");
                    break;
                case "FuelTanks":
                    paramList.Add("LqFCurrent");
                    paramList.Add("OxFCurrent");
                    paramList.Add("MoFCurrent");
                    break;
                case "Hangar":
                    paramList.Add("InStorage");
                    paramList.Add("TargetID");
                    paramList.Add("TargetType");
                    paramList.Add("FacilityLengthUsed");
                    paramList.Add("FacilityWidthUsed");
                    paramList.Add("FacilityHeightUsed");
                    paramList.Add("FacilityMassUsed");
                    break;
                case "Business":
                    paramList.Add("StaffCurrent");
                    paramList.Add("FundsOCurrent");
                    break;
                case "Research":
                    paramList.Add("StaffCurrent");
                    paramList.Add("ScienceOCurrent");
                    break;
                case "Mining":
                    paramList.Add("StaffCurrent");
                    paramList.Add("OreCurrent");
                    break;
                case "Manufacturing":
                    paramList.Add("StaffCurrent");
                    paramList.Add("Producing");
                    paramList.Add("PrOreCurrent");
                    break;
                default:
                    break;
            }

            return paramList;
        }

        /// <summary>
        /// returns a list with the necessarry parameters for a LaunchSite
        /// </summary>
        /// <returns></returns>
        internal static List<string> ParametersForLaunchSite()
        {
            List<string> paramList = new List<string> { "openclosestate", "favouritesite", "missioncount", "missionlog" };
            return paramList;
        }

        /// <summary>
        /// Checks if a facility should be treated as "Open"
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        internal static bool FacilityIsOpen(StaticObject instance)
        {
            if (isSandboxGame)
            {
                return true;
            }
            if (isCareerGame)
            {
                if ((string)instance.getSetting("OpenCloseState") == "Open" || (string)instance.getSetting("OpenCloseState") == "OpenLocked" )
                {
                    return true;
                }

                if ( ( !instance.settings.ContainsKey("OpenCost") ) || ((float)instance.getSetting("OpenCost") == 0f ) )
                {
                    return true;
                }

            }

            return false;

        }

        internal static bool IsString (string parameter)
        {
            return stringAttributes.Contains(parameter);
        }

        internal static bool IsFloat(string parameter)
        {
            return floatAttributes.Contains(parameter);
        }

        internal static bool IsVector3(string parameter)
        {
            return vector3Attributes.Contains(parameter);
        }
   
        internal static bool IsBool(string parameter)
        {
            return boolAttributes.Contains(parameter);
        }

        internal static string KeyFromString(string orginalString)
        {
            return Regex.Replace(orginalString, @"\s+", "");
        }

        internal static string LSKeyFromName(string name)
        {
            return name.Replace(' ', '_').ToUpperInvariant();
        }

    }
}
