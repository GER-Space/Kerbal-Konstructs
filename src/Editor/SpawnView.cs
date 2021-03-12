using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using KodeUI;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.UI
{

	public class SpawnView : VerticalLayout, TabController.ITabItem
	{
#region TabController.ITabItem
		public string TabName { get { return KKLocalization.EditorSpawnNew; } }
		public bool TabEnabled { get { return true; } }

		public void SetTabVisible(bool visible)
		{
			SetActive (visible);
			if (visible) {
				UpdateUI();
			}
		}
#endregion

		ModelItem.List modelItems;
		ListView modelList;
		InputLine titleFilter;
		InputLine categoryFilter;
		ToggleText sortByCategory;
		ToggleText sortByTitle;
		ToggleText sortByMesh;

		bool Filter(string text, string filter)
		{
			return text.ToLower().Contains(filter.ToLower());
		}

		void BuildModelList ()
		{
			bool filterTitle = !String.IsNullOrEmpty(titleFilter.text);
			bool filterCategory = !String.IsNullOrEmpty(categoryFilter.text);

			for (int i = StaticDatabase.allStaticModels.Count; i-- > 0; ) {
				StaticModel model = StaticDatabase.allStaticModels[i];
				if (filterTitle && filterCategory) {
					if (!(Filter(model.title, titleFilter.text)
						  && Filter(model.category, categoryFilter.text))) {
						continue;
					}
				} else if (filterTitle) {
					if (!(Filter(model.title, titleFilter.text))) {
						continue;
					}
				} else if (filterCategory) {
					if (!(Filter(model.category, categoryFilter.text))) {
						continue;
					}
				}
				modelItems.Add(new ModelItem (model));
			}
		}

		enum ModelSort {
			Category,
			Title,
			Mesh,
		};
		ModelSort modelSort = ModelSort.Category;

		void UpdateUI()
		{
			BuildModelList();
			UpdateSort(modelSort);
		}

		void UpdateSort(ModelSort sort)
		{
			modelSort = sort;
			switch (modelSort) {
				case ModelSort.Category:
					modelItems.Sort((a, b) => a.category.CompareTo(b.category));
					break;
				case ModelSort.Title:
					modelItems.Sort((a, b) => a.title.CompareTo(b.title));
					break;
				case ModelSort.Mesh:
					modelItems.Sort((a, b) => a.mesh.CompareTo(b.mesh));
					break;
			}
			UIKit.UpdateListContent(modelItems);
		}

		void UpdateFilters(string str = null)
		{
			UpdateUI();
		}

		public override void CreateUI()
		{
			base.CreateUI();

			ToggleGroup group;

			this.ChildForceExpand(true, false)

				.Add<HorizontalLayout>()
					.ToggleGroup(out group)
					.Add<ToggleText>(out sortByCategory)
						.Text(KKLocalization.SortByCategory)
						.Group(group)
						.SetIsOnWithoutNotify(modelSort == ModelSort.Category)
						.OnValueChanged(on => { if (on) UpdateSort(ModelSort.Category); })
						.Finish()
					.Add<ToggleText>(out sortByTitle)
						.Text(KKLocalization.SortByTitle)
						.Group(group)
						.SetIsOnWithoutNotify(modelSort == ModelSort.Title)
						.OnValueChanged(on => { if (on) UpdateSort(ModelSort.Title); })
						.Finish()
					.Add<ToggleText>(out sortByMesh)
						.Text(KKLocalization.SortByMesh)
						.Group(group)
						.SetIsOnWithoutNotify(modelSort == ModelSort.Mesh)
						.OnValueChanged(on => { if (on) UpdateSort(ModelSort.Mesh); })
						.Finish()
					.Finish()
				.Add<ListView>(out modelList)
					.PreferredHeight(400)
					.Finish()
				.Add<InputLine> (out titleFilter)
					.Label(KKLocalization.FilterByTitle)
					.OnSubmit (UpdateFilters)
					.OnFocusLost (UpdateFilters)
					.Finish()
				.Add<InputLine> (out categoryFilter)
					.Label(KKLocalization.FilterByCategory)
					.OnSubmit (UpdateFilters)
					.OnFocusLost (UpdateFilters)
					.Finish()
				;

			modelItems = new ModelItem.List (modelList.Group);
			modelItems.Content = modelList.Content;
		}

		/*
        internal void ShowModelsFooter()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Filter by ");
            GUILayout.Label(" Category:");
            categoryFilterString = GUILayout.TextField(categoryFilterString, 30, GUILayout.Width(90));
            if (GUILayout.Button(new GUIContent(tSearch, "Apply Filter."), GUILayout.Width(23), GUILayout.Height(23))) {
                categoryfilter = categoryFilterString;
            }
            if (GUILayout.Button(new GUIContent(tCancelSearch, "Remove Filter."), GUILayout.Width(23), GUILayout.Height(23))) {
                categoryFilterString = "";
                categoryfilter = "";
            }
            GUILayout.Label("  Title:");
            titlefilterString = GUILayout.TextField(titlefilterString, 30, GUILayout.Width(90));
            if (GUILayout.Button(new GUIContent(tSearch, "Apply Filter."), GUILayout.Width(23), GUILayout.Height(23))) {
                titleFilter = titlefilterString;
            }
            if (GUILayout.Button(new GUIContent(tCancelSearch, "Remove Filter."), GUILayout.Width(23), GUILayout.Height(23))) {
                titlefilterString = "";
                titleFilter = "";
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Default Grass Preset: " + defaultGrassPreset.name, GUILayout.Height(18));

                if (GUILayout.Button("Load Preset", GUILayout.Width(90), GUILayout.Height(18)))
                {
                    GrassColorPresetUI2.callBack = SetDefaultColorCallBack;
                    GrassColorPresetUI2.instance.Open();
                }
            }
            GUILayout.EndHorizontal();
        }
		*/

		// sort buttons (above) Category Title Mesh
        /*internal void ShowModelsScroll()
        {
            foreach (StaticModel model in allStaticModels) {
                if (showStatic) {
                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button(new GUIContent(model.category, "Filter"), DeadButton, GUILayout.Width(110), GUILayout.Height(23))) {
                        categoryfilter = model.category;
                        categoryFilterString = model.category;
                    }
                    GUILayout.Space(5);

                    if (localGroups.Length > 0) {
                        if (GUILayout.Button(new GUIContent(model.title, "Spawn an instance of this static."), DeadButton2, GUILayout.Height(23))) {
                            EditorGUI.CloseEditors();
                            KerbalKonstructs.instance.DeletePreviewObject();
                            SpawnInstance(model);
                            smessage = "Spawned " + model.title;
                            MiscUtils.HUDMessage(smessage, 10, 2);
                        }
                    } else {
                        if (GUILayout.Button(new GUIContent(model.title, "first a Local Group Center"), DeadButton2, GUILayout.Height(23))) {
                            Log.UserError("No Local Group found");
                            MiscUtils.HUDMessage("Create and place a local Group, then try again!");
                        }
                    }

                    GUILayout.FlexibleSpace();
                    if (localGroups.Length > 0) {
                        if (GUILayout.Button(new GUIContent(" " + model.mesh + " ", "Edit Model Config"), DeadButton, GUILayout.Width(200), GUILayout.Height(23))) {
                            KerbalKonstructs.instance.selectedModel = model;
                            ModelInfo.instance.Open();
                        }
                    } else {
                        if (GUILayout.Button(new GUIContent(" " + model.mesh + " ", "first a Local Group Center"), DeadButton, GUILayout.Width(200), GUILayout.Height(23))) {
                            Log.UserError("No Local Group found");
                            MiscUtils.HUDMessage("Create and place a local Group, then try again!");
                        }
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.Space(2);
                }
            }
        }*/
	}
}
