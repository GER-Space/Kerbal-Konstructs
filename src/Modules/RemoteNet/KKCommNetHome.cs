using CommNet;
#if!KSP12
using KSP.Localization;
#endif
namespace KerbalKonstructs.Modules
{

    internal class KKCommNetHome : CommNetHome
    {
        internal new CommNode comm;
        //internal new CelestialBody body;
        //internal new double lat;

        internal KKCommNetHome()
        {
        }

        internal void RestoreStockStation(ConnectionManager.StockStation stockHome)
        {
            this.nodeName = stockHome.nodeName;
#if !KSP12
            this.displaynodeName = Localizer.Format(stockHome.displaynodeName);
#endif
            this.nodeTransform = stockHome.nodeTransform;
            this.isKSC = stockHome.isKSC;

            this.comm = new CommNode();
            this.comm.antennaTransmit.power = 500000d;


            //this.body = stockHome.GetComponentInParent<CelestialBody>();
        }
    }
}
