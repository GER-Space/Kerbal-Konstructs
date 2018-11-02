using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;
using KerbalKonstructs;
using UnityEngine;

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
                customSpaceCenter = new CustomSpaceCenter();
                customSpaceCenter.isFromFacility = true;
                customSpaceCenter.SpaceCenterName = FacilityName;
                customSpaceCenter.staticInstance = staticInstance;
                customSpaceCenter.gameObject = staticInstance.gameObject;
                SpaceCenterManager.AddSpaceCenter(customSpaceCenter);
                Log.Normal("SpaceCenter created: " + FacilityName);
            }
        }

        new void OnDestroy()
        {
            SpaceCenterManager.RemoveSpaceCenter(customSpaceCenter);
            Log.Normal("SpaceCenter removed: " + FacilityName);
        }


    }
}
