using TaskOrchestrator.Communication.Cluster.Node;

namespace TaskOrchestrator.Communication.Cluster {
	public interface ICluster {
		/// <summary>
		/// Connects to the cluster.
		/// </summary>
		void Connect();
		/// <summary>
		/// Get the list of nodes.
		/// </summary>
		/// <returns></returns>
		IDictionary<int, INode> GetNodes();
		/// <summary>
		/// The current node shares the current cluster topography with another node.
		/// </summary>
		void ShareTopography(INode node);
	}
}