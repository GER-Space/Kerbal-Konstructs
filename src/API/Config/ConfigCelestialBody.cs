using System;

namespace KerbalKonstructs.API.Config
{
	class ConfigCelestialBody : IConfigItem
	{
		private CelestialBody body = null;

		public void setValue(string configValue)
		{
			body = KKAPI.getCelestialBody(configValue);
		}

		public object getValue()
		{
			return body;
		}

		public void setDefaultValue(object value)
		{
			throw new NotImplementedException();
		}

		public object getDefaultValue()
		{
			return null;
		}

		public string convertValueToConfig(object value)
		{
			return ((CelestialBody) value).bodyName;
		}
	}
}
