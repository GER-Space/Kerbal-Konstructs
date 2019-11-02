using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs.UI
{
    /// <summary>
    /// This class is designed to register and draw windows. This is the foundation to seperate the game-logic from the Gui. 
    /// Look in the implementation of the KKWindow class about registering daw functions.
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
            if (instance != null)
            {
                Destroy(this);
                return;
            }
            instance = this;
            //DontDestroyOnLoad(instance);
            draw = delegate { };
            openWindows = new List<Action>();
        }

        #region Monobehavior functions
        /// <summary>
        /// Called after Awake. used for setting up references between objects and initializing windows.
        /// </summary>
        public void Start()
        {
            KKmain = KerbalKonstructs.instance;

        }

        /// <summary>
        /// Called every scene-switch. remove all external references here.
        /// </summary>
        public void OnDestroy()
        {

        }

        /// <summary>
        /// Monobehaviour function for drawing. 
        /// </summary>
        public void OnGUI()
        {
            GUI.skin = HighLogic.Skin;
            if (!UIMain.layoutIsInitialized)
            {
                UIMain.SetTextures();
                UIMain.SetStyles();
                UIMain.layoutIsInitialized = true;
            }
            draw.Invoke();
        }
        #endregion


        #region public Functions

        /// <summary>
        /// Adds a function pointer to the list of drawn windows.
        /// </summary>
        /// <param name="drawfunct"></param>
        public static void OpenWindow(Action drawfunct)
        {
            if (!IsOpen(drawfunct))
            {
                instance.openWindows.Add(drawfunct);
                instance.draw += drawfunct;
            }
        }

        /// <summary>
        /// Removes a function pointer from the list of open windows.
        /// </summary>
        /// <param name="drawfunct"></param>
        public static void CloseWindow(Action drawfunct)
        {
            if (IsOpen(drawfunct))
            {
                instance.openWindows.Remove(drawfunct);
                instance.draw -= drawfunct;
            }
        }

        /// <summary>
        /// Opens a closed window or closes an open one.
        /// </summary>
        /// <param name="drawfunct"></param>
        public static void ToggleWindow(Action drawfunct)
        {
            if (IsOpen(drawfunct))
            {
                CloseWindow(drawfunct);
            }
            else
            {
                OpenWindow(drawfunct);
            }

        }

        /// <summary>
        /// checks if a window is openend
        /// </summary>
        /// <param name="drawfunct"></param>
        /// <returns></returns>
        public static bool IsOpen(Action drawfunct)
        {
            return instance.openWindows.Contains(drawfunct);
        }


        #endregion



    }
}