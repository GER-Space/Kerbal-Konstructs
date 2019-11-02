using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    class RecoveryBase : KKFacility
    {
        private CustomSpaceCenter customSpaceCenter = null;


        internal override void OnPostSetup()
        {
            if (customSpaceCenter == null)
            {
                StaticInstance staticInstance = InstanceUtil.GetStaticInstanceForGameObject(this.gameObject);

                if (!staticInstance.hasLauchSites)
                {

                    customSpaceCenter = new CustomSpaceCenter();
                    customSpaceCenter.isFromFacility = true;
                    customSpaceCenter.SpaceCenterName = FacilityName;
                    customSpaceCenter.staticInstance = staticInstance;
                    customSpaceCenter.gameObject = staticInstance.transform.gameObject;
                    SpaceCenterManager.AddSpaceCenter(customSpaceCenter);
                    Log.Normal("SpaceCenter created: " + FacilityName);
                }
            }
        }

        new void OnDestroy()
        {
            if (customSpaceCenter != null)
            {
                SpaceCenterManager.RemoveSpaceCenter(customSpaceCenter);
                Log.Normal("SpaceCenter removed: " + FacilityName);
            }
        }


    }
}
