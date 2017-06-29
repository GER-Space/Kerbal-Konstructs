using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KerbalKonstructs.Utilities;

namespace KerbalKonstructs.Core
{
	public static class StaticDatabase
	{
		//Groups are stored by name within the body name
		private static Dictionary<string, Dictionary<string, StaticGroup>> groupList = new Dictionary<string,Dictionary<string,StaticGroup>>();
        private static Dictionary<string, StaticModel> modelList = new Dictionary<string, StaticModel>();

        internal static List<StaticModel> allStaticModels = new List<StaticModel>();

        //make the list private, so nobody does easily add or remove from it. the array is updated in the Add and Remove functions
        // arrays are always optimized (also in foreach loops)
        private static List<StaticInstance> _allStaticInstances = new List<StaticInstance>();
        internal static StaticInstance [] allStaticInstances  = new StaticInstance [0] ;

        internal static string activeBodyName = "";




        /// <summary>
        /// Adds the Instance to the instances and Group lists. Also sets the PQSCity.name
        /// </summary>
        /// <param name="instance"></param>
        internal static void AddStatic(StaticInstance instance)
		{
            _allStaticInstances.Add(instance);
            allStaticInstances = _allStaticInstances.ToArray();

            String bodyName = instance.CelestialBody.bodyName;
			String groupName = instance.Group;

			if (!groupList.ContainsKey(bodyName))
				groupList.Add(bodyName, new Dictionary<string, StaticGroup>());

			if (!groupList[bodyName].ContainsKey(groupName))
			{
				StaticGroup group = new StaticGroup(groupName, bodyName);
				if (groupName == "Ungrouped")
				{
					group.alwaysActive = true;

				}				
				group.active = true;				
				groupList[bodyName].Add(groupName, group);
			}
			groupList[bodyName][groupName].AddStatic(instance);

            SetNewName(instance);

		}


        /// <summary>
        /// Removes a Instance from the group and instance lists.
        /// </summary>
        /// <param name="instance"></param>
        internal static void DeleteStatic(StaticInstance instance)
        {
            if (_allStaticInstances.Contains(instance))
            {
                _allStaticInstances.Remove(instance);
                allStaticInstances = _allStaticInstances.ToArray();
            }
            String bodyName = instance.CelestialBody.bodyName;
            String groupName = instance.Group;

            if (groupList.ContainsKey(bodyName))
            {
                if (groupList[bodyName].ContainsKey(groupName))
                {
                    Log.Normal("StaticDatabase deleteObject");
                    groupList[bodyName][groupName].DeleteObject(instance);
                }
            }
        }

        /// <summary>
        /// Changes the group from a instance
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="newGroup"></param>
        internal static void ChangeGroup(StaticInstance instance, string newGroup)
        {
            String bodyName = instance.CelestialBody.bodyName;
            String groupName = instance.Group;

            instance.Group = newGroup;

            if (groupList.ContainsKey(bodyName))
            {
                if (groupList[bodyName].ContainsKey(groupName))
                {
                    Log.Normal("StaticDatabase deleteObject");
                    groupList[bodyName][groupName].RemoveStatic(instance);
                }
            }

            if (!groupList.ContainsKey(bodyName))
                groupList.Add(bodyName, new Dictionary<string, StaticGroup>());

            if (!groupList[bodyName].ContainsKey(newGroup))
            {
                StaticGroup group = new StaticGroup(newGroup, bodyName);
                if (newGroup == "Ungrouped")
                {
                    group.alwaysActive = true;

                }
                group.active = true;
                groupList[bodyName].Add(newGroup, group);
            }
            groupList[bodyName][newGroup].AddStatic(instance);

            SetNewName(instance);
        }


        /// <summary>
        /// Sets the PQSCity Name to Group_Modenlame_(index of the same models in group)
        /// </summary>
        /// <param name="instance"></param>
        private static void SetNewName(StaticInstance instance)
        {
            string modelName = instance.model.name;
            string groupName = instance.Group; 

            int modelCount = (from obj in groupList[instance.CelestialBody.name][groupName].GetStatics() where obj.model.name == instance.model.name select obj).Count();
            if (modelCount == 0)
            {
                Log.Warning("Shock and Horror! We cannot find at least ourself in our own group");
                return;
            }

            modelCount--;
            instance.indexInGroup = modelCount;
            instance.pqsCity.name = groupName + "_" + instance.model.name + "_" + modelCount;
         //   Log.Normal("PQSCity.name: " + instance.pqsCity.name);
        }

