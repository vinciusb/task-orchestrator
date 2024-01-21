using System.Net;
using System.Net.Sockets;

namespace TaskOrchestrator.Communication.Cluster.Node {
	public interface INode {
		int GetId();
		NodeType GetType();
		IPAddress GetIp();
		int GetPort();
		TcpClient GetClient();
		NetworkStream GetStream();
		void Connect();
		void Send(string buffer);
		string Read();
	}
}