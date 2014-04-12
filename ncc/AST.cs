namespace ncc.AST
{
	public interface STMT {}

	public interface EXPR : STMT {}

	public class BLOCK : STMT 
	{
		public STMT[] statements;
	}

	public class EXPR_LIST : EXPR {}

	public class Assign : STMT {
		public SymbolTableEntry variable;
		public EXPR value;
	}

	public class FunctionArg {
		public string name;
		public TypeDef type;
	}

	public class Function : STMT {
		public SymbolTableEntry symbol;
		public FunctionArg[] args;
		public STMT body;
	}

	public class INT_LIT : EXPR {
		public int value;
	}

	public class CHAR_LIT : EXPR {
		public char value;
	}

	public class BYTE_LIT : EXPR {
		public byte value;
	}

	public class STRING_LIT : EXPR {
		public string value;
	}

	public class BOOL_LIT : EXPR {
		public bool value;
	}
}