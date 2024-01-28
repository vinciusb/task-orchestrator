using System.ComponentModel;

namespace TaskOrchestrator.Exceptions {
	[Description("Exception class that is thrown when an connection error happens.")]
	public class FailedToConnectException : Exception {

		public FailedToConnectException() : base("An connection error happend.") { }

		public FailedToConnectException(int id) : base($"An connection error happend with node {id}.") { }
	}
}