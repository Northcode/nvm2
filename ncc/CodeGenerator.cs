using System;
using System.Text;
using ncc.AST;
using System.Collections.Generic;

namespace ncc
{
	public class CodeGenerator
	{
		internal SymbolTableEntry[] symbolTable;
		internal TypeDef[] typeTable;
		STMT[] statements;

		StringBuilder sb;
		int i;

		List<string> stringTable;

		public string Asm {
			get {
				return sb.ToString ();
			}
		}

		public CodeGenerator (STMT[] Statements,SymbolTableEntry[] SymbolTable, TypeDef[] TypeTable)
		{
			sb = new StringBuilder();
			symbolTable = SymbolTable;
			typeTable = TypeTable;
			statements = Statements;
			stringTable = new List<string>();
		}

		public void Generate() {
			i = 0;
			sb.AppendLine("LD DA [_prg_end]");
			sb.AppendLine("SET_PAGE_STACK");
			sb.AppendLine("LD DA [_prg_end]+1024");
			sb.AppendLine("SET_PAGE_HEAP");
			sb.AppendLine("PAGE_INIT_MEM");
			sb.AppendLine("CALL [_main]");
			sb.AppendLine("PAGE_RET");
			while (i < statements.Length) {
				statements[i].ToAsm(sb,this);
				i++;
			}

			for(i = 0; i < stringTable.Count; i++) {
				sb.AppendLine("s_{0}: DS \"{1}\"".f (i,stringTable[i]));
			}

			foreach (var symbol in symbolTable) {
				if(!symbol.isfunc) {
					if(symbol.type.value == "string") {
						sb.AppendLine("{0}: DUI 0".f (symbol.GetFullName()));
					} else {
						sb.AppendLine("{0}: {1} 0".f (symbol.GetFullName(),symbol.type.GetDefine()));
					}
				}
			}
			sb.AppendLine("_prg_end:");
		}


		public string TmpString(string str) {
			stringTable.Add(str);
			return "s_{0}".f(stringTable.Count - 1);
		}

		public object StringConst (string value)
		{
			if (stringTable.Contains (value)) {
				return "s_{0}".f (stringTable.IndexOf (value));
			} else {
				stringTable.Add (value);
				return "s_{0}".f (stringTable.Count - 1);
			}
		}

	}
}

