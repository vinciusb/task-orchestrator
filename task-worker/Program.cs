using CommandLine;
using Microsoft.Extensions.Logging;
using TaskOrchestrator.Communication.Cluster;
using TaskOrchestrator.Communication.Cluster.Node;

class Program {
	static async Task Main(string[] args) {
		var options = Parser.Default
						.ParseArguments<Cli.WorkerOptions>(args)
						.WithNotParsed((_) => throw new ArgumentException("CLI arguments parser failed.")).Value;

		try {
			ICluster cluster = new ClusterBuilder()
						.SetOrchestratorDefaultPort(options.DefaultPort)
						.SetCurrentNodeType(NodeType.Worker)
						.SetOrchestratorAddress(options.IpAdress)
						.SetLogger(LoggerFactory.Create(builder => builder.AddConsole())
												.CreateLogger("Worker"))
						.Build();

			cluster.Connect();
		}
		catch(Exception ex) {
			Console.WriteLine(ex.Message);
		}
	}
}