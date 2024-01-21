using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TaskOrchestrator.Communication.Cluster.DTO;
using TaskOrchestrator.Communication.Cluster.Node;

namespace TaskOrchestrator.Communication.Cluster {
	public class Cluster : ICluster {
		/// <summary>
		/// Number of nodes in the cluster. Does not consider the current element (then it is K - 1, where K is the real number of elements)
		/// </summary>
		/// <value></value>
		private int NumberOfNodes { get; set; }
		/// <summary>
		/// Maps node ID to node.
		/// </summary>
		/// <value></value>
		private Dictionary<int, INode> Nodes { get; set; } = new();
		private INode Orchestrator { get; set; }
		private NodeType NodeType { get; set; }
		private int CurNodeId { get; set; } = -1;
		private ILogger Logger { get; set; }

		/// <summary>
		/// Cluster constructor view by the Orchestrator.
		/// </summary>
		/// <param name="numberOfNodes"></param>
		/// <param name="defaultPort"></param>
		/// <param name="type"></param>
		public Cluster(int numberOfNodes, int defaultPort, NodeType type, ILogger logger) {
			NumberOfNodes = numberOfNodes;
			NodeType = type;
			Orchestrator = new ClusterNode(type, defaultPort);
			Logger = logger;
		}

		/// <summary>
		/// Cluster constructor view by the Worker.
		/// </summary>
		/// <param name="defaultPort"></param>
		/// <param name="type"></param>
		/// <param name="orchestratorIp"></param>
		public Cluster(int defaultPort, NodeType type, string orchestratorIp, ILogger logger) {
			NumberOfNodes = -1;
			NodeType = type;
			Orchestrator = new ClusterNode(NodeType.Orchestrator, orchestratorIp, defaultPort);
			Logger = logger;
		}

		public IDictionary<int, INode> GetNodes() => Nodes;

		public void Connect() {
			if(NodeType == NodeType.Orchestrator) ConnectOrchestrator();
			else ConnectWorker();
		}

		private void ConnectOrchestrator() {
			TcpListener tmpListener = new(Orchestrator.GetIp(), Orchestrator.GetPort());
			tmpListener.Start();
			Logger.LogInformation("Listening to {IP} at port {Port}",
								 (tmpListener.Server.LocalEndPoint as IPEndPoint)!.Address,
								 (tmpListener.Server.LocalEndPoint as IPEndPoint)!.Port);

			// While there are nodes to be connected yet
			while(Nodes.Count != NumberOfNodes) {
				var tmpClient = tmpListener.AcceptTcpClient();
				var tmpWorker = new ClusterNode(NodeType.Worker, tmpClient);

				var freePort = Utils.Communication.FreeTcpPort();
				var newNodeAssignment = new NodeAssignmentDTO(Nodes.Count, freePort);

				TcpListener listener = new(Orchestrator.GetIp(), freePort);
				listener.Start();

				tmpWorker.Send(JsonSerializer.Serialize(newNodeAssignment));

				var client = listener.AcceptTcpClient();

				var worker = new ClusterNode(NodeType.Worker, client, Nodes.Count);

				Logger.LogInformation("New node with id {id} and port {port} connected.", newNodeAssignment.Id, newNodeAssignment.Port);

				Nodes.Add(Nodes.Count, worker);
			}

			tmpListener.Stop();
			tmpListener.Dispose();

			Logger.LogInformation("Cluster built with {n} nodes.", Nodes.Count);

			Logger.LogInformation("Sharing topography between nodes...");
		}

		private void ConnectWorker() {
			Orchestrator.Connect();
			Logger.LogInformation("Connected to orchestrator at {ip} and port {port} connected.", Orchestrator.GetIp(), Orchestrator.GetPort());

			var nodeAssignment = JsonSerializer.Deserialize<NodeAssignmentDTO>(Orchestrator.Read());
			CurNodeId = nodeAssignment.Id;

			Logger.LogInformation("Node assigned with id {id} and connected to orchestrator through port {port}.", CurNodeId, nodeAssignment.Port);
			Orchestrator = new ClusterNode(NodeType.Orchestrator, nodeAssignment.Port);
			Orchestrator.Connect();

			while(true) {
				Thread.Sleep(2);
			}
		}

		public void ShareTopography(INode node) {
			throw new NotImplementedException();
		}
	}
}