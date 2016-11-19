using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalKonstructs.Core
{
	public class StaticModule
	{
		public string moduleNamespace;
		public string moduleClassname;
		public Dictionary<String, String> moduleFields = new Dictionary<string,string>();
	}
}
