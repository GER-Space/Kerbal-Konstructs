namespace KerbalKonstructs.ResourceManager {
	public class ResourceSetContainer : IResourceContainer {
		RMResourceSet resourceSet;
		string resource;

		public double maxAmount
		{
			get {
				return resourceSet.ResourceCapacity (resource);
			}
		}
		public double amount
		{
			get {
				return resourceSet.ResourceAmount (resource);
			}
			set {
				double current = resourceSet.ResourceAmount (resource);
				resourceSet.TransferResource (resource, value - current);
			}
		}
		public bool flowState
		{
			get {
				return resourceSet.GetFlowState (resource);
			}
			set {
				resourceSet.SetFlowState (resource, value);
			}
		}
		public Part part
		{
			get {
				return null;
			}
		}

		public RMResourceSet set { get { return resourceSet; } }

		public ResourceSetContainer (string resource, RMResourceSet resourceSet)
		{
			this.resourceSet = resourceSet;
			this.resource = resource;
		}
	}
}
