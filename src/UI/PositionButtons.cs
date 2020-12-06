using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KodeUI;
using TMPro;

namespace KerbalKonstructs.UI
{

	public class PositionButtons : HorizontalLayout
	{
		public class PositionButtonEvent : UnityEvent { };

		public bool fromVesselInteractable
		{
			get {
				return increment.interactable && incrementRepeat.interactable;
			}
			set {
				increment.interactable = value;
				incrementRepeat.interactable = value;
			}
		}

		public bool toVesselInteractable
		{
			get {
				return decrement.interactable && decrementRepeat.interactable;
			}
			set {
				decrement.interactable = value;
				decrementRepeat.interactable = value;
			}
		}

		PositionButtonEvent onIncrement;
		PositionButtonEvent onDecrement;

		UIText label;
		UIButton increment;
		UIRepeatButton incrementRepeat;
		UIButton decrement;
		UIRepeatButton decrementRepeat;

		public override void CreateUI()
		{
			onIncrement = new PositionButtonEvent();
			onDecrement = new PositionButtonEvent();

			base.CreateUI();

			this.FlexibleLayout(true, false)
				.Add<UIText>(out label)
					.Finish()
				.Add<FlexibleSpace>() .Finish()
				.Add<HorizontalLayout>()
					.ChildAlignment(TextAnchor.MiddleCenter)
					.Add<UIRepeatButton>(out decrementRepeat)
						.Rate(10)
						.Text("<<")
						.OnClick(OnDecrement)
						.Finish()
					.Add<UIButton>(out decrement)
						.Text("<")
						.OnClick(OnDecrement)
						.Finish()
					.Add<UIButton>(out increment)
						.Text(">")
						.OnClick(OnIncrement)
						.Finish()
					.Add<UIRepeatButton>(out incrementRepeat)
						.Rate(10)
						.Text(">>")
						.OnClick(OnIncrement)
						.Finish()
					.Finish()
				;
		}

		void OnIncrement()
		{
			Debug.Log("[PositionButtons] OnIncrement");
			onIncrement.Invoke();
		}

		void OnDecrement()
		{
			Debug.Log("[PositionButtons] OnDecrement");
			onDecrement.Invoke();
		}

		public PositionButtons Label(string label)
		{
			this.label.Text(label + ":");
			return this;
		}

		public PositionButtons OnIncrement(UnityAction action)
		{
			onIncrement.AddListener(action);
			return this;
		}

		public PositionButtons OnDecrement(UnityAction action)
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
