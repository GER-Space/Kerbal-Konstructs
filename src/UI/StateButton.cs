using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KodeUI;

namespace KerbalKonstructs.UI
{

	public class StateButton : UIButton
	{
		public class StateChangeEvent : UnityEvent<State> { }
		public class State
		{
			bool _state;
			public StateChangeEvent onStateChanged;

			public bool state
			{
				get { return _state; }
				set {
					bool changed = _state != value;
					_state = value;
					if (changed) {
						onStateChanged.Invoke(this);
					}
				}
			}

			public static implicit operator bool(State s)
			{
				return s.state;
			}

			public bool Toggle()
			{
				state = !state;
				return state;
			}

			public State(bool initialState = false)
			{
				_state = initialState;
				onStateChanged = new StateChangeEvent();
			}
		}

		State state;
		Sprite onSprite;
		Sprite offSprite;

		void UpdateImage()
		{
			if (state != null && onSprite != null && offSprite != null) {
				Image(state ? onSprite : offSprite);
			}
		}

		void ToggleState()
		{
			state.Toggle();
			UpdateImage();
		}

		public override void CreateUI()
		{
			base.CreateUI();
			this.OnClick(ToggleState);
		}

		public override void Style()
		{
			base.Style();
		}

		public StateButton OnSprite(Sprite sprite)
		{
			//FIXME custom style in KodeUI
			onSprite = sprite;
			UpdateImage();
			return this;
		}

		public StateButton OffSprite(Sprite sprite)
		{
			//FIXME custom style in KodeUI
			offSprite = sprite;
			UpdateImage();
			return this;
		}

		public StateButton StateVar(State state)
		{
			this.state = state;
			UpdateImage();
			return this;
		}

		public StateButton Tooltip(string tooltip)
		{
			//FIXME implement
			return this;
		}

		protected override void OnEnable()
		{
			UpdateImage();
		}
	}
}
