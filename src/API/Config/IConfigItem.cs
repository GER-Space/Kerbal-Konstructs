using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalKonstructs.API.Config
{
	public interface IConfigItem
	{
		void setValue(string configValue);
		object getValue();
		void setDefaultValue(object value);
		object getDefaultValue();
		string convertValueToConfig(object value);
	}
}
