using System.Buffers;
using System.Text.RegularExpressions;

namespace TaskOrchestrator.Orchestrator.Graph {
	public class TaskGraph {
		public List<Job> Roots { get; set; } = new();

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

				if(!nodes.ContainsKey(from)) nodes.Add(from, new() { to });
				else nodes[from].Add(to);

				if(!nodes.ContainsKey(to)) nodes.Add(to, new());

				lineIdx++;
			}

			var jobs = new Dictionary<int, Job>();
			foreach(var node in nodes) {
				if(!jobs.TryGetValue(node.Key, out Job job)) {
					job = new Job(node.Key);
					jobs.Add(job.Id, job);
				}

				foreach(var children in node.Value) {
					if(!jobs.ContainsKey(children)) jobs.Add(children, new(children));
					job.AddOutboundJob(jobs[children]);
					jobs[children].AddInboundJob(job);
				}
			}

			Roots = jobs
					.Where(p => p.Value.ParentJobs.Count == 0)
					.Select(p => p.Value)
					.ToList();
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
	}
}