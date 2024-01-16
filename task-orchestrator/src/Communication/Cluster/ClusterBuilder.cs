using TaskOrchestrator.Communication.Cluster.Node;

namespace TaskOrchestrator.Communication.Cluster {
	public class ClusterBuilder {
		/// <summary>
		/// Number of nodes in the cluster. Does not consider the current element (then it is K - 1, where K is the real number of elements)
		/// </summary>
		/// <value></value>
		private int NumberOfNodes { get; set; }
		/// <summary>
		/// The default port to start the communincation with the orchestrator.
		/// </summary>
		/// <value></value>
		private int OrchestratorDefaultPort { get; set; }
		private NodeType CurrentNodeType { get; set; }

		public ClusterBuilder() { }

		public ClusterBuilder SetNumberOfNodes(int numberOfNodes) {
			this.NumberOfNodes = numberOfNodes;
			return this;
		}

		public ClusterBuilder SetOrchestratorDefaultPort(int defaultPort) {
			this.OrchestratorDefaultPort = defaultPort;
			return this;
		}

		public ClusterBuilder SetCurrentNodeType(NodeType type) {
			this.CurrentNodeType = type;
			return this;
		}

		public Cluster Build() {
			var cluster = new Cluster(NumberOfNodes,
									  OrchestratorDefaultPort,
									  CurrentNodeType);
			return cluster;
		}
	}
}