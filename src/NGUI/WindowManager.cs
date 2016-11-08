using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using KerbalKonstructs.UI;
using KerbalKonstructs.Utilities;

namespace KerbalKonstructs.NGUI
{

    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class WindowManager : MonoBehaviour
    {

        public static WindowManager instance = null;

        #region GUI Windows
        internal KSCManagerNGUI GUI_KSCManager;
        internal KKSettingsNGUI GUI_Settings;

        internal EditorGUI GUI_Editor;
        internal StaticsEditorGUI GUI_StaticsEditor ;
        internal NavGuidanceSystem GUI_NGS;
        internal DownlinkGUI GUI_Downlink;
        internal BaseBossFlight GUI_FlightManager ;
        internal FacilityManager GUI_FacilityManager;
        internal LaunchSiteSelectorGUI GUI_LaunchSiteSelector;
        internal MapIconManager GUI_MapIconManager;
        internal AirRacing GUI_AirRacingApp;
        internal BaseManager GUI_BaseManager;
        internal ModelInfo GUI_ModelInfo;
        #endregion

        private Action draw;
        private List<Action> openWindows;


        /// <summary>
        /// First called before start. used settig up internal vaiabled
        /// </summary>
        public void Awake()
        {
            Log.Warning("WindowManager Awake");
            if (instance != null)
            {
                Destroy(this);
                return;
            }
            instance = this;
            draw = delegate { };
            openWindows = new List<Action>();
        }

        #region Monobehavior functions
        /// <summary>
        /// Called after Awake. used for setting up references between objects and initializing windows.
        /// </summary>
        public void Start()
        {
            Log.Warning("WindowManager Start");
            KerbalKonstructs.instance.WindowManager = this;

            GUI_Editor = new EditorGUI();
            GUI_StaticsEditor = new StaticsEditorGUI();
            GUI_NGS = new NavGuidanceSystem();
            GUI_Downlink = new DownlinkGUI();
            GUI_FlightManager = new BaseBossFlight();
            GUI_FacilityManager = new FacilityManager();
            GUI_LaunchSiteSelector = new LaunchSiteSelectorGUI();
            GUI_MapIconManager = new MapIconManager();
            GUI_KSCManager = new KSCManagerNGUI();
            GUI_AirRacingApp = new AirRacing();
            GUI_BaseManager = new BaseManager();
            GUI_Settings = new KKSettingsNGUI();
            GUI_ModelInfo = new ModelInfo();
        }

        /// <summary>
        /// Called every scene-switch. remove all external references here.
        /// </summary>
        public void OnDestroy()
        {
            KerbalKonstructs.instance.WindowManager = null;
            Log.Warning("WindowManager Destroyed");

        }

        public void OnGUI()
        {
            GUI.skin = HighLogic.Skin;

            if (!KerbalKonstructs.instance.bStylesSet)
            {
                UIMain.setStyles();
                KerbalKonstructs.instance.bStylesSet = true;
            }
            
            draw.Invoke();
        }
        #endregion
        #region public Functions

        /// <summary>
        /// Adds a function pointer to the list of drawn windows.
        /// </summary>
        /// <param name="drawfunct"></param>
        public void OpenWindow(Action drawfunct)
        {
            openWindows.Add(drawfunct);
            draw += (Action)drawfunct;
        }

        /// <summary>
        /// Removes a function pointer from the list of open windows.
        /// </summary>
        /// <param name="drawfunct"></param>
        public void CloseWindow(Action drawfunct)
        {
            openWindows.Remove(drawfunct);
            draw -= (Action)drawfunct;
        }

        /// <summary>
        /// Opens a closed window or closes an open one.
        /// </summary>
        /// <param name="drawfunct"></param>
        public void ToggleWindow(Action drawfunct)
        {
            if (IsOpen(drawfunct))
            {
                CloseWindow(drawfunct);
            } else
            {
                OpenWindow(drawfunct);
            }

        }

        /// <summary>
        /// checks if a window is openend
        /// </summary>
        /// <param name="drawfunct"></param>
        /// <returns></returns>
        public bool IsOpen(Action drawfunct)
        {
            return openWindows.Contains(drawfunct);
        }

        #endregion



    }
}