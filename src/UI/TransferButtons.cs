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

		TransferButtonEvent onIncrement;
		TransferButtonEvent onDecrement;

		public override void CreateUI()
		{
			onIncrement = new TransferButtonEvent();
			onDecrement = new TransferButtonEvent();

			base.CreateUI();

			var incrementMin = new Vector2(0, 0);
			var incrementMax = new Vector2(0.48f, 1);
			var decrementMin = new Vector2(0.52f, 0);
			var decrementMax = new Vector2(1, 1);

			this.DoPreferredWidth (false)
				.DoPreferredHeight (true)
				.Anchor(incrementMin, incrementMax)
				.FlexibleLayout(true, false)
				.Add<VerticalLayout>()
					.ChildAlignment(TextAnchor.MiddleCenter)
					.Anchor(incrementMin, incrementMax)
					.Add<HorizontalLayout>()
						.Add<UIButton>()
							.Text("+")
							.OnClick(Increment)
							.Finish()
						.Add<UIRepeatButton>()
							.Rate(10)
							.Text("++")
							.OnClick(Increment)
							.Finish()
						.Finish()
					.Finish()
				.Add<VerticalLayout>()
					.ChildAlignment(TextAnchor.MiddleCenter)
					.Anchor(decrementMin, decrementMax)
					.Add<HorizontalLayout>()
						.Add<UIButton>()
							.Text("-")
							.OnClick(Decrement)
							.Finish()
						.Add<UIRepeatButton>()
							.Rate(10)
							.Text("--")
							.OnClick(Decrement)
							.Finish()
						.Finish()
					.Finish()
				;
		}

		void Increment()
		{
			Debug.Log("[TransferButtons] Increment");
			onIncrement.Invoke();
		}

		void Decrement()
		{
			Debug.Log("[TransferButtons] Decrement");
			onDecrement.Invoke();
		}

		public TransferButtons OnIncrement(UnityAction action)
		{
			onIncrement.AddListener(action);
			return this;
		}

		public TransferButtons OnDecrement(UnityAction action)
		{
			onDecrement.AddListener(action);
			return this;
		}

		public override void Style()
		{
			base.Style();
		}
	}
}
