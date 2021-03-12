using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KodeUI;
using TMPro;

namespace KerbalKonstructs.UI
{

	public class ListView : ScrollView
	{
		ToggleGroup group;
		public ToggleGroup Group { get { return group; } }

		public override void CreateUI()
		{
			base.CreateUI();

			UIScrollbar scrollbar;

			this.Horizontal(false)
				.Vertical(true)
				.Horizontal()
				.ControlChildSize(true, true)
				.ChildForceExpand(false, true)
				.FlexibleLayout(true, false)
				.PreferredSize(-1, 120)
				.Add<UIScrollbar>(out scrollbar, "Scrollbar")
					.Direction(Scrollbar.Direction.BottomToTop)
					.PreferredWidth(15)
					.FlexibleLayout(false, true)
					.Finish()
				;

			VerticalScrollbar = scrollbar;
			Viewport.FlexibleLayout(true, true);
			Content
				.Vertical()
				.ControlChildSize(true, true)
				.ChildForceExpand(false, false)
				.Anchor(AnchorPresets.HorStretchTop)
				.PreferredSizeFitter(true, false)
				.WidthDelta(0)
				.ToggleGroup (out group)
				.Finish();
		}

		public override void Style()
		{
			base.Style();
		}

		public ListView Items(UIKit.IListObject items)
		{
			UIKit.UpdateListContent (items);
			return this;
		}
	}
}
