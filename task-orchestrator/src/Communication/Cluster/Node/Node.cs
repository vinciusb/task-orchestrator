using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TaskOrchestrator.Communication.Cluster.Node {
	public class ClusterNode : INode {
		private int Id { get; set; }
		private NodeType Type { get; set; }
		private IPAddress IpAddress { get; set; }
		private int Port { get; set; }

		// ================== After connection properties ======================
		private TcpClient Client { get; set; }
		private NetworkStream Stream { get; set; }

		public ClusterNode(NodeType type, int port, int id = -1) {
			Id = id;
			Type = type;
			IpAddress = Utils.Communication.GetLocalIPAddress();
			Port = port;
		}

		public ClusterNode(NodeType type, string address, int port, int id = -1) {
			Id = id;
			Type = type;
			IpAddress = IPAddress.Parse(address);
			Port = port;
		}

		public ClusterNode(NodeType type, TcpClient client, int id = -1) {
			Id = id;
			Type = type;
			Client = client;
			Stream = client.GetStream();
			var tmpEndpoint = Client.Client.RemoteEndPoint as IPEndPoint;
			IpAddress = tmpEndpoint!.Address;
			Port = tmpEndpoint.Port;
		}

		public int GetId() => Id;

		public new NodeType GetType() => Type;

		public IPAddress GetIp() => IpAddress;

		public int GetPort() => Port;
		public TcpClient GetClient() => Client;
		public NetworkStream GetStream() => Stream;

		public void Connect() {
			IPEndPoint endpoint = new(IpAddress, Port);
			Client = new();
			Client.Connect(endpoint);
			Stream = Client.GetStream();
		}

		public void Send(string msg) {
			if(Stream.CanWrite) {
				var buffer = Encoding.UTF8.GetBytes(msg);
				Stream.Write(buffer, 0, buffer.Length);
			}
		}

		public string Read() {
			if(Stream.CanRead) {
				using MemoryStream memStream = new();

				byte[] buffer = new byte[2048]; int bytesRead = 0;

				do {
					bytesRead = Stream.Read(buffer, 0, buffer.Length);
					memStream.Write(buffer, 0, bytesRead);
				} while(Stream.DataAvailable);

				return Encoding.UTF8.GetString(memStream.ToArray());
			}
			return null;
		}
	}
}