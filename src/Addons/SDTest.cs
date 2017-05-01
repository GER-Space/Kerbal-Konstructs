using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs;
using KerbalKonstructs.Core;
using UnityEngine;


/// <summary>
/// Test implementation for Simga88
/// </summary>

namespace KerbalKonstructs.Addons
{
    class SDTest
    {

        private static Dictionary<string, string> pos2Group = new Dictionary<string, string>();
        private static Dictionary<string, Vector3> group2Center = new Dictionary<string, Vector3>();

        // needed for dynamic group center
        private static Dictionary<string, List<Vector3>> groupMembers = new Dictionary<string, List<Vector3>>();


        private static bool initialized = false;

        private static void Add2Group(string groupName, Vector3 pos )
        {
            if (! groupMembers.ContainsKey(groupName) )
            {
                groupMembers.Add(groupName, new List<Vector3>());
            }
            groupMembers[groupName].Add(pos);

            if (!pos2Group.ContainsKey(pos.ToString()))
            {
                pos2Group.Add(pos.ToString(), groupName);
            }
        }


        private static void ScanGroups()
        {
            string group = "";
            string key = "";

            group2Center.Add("KSCUpgrades", new Vector3(157000, -1000, -570000) ) ;

            foreach (StaticObject instance in KerbalKonstructs.instance.getStaticDB().GetAllStatics() )
            {
                group = (string)instance.getSetting("Group");
                key = instance.pqsCity.repositionRadial.ToString();

                switch (group)
                {
                    case "KSCUpgrades":
                        break;
                    case "Ungrouped":
                        break;
                    default:
                        // we only update the groupcenter when we are not KSCUgrades or Ungrouped
                        if (!group2Center.ContainsKey(group))
                        {
                            group2Center.Add(group, Vector3.zero);
                        }
                        int count = (groupMembers.ContainsKey(group)) ? groupMembers[group].Count : 0; 
                        group2Center[group] = group2Center[group] * count/(count+1) + instance.pqsCity.repositionRadial/(count + 1) ;
                        break;
                }

                Add2Group(group, instance.pqsCity.repositionRadial);
            }
            initialized = true;
        }

        /// <summary>
        /// Returns Vector3.zero if no group is found, else the Groups Center
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector3 GetCenter (Vector3 position)
        {
            if (! initialized)
            {
                ScanGroups();
            }

            if (!pos2Group.ContainsKey(position.ToString()) ) {
                return Vector3.zero;
            }
            string groupName = pos2Group[position.ToString()];

            if (groupName == "Ungrouped")
            {
                return position;
            }

            return group2Center[groupName];
        }
    }
}
