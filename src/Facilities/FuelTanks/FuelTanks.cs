using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    public class FuelTanks : KKFacility
    {
        static string[] resourceTypes = new string[] { "LiquidFuel", "MonoPropellant", "Oxidizer" };

        internal override KKFacility ParseConfig(ConfigNode cfgNode)
        {
            base.ParseConfig(cfgNode);
            Merchant fuelTankMerchant = gameObject.AddComponent<Merchant>();
            fuelTankMerchant.ParseConfig(cfgNode);
            foreach (var resourceName in resourceTypes) {
				var partResource = PartResourceLibrary.Instance.resourceDefinitions[resourceName];
				if (partResource != null) {
					fuelTankMerchant.AddResource(new TradedResource { resource = partResource, multiplierSell = 1, multiplierBuy = 1 });
				}
            }
            fuelTankMerchant.FacilityType = "Merchant";
            fuelTankMerchant.FacilityName = "Refueling Station";
            return fuelTankMerchant;

        }

        void Awake()
        {
            Log.Normal("Destroying old FuelTank");
            Destroy(this);
        }
    }
}
