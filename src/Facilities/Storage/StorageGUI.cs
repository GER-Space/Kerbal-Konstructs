using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;
using KerbalKonstructs.ResourceManager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using KodeUI;

namespace KerbalKonstructs.UI
{
    internal class StorageGUI : VerticalLayout
    {

        HashSet<int> resourceSet;
		List<PartResourceDefinition> resourceList;

        static HashSet<string> blackListedResources = new HashSet<string> { "ElectricCharge", "IntakeAir" };

		StackSize stackSize;
		InfoLine volumeUsed;
		ListView storageList;
		InfoLine thanksForUsing;
		StorageItem.List storageItems;

		public override void CreateUI()
		{
			resourceSet = new HashSet<int>();
			resourceList = new List<PartResourceDefinition>();
			base.CreateUI();

			this.ChildForceExpand(true, false)
				.Add<FixedSpace>() .Size(4) .Finish()
				.Add<StackSize>(out stackSize) .Finish()
				.Add<UIText>()
					.Text(KKLocalization.StoreRetrieveResources)
					.Finish()
				.Add<InfoLine>(out volumeUsed)
					.Label(KKLocalization.VolumeUsed)
					.Finish()
				.Add<ListView>(out storageList)
					.PreferredHeight(120)
					.Finish()
				.Add<InfoLine>(out thanksForUsing)
					.Label(KKLocalization.ThanksForUsing)
					.Finish()
				;

			storageItems = new StorageItem.List();
			storageItems.Content = storageList.Content;
			storageItems.onFromVessel = OnFromVessel;
			storageItems.onToVessel = OnToVessel;
		}

		public void UpdateUI(StaticInstance instance)
		{
			var storage = instance.myFacilities[0] as Storage;
			volumeUsed.Info($"{storage.currentVolume} / {storage.maxVolume}");
			if (storageItems.Storage != storage || storageItems.Vessel != FlightGlobals.ActiveVessel) {
				BuildStorageItems(storage);
			}
			thanksForUsing.Info(storage.FacilityName);
		}

		void BuildStorageItems(Storage storage)
		{
			Vessel vessel = FlightGlobals.ActiveVessel;
			resourceSet.Clear();
			resourceList.Clear();

			Part rootPart = vessel.parts[0].localRoot;
			var manager = new RMResourceManager(vessel.parts, rootPart);
			GetVesselResources (manager);
			GetStoredResources (storage);

			storageItems.Clear();
			storageItems.Storage = storage;
			storageItems.Vessel = vessel;

			for (int i = 0; i < resourceList.Count; i++) {
				var resDef = resourceList[i];
				var storedResource = storage.GetResource(resDef);
				RMResourceInfo vesselResource;
				manager.masterSet.resources.TryGetValue(resDef.name, out vesselResource);
				storageItems.Add (new StorageItem(storage, storedResource, vesselResource));
			}
			UIKit.UpdateListContent(storageItems);
		}

		void OnFromVessel(StorageItem storageItem)
		{
			// vessel to storage
			double increment = stackSize.Increment;
			double storableUnits = Math.Min(increment, storageItem.maxAmount - storageItem.storedAmount);
			Debug.Log($"[StorageGUI] OnFromVessel {storageItem.name} {increment} {storableUnits}");
			// Transfer uses +ve for in, -ve for out
			storableUnits += storageItem.vesselResource.Transfer(-storableUnits);
			Debug.Log($"	{storableUnits}");
			storageItem.storage.StoreResource(storageItem.storedResource.resource, storableUnits);
			storageItems.Update(storageItem);
		}

		void OnToVessel(StorageItem storageItem)
		{
			// storage to vessel
			double increment = stackSize.Increment;
			double retrievableUnits = Math.Min(increment, storageItem.storedAmount);
			Debug.Log($"[StorageGUI] OnToVessel {storageItem.name} {increment} {retrievableUnits}");
			// Transfer uses +ve for in, -ve for out
			retrievableUnits -= storageItem.vesselResource.Transfer(retrievableUnits);
			Debug.Log($"	{retrievableUnits}");
			storageItem.storage.StoreResource(storageItem.storedResource.resource, -retrievableUnits);
			storageItems.Update(storageItem);
		}

        internal void GetVesselResources(RMResourceManager manager)
        {
			var definitions = PartResourceLibrary.Instance.resourceDefinitions;

			foreach (string r in manager.masterSet.resources.Keys) {
				if (blackListedResources.Contains(r)) {
					continue;
				}
				var resDef = definitions[r];
				if (resDef != null) {	// null shouldn't happen, but...
					resourceSet.Add(resDef.id);
					resourceList.Add(resDef);
				}
			}
        }

        internal void GetStoredResources(Storage storage)
        {
            foreach (var resource in storage.storedResources.Values)
            {
                if (!resourceSet.Contains(resource.id))
                {
					resourceSet.Add(resource.id);
                    resourceList.Add(resource.resource);
                }
            }
        }
    }
}