        /// <summary>
        /// toggles the visiblility for all Instances at once
        /// </summary>
        /// <param name="bActive"></param>
        internal static void ToggleActiveAllStatics(bool bActive = true)
		{
            Log.Debug("StaticDatabase.ToggleActiveAllStatics");

			foreach (StaticInstance obj in allStaticInstances)
			{
				InstanceUtil.SetActiveRecursively(obj, bActive);
			}
		}

        /// <summary>
        /// toggles the visiblility of all statics on a planet
        /// </summary>
        /// <param name="cBody"></param>
        /// <param name="bActive"></param>
        /// <param name="bOpposite"></param>
        internal static void ToggleActiveStaticsOnPlanet(CelestialBody cBody, bool bActive = true, bool bOpposite = false)
		{
            Log.Debug("StaticDatabase.ToggleActiveStaticsOnPlanet " + cBody.bodyName);

			foreach (StaticInstance instance in allStaticInstances)
			{
				if (instance.CelestialBody == cBody)
					InstanceUtil.SetActiveRecursively(instance, bActive);
				else
					if (bOpposite)
						InstanceUtil.SetActiveRecursively(instance, !bActive);
			}
		}

        /// <summary>
        /// toggles the visiblility of all statics in a group
        /// </summary>
        /// <param name="sGroup"></param>
        /// <param name="bActive"></param>
        /// <param name="bOpposite"></param>
        internal static void ToggleActiveStaticsInGroup(string sGroup, bool bActive = true, bool bOpposite = false)
		{
            Log.Debug("StaticDatabase.ToggleActiveStaticsInGroup");

			foreach (StaticInstance instance in allStaticInstances)
			{
				if (instance.Group == sGroup)
					InstanceUtil.SetActiveRecursively(instance, bActive);
				else
					if (bOpposite)
						InstanceUtil.SetActiveRecursively(instance, !bActive);
			}
		}

        internal static void CacheAll()
		{
			if (activeBodyName == "")
			{
                Log.Debug("StaticDatabase.cacheAll() skipped. No activeBodyName.");
				
				return;
			}

			if (groupList.ContainsKey(activeBodyName))
			{
                Log.Debug("StaticDatabase.cacheAll(): groupList containsKey " + activeBodyName);

				foreach (StaticGroup group in groupList[activeBodyName].Values)
				{
                    Log.Debug("StaticDatabase.cacheAll(): cacheAll() " + group.groupName);
					
					if (group.active)
						group.CacheAll();

					if (!group.alwaysActive)
					{
						group.active = false;
                        Log.Debug("StaticDatabase.cacheAll(): group is not always active. group.active is set false for " + group.groupName);
					}
				}
			}
			else
			{
                Log.Debug("StaticDatabase.cacheAll(): groupList DOES NOT containsKey " + activeBodyName);
			}
		}

        internal static void LoadObjectsForBody(String bodyName)
		{
			activeBodyName = bodyName;

			if (groupList.ContainsKey(bodyName))
			{
				foreach (KeyValuePair<String, StaticGroup> bodyGroups in groupList[bodyName])
				{
					bodyGroups.Value.active = true;
				}
			}
		}

        internal static void OnBodyChanged(CelestialBody body)
		{
			if (body != null)
			{
                Log.Debug("StaticDatabase.onBodyChanged(): body is not null.");

				if (body.bodyName != activeBodyName)
				{
                    Log.Debug("StaticDatabase.onBodyChanged(): bodyName is not activeBodyName. cacheAll(). Load objects for body. Set activeBodyName to body.");
                    Log.Debug("bodyName " + body.bodyName + " activeBodyName " + activeBodyName);

					CacheAll();
					LoadObjectsForBody(body.bodyName);
					activeBodyName = body.bodyName;
				}
			}
			else
			{
                Log.Debug("StaticDatabase.onBodyChanged(): body is null. cacheAll(). Set activeBodyName empty " + activeBodyName);
				CacheAll();
				activeBodyName = "";
			}
		}

