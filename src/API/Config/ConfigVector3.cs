using System;
using UnityEngine;

namespace KerbalKonstructs.API.Config
{
	public class ConfigVector3 : IConfigItem
	{
		private Vector3 v3;
		private Vector3 defaultValue = Vector3.zero;

		public void setValue(string configValue)
		{
			v3 = ConfigNode.ParseVector3(configValue);
		}

		public object getValue()
		{
			return v3;
		}

		public void setDefaultValue(object value)
		{
			defaultValue = (Vector3) value;
		}

		public object getDefaultValue()
		{
			return defaultValue;
		}

		public string convertValueToConfig(object value)
		{
			return ConfigNode.WriteVector((Vector3) value);
		}
	}
}
