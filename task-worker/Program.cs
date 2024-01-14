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
		}
		catch(Exception ex) {
			Console.WriteLine(ex.Message);
		}
	}
}