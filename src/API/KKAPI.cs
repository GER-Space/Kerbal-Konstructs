using KerbalKonstructs.API.Config;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs.API
{
	public class KKAPI
	{
		//Static object config settings
		private static Dictionary<string, IConfigItem> instanceSettings = new Dictionary<string, IConfigItem>();
		private static Dictionary<string, IConfigItem> modelSettings = new Dictionary<string, IConfigItem>();
		private static Dictionary<string, IConfigItem> launchsiteSettings = new Dictionary<string, IConfigItem>();

		public static void addInstanceSetting(string name, IConfigItem conf)
		{
			instanceSettings.Add(name, conf);
		}

		public static void addModelSetting(string name, IConfigItem conf)
		{
			modelSettings.Add(name, conf);
		}

		public static void addLaunchsiteSetting(string name, IConfigItem conf)
		{
			launchsiteSettings.Add(name, conf);
		}

		public static Dictionary<string, IConfigItem> getInstanceSettings()
		{
			return instanceSettings;
		}

		public static Dictionary<string, IConfigItem> getModelSettings()
		{
			return modelSettings;
		}

		public static Dictionary<string, IConfigItem> getLaunchsiteSettings()
		{
			return launchsiteSettings;
		}

		public static Dictionary<string, object> loadConfig(ConfigNode cfgNode, Dictionary<string, IConfigItem> kkConfig)
		{
			Dictionary<string, object> settings = new Dictionary<string, object>();
			foreach (KeyValuePair<string, IConfigItem> configValue in kkConfig)
			{
				if (cfgNode.GetValue(configValue.Key) != null && cfgNode.GetValue(configValue.Key) != "")
				{
					IConfigItem item = configValue.Value;
					item.setValue(cfgNode.GetValue(configValue.Key));
					settings.Add(configValue.Key, item.getValue());
				}
			}

			return settings;
		}

		//Utility
		public static CelestialBody getCelestialBody(String name)
		{
			CelestialBody[] bodies = GameObject.FindObjectsOfType(typeof(CelestialBody)) as CelestialBody[];
			foreach (CelestialBody body in bodies)
			{
				if (body.bodyName == name)
					return body;
			}
			Debug.LogError("KK: Couldn't find body \"" + name + "\"");
			return null;
		}
	}
}
