using System.Net;
using System.Net.Sockets;

namespace Utils {
	public class Communication {
		public static int FreeTcpPort() {
			TcpListener l = new(IPAddress.Loopback, 0);
			l.Start();
			int port = ((IPEndPoint)l.LocalEndpoint).Port;
			l.Stop();
			return port;
		}

		public static IPAddress GetLocalIPAddress() {
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach(var ip in host.AddressList) {
				if(ip.AddressFamily == AddressFamily.InterNetwork) {
					return ip;
				}
			}
			throw new Exception("No network adapters with an IPv4 address in the system!");
		}
	}
}