using System.Net.Sockets;
using TaskOrchestrator.Communication.Cluster.Node;

namespace TaskOrchestrator.Communication.Cluster {
	public class Cluster : ICluster {
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
		/// <summary>
		/// Maps node ID to node.
		/// </summary>
		/// <value></value>
		private IDictionary<int, INode> Nodes { get; set; }
		private INode Orchestrator { get; set; }
		private INode CurNode { get; set; }
		/// <summary>
		/// Max number of tries the worker has before considering that the orchestrator is unreachable.
		/// </summary>
		private const int MAX_TRIES_TO_CONNECT_TO_ORCH = 10;

		public Cluster(int numberOfNodes, int defaultPort, NodeType type) {
			NumberOfNodes = numberOfNodes;
			OrchestratorDefaultPort = defaultPort;
			CurNode = new ClusterNode(type);
			if(type == NodeType.Orchestrator) Orchestrator = CurNode;
			Nodes = new Dictionary<int, INode>();
		}

		public IDictionary<int, INode> GetNodes() => Nodes;

		public void Connect() {
			if(CurNode.GetType() == NodeType.Orchestrator) ConnectOrchestrator();
			else ConnectWorker();
		}

		private void ConnectOrchestrator() {
			TcpListener listener = new(Orchestrator.GetIp(), OrchestratorDefaultPort);
			listener.Start();

			// While there are nodes to be connected yet
			while(Nodes.Count != NumberOfNodes) {
				var client = listener.AcceptTcpClient();
				client.GetStream();

				var freePort = Utils.Communication.FreeTcpPort();

				var worker = new ClusterNode(NodeType.Worker);
				Nodes.Add(Nodes.Count, worker);
			}

			listener.Stop();
		}

		private void ConnectWorker() {
			// MAX_TRIES_TO_CONNECT_TO_ORCH


		}

		public void ShareTopography(INode node) {
			throw new NotImplementedException();
		}
	}
}