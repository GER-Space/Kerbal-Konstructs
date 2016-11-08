using System.Collections.Generic;
using KerbalKonstructs.API;
using KerbalKonstructs.Utilities;
using UnityEngine;

namespace KerbalKonstructs.KerbinNations
{
	public class KerbinNation
	{
		public string config;
		public string configPath;
		public string path;
		public Dictionary<string, object> settings = new Dictionary<string, object>();

		public object getSetting(string setting)
		{
			if (settings.ContainsKey(setting))
				return settings[setting];
			
			if (KerbalKonstructs.instance.DebugMode)
				Log.Normal("KK: Setting " + setting + " not found in nation " + config + ". This is harmless. Not a bug.");
			
			object defaultValue = KKAPI.getNationSettings()[setting].getDefaultValue();

			if (defaultValue != null)
			{
				settings.Add(setting, defaultValue);
				return defaultValue;
			}
			else
			{
				if (KerbalKonstructs.instance.DebugMode) Log.Normal("KK: Setting " + setting + " not found in nation API. It may be on purpose. Not a bug.");
				
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
