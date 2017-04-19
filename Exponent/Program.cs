using System;
using System.Threading;


namespace Exponent
{
	class Program
	{
		static void Main(string[] args)
		{
			while (true)
			{
				Console.WriteLine(@"Input numbers by Enter. Numbers should be in such formats: 1, 0.5, 1.23e+2, 1.23e-2.
Type '=' if you need to get sum. 
Type ""stop"" if you want to close a program.");


				BigDecimal bd = new BigDecimal();
				do
				{
					string line = Console.ReadLine().ToLower().Trim();
					if (line == "stop")
					{
						Console.WriteLine("App is closing...");
						Thread.Sleep(2000);
						Environment.Exit(0);
					}
					if (line == "=")
					{
						break;
					}
					try
					{
						bd += BigDecimal.Parse(line);
					}
					catch (FormatException)
					{
						Console.WriteLine("invalid number! Numbers should be in such formats: 1, 0.5, 1.23e+2, 1.23e-2.");
					}

				}
				while (true);
				Console.WriteLine($"Sum: {bd.ToString()}" + Environment.NewLine);
			}
		}
	}
}
