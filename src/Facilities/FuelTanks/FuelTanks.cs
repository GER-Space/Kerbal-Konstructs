using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    internal class FuelTanks : KKFacility
    {
        [CFGSetting]
        public float LqFMax;
        [CFGSetting]
        public float OxFMax;
        [CFGSetting]
        public float MoFMax;

        [CFGSetting]
        public string LqFAlt = "";
        [CFGSetting]
        public string OxFAlt = "";
        [CFGSetting]
        public string MoFAlt = "";

        [CareerSetting]
        public float LqFCurrent;
        [CareerSetting]
        public float OxFCurrent;
        [CareerSetting]
        public float MoFCurrent;

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
            return fuelTankMerchant;

        }

    }

}
