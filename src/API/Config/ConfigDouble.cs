using System;

namespace KerbalKonstructs.API.Config
{
	public class ConfigDouble : IConfigItem
	{
		private double val = 0d;
		private double defaultValue = 0d;

		public void setValue(string configValue)
		{
			val = double.Parse(configValue);
		}

		public object getValue()
		{
			return val;
		}

		public void setDefaultValue(object value)
		{
			defaultValue = (double) value;
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
