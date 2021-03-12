using KerbalKonstructs.Core;
using System.Collections.Generic;

namespace KerbalKonstructs.Modules
{
    public class StoredResource
    {
		public int id { get { return resource.id; } }
		public double volume { get { return resource.volume; } }
        public PartResourceDefinition resource;
        public double amount = 0f;
    }


    public class Storage : KKFacility
    {
        [CFGSetting]
        public double maxVolume = 0f;


        public Dictionary<int, StoredResource> storedResources { get; private set; }

        public double currentVolume
        {
            get {
				if (storedResources == null) {
					return 0;
				}
                double retval = 0;
                foreach (StoredResource resource in storedResources.Values) {
                    retval += (resource.amount * resource.resource.volume);
                }
                return retval;
            }
        }

		public StoredResource GetResource(PartResourceDefinition resource)
		{
			StoredResource storedResource;
			if (!storedResources.TryGetValue(resource.id, out storedResource)) {
				storedResource = new StoredResource { resource = resource, amount = 0 };
				storedResources[resource.id] = storedResource;
			}
			return storedResource;
		}

        /// <summary>
        /// Stores or retrieves a resource to the facility. Deletes the resource if nothing is left
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="amount"></param>
        public void StoreResource(PartResourceDefinition resource, double delta)
        {
			StoredResource storedResource;
			if (!storedResources.TryGetValue(resource.id, out storedResource)) {
				if (delta <= 0) {
					return;
				}
				storedResource = new StoredResource { resource = resource, amount = 0 };
				storedResources[resource.id] = storedResource;
			}
			storedResource.amount += delta;

            if (storedResource.amount < 0) {
                storedResource.amount = 0;
            }
        }

        /// <summary>
        /// Override to the normal config parser, so we can load the resources
        /// </summary>
        /// <param name="cfgNode"></param>
        /// <returns></returns>
        internal override void LoadCareerConfig(ConfigNode cfgNode)
        {
            base.ParseConfig(cfgNode);
            storedResources = new Dictionary<int, StoredResource>();

            string resourceName = null;
            PartResourceDefinition foundResource = null;
            StoredResource tradedResource = null;


            foreach (ConfigNode resourceNode in cfgNode.GetNodes("StoredResource")) {
                resourceName = resourceNode.GetValue("ResourceName");
                foundResource = PartResourceLibrary.Instance.GetDefinition(resourceName);
                if (foundResource == null) {
                    Log.UserWarning("Resource not found: " + resourceName);
                } else {
					double Amount;
					double.TryParse(resourceNode.GetValue("Amount"), out Amount);
                    tradedResource = new StoredResource()
                    {
                        resource = foundResource,
                        amount = Amount
                    };
                    storedResources[tradedResource.id] = tradedResource;
                }
            }
        }

        internal override void SaveCareerConfig(ConfigNode cfgNode)
        {
            ConfigNode resourceNode = null;

            base.WriteConfig(cfgNode);

			if (storedResources != null) {
				foreach (StoredResource resource in storedResources.Values) {
					if (resource.amount > 0) {
						resourceNode = new ConfigNode("StoredResource");
						resourceNode.SetValue("ResourceName", resource.resource.name, true);
						resourceNode.SetValue("Amount", resource.amount, true);
						cfgNode.AddNode(resourceNode);
					}
				}
            }
        }

    }
}
