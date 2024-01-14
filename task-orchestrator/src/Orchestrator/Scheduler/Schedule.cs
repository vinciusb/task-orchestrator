namespace TaskOrchestrator.Orchestrator.Scheduler {
	public class Schedule(int id) {
		public int Id { get; set; } = id;
		public List<ITask<int>> Tasks { get; set; } = new();
		public int Count { get => Tasks.Count; }

		public void AddJob(ITask<int> task) => Tasks.Add(task);
	}
}