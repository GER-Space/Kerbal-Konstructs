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

        internal string DefaultLaunchPadTransform;
        internal float DefaultLaunchSiteLength;
        internal float DefaultLaunchSiteWidth;

        internal bool keepConvex;
        internal bool isSquad = false;

        // need checking
        internal float cost;

        // facility settings below
        internal string DefaultFacilityType = "None";
        internal float DefaultFacilityLength;
        internal float DefaultFacilityWidth ;
        internal float DefaultFacilityHeight;
        internal float DefaultFacilityMassCapacity;
        internal float DefaultFacilityCraftCapacity;

        internal float DefaultStaffMax;
        internal float LqFMax;
        internal float OxFMax;
        internal float MoFMax;
        internal float DefaultProductionRateMax;
        internal float DefaultScienceOMax;
        internal float DefaultRepOMax;
        internal float DefaultFundsOMax;
	}
}
