using System;

namespace GamePlay
{
	public class ChatCommand
	{
		public readonly string Name;
		public readonly string Usage;
		public readonly string Description;

		private readonly Func<string[], string> _execute;

		// The handler runs on the server and returns the reply shown to the host (null for no reply).
		public ChatCommand(string name, string usage, string description, Func<string[], string> execute)
		{
			Name = name;
			Usage = usage;
			Description = description;
			_execute = execute;
		}

		public string Execute(string[] arguments)
		{
			return _execute(arguments);
		}
	}
}
