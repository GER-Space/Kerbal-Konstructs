using System;

namespace KerbalKonstructs.API.Config
{
	class ConfigRTGuid : IConfigItem
	{
		private Guid guid;

		private object defaultValue = Guid.Empty;

		public void setValue(Guid configValue)
		{
			guid = configValue;
		}

		public void setValue(string configValue)
		{

		}

		public object getValue()
		{
			return guid;
		}

		public void setDefaultValue(object value)
		{
			defaultValue = value;
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