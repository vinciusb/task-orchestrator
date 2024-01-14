using System.Diagnostics;
using TaskOrchestrator.Orchestrator.Scheduler;

namespace TaskOrchestrator.Orchestrator.Graph {
	[DebuggerDisplay("Job {Id}: {ParentJobs.Count} parents | {OutboundJobs.Count} dependents")]
	public class Job : ITask<int> {
		public int Id { get; set; }
		public List<Job> OutboundJobs { get; set; } = new();
		public List<Job> ParentJobs { get; set; } = new();

		public Job(int Id) => this.Id = Id;
		public void AddOutboundJob(Job j) => OutboundJobs.Add(j);
		public void AddInboundJob(Job j) => ParentJobs.Add(j);

		public int GetId() => Id;
		public IEnumerable<ITask<int>> GetDependents() => OutboundJobs;
		public IEnumerable<ITask<int>> GetDependencies() => ParentJobs;
		public int GetNumberOfDependencies() => ParentJobs.Count;
	}
}