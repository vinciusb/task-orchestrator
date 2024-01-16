using TaskOrchestrator.Orchestrator.Graph;

namespace TaskOrchestrator.Orchestrator.Scheduler {
	public class Scheduler {

		private const int BIGGEST_DIFF_SCHEDULES_HEIGHT = 2;

		public Scheduler() { }

		public static async Task<List<Schedule>> Schedule(TaskGraph graph, int NPU) {
			SortedSet<Schedule> schedules = new(Enumerable.Range(0, NPU).Select(i => new Schedule(i)), new ScheduleComparer());
			Dictionary<int, Schedule> idToScheduleMap = schedules.ToDictionary(s => s.Id, s => s);
			Dictionary<int, int> howManyDepCompleted = new(graph.NumberOfJobs * 2);
			Dictionary<int, int> whichScheduleTookIt = new(graph.NumberOfJobs / 4);

			// Number of Jobs / 4 seems to be a good estimate for the max nodes on each layer
			List<ITask<int>> currentLayer = new(graph.NumberOfJobs / 4), nextLayer = new(graph.NumberOfJobs / 4);
			foreach(var node in graph.Roots) currentLayer.Add(node);

			// DFS through the graph
			while(currentLayer.Count != 0) {
				AssignCurrentLayer(in currentLayer, ref whichScheduleTookIt, schedules, idToScheduleMap);

				FillNextLayer(ref nextLayer, in currentLayer, ref howManyDepCompleted);

				nextLayer.Sort((t1, t2) => t1.GetNumberOfDependencies().CompareTo(t2.GetNumberOfDependencies()));

				(currentLayer, nextLayer) = (nextLayer, currentLayer);
				nextLayer.Clear();
			}

			return schedules.ToList();
		}

		private static void FillNextLayer(ref List<ITask<int>> nextLayer,
								   in List<ITask<int>> currentLayer,
								   ref Dictionary<int, int> howManyDepCompleted) {
			foreach(var curNode in currentLayer) {
				foreach(var children in curNode.GetDependents()) {
					int childrenId = children.GetId();

					if(howManyDepCompleted.TryGetValue(childrenId, out int completedDeps)) {
						if(completedDeps == children.GetNumberOfDependencies() - 1) {
							nextLayer.Add(children);
							howManyDepCompleted.Remove(childrenId); // Make sure this node is removed from hashmap and reduce collision chances
						}
						else {
							howManyDepCompleted[childrenId]++;
						}
					}
					else if(1 == children.GetNumberOfDependencies()) nextLayer.Add(children);
					else howManyDepCompleted.Add(childrenId, 1);
				}
			}
		}

		private static void AssignCurrentLayer(in List<ITask<int>> currentLayer,
										ref Dictionary<int, int> whichScheduleTookIt,
										SortedSet<Schedule> schedules,
										Dictionary<int, Schedule> idToScheduleMap) {
			foreach(var node in currentLayer) {
				var parents = node.GetDependencies();
				Schedule minWeightSchedule = schedules.Min!;
				Schedule? scheduleToTake = null; // Assignment just to make sure compiler doesn't warns

				foreach(var parent in parents) {
					var parentSchedule = idToScheduleMap[whichScheduleTookIt[parent.GetId()]];

					if(parentSchedule.Count + 1 <= minWeightSchedule.Count + BIGGEST_DIFF_SCHEDULES_HEIGHT) {
						scheduleToTake = parentSchedule;
						break;
					}
				}

				scheduleToTake ??= minWeightSchedule;

				whichScheduleTookIt[node.GetId()] = scheduleToTake.Id;
				schedules.Remove(scheduleToTake);
				scheduleToTake.AddJob(node);
				schedules.Add(scheduleToTake);
			}
		}
	}

	public class ScheduleComparer : IComparer<Schedule> {
		public int Compare(Schedule? sch1, Schedule? sch2) {
			int size1 = sch1!.Count, size2 = sch2!.Count;
			return size1 == size2 ? sch1.Id.CompareTo(sch2.Id) : size1.CompareTo(size2);
		}
	}
}