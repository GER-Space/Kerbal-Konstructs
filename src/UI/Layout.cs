using UnityEngine;
using UnityEngine.UI;
using KodeUI;

namespace KerbalKonstructs.UI {

	public class HorizontalLayout : Layout
	{
		public override void CreateUI ()
		{
			this.Horizontal ()
				.ControlChildSize (true, true)
				.ChildForceExpand (false,false)
				;
		}
	}

	public class FlexibleSpace : UIEmpty
	{
		public override void CreateUI ()
		{
			var parent = rectTransform.parent.GetComponent<Layout>();
			if (parent != null) {
				if (parent.layoutGroup is HorizontalLayoutGroup) {
					this.FlexibleLayout (true, false);
				} else if (parent.layoutGroup is VerticalLayoutGroup) {
					this.FlexibleLayout (false, true);
				} else {
					Debug.LogError("don't know how to fiex");
				}
			}
		}
	}

	public class FixedSpace : UIEmpty
	{
		public FixedSpace Size(float size)
		{
			PreferredSize(size, size);
			return this;
		}
	}

	public class VerticalLayout : Layout
	{
		public override void CreateUI ()
		{
			this.Vertical ()
				.ControlChildSize (true, true)
				.ChildForceExpand (false,false)
				;
		}
	}
}
