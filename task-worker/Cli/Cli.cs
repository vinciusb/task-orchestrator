using CommandLine;

namespace Cli {
	public class WorkerOptions {
		[Option("orchestrator-ip", Required = false, HelpText = "The orchestrator IP address.")]
		public string IpAdress { get; set; } = "";

		[Option('p', "port", Required = false, Default = 13500, HelpText = "The orchestrator default port.")]
		public int DefaultPort { get; set; }
	}
}