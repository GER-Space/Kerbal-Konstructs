namespace KerbalKonstructs.ResourceManager {
	public interface IResourceContainer {
		double maxAmount { get; }
		double amount { get; set; }
		bool flowState { get; set; }
		Part part { get; }
	}
}
