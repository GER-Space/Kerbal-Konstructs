using System.Collections.Generic;
using UnityEngine;
using KerbalKonstructs.Utilities;

namespace KerbalKonstructs.Core
{
	public class StaticModel
	{
        internal GameObject prefab;
       // internal Dictionary<string, object> settings = new Dictionary<string, object>();
        internal List<StaticModule> modules = new List<StaticModule>();

        internal string config;
        internal string configPath;
        internal string path;

        [CFGSetting]
        internal string name;
        [CFGSetting]
        internal string mesh;
        [CFGSetting]
        internal string author = "Unknown";
        [CFGSetting]
        internal string title;
        [CFGSetting]
        internal string category;
        [CFGSetting]
        internal string manufacturer;
        [CFGSetting]
        internal string description;

        [CFGSetting]
        internal string DefaultLaunchPadTransform;
        [CFGSetting]
        internal float DefaultLaunchSiteLength;
        [CFGSetting]
        internal float DefaultLaunchSiteWidth;

        [CFGSetting]
        internal bool keepConvex;
        [CFGSetting]
        internal bool isSquad = false;

        // need checking
        [CFGSetting]
        internal float cost;

        // facility settings below
        [CFGSetting]
        internal string DefaultFacilityType = "None";


        [CFGSetting]
        internal float DefaultFacilityMassCapacity;
        [CFGSetting]
        internal int DefaultFacilityCraftCapacity;

        [CFGSetting]
        internal int DefaultStaffMax;
        [CFGSetting]
        internal float DefaultProductionRateMax;
        [CFGSetting]
        internal float DefaultScienceOMax;
        [CFGSetting]
        internal float DefaultFundsOMax;
	}
}
