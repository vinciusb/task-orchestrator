using TaskOrchestrator.Orchestrator.Graph;

namespace task_orchestrator;

class Program {
	static void Main(string[] args) {
		if(args.Length == 0) {
			Console.WriteLine("Haven't passed arguments");
			return;
		}

		try {
			string dotText = File.ReadAllText(args[0]);
			var taskGraph = new TaskGraph(dotText);
			// taskGraph
		}
		catch(Exception ex) {
			Console.WriteLine(ex.Message);
		}
	}
}