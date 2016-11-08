using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using KerbalKonstructs.Utilities;

namespace KerbalKonstructs.UI
{
    /// <summary>
    /// This class is designed to register and draw windows. This is the foundation to seperate the game-logic from the Gui. 
    /// The "in the future to be implemented" Window-class should have: "Open" and "Close" functions which are calls to the WindowManager to register the "Draw" function.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class WindowManager : MonoBehaviour
    {

        public static WindowManager instance = null;


        private Action draw;
        private List<Action> openWindows;
        private KerbalKonstructs KKmain;

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
            KKmain = KerbalKonstructs.instance;
            KKmain.WindowManager = this;
            
        }

        /// <summary>
        /// Called every scene-switch. remove all external references here.
        /// </summary>
        public void OnDestroy()
        {
            KKmain.WindowManager = null;
            Log.Warning("WindowManager Destroyed");

        }

        /// <summary>
        /// Monobehaviour function for drawing. 
        /// </summary>
        public void OnGUI()
        {
            GUI.skin = HighLogic.Skin;

            if (!KKmain.bStylesSet)
            {
                UIMain.setStyles();
                KKmain.bStylesSet = true;
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
            if (!IsOpen(drawfunct))
            {
                openWindows.Add(drawfunct);
                draw += drawfunct;
            }
        }

        /// <summary>
        /// Removes a function pointer from the list of open windows.
        /// </summary>
        /// <param name="drawfunct"></param>
        public void CloseWindow(Action drawfunct)
        {
            if (IsOpen(drawfunct))
            {
                openWindows.Remove(drawfunct);
                draw -= drawfunct;
            }
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