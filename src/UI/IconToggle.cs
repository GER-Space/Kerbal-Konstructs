using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KodeUI;

namespace KerbalKonstructs.UI
{

	public class IconToggle : UIObject
	{
		UIImage icon;
		Sprite onSprite;
		Sprite offSprite;
		Image image;
		Toggle toggle;

		public bool interactable
		{
			get { return toggle.interactable; }
			set { toggle.interactable = value; }
		}

		public bool isOn
		{
			get { return toggle.isOn; }
			set { toggle.isOn = value; }
		}

		void UpdateImage(bool on)
		{
			icon.image.sprite = on ? onSprite : offSprite;
		}

		public override void CreateUI()
		{
			toggle = gameObject.AddComponent<Toggle>();

			image = gameObject.AddComponent<Image>();
			toggle.targetGraphic = image;
			toggle.onValueChanged.AddListener (UpdateImage);
			this
				.Pivot(PivotPresets.MiddleCenter)
				.Add<UIImage>(out icon, "Icon")
				.Anchor(AnchorPresets.StretchAll)
				.Pivot(PivotPresets.MiddleCenter)
				.SizeDelta(0, 0)
				.Finish()
				;
		}

		public override void Style()
		{
		}

		public IconToggle OnSprite(Sprite sprite)
		{
			//FIXME custom style in KodeUI
			onSprite = sprite;
			return this;
		}

		public IconToggle OffSprite(Sprite sprite)
		{
			//FIXME custom style in KodeUI
			offSprite = sprite;
			return this;
		}

		public IconToggle Tooltip(string tooltip)
		{
			//FIXME implement
			return this;
		}

		public IconToggle OnValueChanged(UnityAction<bool> action)
		{
			toggle.onValueChanged.AddListener(action);
			return this;
		}

		public IconToggle SetIsOnWithoutNotify(bool on)
		{
			toggle.SetIsOnWithoutNotify(on);
			UpdateImage(on);
			return this;
		}

		public IconToggle Group(ToggleGroup group)
		{
			toggle.group = group;
			return this;
		}

		protected override void OnEnable()
		{
			UpdateImage(isOn);
		}
	}
}
