using System;
using System.IO;
using ncc.AST;

namespace ncc
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			string input = "";
			string output = "";

			if (args.Length == 0) {
				args = Console.ReadLine ().Split (' ');
			}

			for (int i = 0; i < args.Length; i++) {
				if (args [i] == "-o") {
					i++;
					output = args [i];
				} else {
					input = args [i];
				}
			}

			Scanner scanner = new Scanner ();

			Directory.SetCurrentDirectory ("../../samples");
			scanner.Scan (File.ReadAllText (input));

			/*
			foreach (var token in scanner.getTokens()) {
				Console.WriteLine (token);
			}
			*/

			Parser parser = new Parser (scanner.getTokens ().ToArray ());
			STMT[] stmts = parser.Parse ();
			/*
			foreach (STMT s in stmts) {
				Console.WriteLine (s.ToString ());
			}
			*/
			CodeGenerator generator = new CodeGenerator (stmts, parser.symbolTable.ToArray (), parser.typeTable.ToArray ());
			generator.Generate ();
			Console.WriteLine (generator.Asm);
			if (output != "") {
				File.WriteAllText (output, generator.Asm);
			}
		}
	}

	static class Extentions
	{
		public static string f(this string that,params object[] args) {
			return string.Format(that,args);
		}
	}
}
