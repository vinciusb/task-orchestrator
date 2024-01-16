using CommandLine;

class Program {
	static async Task Main(string[] args) {
		var options = Parser.Default
						.ParseArguments<Cli.WorkerOptions>(args)
						.WithNotParsed((_) => throw new ArgumentException("CLI arguments parser failed.")).Value;

		try {
		}
		catch(Exception ex) {
			Console.WriteLine(ex.Message);
		}
	}
}