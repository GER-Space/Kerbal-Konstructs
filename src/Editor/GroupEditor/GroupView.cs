using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using KodeUI;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.UI
{

	public class GroupView : VerticalLayout, TabController.ITabItem
	{
#region TabController.ITabItem
		public string TabName { get { return KKLocalization.EditorGroups; } }
		public bool TabEnabled { get { return true; } }

		public void SetTabVisible(bool visible)
		{
			SetActive (visible);
			UpdateUI();
		}
#endregion

		GroupCenter _activeGroup;
		GroupCenter activeGroup
		{
			get { return _activeGroup; }
			set {
				_activeGroup = value;
				cloneToActive.interactable = _activeGroup != null;
				if (_activeGroup != null) {
					activeGroupName.Info(_activeGroup.Group);
				} else {
					activeGroupName.Info("");
				}
			}
		}
		GroupItem.List localGroups;
		ListView groupList;
		InfoLine activeGroupName;
		UIButton cloneToActive;

		void FindLocalGroups()
		{
			CelestialBody body = FlightGlobals.currentMainBody;
			Vector3 position = FlightGlobals.ActiveVessel.GetTransform().position;
			float range = KerbalKonstructs.localGroupRange;
			range *= range;

			localGroups.Clear();

			for (int i = 0; i < StaticDatabase.allGroupCenters.Length; i++) {
				var groupCenter = StaticDatabase.allGroupCenters[i];
				//if (groupCenter.isInSavegame || groupCenter.isBuiltIn) {
				//	continue;
				//}
				if (groupCenter.CelestialBody == body) {
					Vector3 dist = position - groupCenter.gameObject.transform.position;
					if (dist.sqrMagnitude < range) {
						localGroups.Add(new GroupItem(groupCenter));
					}
				}
			}
			UIKit.UpdateListContent(localGroups);
		}

		void UpdateUI()
		{
			FindLocalGroups();
		}

		void OnSelect(GroupItem item)
		{
			activeGroup = item.group;
			EditorGUI.CloseEditors();
			MapDecalEditor.Instance.Close();
			GroupEditor.instance.Close();
			GroupEditor.selectedGroup = item.group;
			// MapDecalEditor.selectedDecal = mapDecalInstance;
			GroupEditor.instance.Open();
		}

		void SpawnGroup()
		{
			EditorGUI.instance.Close();
			MapDecalEditor.Instance.Close();
			EditorGUI.selectedInstance = null;

			GroupCenter groupCenter = new GroupCenter {
				RadialPosition = FlightGlobals.currentMainBody.transform.InverseTransformPoint(FlightGlobals.ActiveVessel.transform.position),
				Group = "NewGroup",
				CelestialBody = FlightGlobals.currentMainBody
			};
			groupCenter.Spawn();

			GroupEditor.selectedGroup = groupCenter;

			if (GroupEditor.selectedGroup == null) {
				Log.UserError("No Group created");

			} else {
				Log.Normal("Group Editor spawned");

				localGroups.Add(new GroupItem(groupCenter));
				UIKit.UpdateListContent(localGroups);

				GroupEditor.instance.Open();
			}
		}

		void CloneToActiveGroup()
		{
			/*GroupSelectorUI.instance.Close();
			GroupSelectorUI.showOnlyLocal = false;
			GroupSelectorUI.titleText = "Select Group to Clone";
			GroupSelectorUI.callBack = activeGroup.CopyGroup;
			GroupSelectorUI.instance.Open();*/
		}

		internal GroupCenter GetCloesedCenter(Vector3 position)
		{
			if (activeGroup != null) {
				return activeGroup;
			}

			float bestDist = float.PositiveInfinity;
			GroupCenter closest = null;
			for (int i = localGroups.Count; i-- > 0; ) {
				var gc = localGroups[i].group;
				float dist = (position - gc.gameObject.transform.position).sqrMagnitude;
				if (dist < bestDist) {
					bestDist = dist;
					closest = gc;
				}
			}
			return closest;
		}

		public override void CreateUI()
		{
			base.CreateUI();

			this.ChildForceExpand(true, false)

				.Add<ListView>(out groupList)
					.PreferredHeight(400)
					.Finish()
				.Add<InfoLine>(out activeGroupName)
					.Label(KKLocalization.ActiveGroup)
					.Finish()
				.Add<HorizontalLayout>()
					.Add<UIButton>()
						.Text(KKLocalization.SpawnGroup)
						.OnClick(SpawnGroup)
						.FlexibleLayout(true, false)
						.Finish()
					.Add<UIButton>(out cloneToActive)
						.Text(KKLocalization.CloneToActiveGroup)
						.OnClick(CloneToActiveGroup)
						.FlexibleLayout(true, false)
						.Finish()
					.Finish()
				;

			localGroups = new GroupItem.List(groupList.Group);
			localGroups.Content = groupList.Content;
			localGroups.onSelect = OnSelect;
		}
	}
}
