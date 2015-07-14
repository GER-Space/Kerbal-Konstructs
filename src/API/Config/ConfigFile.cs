using System;

namespace KerbalKonstructs.API.Config
{
	public class ConfigFile : ConfigGenericString
	{
		public override void setValue(String configValue)
		{
			//Strips file extension \o/
			value = configValue.Substring(0, configValue.LastIndexOf('.'));
		}
	}
}
