using System.Collections.Generic;
using KerbalKonstructs.API;
using UnityEngine;
using KerbalKonstructs.Utilities;

namespace KerbalKonstructs.Core
{
	internal class StaticModel
	{
        internal GameObject prefab;
       // internal Dictionary<string, object> settings = new Dictionary<string, object>();
        internal List<StaticModule> modules = new List<StaticModule>();

        internal string config;
        internal string configPath;
        internal string path;
        internal string name;

        internal string mesh;
        internal string author = "Unknown";
        internal string title;
        internal string category;
        internal string manufacturer;
        internal string description;

        internal string defaultLaunchPadTransform;
        internal float defaultLaunchSiteLength;
        internal float defaultLaunchSiteWidth;

        internal bool keepConvex;
        internal bool isSquad = false;

        // need checking
        internal float cost;

        // facility settings below
        internal string defaultFacilityType = "None";
        internal float defaultFacilityLength;
        internal float defaultFacilityWidth ;
        internal float defaultFacilityHeight;
        internal float defaultFacilityMassCapacity;
        internal float defaultFacilityCraftCapacity;

        internal float defaultStaffMax;
        internal float lqFMax;
        internal float oxFMax;
        internal float moFMax;
        internal float defaultProductionRateMax;
        internal float defaultScienceOMax;
        internal float defaultRepOMax;
        internal float defaultFundsOMax;
	}
}
