using TaskOrchestrator.Orchestrator.Scheduler;

namespace TaskOrchestrator.Orchestrator {
	public class Orchestrator {
		private List<Schedule> Schedules { get; set; }

		public Orchestrator(List<Schedule> schedules) {
			Schedules = schedules;
		}

		public void OrchestrateTasks() {
			return;
		}
	}
}