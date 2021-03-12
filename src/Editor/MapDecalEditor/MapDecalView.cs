using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using KodeUI;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.UI
{

	public class MapDecalView : VerticalLayout, TabController.ITabItem
	{
#region TabController.ITabItem
		public string TabName { get { return KKLocalization.EditorMapDecals; } }
		public bool TabEnabled { get { return true; } }

		public void SetTabVisible(bool visible)
		{
			SetActive (visible);
			UpdateUI();
		}
#endregion

		ListView decalList;
		//InputLine groupFilter;
		MapDecalItem.List decalItems;

		void BuildDecalList()
		{
			CelestialBody body = FlightGlobals.currentMainBody;
			Vector3 position = FlightGlobals.ActiveVessel.GetTransform().position;
			float range = KerbalKonstructs.localGroupRange;
			range *= range;

			decalItems.Clear();

			for (int i = DecalsDatabase.allMapDecalInstances.Length; i--> 0; ) {
				var decal = DecalsDatabase.allMapDecalInstances[i];
				if (decal.CelestialBody == body) {
					Vector3 dist = position - decal.mapDecal.transform.position;
					if (dist.sqrMagnitude < range) {
						decalItems.Add(new MapDecalItem(decal));
					}
				}
			}
			UIKit.UpdateListContent(decalItems);
		}

		void OnSelect(MapDecalItem item)
		{
			EditorGUI.CloseEditors();
			MapDecalEditor.Instance.Close();
			MapDecalEditor.selectedDecal = item.decal;
			MapDecalEditor.Instance.Open();
		}

		/*void UpdateFilters()
		{
		}*/

		void SpawnDecal()
		{
			EditorGUI.instance.Close();
			EditorGUI.selectedInstance = null;

			MapDecalEditor.selectedDecal = MapDecalUtils.SpawnNewDecalInstance();

			if (MapDecalEditor.selectedDecal == null)
			{
				Log.UserError("No MapDecalInstance created");
			}

			MapDecalEditor.Instance.Open();

			Log.Normal("MapDecal Editor spawned");

		}

		public override void CreateUI()
		{
			base.CreateUI();

			this.ChildForceExpand(true, false)

				.Add<ListView>(out decalList)
					.PreferredHeight(400)
					.Finish()
				/*.Add<InputLine>(out groupFilter)
					.Label(KKLocalization.FilterByGroup)
					.OnSubmit(UpdateFilters)
					.OnFocusLost(UpdateFilters)
					.Finish()*/
				.Add<UIButton>()
					.Text(KKLocalization.SpawnDecal)
					.OnClick(SpawnDecal)
					.Finish()
				;

			decalItems = new MapDecalItem.List(decalList.Group);
			decalItems.Content = decalList.Content;
			decalItems.onSelect = OnSelect;
		}

		void UpdateUI()
		{
			BuildDecalList();
		}
	}
}
