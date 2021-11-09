#if DEBUG

using System;

namespace SimpleDiscord;

// You can ignore this class, it's just to make logging a bit easier for me when debugging.
internal static class Debug
{
	public static void Log(string message)
	{
		Console.WriteLine($"[{DateTime.Now}] [SimpleDiscord] {message}");
	}

	public static void Warn(string message)
	{
		ConsoleColor oldColor = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Red;
		Log(message);
		Console.ForegroundColor = oldColor;
	}
}

#endif
