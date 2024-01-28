using CommandLine;
using Microsoft.Extensions.Logging;
using TaskOrchestrator.Communication.Cluster;
using TaskOrchestrator.Communication.Cluster.Node;

class Program {
	static async Task Main(string[] args) {
		var options = Parser.Default
						.ParseArguments<Cli.WorkerOptions>(args)
						.WithNotParsed((_) => throw new ArgumentException("CLI arguments parser failed.")).Value;
		var logger = LoggerFactory.Create(builder => builder.AddConsole())
														.CreateLogger("Worker");
		try {
			ICluster cluster = new ClusterBuilder()
						.SetOrchestratorDefaultPort(options.DefaultPort)
						.SetCurrentNodeType(NodeType.Worker)
						.SetOrchestratorAddress(options.IpAdress)
						.SetLogger(logger)
						.Build();

			cluster.Connect();
		}
		catch(Exception ex) {
			logger.LogError(ex, "== Error ==");
			// Fazer com conexões perdidas levem aqui
		}
	}
}