using System.Text;
using System;

namespace ncc.AST
{
	public interface STMT {
		void ToAsm(StringBuilder sb, CodeGenerator generator);
	}

	public interface EXPR : STMT {
		void Push (StringBuilder sb, CodeGenerator generator);
		void Load (SymbolTableEntry variable, StringBuilder sb, CodeGenerator generator);
	}

	public class BLOCK : STMT
	{
		public STMT[] statements;

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder();
			foreach (var s in statements) {
				sb.AppendLine(s.ToString());
			}
			return string.Format ("[BLOCK]\n{0}",sb.ToString());
		}

		#region STMT implementation
		public void ToAsm (StringBuilder sb, CodeGenerator generator)
		{
			foreach (var s in statements) {
				s.ToAsm(sb,generator);
			}
		}
		#endregion

	}

	public class EXPR_LIST : EXPR {
		public EXPR[] list;

		public void ToAsm (StringBuilder sb, CodeGenerator generator)
		{
			foreach (var e in list) {
				e.ToAsm(sb,generator);
			}
		}
		#region EXPR implementation
		public void Push (StringBuilder sb, CodeGenerator generator)
		{
			foreach (var e in list) {
				e.Push(sb,generator);
			}
		}

		public void Load (SymbolTableEntry variable, StringBuilder sb, CodeGenerator generator)
		{
			foreach (var e in list) {
				e.Load(variable,sb,generator);
			}
		}
		#endregion

	}

	public class Assign : STMT {
		public SymbolTableEntry variable;
		public EXPR value;

		public override string ToString ()
		{
			return string.Format ("[Assign] {0} = {1}",variable.name,value.ToString());
		}

		public void ToAsm (StringBuilder sb, CodeGenerator generator)
		{
			if (value is INT_LIT) {
				sb.AppendLine (string.Format ("LD DA [{0}]", variable.GetFullName()));
				sb.AppendLine (string.Format ("LD AX {0}", (value as INT_LIT).value.ToString ()));
				sb.AppendLine ("WRITEI");
			} else if (value is STRING_LIT) {
				value.Push(sb,generator);
				sb.AppendLine(string.Format("LD AX {0}",(value as STRING_LIT).value.Length + 4));
				sb.AppendLine("MALLOC");
				sb.AppendLine("MV DB DA");
				sb.AppendLine("LD DA [{0}]".f(variable.GetFullName()));
				sb.AppendLine("WRITEUI");
				sb.AppendLine("MV DA DB");
				sb.AppendLine("POPS");
			}
		}
	}

	public class FunctionArg {
		public SymbolTableEntry symbol;
		public TypeDef type;
	}

	public class Function : STMT {
		public SymbolTableEntry symbol;
		public FunctionArg[] args;
		public STMT body;

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder();
			foreach (var s in args) {
				sb.AppendLine(string.Format("{0} {1},",s.type.name,s.symbol.GetFullName()));
			}
			return string.Format ("[FUNCITON] {0} {1} ({2}):\n{3}",symbol.type.name,symbol.name,sb.ToString(),body.ToString());
		}

		public void ToAsm (StringBuilder sb, CodeGenerator generator)
		{
			sb.AppendLine("{0}:".f (symbol.GetFullName()));
			foreach (var arg in args) {
				switch (arg.type.value) {
				case "int":
					sb.AppendLine("POPI AX");
					sb.AppendLine("LD DA [{0}]".f (arg.symbol.GetFullName()));
					sb.AppendLine("WRITEI");
					break;
				case "uint":
					sb.AppendLine("POPUI DB");
					sb.AppendLine("LD DA [{0}]".f (arg.symbol.GetFullName()));
					sb.AppendLine("WRITEUI");
					break;
				case "string":
					sb.AppendLine("POPI AX");
					sb.AppendLine("PUSHR AX");
					sb.AppendLine("LD BX 4");
					sb.AppendLine("ADD INT");
					sb.AppendLine("MV AX EX");
					sb.AppendLine("MALLOC");
					sb.AppendLine("MV DB DA");
					sb.AppendLine("LD DA [{0}]".f (arg.symbol.GetFullName()));
					sb.AppendLine("WRITEUI");
					break;
				default:
					break;
				}
			}
			body.ToAsm(sb,generator);
			sb.AppendLine("RET");
		}
	}

	public class ASM_CALL : STMT {
		public string asm;

		public override string ToString ()
		{
			return string.Format ("[ASM] ({0})",asm);
		}

		public void ToAsm (StringBuilder sb, CodeGenerator generator)
		{
			sb.AppendLine(asm);
		}
	}

	public class INT_LIT : EXPR {
		public int value;

		public override string ToString ()
		{
			return value.ToString();
		}

		public void ToAsm (StringBuilder sb, CodeGenerator generator)
		{
			throw new NotImplementedException();
		}

		#region EXPR implementation
		public void Push (StringBuilder sb, CodeGenerator generator)
		{
			sb.AppendLine("PUSHI {0}".f (value));
		}

		public void Load (SymbolTableEntry variable, StringBuilder sb, CodeGenerator generator)
		{
			sb.AppendLine("LD DA [{0}]".f (variable.GetFullName()));
			sb.AppendLine("LD AX {0}".f (value));
			sb.AppendLine("WRITEI");
		}
		#endregion

	}

	public class CHAR_LIT : EXPR {
		public char value;

		public override string ToString ()
		{
			return value.ToString();
		}

		public void ToAsm (StringBuilder sb, CodeGenerator generator)
		{
			throw new NotImplementedException();
		}
		#region EXPR implementation
		public void Push (StringBuilder sb, CodeGenerator generator)
		{
			sb.AppendLine("PUSHB '{0}'".f(value));
		}

		public void Load (SymbolTableEntry variable, StringBuilder sb, CodeGenerator generator)
		{
			sb.AppendLine("LD DA [{0}]".f (variable.GetFullName()));
			sb.AppendLine("LD A '{0}'".f(value));
			sb.AppendLine("WRITEB");
		}
		#endregion

	}

	public class BYTE_LIT : EXPR {
		public byte value;

		public override string ToString ()
		{
			return value.ToString();
		}

		public void ToAsm (StringBuilder sb, CodeGenerator generator)
		{
			throw new NotImplementedException();
		}
		#region EXPR implementation
		public void Push (StringBuilder sb, CodeGenerator generator)
		{
			sb.AppendLine("PUSHB {0}".f(value));
		}

		public void Load (SymbolTableEntry variable, StringBuilder sb, CodeGenerator generator)
		{
			sb.AppendLine("LD DA [{0}]".f (variable.GetFullName()));
			sb.AppendLine("LD A {0}".f(value));
			sb.AppendLine("WRITEB");
		}
		#endregion

	}

	public class STRING_LIT : EXPR {
		public string value;
		public string vname;

		public override string ToString ()
		{
			return value;
		}

		public void ToAsm (StringBuilder sb, CodeGenerator generator)
		{
			throw new NotImplementedException();
		}
		#region EXPR implementation
		public void Push (StringBuilder sb, CodeGenerator generator)
		{
			sb.AppendLine("LD DA [{0}]".f (generator.StringConst(value)));
			sb.AppendLine("LODS");
		}

		public void Load (SymbolTableEntry variable, StringBuilder sb, CodeGenerator generator)
		{
			sb.AppendLine("LD DB [{0}]".f (generator.StringConst(value)));
			sb.AppendLine("LD DA [{0}]".f (variable.GetFullName()));
			sb.AppendLine("WRITEUI");
		}
		#endregion

	}

	public class BOOL_LIT : EXPR {
		public bool value;

		public override string ToString ()
		{
			return value.ToString();
		}

		public void ToAsm (StringBuilder sb, CodeGenerator generator)
		{
			throw new NotImplementedException();
		}
		#region EXPR implementation
		public void Push (StringBuilder sb, CodeGenerator generator)
		{
			sb.AppendLine("PUSHB {0}".f((value ? 1 : 0)));
		}

		public void Load (SymbolTableEntry variable, StringBuilder sb, CodeGenerator generator)
		{
			sb.AppendLine("LD DA [{0}]".f (variable.GetFullName()));
			sb.AppendLine("LD A {0}".f((value ? 1 : 0)));
			sb.AppendLine("WRITEB");
		}
		#endregion

	}

	public class FUNC_CALL : EXPR {
		public SymbolTableEntry name;
		public EXPR args;

		public override string ToString ()
		{
			return string.Format("{0}({1})",name.GetFullName(),args.ToString());
		}

		public void ToAsm (StringBuilder sb, CodeGenerator generator)
		{
			args.Push(sb,generator);
			sb.AppendLine("CALL [{0}]".f (name.GetFullName()));
		}
		#region EXPR implementation
		public void Push (StringBuilder sb, CodeGenerator generator)
		{
			ToAsm(sb,generator);
		}

		public void Load (SymbolTableEntry variable, StringBuilder sb, CodeGenerator generator)
		{
			if (variable.type.value == "int") {
				sb.AppendLine ("POPI AX");
				sb.AppendLine ("LD DA [{0}]".f (variable.GetFullName ()));
				sb.AppendLine ("WRITEI");
			} else if (variable.type.value == "string") {
				sb.AppendLine("POPI AX");
				sb.AppendLine("PUSHR AX");
				sb.AppendLine("LD BX 4");
				sb.AppendLine("ADD INT");
				sb.AppendLine("MV AX EX");
				sb.AppendLine("MALLOC");
				sb.AppendLine("MV DB DA");
				sb.AppendLine("LD DA [{0}]".f (variable.GetFullName()));
				sb.AppendLine("WRITEUI");
				sb.AppendLine("MV DA DB");
				sb.AppendLine("POPS");
			}
		}
		#endregion

	}
}