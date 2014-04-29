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

			if (args.Length == 0)
				args = Console.ReadLine ().Split (' ');

			for (int i = 0; i < args.Length; i++)
				if (args [i] == "-o") {
					i++;
					output = args [i];
				} else
					input = args [i];


		}
	}

	static class Extentions
	{
		public static string f(this string that,params object[] args) {
			return string.Format(that,args);
		}
	}
}
