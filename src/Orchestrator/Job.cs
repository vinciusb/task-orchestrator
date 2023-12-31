namespace TaskOrchestrator.Orchestrator.Graph {
	public class Job {
		public int Id { get; set; }
		public List<Job> OutboundJobs { get; set; } = new();
		public List<Job> ParentJobs { get; set; } = new();

		public Job(int Id) => this.Id = Id;
		public void AddOutboundJob(Job j) => OutboundJobs.Add(j);
		public void AddInboundJob(Job j) => ParentJobs.Add(j);
	}
}