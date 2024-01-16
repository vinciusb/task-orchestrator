using System.Net;

namespace TaskOrchestrator.Communication.Cluster.Node {
	public interface INode {
		NodeType GetType();
		IPAddress GetIp();
	}
}