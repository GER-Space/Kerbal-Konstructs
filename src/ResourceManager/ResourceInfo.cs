// Thanks to Taranis Elsu and his Fuel Balancer mod for the inspiration.
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace KerbalKonstructs.ResourceManager {
	public class RMResourceInfo {
		public List<IResourceContainer> containers = new List<IResourceContainer>();

		public bool flowState
		{
			get {
				for (int i = containers.Count; i-- > 0; ) {
					if (containers[i].flowState) {
						return true;
					}
				}
				return false;
			}
			set {
				for (int i = containers.Count; i-- > 0; ) {
					containers[i].flowState = value;
				}
			}
		}

		public double amount
		{
			get {
				double amount = 0;
				for (int i = containers.Count; i-- > 0; ) {
					amount += containers[i].amount;
				}
				return amount;
			}
		}

		public double maxAmount
		{
			get {
				double maxAmount = 0;
				for (int i = containers.Count; i-- > 0; ) {
					maxAmount += containers[i].maxAmount;
				}
				return maxAmount;
			}
		}

		public void RemoveAllResources ()
		{
			for (int i = containers.Count; i-- > 0; ) {
				containers[i].amount = 0.0;
			}
		}

		// Returns the amount of resource not transfered (0 = all has been
		// transfered).
		public double Transfer(double amount, bool balanced = true)
		{
			if (balanced) {
				return RMResourceSet.BalancedTransfer (this, amount);
			} else {
				return RMResourceSet.UnbalancedTransfer (this, amount);
			}
		}
	}
}
