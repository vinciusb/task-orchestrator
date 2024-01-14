using TaskOrchestrator.Orchestrator;
using TaskOrchestrator.Orchestrator.Graph;
using TaskOrchestrator.Orchestrator.Scheduler;

namespace task_orchestrator;

class Program {
	static void Main(string[] args) {
		if(args.Length < 2) {
			Console.WriteLine("Haven't passed enough arguments");
			return;
		}

		int numberOfProcessUnits = Int32.Parse(args[1]);
		if(numberOfProcessUnits < 1) {
			Console.WriteLine("Not enough process units");
			return;
		}

		try {
			string dotText = File.ReadAllText(args[0]);
			var taskGraph = new TaskGraph(dotText);

			var schedules = Scheduler.Schedule(taskGraph, numberOfProcessUnits);

			var orchestrator = new Orchestrator(schedules);
			orchestrator.OrchestrateTasks();
		}
		catch(Exception ex) {
			Console.WriteLine(ex.Message);
		}
	}
}