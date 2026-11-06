using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    internal class FuelTanks : KKFacility
    {

        private string[] resourceTypes = new string[] { "LiquidFuel", "MonoPropellant", "Oxidizer" };

        public override KKFacility ParseConfig(ConfigNode cfgNode)
        {
            base.ParseConfig(cfgNode);
            Merchant fuelTankMerchant = gameObject.AddComponent<Merchant>();
            fuelTankMerchant.ParseConfig(cfgNode);
            foreach (var resourceName in resourceTypes)
            {

                foreach (var partResource in PartResourceLibrary.Instance.resourceDefinitions)
                {
                    if (partResource.name == resourceName)
                    {
                        fuelTankMerchant.tradedResources.Add(new TradedResource { resource = partResource, multiplierSell = 1, multiplierBuy = 1 });
                        break;
                    }
                }

            }
            fuelTankMerchant.FacilityType = "Merchant";
            fuelTankMerchant.FacilityName = "Refueling Station";
            return fuelTankMerchant;

        }

        public void Awake()
        {
            Log.Normal("Destroying old FuelTank");
            Destroy(this);
        }


    }
}
