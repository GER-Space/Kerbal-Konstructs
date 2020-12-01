namespace KerbalKonstructs.ResourceManager {
	public class PartResourceContainer : IResourceContainer {
		PartResource partResource;

		public double maxAmount
		{
			get {
				return partResource.maxAmount;
			}
		}
		public double amount
		{
			get {
				return partResource.amount;
			}
			set {
				partResource.amount = value;
			}
		}
		public bool flowState
		{
			get {
				return partResource.flowState;
			}
			set {
				partResource.flowState = value;
			}
		}
		public Part part
		{
			get {
				return partResource.part;
			}
		}

		public PartResourceContainer (PartResource partResource)
		{
			this.partResource = partResource;
		}
	}
}
