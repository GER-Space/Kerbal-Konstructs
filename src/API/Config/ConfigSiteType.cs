using KerbalKonstructs.LaunchSites;

namespace KerbalKonstructs.API.Config
{
	public class ConfigSiteType : IConfigItem
	{
		private SiteType type;
		private SiteType defaultType = SiteType.Any;

		public void setValue(string configValue)
		{
			type = getSiteTypefromString(configValue);
		}

		public object getValue()
		{
			return type;
		}

		public void setDefaultValue(object value)
		{
			defaultType = (SiteType) value;
		}

		public object getDefaultValue()
		{
			return defaultType;
		}

		public string convertValueToConfig(object value)
		{
			return value.ToString().ToUpper();
		}

		public static SiteType getSiteTypefromString(string siteType)
		{
			SiteType outType;
			switch (siteType)
			{
				case "VAB":
					outType = SiteType.VAB;
					break;
				case "SPH":
					outType = SiteType.SPH;
					break;
				default:
					outType = SiteType.Any;
					break;
			}
			return outType;
		}
	}
}
