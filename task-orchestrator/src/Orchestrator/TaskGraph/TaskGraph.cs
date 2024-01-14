using System.Text.RegularExpressions;

namespace TaskOrchestrator.Orchestrator.Graph {
	public class TaskGraph {
		public List<Job> Roots { get; set; } = new();
		public int NumberOfJobs { get; set; } = 0;
		private Dictionary<int, int> NumberOfParentsMap { get; set; } = new();

		public TaskGraph(string dotDescription) {
			IEnumerable<string> lines = dotDescription.Trim().Split('\n');
			lines = lines.Skip(1).Take(lines.Count() - 2);

			// Maps id to node
			var nodes = new Dictionary<int, HashSet<int>>();

			int lineIdx = 2;
			foreach(var line in lines) {
				var matches = Regex.Matches(line, "\\d+");
				if(matches.Count < 2) throw new Exception($"No numbers matched in line {lineIdx}");

				var from = Int32.Parse(matches[0].Value);
				var to = Int32.Parse(matches[1].Value);

				// Adds a new node
				if(!nodes.ContainsKey(from)) nodes.Add(from, new() { to });
				else nodes[from].Add(to);

				if(!nodes.ContainsKey(to)) nodes.Add(to, new());

				// Adds a new edge to the parent counting
				if(NumberOfParentsMap.TryGetValue(to, out int numberOfParents)) numberOfParents++;
				else NumberOfParentsMap.Add(to, 1);

				lineIdx++;
			}

			var jobs = new Dictionary<int, Job>();
			foreach(var node in nodes) {
				if(!jobs.TryGetValue(node.Key, out Job? job)) {
					job = new Job(node.Key);
					jobs.Add(job.Id, job);
				}

				foreach(var children in node.Value) {
					if(!jobs.ContainsKey(children)) jobs.Add(children, new(children));
					job.AddOutboundJob(jobs[children]);
					jobs[children].AddInboundJob(job);
				}
			}

			NumberOfJobs = nodes.Count;
			Roots = jobs
					.Where(p => p.Value.ParentJobs.Count == 0)
					.Select(p => p.Value)
					.ToList();

			if(HasCycle()) throw new Exception("Graph has a landing cycle. Make sure  it is a DAG.");
		}

		public void SaveDotGraph(string outPath) {
			// Write the string array to a new file named "WriteLines.txt".
			using StreamWriter outputFile = new StreamWriter(outPath);
			var alreadyPrinted = new HashSet<int>();

			outputFile.WriteLine("digraph {");
			foreach(var rootJob in Roots)
				RecursivelyPrintGraph(rootJob, outputFile, alreadyPrinted);

			outputFile.WriteLine("}\n");
		}

		private static void RecursivelyPrintGraph(Job job, StreamWriter writer, HashSet<int> alreadyPrinted) {
			if(alreadyPrinted.Contains(job.Id)) return;
			alreadyPrinted.Add(job.Id);

			foreach(var children in job.OutboundJobs) {
				writer.WriteLine($"  {job.Id} -> {children.Id};");
				RecursivelyPrintGraph(children, writer, alreadyPrinted);
			}
		}

		public bool HasCycle() {
			var hasBeenVisited = new HashSet<int>(2 * NumberOfJobs);
			var currentPath = new HashSet<int>(2 * NumberOfJobs);

			foreach(var node in Roots) {
				if(HasCycleRecursively(node, hasBeenVisited, currentPath)) return true;
			}

			return false;
		}

		private static bool HasCycleRecursively(Job node, HashSet<int> hasBeenVisited, HashSet<int> currentPath) {
			if(currentPath.Contains(node.Id)) return true;
			if(hasBeenVisited.Contains(node.Id)) return false;

			hasBeenVisited.Add(node.Id);
			currentPath.Add(node.Id);

			foreach(var neighbour in node.OutboundJobs)
				if(HasCycleRecursively(neighbour, hasBeenVisited, currentPath)) return true;

			currentPath.Remove(node.Id);
			return false;
		}

		public int GetNumberOfParents(int nodeId) => NumberOfParentsMap[nodeId];
	}
}