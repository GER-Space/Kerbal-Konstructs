using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    internal class StoredResource 
    {
        internal PartResourceDefinition resource;
        internal float amount = 0f;
    }


    public class Storage : KKFacility
    {

        [CFGSetting]
        public float MaxAmount = 0f;


        internal HashSet<StoredResource> storedResources = new HashSet<StoredResource>();

        /// <summary>
        /// Override to the normal config parser, so we can load the resources
        /// </summary>
        /// <param name="cfgNode"></param>
        /// <returns></returns>
        internal override void LoadCareerConfig(ConfigNode cfgNode)
        {
            base.ParseConfig(cfgNode);
            storedResources = new HashSet<StoredResource>();

            string resourceName = null;
            PartResourceDefinition foundResource = null;
            StoredResource tradedResource = null;


            foreach (ConfigNode resourceNode in cfgNode.GetNodes("StoredResource"))
            {
                resourceName = resourceNode.GetValue("ResourceName");
                foundResource = PartResourceLibrary.Instance.GetDefinition(resourceName);
                if (foundResource == null)
                {
                    Log.UserWarning("Resource not found: " + resourceName);
                } else {
                    tradedResource = new StoredResource()
                    {
                        resource = foundResource,
                        amount = float.Parse(resourceNode.GetValue("Amount")),
                    };
                    storedResources.Add(tradedResource);
                }
            }
        }

        internal override void SaveCareerConfig(ConfigNode cfgNode)
        {
            ConfigNode resourceNode = null;

            base.WriteConfig(cfgNode);

            foreach (StoredResource resource in storedResources)
            {
                resourceNode = new ConfigNode("StoredResource");
                resourceNode.SetValue("ResourceName", resource.resource.name,true);
                resourceNode.SetValue("Amount", resource.amount, true);
                cfgNode.AddNode(resourceNode);
            }

        }

    }
}