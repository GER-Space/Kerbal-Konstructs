using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KodeUI;
using TMPro;

namespace KerbalKonstructs.UI
{
	public class InputLine : HorizontalLayout
	{
		UIText label;
		UIInputField input;

		public string text
		{
			get { return input.text; }
			set { input.text = value; }
		}

		public override void CreateUI()
		{
			base.CreateUI();

			this.Add<UIText>(out label)
					.Alignment(TextAlignmentOptions.Left)
					.Finish()
				.Add<UIInputField>(out input)
					.FlexibleLayout(true, false)
					.Finish()
				;

			input.OnFocusGained(SetControlLock);
			input.OnFocusLost(ClearControlLock);
		}

		public override void Style()
		{
			base.Style();
		}

		public InputLine Label(string label)
		{
			this.label.Text(label + ":");
			return this;
		}

		public InputLine OnSubmit(UnityAction<string> evt)
		{
			input.OnSubmit(evt);
			return this;
		}

		public InputLine OnFocusGained(UnityAction<string> evt)
		{
			input.OnFocusGained(evt);
			return this;
		}

		public InputLine OnFocusLost(UnityAction<string> evt)
		{
			input.OnFocusLost(evt);
			return this;
		}

		void SetControlLock (string str = null)
		{
			InputLockManager.SetControlLock ("KKInputLine_lock");
		}

		void ClearControlLock (string str = null)
		{
			InputLockManager.RemoveControlLock ("KKInputLine_lock");
		}
	}
}
