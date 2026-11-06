using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    internal class TradedResource 
    {
        internal PartResourceDefinition resource;
        internal float multiplierBuy = 1f;
        internal float multiplierSell = 0.9f;
        internal bool canBeBought = true;
        internal bool canBeSold = true;
    }


    public class Merchant : KKFacility
    {
        internal HashSet<TradedResource> tradedResources = new HashSet<TradedResource>();

        /// <summary>
        /// Override to the normal config parser, so we can load the resources
        /// </summary>
        /// <param name="cfgNode"></param>
        /// <returns></returns>
        public override KKFacility ParseConfig(ConfigNode cfgNode)
        {
            base.ParseConfig(cfgNode);
            tradedResources = new HashSet<TradedResource>();

            string resourceName = null;
            PartResourceDefinition foundResource = null;
            TradedResource tradedResource = null;


            foreach (ConfigNode resourceNode in cfgNode.GetNodes("TradedResource"))
            {
                resourceName = resourceNode.GetValue("ResourceName");
                foundResource = PartResourceLibrary.Instance.GetDefinition(resourceName);
                if (foundResource == null)
                {
                    Log.UserWarning("Resource not found: " + resourceName);
                } else {
                    tradedResource = new TradedResource()
                    {
                        resource = foundResource,
                        multiplierBuy = float.Parse(resourceNode.GetValue("MultiplierBuy")),
                        multiplierSell = float.Parse(resourceNode.GetValue("MultiplierSell")),
                        canBeBought = bool.Parse(resourceNode.GetValue("CanBeBought")),
                        canBeSold = bool.Parse(resourceNode.GetValue("CanBeSold"))
                    };
                    tradedResources.Add(tradedResource);
                }
            }
            return this;
        }

        internal override void WriteConfig(ConfigNode cfgNode)
        {
            ConfigNode resourceNode = null;

            base.WriteConfig(cfgNode);

            foreach (TradedResource resource in tradedResources)
            {
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