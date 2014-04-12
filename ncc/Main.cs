using System;
using System.IO;
using ncc.AST;

namespace ncc
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Scanner scanner = new Scanner();

			Directory.SetCurrentDirectory("../../samples");
			scanner.Scan(File.ReadAllText("helloworld.c"));

			foreach (var token in scanner.getTokens()) {
				Console.WriteLine(token);
			}

			Parser parser = new Parser(scanner.getTokens().ToArray());
			STMT stmt = parser.ParseStmt(new Scope("",null));
			Console.WriteLine(stmt.ToString());
		}
	}
}
