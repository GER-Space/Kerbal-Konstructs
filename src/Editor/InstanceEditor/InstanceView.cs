using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using KodeUI;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.UI
{

	public class InstanceView : VerticalLayout, TabController.ITabItem
	{
#region TabController.ITabItem
		public string TabName { get { return KKLocalization.EditorLocalInstances; } }
		public bool TabEnabled { get { return true; } }

		public void SetTabVisible(bool visible)
		{
			SetActive (visible);
			if (visible) {
				UpdateUI();
			}
		}
#endregion

		InstanceItem.List localInstances;
		ListView instanceList;

		void FindLocalInstances()
		{
			CelestialBody body = FlightGlobals.currentMainBody;
			Vector3 position = FlightGlobals.ActiveVessel.GetTransform().position;
			float range = KerbalKonstructs.localGroupRange;
			range *= range;

			localInstances.Clear();

			for (int i = StaticDatabase.allStaticInstances.Length; i-- > 0; ) {
				var instance = StaticDatabase.allStaticInstances[i];
				if (instance.CelestialBody == body) {
					Vector3 dist = position - instance.position;
					if (dist.sqrMagnitude < range) {
						localInstances.Add(new InstanceItem(instance));
					}
				}
			}
			UIKit.UpdateListContent(localInstances);
		}

		void UpdateUI()
		{
			FindLocalInstances();
		}

		StaticInstance snapTargetInstancePrevious;
		StaticInstance snapTargetInstance;
		StaticInstance selectedObject;

		void OnSelect(InstanceItem item)
		{
			var instance = item.instance;

			EditorGUI.CloseEditors();
			if (!EditorGUI.instance.IsOpen()) {
				EditorGUI.instance.Open();
			}

			if (selectedObject != null) {
				selectedObject.HighlightObject(UnityEngine.Color.clear);
				selectedObject = instance;
			}

			if (snapTargetInstance == instance) {
				snapTargetInstance = null;
			}
			KerbalKonstructs.SelectInstance(instance, false);

			instance.HighlightObject(XKCDColors.Green_Yellow);
		}

		void OnSetAsSnap(InstanceItem item)
		{
			if (snapTargetInstance != null) {
				snapTargetInstancePrevious = snapTargetInstance;
				Color highlightColor3 = new Color(0, 0, 0, 0);
				snapTargetInstance.HighlightObject(highlightColor3);
			}

			snapTargetInstance = item.instance;

			Color highlightColor4 = XKCDColors.RedPink;
			snapTargetInstance.HighlightObject(highlightColor4);
		}

		public override void CreateUI()
		{
			base.CreateUI();

			this.ChildForceExpand(true, false)

				.Add<ListView>(out instanceList)
					.PreferredHeight(400)
					.Finish()
				;

			localInstances = new InstanceItem.List (instanceList.Group);
			localInstances.Content = instanceList.Content;
			localInstances.onSelect = OnSelect;
			localInstances.onSetAsSnap = OnSetAsSnap;
		}
	}
}
