using System;

namespace ncc
{
	public class PreProcessor
	{
		string code;

		string Output { get; private set; }

		public PreProcessor (string Code)
		{

		}

		public Process() {
			string[] lines = code.Split('\n');
			for (int i = 0; i < lines.Length; i++) {
				if (lines[i].StartsWith("#include ")) {
					if (lines[i].Contains("<")) {
						string file = lines[i].Substring(lines[i].IndexOf('<'),lines[i].LastIndexOf('>') - lines[i].IndexOf('<'));
					}
				}
			}
		}
	}
}

