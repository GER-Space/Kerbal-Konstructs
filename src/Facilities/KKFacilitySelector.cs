using KerbalKonstructs.Modules;
using KerbalKonstructs.UI;
using KSP.UI;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KerbalKonstructs.Core
{

    internal class KKFacilitySelector : MonoBehaviour
    {
        // we get this passed through the facility module
        internal StaticInstance staticInstance = null;
        internal KKFacilityType facType = KKFacilityType.None;
        internal KKFacility facility = null;


        internal bool initialized = false;

        private List<KKFacilityType> spaceCenterBlackList = new List<KKFacilityType> { KKFacilityType.Hangar, KKFacilityType.FuelTanks, KKFacilityType.LandingGuide, KKFacilityType.Merchant, KKFacilityType.Storage, KKFacilityType.TouchdownGuideL, KKFacilityType.TouchdownGuideR, KKFacilityType.TrackingStation };


        internal void Initialize()
        {
            staticInstance = facility.staticInstance;

            if (this.gameObject == null)
            {
                Destroy(this);
            }
            if (staticInstance == null)
            {
                staticInstance = InstanceUtil.GetStaticInstanceForGameObject(this.gameObject);
            }
            if (staticInstance == null)
            {
                Log.UserInfo("Cound not determin instance for mouse selector");
                Destroy(this);
            }
            facType = facility.facType;
            initialized = true;
        }


        internal void SpaceCenterAction()
        {
            switch (facType)
            {
                case KKFacilityType.Barracks:
                    var acPrefab = FindObjectOfType<ACSceneSpawner>();
                    UIMasterController.Instance.AddCanvas(acPrefab.ACScreenPrefab, true);
                    break;
                case KKFacilityType.Business:
                    if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    {
                        var adminPrefab = FindObjectOfType<AdministrationSceneSpawner>();
                        UIMasterController.Instance.AddCanvas(adminPrefab.AdministrationScreenPrefab, true);
                    }
                    break;
                case KKFacilityType.Research:
                    if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
                    {
                        var researchPrefab = FindObjectOfType<RDSceneSpawner>();
                        UIMasterController.Instance.AddCanvas(researchPrefab.RDScreenPrefab, true);
                    }
                    break;
                case KKFacilityType.GroundStation:
                    HighLogic.LoadScene(GameScenes.TRACKSTATION);
                    break;
            }
        }


        #region Unity mouse extension

        void OnMouseDown()
        {
            if (EventSystem.current.IsPointerOverGameObject()) {
                return;
            }
            if (!initialized)
            {
                Initialize();
            }

            if (HighLogic.LoadedScene == GameScenes.FLIGHT && !InputLockManager.IsLocked(ControlTypes.FLIGHTUIMODE))
            {
                FacilityManager.selectedInstance = facility.staticInstance;
                FacilityManager.instance.Open();
            }

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER && !InputLockManager.IsLocked(ControlTypes.KSC_FACILITIES) && !(InputLockManager.GetControlLock("KK_KSC") == ControlTypes.UI))
            {
                staticInstance.HighlightObject(Color.clear);
                SpaceCenterAction();
            }


        }

        void OnMouseEnter()
        {
            if (EventSystem.current.IsPointerOverGameObject()) {
                return;
            }
            if (!initialized)
            {
                Initialize();
            }

            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                if (!KerbalKonstructs.enableInflightHighlight && facType != KKFacilityType.Merchant)
                {
                    return;
                }
                try
                {
                    if (staticInstance.myFacilities.First().isOpen)
                    {
                        staticInstance.HighlightObject(new Color(0.4f, 0.9f, 0.4f, 0.5f));
                    }
                    else
                    {
                        staticInstance.HighlightObject(new Color(0.9f, 0.4f, 0.4f, 0.5f));
                    }
                }
                catch
                {
                    Destroy(this);
                }
            }

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER && !(InputLockManager.GetControlLock("KK_KSC") == ControlTypes.UI))
            {
                if (spaceCenterBlackList.Contains(facType))
                {
                    return;
                }

                try
                {
                    staticInstance.HighlightObject(new Color(0.7f, 0.7f, 0.7f));
                }
                catch
                {
                    Destroy(this);
                }
            }
        }

        void OnMouseExit()
        {
            if (EventSystem.current.IsPointerOverGameObject()) {
                return;
            }
            if (!initialized)
            {
                Initialize();
            }
            if ((HighLogic.LoadedScene == GameScenes.FLIGHT) || (HighLogic.LoadedScene == GameScenes.SPACECENTER))
            {
                staticInstance.HighlightObject(Color.clear);
            }
        }
        #endregion


    }

}
