using System;

namespace KerbalKonstructs.API.Config
{
	public class ConfigGenericString : IConfigItem
	{
		protected string value = "";
		protected string defaultValue = "";

		public virtual void setValue(String configValue)
		{
			value = configValue;
		}

		public object getValue()
		{
			return value;
		}

		public void setDefaultValue(object value)
		{
			defaultValue = (string) value;
		}

		public object getDefaultValue()
		{
			return defaultValue;
		}

		public string convertValueToConfig(object value)
		{
			return (string) value;
		}
	}
}
