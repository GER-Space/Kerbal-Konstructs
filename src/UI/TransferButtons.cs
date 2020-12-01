using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KodeUI;
using TMPro;

namespace KerbalKonstructs.UI
{

	public class TransferButtons : LayoutAnchor
	{
		public class TransferButtonEvent : UnityEvent { };

		TransferButtonEvent onFromVessel;
		TransferButtonEvent onToVessel;

		public override void CreateUI()
		{
			onFromVessel = new TransferButtonEvent();
			onToVessel = new TransferButtonEvent();

			base.CreateUI();

			var fromVesselMin = new Vector2(0, 0);
			var fromVesselMax = new Vector2(0.48f, 1);
			var toVesselMin = new Vector2(0.52f, 0);
			var toVesselMax = new Vector2(1, 1);

			this.DoPreferredWidth (false)
				.DoPreferredHeight (true)
				.FlexibleLayout(true, false)
				.Add<VerticalLayout>()
					.ChildAlignment(TextAnchor.MiddleCenter)
					.Anchor(fromVesselMin, fromVesselMax)
					.Add<HorizontalLayout>()
						.Add<UIButton>()
							.Text("+")
							.OnClick(FromVessel)
							.Finish()
						.Add<UIRepeatButton>()
							.Rate(10)
							.Text("++")
							.OnClick(FromVessel)
							.Finish()
						.Finish()
					.Finish()
				.Add<VerticalLayout>()
					.ChildAlignment(TextAnchor.MiddleCenter)
					.Anchor(toVesselMin, toVesselMax)
					.Add<HorizontalLayout>()
						.Add<UIButton>()
							.Text("-")
							.OnClick(ToVessel)
							.Finish()
						.Add<UIRepeatButton>()
							.Rate(10)
							.Text("--")
							.OnClick(ToVessel)
							.Finish()
						.Finish()
					.Finish()
				;
		}

		void FromVessel()
		{
			Debug.Log("[TransferButtons] FromVessel");
			onFromVessel.Invoke();
		}

		void ToVessel()
		{
			Debug.Log("[TransferButtons] ToVessel");
			onToVessel.Invoke();
		}

		public TransferButtons OnFromVessel(UnityAction action)
		{
			onFromVessel.AddListener(action);
			return this;
		}

		public TransferButtons OnToVessel(UnityAction action)
		{
			onToVessel.AddListener(action);
			return this;
		}

		public override void Style()
		{
			base.Style();
		}
	}
}
