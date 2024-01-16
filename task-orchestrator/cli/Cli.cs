using CommandLine;

namespace Cli {
	public class OrchestratorOptions {
		[Option("dot", Required = true, HelpText = "Task dot file path.")]
		public string DotFile { get; set; } = "";

		[Option('n', "number-workers", Required = false, Default = 1, HelpText = "Number of workers in the cluster.")]
		public int NumberOfProcessUnits { get; set; }

		[Option('p', "port", Required = false, Default = 13500, HelpText = "The orchestrator default port.")]
		public int DefaultPort { get; set; }
	}
}