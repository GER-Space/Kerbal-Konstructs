using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using KodeUI;

namespace KerbalKonstructs.UI
{
    public class StaticsEditorGUI : Window
    {
        private enum EditorMode {
            SPAWN,
            LOCAL,
            PQS,
            GROUP
        }

        private static StaticsEditorGUI _instance = null;

        public static StaticsEditorGUI instance
        {
            get {
                if (_instance == null) {
					_instance = UIKit.CreateUI<StaticsEditorGUI> (UIMain.appCanvasRect, "KKStaticsEditorGUI");
                }
                return _instance;
            }
        }

        Rect editorRect = new Rect(10, 25, 690, 540);

        public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep3", false);
        public Texture tTick = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/settingstick", false);
        public Texture tCross = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/settingscross", false);
        public Texture tSearch = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/search", false);
        public Texture tCancelSearch = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/cancelsearch", false);
        public Texture tFocus = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/focuson", false);
        public Texture tFoldOut = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldin", false);
        public Texture tFoldIn = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);
        public Texture tFolded = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/foldout", false);

        public Boolean bDisableEditingSetting = false;

        public StaticInstance snapTargetInstance = null;

        //internal static Color defaultGrasColor = new Color(0.640f, 0.728f, 0.171f, 0.729f);
        //internal static string defaultGrasTexture = "BUILTIN:/terrain_grass00_new";
        //private string grasColorRStr = "0.640";
        //private string grasColorGStr = "0.728";
        //private string grasColorBStr = "0.171";
        //private string grasColorAStr = "0.729";

        internal static GrassColorPresetUI2.ColorPreset2 defaultGrassPreset = new GrassColorPresetUI2.ColorPreset2();

        public float fButtonWidth = 0f;

        //float localRange = 10000f;

        //public Boolean foldedIn = false;
        //public Boolean doneFold = false;
        public Boolean bSortCategory = false;
        public Boolean bSortTitle = false;
        private string smessage = "";

		TabController tabController;
		SpawnView spawnView;
		InstanceView instanceView;
		MapDecalView mapDecalView;
		GroupView groupView;

        public void ToggleEditor()
        {
            if (KerbalKonstructs.selectedInstance != null)
            {
                KerbalKonstructs.DeselectObject(true, true);
            }

            this.Toggle();

            if (snapTargetInstance != null)
            {
                snapTargetInstance.HighlightObject(UnityEngine.Color.clear);
                snapTargetInstance = null;
            }
        }

		void Toggle()
		{
			if (IsOpen()) {
				Close();
			} else {
				Open();
			}
		}

        public void Open()
        {
            ConfigUtil.CreateNewInstanceDirIfNeeded();

			SetActive(true);
			tabController.UpdateTabStates();
        }

        public void Close()
        {
            EditorGUI.instance.Close();
            KerbalKonstructs.instance.DeletePreviewObject();
            MapDecalEditor.Instance.Close();

			SetActive(false);
        }

		public static bool IsOpen()
		{
			return _instance && _instance.gameObject.activeInHierarchy;
		}

		public override void CreateUI()
		{
			base.CreateUI();

			this.Title(KKLocalization.StaticsEditor)
				.Vertical()
				.ControlChildSize(true, true)
				.ChildForceExpand(false,false)
				.PreferredSizeFitter(true, true)
				.Anchor(AnchorPresets.TopLeft)
				.Pivot(PivotPresets.TopLeft)
				.SetSkin("KK.Default")

				.Add<HorizontalSep>("HorizontalSep3") .Space(1, 2) .Finish()

				.Add<HorizontalLayout>()
					.Add<TabController>(out tabController)
						.Horizontal()
						.ControlChildSize(true, true)
						.ChildForceExpand(false,false)
						.Finish()
					.Add<FlexibleSpace>() .Finish()
					.Add<UIButton>()
						.Text(KKLocalization.EditorExport)
						.OnClick(Export)
						//.ToolTip(KKLocalization.EditorExportTooltip)
						.Finish()
					.Add<UIButton>()
						.Text(KKLocalization.EditorSave)
						.OnClick(Save)
						//.ToolTip()KKLocalization.EditorSaveTooltip
						.Finish()
					.Finish()
				.Add<SpawnView>(out spawnView) .Finish()
				.Add<InstanceView>(out instanceView) .Finish()
				.Add<MapDecalView>(out mapDecalView) .Finish()
				.Add<GroupView>(out groupView) .Finish()
				.Add<HorizontalSep>("HorizontalSep3") .Space(1, 2) .Finish()
				.Finish();

			spawnView.SetActive(false);
			instanceView.SetActive(false);
			mapDecalView.SetActive(false);
			groupView.SetActive(false);

			var tabItems = new List<TabController.ITabItem> () {
				spawnView,
				instanceView,
				mapDecalView,
				groupView,
			};
			tabController.Items(tabItems);
			tabController.UpdateTabStates();
			tabController.SelectTab(1);

			UIMain.SetTitlebar(titlebar, Close);
		}

		void Export()
		{
			ConfigParser.ExportAll();
		}

		void Save()
		{
			KerbalKonstructs.instance.SaveObjects();
			smessage = "Saved all changes to all objects.";
			MiscUtils.HUDMessage(smessage, 10, 2);
		}

        /// <summary>
        /// wrapper for editorGUI spawnInstance
        /// </summary>
        /// <param name="model"></param>
		internal void SpawnInstance(StaticModel model)
        {
            /*XXX GroupCenter center = GetCloesedCenter(FlightGlobals.ActiveVessel.transform.position);
            EditorGUI.instance.SpawnInstance(model, center, FlightGlobals.ActiveVessel.transform.position, Vector3.zero);

            if (!EditorGUI.instance.IsOpen())
            {
                EditorGUI.instance.Open();
            }*/
        }

        internal void SetDefaultColorCallBack(GrassColorPresetUI2.ColorPreset2 preset)
        {

            defaultGrassPreset = preset;

            //defaultGrasColor = preset.grassColor;
            //defaultGrasTexture = preset.nearGrassTexture;
            //grasColorRStr = defaultGrasColor.r.ToString();
            //grasColorGStr = defaultGrasColor.g.ToString();
            //grasColorBStr = defaultGrasColor.b.ToString();
            //grasColorAStr = defaultGrasColor.a.ToString();

        }

        /// <summary>
        /// Selects Object under the mouse curser.
        /// </summary>
        internal void SelectMouseObject()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            int layerMask = ~0;

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                return;
            }

            StaticInstance myHitinstance = GetRootFromHit(hit.collider.gameObject);

            if (myHitinstance == null)
            {
                Log.Normal("No RootObject found");
                return;
            }
            else
            {
                if (KerbalKonstructs.selectedInstance != null)
                {
                    KerbalKonstructs.DeselectObject(true, true);
                }
                //Log.Normal("Try to select Object: " + myHitinstance.mesh.name);
                myHitinstance.HighlightObject(XKCDColors.Green_Yellow);
                KerbalKonstructs.SelectInstance(myHitinstance, true);
                if (!EditorGUI.instance.IsOpen())
                {
                    EditorGUI.instance.Open();
                }
            }

        }

        /// <summary>
        /// tries to find a Static Object attached to a child GameObject.
        /// </summary>
        /// <param name="foundObject"></param>
        /// <returns></returns>
        private StaticInstance GetRootFromHit(GameObject foundObject)
        {
			while (foundObject != null) {
				var instance = foundObject.GetComponent<StaticInstance>();
				if (instance != null) {
					return instance;
				}
			}
            // we didn't find any root object
            return null;
        }

    }
}
