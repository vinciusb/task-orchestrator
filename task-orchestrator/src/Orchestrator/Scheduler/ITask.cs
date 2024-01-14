namespace TaskOrchestrator.Orchestrator.Scheduler {
	public interface ITask<T> {
		public T GetId();
		public int GetNumberOfDependencies();
		public IEnumerable<ITask<T>> GetDependencies();
		public IEnumerable<ITask<T>> GetDependents();
	}
}