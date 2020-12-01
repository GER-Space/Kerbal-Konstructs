using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KodeUI;
using TMPro;

namespace KerbalKonstructs.UI
{

	public class StackSize : HorizontalLayout
	{
		public double Increment { get; private set; }

		public override void CreateUI()
		{
			base.CreateUI();

			Increment = 10;

			this.Add<UIText>()
					.Text(KKLocalization.StackSize)
					.Finish()
				.Add<FlexibleSpace>() .Finish()
				.Add<UIButton>()
					.Text("1")
					.OnClick(() => Increment = 1)
					.Finish()
				.Add<UIButton>()
					.Text("10")
					.OnClick(() => Increment = 10)
					.Finish()
				.Add<UIButton>()
					.Text("100")
					.OnClick(() => Increment = 100)
					.Finish()
				.Add<FlexibleSpace>() .Finish()
				;
		}

		public override void Style()
		{
			base.Style();
		}
	}
}
