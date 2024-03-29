﻿using CommandLine;
using Microsoft.Extensions.Logging;
using TaskOrchestrator.Communication.Cluster;
using TaskOrchestrator.Communication.Cluster.Node;
using TaskOrchestrator.Orchestrator;
using TaskOrchestrator.Orchestrator.Graph;
using TaskOrchestrator.Orchestrator.Scheduler;

class Program {
	static async Task Main(string[] args) {
		var options = Parser.Default
					.ParseArguments<Cli.OrchestratorOptions>(args)
					.WithNotParsed((_) => throw new ArgumentException("CLI arguments parser failed.")).Value;
		var logger = LoggerFactory.Create(builder => builder.AddConsole())
															.CreateLogger("Orchestrator");
		try {
			string dotText = File.ReadAllText(options.DotFile);
			var taskGraph = new TaskGraph(dotText);

			var schedules = Scheduler.Schedule(taskGraph, options.NumberOfProcessUnits);

			ICluster cluster = new ClusterBuilder()
									.SetNumberOfNodes(options.NumberOfProcessUnits)
									.SetOrchestratorDefaultPort(options.DefaultPort)
									.SetCurrentNodeType(NodeType.Orchestrator)
									.SetLogger(logger)
									.Build();

			cluster.Connect();

			var orchestrator = new Orchestrator(await schedules);
			orchestrator.OrchestrateTasks();
		}
		catch(Exception ex) {
			logger.LogError(ex, "== Error ==");
			// Se algum erro acontecer, fechar todos sockets caso tenha alguma conexão ja criada
		}
	}
}