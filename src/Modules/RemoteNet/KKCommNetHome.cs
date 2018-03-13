using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommNet;
using KSP.Localization;

namespace KerbalKonstructs.Modules
{

    internal class KKCommNetHome : CommNetHome
    {
        //internal new CommNode comm;
        //internal new CelestialBody body;
        //internal new double lat;

        internal KKCommNetHome()
        {
            this.CreateNode();
        }

        internal void RestoreStockStation(ConnectionManager.StockStation stockHome)
        {
            this.nodeName = stockHome.nodeName;
            this.displaynodeName = Localizer.Format(stockHome.displaynodeName);
            this.nodeTransform = stockHome.nodeTransform;
            this.isKSC = stockHome.isKSC;
            //this.body = stockHome.GetComponentInParent<CelestialBody>();
        }
    }
}
