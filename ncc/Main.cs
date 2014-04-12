using System;
using System.IO;

namespace ncc
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Scanner scanner = new Scanner();

			Directory.SetCurrentDirectory("../../samples");
			scanner.Scan(File.ReadAllText("helloworld.nc"));

			foreach (var token in scanner.getTokens()) {
				Console.WriteLine(token);
			}
		}
	}
}
