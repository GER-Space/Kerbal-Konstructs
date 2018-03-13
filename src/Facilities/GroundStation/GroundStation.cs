using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    class GroundStation : KKFacility
    {
        [CFGSetting]
        public float TrackingShort = 0f;


        internal override void SetOpen()
        {
            base.SetOpen();
            // Callback to CommNet.
            ConnectionManager.AttachGroundStation(staticInstance);
        }

        internal override void SetClosed()
        {
            base.SetClosed();
            ConnectionManager.DetachGroundStation(staticInstance);
        }

    }
}