using System.Net;
using System.Net.Sockets;

namespace TaskOrchestrator.Communication.Cluster.Node {
	public class ClusterNode : INode {
		private NodeType Type { get; set; }
		private IPAddress IpAddress { get; set; }
		private int Port { get; set; }

		public ClusterNode(NodeType type) {
			Type = type;
			IpAddress = Utils.Communication.GetLocalIPAddress();
		}

		public new NodeType GetType() => Type;

		public IPAddress GetIp() => IpAddress;


	}
}