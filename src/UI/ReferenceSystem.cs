using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KodeUI;
using TMPro;

namespace KerbalKonstructs.UI
{

	public class ReferenceSystem : HorizontalLayout
	{
		public class ReferenceSystemEvent : UnityEvent<ReferenceSystem> { }
		ReferenceSystemEvent onSpaceChanged;
		Space _space;
		public Space space
		{
			get {return _space; }
			set {
				_space = value;
				onSpaceChanged.Invoke(this);
			} 
		}

		public override void CreateUI()
		{
			base.CreateUI();

			ToggleGroup group;
			_space = Space.Self;

			onSpaceChanged = new ReferenceSystemEvent();

			this.Add<UIText>()
					.Text(KKLocalization.ReferenceSystem)
					.Finish()
				.Add<HorizontalLayout>()
					.ToggleGroup (out group)
					.Add<ToggleText>()
						.Text(KKLocalization.SpaceModel)
						.Group(group)
						.SetIsOnWithoutNotify (space == Space.Self)
						.OnValueChanged(on => { if (on) space = Space.Self; })
						.Finish()
					.Add<ToggleText>()
						.Text(KKLocalization.SpaceWorld)
						.Group(group)
						.SetIsOnWithoutNotify (space == Space.World)
						.OnValueChanged(on => { if (on) space = Space.World; })
						.Finish()
					.Finish()
				;
		}

		public override void Style()
		{
			base.Style();
		}

		public ReferenceSystem OnSpaceChanged(UnityAction<ReferenceSystem> action)
		{
			onSpaceChanged.AddListener (action);
			return this;
		}
	}
}
