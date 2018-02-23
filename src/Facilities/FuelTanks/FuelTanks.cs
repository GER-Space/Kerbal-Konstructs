using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    internal class FuelTanks : KKFacility
    {

        private string[] resourceTypes = new string[] { "LiquidFuel", "MonoPropellant" , "Oxidizer" };

        internal override KKFacility ParseConfig(ConfigNode cfgNode)
        {
            base.ParseConfig(cfgNode);

            Merchant fuelTankMerchant = new Merchant();
            foreach (var resourceName in resourceTypes)
            {

                foreach (var partResource in PartResourceLibrary.Instance.resourceDefinitions)
                {
                    if (partResource.name == resourceName)
                    {
                        fuelTankMerchant.tradedResources.Add(new TradedResource { resource = partResource , multiplierSell = 1 , multiplierBuy = 1});
                        break;
                    }
                }

            }
            fuelTankMerchant.FacilityType = "Merchant";
            fuelTankMerchant.FacilityName = "Refueling Station";
            return fuelTankMerchant;

        }

    }

}
