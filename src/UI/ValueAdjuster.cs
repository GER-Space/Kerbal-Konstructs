using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KodeUI;
using TMPro;

namespace KerbalKonstructs.UI
{

	public class ValueAdjuster : HorizontalLayout
	{
		public class ValueAdjusterEvent : UnityEvent { };

		ValueAdjusterEvent onIncrement;
		ValueAdjusterEvent onDecrement;
		ValueAdjusterEvent onValueSet;

		UIText label;
		UIInputField input;
		UIButton increment;
		UIRepeatButton incrementRepeat;
		UIButton decrement;
		UIRepeatButton decrementRepeat;

		string format;

		double value;
		public double Value
		{
			get {
				return value;
			}
			set {
				this.value = value;
				input.text = value.ToString(format);
			}
		}

		public override void CreateUI()
		{
			onIncrement = new ValueAdjusterEvent();
			onDecrement = new ValueAdjusterEvent();
			onValueSet = new ValueAdjusterEvent();

			base.CreateUI();

			this.ChildAlignment(TextAnchor.MiddleCenter)
				.FlexibleLayout(true, false)
				.Add<UIText>(out label)
					.Finish()
				.Add<FlexibleSpace>() .Finish()
				.Add<HorizontalLayout>()
					.ChildAlignment(TextAnchor.MiddleRight)
					.Add<UIInputField>(out input)
						.OnFocusGained(SetControlLock)
						.OnFocusLost(ClearControlLock)
						.OnFocusLost(ParseInput)
						.OnSubmit(ParseInput)
						.FlexibleLayout(true, false)
						.Finish()
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

		void ParseInput(string str)
		{
			double val;
			double.TryParse(str, out val);
			Value = val;
			onValueSet.Invoke();
		}

		void SetControlLock (string str = null)
		{
			InputLockManager.SetControlLock ("KKValueAdjuster_lock");
		}

		void ClearControlLock (string str = null)
		{
			InputLockManager.RemoveControlLock ("KKValueAdjuster_lock");
		}

		void OnIncrement()
		{
			Debug.Log("[ValueAdjuster] OnIncrement");
			onIncrement.Invoke();
		}

		void OnDecrement()
		{
			Debug.Log("[ValueAdjuster] OnDecrement");
			onDecrement.Invoke();
		}

		public ValueAdjuster Label(string label)
		{
			this.label.Text(label + ":");
			return this;
		}

		public ValueAdjuster OnIncrement(UnityAction action)
		{
			onIncrement.AddListener(action);
			return this;
		}

		public ValueAdjuster OnDecrement(UnityAction action)
		{
			onDecrement.AddListener(action);
			return this;
		}

		public ValueAdjuster OnValueSet(UnityAction action)
		{
			onValueSet.AddListener(action);
			return this;
		}

		public ValueAdjuster Format(string format)
		{
			this.format = format;
			return this;
		}

		public ValueAdjuster InputWidth(float width)
		{
			input.LayoutElement.flexibleWidth = -1;
			input.PreferredWidth(width);
			input.MinSize(width, -1);
			return this;
		}

		public override void Style()
		{
			base.Style();
		}
	}
}
