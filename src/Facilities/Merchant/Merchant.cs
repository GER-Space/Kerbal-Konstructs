using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    public class TradedResource
    {
        public PartResourceDefinition resource;
        public double multiplierBuy = 1;
        public double multiplierSell = 0.9;
        public bool canBeBought = true;
        public bool canBeSold = true;
    }


    public class Merchant : KKFacility
    {
        Dictionary<int, TradedResource> tradedResByID;
        Dictionary<string, TradedResource> tradedResources;

		public List<TradedResource> Resources
		{
			get {
				return new List<TradedResource>(tradedResources.Values);
			}
		}

		public void AddResource (TradedResource resource)
		{
			tradedResByID[resource.resource.id] = resource;
			tradedResources[resource.resource.name] = resource;
		}

		public void RemoveResource (TradedResource resource)
		{
			tradedResByID.Remove(resource.resource.id);
			tradedResources.Remove(resource.resource.name);
		}

		public TradedResource GetResource(int id)
		{
			TradedResource resource;
			tradedResByID.TryGetValue (id, out resource);
			return resource;
		}

		public TradedResource GetResource(string name)
		{
			TradedResource resource;
			tradedResources.TryGetValue (name, out resource);
			return resource;
		}

        /// <summary>
        /// Override to the normal config parser, so we can load the resources
        /// </summary>
        /// <param name="cfgNode"></param>
        /// <returns></returns>
        internal override KKFacility ParseConfig(ConfigNode cfgNode)
        {
            base.ParseConfig(cfgNode);
			tradedResByID = new Dictionary<int, TradedResource>();
			tradedResources = new Dictionary<string, TradedResource>();

            string resourceName = null;
            PartResourceDefinition foundResource = null;
            TradedResource tradedResource = null;


            foreach (ConfigNode resourceNode in cfgNode.GetNodes("TradedResource")) {
                resourceName = resourceNode.GetValue("ResourceName");
                foundResource = PartResourceLibrary.Instance.GetDefinition(resourceName);
                if (foundResource == null) {
                    Log.UserWarning("Resource not found: " + resourceName);
                } else {
					double multiplierBuy;
					double multiplierSell;
					bool canBeBought;
					bool canBeSold;

					double.TryParse(resourceNode.GetValue("MultiplierBuy"), out multiplierBuy);
					double.TryParse(resourceNode.GetValue("MultiplierSell"), out multiplierSell);
					bool.TryParse(resourceNode.GetValue("CanBeBought"), out canBeBought);
					bool.TryParse(resourceNode.GetValue("CanBeSold"), out canBeSold);
                    tradedResource = new TradedResource() {
                        resource = foundResource,
                        multiplierBuy = multiplierSell,
                        multiplierSell = multiplierSell,
                        canBeBought = canBeBought,
                        canBeSold = canBeSold
                    };
					AddResource (tradedResource);
                }
            }
            return this;
        }

        internal override void WriteConfig(ConfigNode cfgNode)
        {
            ConfigNode resourceNode = null;

            base.WriteConfig(cfgNode);

			if (tradedResources != null) {
				foreach (TradedResource resource in tradedResources.Values) {
					resourceNode = new ConfigNode("TradedResource");
					resourceNode.SetValue("ResourceName", resource.resource.name,true);
					resourceNode.SetValue("MultiplierBuy", resource.multiplierBuy, true);
					resourceNode.SetValue("MultiplierSell", resource.multiplierSell, true);
					resourceNode.SetValue("CanBeBought", resource.canBeBought, true);
					resourceNode.SetValue("CanBeSold", resource.canBeSold, true);
					cfgNode.AddNode(resourceNode);
				}
			}
        }
    }
}
