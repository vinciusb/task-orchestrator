using System.Net;
using System.Net.Sockets;
using TaskOrchestrator.Orchestrator.Scheduler;

namespace TaskOrchestrator.Orchestrator {
	public class Orchestrator {
		private string IpAddress { get; set; }
		private List<Schedule> Schedules { get; set; }

		public Orchestrator(List<Schedule> schedules) {
			IpAddress = GetLocalIPAddress();
			Console.WriteLine(IpAddress);
			Schedules = schedules;
		}

		private static string GetLocalIPAddress() {
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach(var ip in host.AddressList) {
				if(ip.AddressFamily == AddressFamily.InterNetwork) {
					return ip.ToString();
				}
			}
			throw new Exception("No network adapters with an IPv4 address in the system!");
		}

		public void OrchestrateTasks() {
			return;
		}
	}
}