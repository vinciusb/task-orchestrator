using TaskOrchestrator.Communication.Cluster.Node;
using Microsoft.Extensions.Logging;

namespace TaskOrchestrator.Communication.Cluster {
	public class ClusterBuilder {
		/// <summary>
		/// Number of nodes in the cluster. Does not consider the current element (then it is K - 1, where K is the real number of elements)
		/// </summary>
		/// <value></value>
		private int NumberOfNodes { get; set; } = -1;
		/// <summary>
		/// The default port to start the communincation with the orchestrator.
		/// </summary>
		/// <value></value>
		private int OrchestratorDefaultPort { get; set; }
		private string OrchestratorAddress { get; set; }
		private NodeType CurrentNodeType { get; set; }
		private ILogger Logger { get; set; }

		public ClusterBuilder() { }

		public ClusterBuilder SetNumberOfNodes(int numberOfNodes) {
			NumberOfNodes = numberOfNodes;
			return this;
		}

		public ClusterBuilder SetOrchestratorDefaultPort(int defaultPort) {
			OrchestratorDefaultPort = defaultPort;
			return this;
		}

		public ClusterBuilder SetOrchestratorAddress(string addr) {
			OrchestratorAddress = addr;
			return this;
		}

		public ClusterBuilder SetCurrentNodeType(NodeType type) {
			CurrentNodeType = type;
			return this;
		}

		public ClusterBuilder SetLogger(ILogger logger) {
			Logger = logger;
			return this;
		}

		public Cluster Build() {
			var cluster = NumberOfNodes != -1 ?
							new Cluster(NumberOfNodes,
									  	OrchestratorDefaultPort,
									  	CurrentNodeType,
										Logger) :
							new Cluster(OrchestratorDefaultPort,
									  	CurrentNodeType,
										OrchestratorAddress,
										Logger);
			return cluster;
		}
	}
}