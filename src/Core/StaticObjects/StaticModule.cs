using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalKonstructs.Core
{
	public class StaticModule: MonoBehaviour
	{
		public string moduleNamespace;
		public string moduleClassname;
		public Dictionary<String, String> moduleFields = new Dictionary<string,string>();
		
		public virtual void StaticObjectUpdate() {}

		public virtual void StaticObjectEditorOpen () {}

		public virtual void StaticObjectEditorClose () {}
	}
}
