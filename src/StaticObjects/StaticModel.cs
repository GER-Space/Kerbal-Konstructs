using System.Collections.Generic;
using KerbalKonstructs.API;
using UnityEngine;

namespace KerbalKonstructs.StaticObjects
{
	public class StaticModel
	{
		public string config;
		public string configPath;
		public string path;
		public Dictionary<string, object> settings = new Dictionary<string, object>();
		public List<StaticModule> modules = new List<StaticModule>();

		public object getSetting(string setting)
		{
			if (settings.ContainsKey(setting))
				return settings[setting];
			
			if (KerbalKonstructs.instance.DebugMode)
				Debug.Log("KK: Setting " + setting + " not found in model " + config + ". This is harmless. Not a bug.");
			
			object defaultValue = KKAPI.getModelSettings()[setting].getDefaultValue();

			if (defaultValue != null)
			{
				settings.Add(setting, defaultValue);
				return defaultValue;
			}
			else
			{
				if (KerbalKonstructs.instance.DebugMode) Debug.Log("KK: Setting " + setting + " not found in model API. It may be on purpose. Not a bug.");
				
				return null;
			}
		}

		public void setSetting(string setting, object value)
		{
			if (settings.ContainsKey(setting))
			{
				settings[setting] = value;
			}
			else
			{
				settings.Add(setting, value);
			}
		}
	}
}
