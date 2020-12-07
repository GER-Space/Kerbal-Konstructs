using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KodeUI;
using TMPro;

namespace KerbalKonstructs.UI
{

	public class IncrementSize : VerticalLayout
	{
		float _increment;
		public float Increment
		{
			get { return _increment; }
			set {
				_increment = value;
				increment.Info($"{_increment:G4}");
			}
		}
		InfoLine increment;

		public override void CreateUI()
		{
			base.CreateUI();

			this.Add<InfoLine>(out increment)
					.Label(KKLocalization.Increment)
					.Finish()
				.Add<HorizontalLayout>()
					.Add<UIButton>()
						.Text("0.001")
						.OnClick(() => Increment = 0.001f)
						.Finish()
					.Add<UIButton>()
						.Text("0.01")
						.OnClick(() => Increment = 0.01f)
						.Finish()
					.Add<UIButton>()
						.Text("0.1")
						.OnClick(() => Increment = 0.1f)
						.Finish()
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
					.Finish()
				;

			Increment = 1;
		}

		public override void Style()
		{
			base.Style();
		}
	}
}
