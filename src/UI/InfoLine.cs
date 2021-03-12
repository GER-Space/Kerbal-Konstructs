using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KodeUI;
using TMPro;

namespace KerbalKonstructs.UI
{

	public class InfoLine : HorizontalLayout
	{
		UIText label;
		UIText info;

		public override void CreateUI()
		{
			base.CreateUI();

			this.Add<UIText>(out label)
					.Alignment(TextAlignmentOptions.Left)
					.Finish()
				.Add<UIText>(out info)
					.Alignment(TextAlignmentOptions.Right)
					.FlexibleLayout(true, false)
					.Finish()
				;
		}

		public override void Style()
		{
			base.Style();
		}

		public InfoLine Label(string label)
		{
			this.label.Text(label + ":");
			return this;
		}

		public InfoLine Info(string info)
		{
			this.info.Text(info);
			return this;
		}
	}
}
