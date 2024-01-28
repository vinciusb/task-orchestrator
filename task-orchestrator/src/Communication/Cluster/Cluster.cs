using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TaskOrchestrator.Communication.Cluster.DTO;
using TaskOrchestrator.Communication.Cluster.Node;
using System.Dynamic;
using System.Text.Json.Nodes;
using TaskOrchestrator.Exceptions;

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
		private List<Task<TcpClient>> ToAcceptClients { get; set; } = new();

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

		public void LogInfo(string msg) => Logger.LogInformation("{str}", msg);

		public void LogError(string errMsg) => Logger.LogError("{err}", errMsg);

		public IDictionary<int, INode> GetNodes() => Nodes;

		public void Connect() {
			if(NodeType == NodeType.Orchestrator) ConnectOrchestrator();
			else ConnectWorker();
		}

		private void ConnectOrchestrator() {
			var workerPortsMap = new Dictionary<int, List<int>>();
			TcpListener tmpListener = new(Orchestrator.GetIp(), Orchestrator.GetPort());
			tmpListener.Start();
			LogInfo($"Listening to {(tmpListener.Server.LocalEndPoint as IPEndPoint)!.Address} at port {(tmpListener.Server.LocalEndPoint as IPEndPoint)!.Port}.");

			// While there are nodes to be connected yet
			while(Nodes.Count != NumberOfNodes) {
				var tmpClient = tmpListener.AcceptTcpClient();
				var tmpWorker = new ClusterNode(NodeType.Worker, tmpClient);

				var freePort = Utils.Communication.FreeTcpPort();
				var newNodeAssignment = new NodeAssignmentDTO(Nodes.Count, freePort, NumberOfNodes);

				TcpListener listener = new(Orchestrator.GetIp(), freePort);
				listener.Start();

				tmpWorker.Send(JsonSerializer.Serialize(newNodeAssignment));

				var client = listener.AcceptTcpClient();

				var worker = new ClusterNode(NodeType.Worker, client, Nodes.Count);
				var avaiablePorts = JsonSerializer.Deserialize<IEnumerable<int>>(worker.Read())!;
				workerPortsMap.Add(Nodes.Count, avaiablePorts.ToList());
				LogInfo($"New node with id {newNodeAssignment.Id} and port {newNodeAssignment.Port} connected. Provided the following ports: [{String.Join(',', avaiablePorts)}]");

				Nodes.Add(Nodes.Count, worker);
			}

			tmpListener.Stop();
			tmpListener.Dispose();

			LogInfo($"Cluster built with {Nodes.Count} nodes.");

			FloodWithTwopography(workerPortsMap);

			foreach(var (_, node) in Nodes) {
				var msg = node.Read();
				if(!msg.Equals("CONNECTED")) throw new FailedToConnectException(node.GetId());
			}
			LogInfo($"Successfully shared topography.");
		}

		private async void ConnectWorker() {
			Orchestrator.Connect();
			LogInfo($"Connected to orchestrator at {Orchestrator.GetIp()} and port {Orchestrator.GetPort()} connected.");

			var nodeAssignment = JsonSerializer.Deserialize<NodeAssignmentDTO>(Orchestrator.Read())!;
			CurNodeId = nodeAssignment.Id;

			LogInfo($"Node assigned with id {CurNodeId} and connected to orchestrator through port {nodeAssignment.Port}.");
			Orchestrator = new ClusterNode(NodeType.Orchestrator, nodeAssignment.Port);
			Orchestrator.Connect();

			var ports = Enumerable.Range(0, CurNodeId).Select(_ => Utils.Communication.FreeTcpPort()).ToList();
			LogInfo($"Allocating {ports.Count} ports: [{string.Join(',', ports)}] and sharing with other nodes.");
			Orchestrator.Send(JsonSerializer.Serialize(ports));

			var localIp = Utils.Communication.GetLocalIPAddress();
			foreach(var port in ports) {
				TcpListener listener = new(localIp, port);
				listener.Start();
				ToAcceptClients.Add(listener.AcceptTcpClientAsync());
			}

			await ReceiveTopography();
		}

		public void ShareTopography(INode node, List<int> avaiablePorts) {
			var curNodeId = node.GetId();

			dynamic ExtractNodesInfo(KeyValuePair<int, INode> p, int i) {
				var n = p.Value;
				dynamic obj = new ExpandoObject();
				obj.Id = n.GetId();
				obj.Ip = n.GetIp().ToString();
				obj.Port = avaiablePorts[i];
				return obj;
			}

			var filteredNodes = Nodes.Where((pair) => node.GetId() < pair.Key).Select(ExtractNodesInfo);
			var str = JsonSerializer.Serialize(filteredNodes);
			node.Send(str);
			LogInfo($"Topography successfully shared to node {node.GetId()}");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="workerPortsMap">[node id] -> avaiable ports on the worker side</param>
		private void FloodWithTwopography(Dictionary<int, List<int>> workerPortsMap) {
			LogInfo("Sharing topography between nodes...");
			var tasks = new List<Task>();

			foreach(var (id, node) in Nodes) {
				var portsForThisNode = workerPortsMap.Where(pair => pair.Value.Count > id).Select((p) => p.Value[id]).ToList();
				ShareTopography(node, portsForThisNode);
			}

			tasks.ForEach(t => t.Wait());
		}

		private async Task ReceiveTopography() {
			var nodesInfos = JsonSerializer.Deserialize<IEnumerable<JsonNode>>(Orchestrator.Read())!;

			// Create the interface to the nodes that is connecting on
			foreach(var nodeInfo in nodesInfos) {
				var ip = (string)nodeInfo["Ip"]!;
				var port = (int)nodeInfo["Port"]!;
				var id = (int)nodeInfo["Id"]!;

				var node = new ClusterNode(NodeType.Worker, ip, port, id);
				Nodes.Add(id, node);
				node.Connect();
			}

			// Create the interface to the nodes that connected to it
			for(int i = 0; i < ToAcceptClients.Count; i++) {
				int nodeId = i; // I can garantee that all nodes that tried to connect to this node have an lower id and all nodes he has tried to connect has an higher id
				var client = await ToAcceptClients[i];
				var node = new ClusterNode(NodeType.Worker, client, nodeId);

				Nodes.Add(nodeId, node);
			}

			foreach(var (id, node) in Nodes)
				node.Send($"Message send from node {CurNodeId} to {id}.");

			var ss = new List<string>();
			var sum = 0;
			foreach(var (_, node) in Nodes) {
				ss.Add(node.Read());
				sum += node.GetId();
			}
			LogInfo($"{string.Join('\n', ss)}");


			// Expected sum with all the nodes that should connect to this node
			var expectedSum = Nodes.Count * (Nodes.Count + 1) / 2 - CurNodeId;
			if(sum == expectedSum) {
				LogInfo("Connected to all nodes");
				Orchestrator.Send("CONNECTED");
			}
			else {
				Orchestrator.Send("FAILED TO CONNECT");
				throw new FailedToConnectException();
			}

			// TODO: Remove
			while(true) {
				Thread.Sleep(2);
			}
		}
	}
}