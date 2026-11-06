using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    public class GroundStation : KKFacility
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