        internal static void UpdateCache(Vector3 playerPos)
		{
            //Log.Normal("StaticDatabase.updateCache(): activeBodyName is " + activeBodyName);

			Vector3 vPlayerPos = Vector3.zero;

			if (FlightGlobals.ActiveVessel != null)
			{
				vPlayerPos = FlightGlobals.ActiveVessel.GetTransform().position;
                //Log.Normal("StaticDatabase.updateCache(): using active vessel " + FlightGlobals.ActiveVessel.vesselName);
			}
			else
				vPlayerPos = playerPos;

			if (vPlayerPos == Vector3.zero)
			{
                    //Log.Warning("StaticDatabase.updateCache(): vPlayerPos is still v3.zero ");
			}
			
			if (groupList.ContainsKey(activeBodyName))
			{
				foreach (StaticGroup group in groupList[activeBodyName].Values)
				{
					if (!group.bLiveUpdate)
					{
                        //Log.Normal("StaticDatabase.updateCache(): live update (updateCacheSettings) of group " + group.groupName);
						
						group.UpdateCacheSettings();
						group.bLiveUpdate = true;
					}

					if (!group.alwaysActive)
					{
						var center = group.centerPoint;
						var dist = Vector3.Distance(center, vPlayerPos);

						List<StaticInstance> groupchildObjects = group.groupInstances;

						foreach (StaticInstance obj in groupchildObjects)
						{
							dist = Vector3.Distance(vPlayerPos, obj.gameObject.transform.position);
                            Log.Debug("StaticDatabase.updateCache(): distance to first group object is " + dist.ToString() + " for " + group.groupName);

							break;
						}

						if (center == Vector3.zero)
						{
                            Log.Debug("StaticDatabase.updateCache(): center of group is still v3.zero " + group.groupName);
						}
						
						//if (KerbalKonstructs.instance.DebugMode)
						//	Debug.Log("KK: StaticDatabase.updateCache(): dist is " + dist.ToString() + " to " + group.groupName);
						
						Boolean bGroupIsClose = dist < group.visibilityRange;
                        Log.Debug("StaticDatabase.updateCache(): group visrange is " + group.visibilityRange.ToString() + " for " + group.groupName);
						
						if (!bGroupIsClose)
						{
                            Log.Debug("StaticDatabase.updateCache(): Group is not close. cacheAll()  " + group.groupName);
							group.CacheAll();
						}
						
						group.active = bGroupIsClose;
					}
					else
					{
						Log.Debug("StaticDatabase.updateCache(): Group is always active. Check if updateCache goes off. " + group.groupName);
						group.active = true;
					}

					if (group.active)
					{
                        Log.Debug("StaticDatabase.updateCache(): Group is active. group.updateCache() " + group.groupName);
						group.UpdateCache(vPlayerPos);
					}
					else
					{
                        Log.Debug("StaticDatabase.updateCache(): Group is not active " + group.groupName);
					}
				}
			}

		}

        public static StaticInstance[] GetAllStatics()
		{
            return allStaticInstances;
		}

        internal static void RegisterModel(StaticModel model, string name)
		{
            allStaticModels.Add(model);
            if (modelList.ContainsKey(name))
            {
                Log.UserInfo("duplicate model name: " + name + " ,found in: "  + model.configPath + " . This might be OK.");
                return;
            }
            else
            {
                modelList.Add(name, model);
            }
		}

        internal static StaticModel GetModelByName(string name)
        {
            if (!modelList.ContainsKey(name))
            {
                Log.UserError("No StaticModel found with name: " + name);
                return null;
            }
            else
            {
                return modelList[name];   
            }
        }


        internal static List<StaticInstance> GetInstancesFromModel(StaticModel model)
		{
			return (from obj in allStaticInstances where obj.model == model select obj).ToList();
		}
	}
}
