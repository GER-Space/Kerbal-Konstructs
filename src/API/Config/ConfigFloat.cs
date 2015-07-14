using System;

namespace KerbalKonstructs.API.Config
{
	public class ConfigFloat : IConfigItem
	{
		private float val = 0;
		private float defaultValue = 0;

		public void setValue(string configValue)
		{
			val = float.Parse(configValue);
		}

		public object getValue()
		{
			return val;
		}

		public void setDefaultValue(object value)
		{
			defaultValue = (float) value;
		}

		public object getDefaultValue()
		{
			return defaultValue;
		}

		public string convertValueToConfig(object value)
		{
			return value.ToString();
		}
	}
}
