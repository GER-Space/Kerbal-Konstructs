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
			Debug.Log("Setting " + setting + " not found in model " + config);
			object defaultValue = KKAPI.getModelSettings()[setting].getDefaultValue();

			if (defaultValue != null)
			{
				settings.Add(setting, defaultValue);
				return defaultValue;
			}
			else
			{
				Debug.Log("KK: Setting " + setting + " not found in model API. BUG BUG BUG.");
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
