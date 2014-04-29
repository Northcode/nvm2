/*

TEST COMPILER LANG

stmt 	:= 	word
		|	expr
		|	definition
		|	assign
		|	block

expr	:=	string
		|	int
		|	decimal
		|	function_call
		|	bool
		|	expr_list
		|	arith
		|	factor

expr_list	:=	[expr',']+

definition	:=	function_definition
			|	class_definition

block		:= stmt* "end"

char	:=	[a-zA-Z]
int		:=	0-9
decimal	:=	[int'.'int]
word	:=	char+[char|int|'_']*
symbol	:=	[type]* name

function_definition	:=	type name (args) block

name	:=	word
fargs	:=	[symbol',']*

function_call	:=	name(expr_list)

assign	:=	symbol '=' expr

factor	:=	expr [*|/] expr
arith	:=	factor [+|-] factor

*/


using System;
using System.Text;

namespace ncc.AST
{
	public interface stmt
	{

	}

	public interface expr
	{

	}

	public class block : stmt
	{
		public stmt[] list;
		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			foreach (var s in list)
				sb.AppendLine (s.ToString ());
			string str = sb.ToString();
			return string.Format ("[block] {0}",str);
		}
	}

	public class expr_list : expr
	{
		public expr first;
		public expr last;

		public override string ToString ()
		{
			return string.Format ("{0},{1}",first.ToString(),last.ToString());
		}
	}

	public class type
	{
		public string Name;
		public string Value;

		public override string ToString ()
		{
			return string.Format ("[{0}]",Name);
		}
	}

	public class @class : type
	{
		public symbol[] fields;
		public type parent;
	}

	public class symbol
	{
		public type Type;
		public string Name;
		public bool isFunc;
		public bool isPointer;
	}

	public class args
	{
		public symbol[] symbols;

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			foreach (var s in symbols)
				sb.Append (String.Format ("[{0} {1}]", s.Type.Name, s.Name));
			return string.Format ("[args {0}]",sb.ToString());
		}
	}

	public class definition : stmt
	{

	}

	public class function_definition : definition
	{
		public symbol Symbol;
		public args Args;
		public stmt Body;

		public override string ToString ()
		{
			return string.Format ("[function] {0} {1} ({2}) {3}",Symbol.Type.ToString(),Symbol.Name,Args.ToString(),Body.ToString());
		}
	}

	public class ret : stmt
	{
		public expr Value;

		public override string ToString ()
		{
			return string.Format ("[ret] {0}",Value.ToString());
		}
	}

	public class function_call : expr
	{
		public symbol Name;
		public expr Argument;

		public override string ToString ()
		{
			return string.Format ("[call] {0} ({1})",Name.Name,Argument.ToString());
		}
	}

	public class scope
	{
		public string name;
		public scope parent;

		public string GetFullName()
		{
			if (parent != null) return parent.GetFullName () + "_" + name;
			else return name;
		}
	}

	public class assign : stmt
	{
		public symbol Name;
		public expr Value;

		public override string ToString ()
		{
			return string.Format ("[assign] {0} = {1}",Name.Name,Value.ToString());
		}
	}

	public class assign_pointer : stmt
	{
		public symbol Name;
		public expr Value;

		public override string ToString ()
		{
			return string.Format ("[assign_ptr] {0} = {1}",Name.Name,Value.ToString());
		}
	}

	public enum op 
	{
		plus,
		minus,
		mul,
		div
	}

	public class factor : expr
	{
		public expr Term1;
		public op Op;
		public expr Term2;

		public override string ToString ()
		{
			return string.Format ("[factor] ({0}) {1} ({2})",Term1.ToString(),Op.ToString(),Term2.ToString());
		}
	}

	public class arith : expr
	{
		public expr Factor1;
		public op Op;
		public expr Factor2;
		
		public override string ToString ()
		{
			return string.Format ("[arith] ({0}) {1} ({2})",Factor1.ToString(),Op.ToString(),Factor2.ToString());
		}
	}

	public class stringl : expr
	{
		public string value;

		public override string ToString ()
		{
			return string.Format ("\"{0}\"",value);
		}
	}

	public class intl : expr
	{
		public int value;

		public override string ToString ()
		{
			return string.Format ("{0}",value);
		}
	}

	public class decimall : expr
	{
		public double value;

		public override string ToString ()
		{
			return string.Format ("{0}",value);
		}
	}

	public class booll : expr
	{
		public bool value;

		public override string ToString ()
		{
			return string.Format ("{0}",value);
		}
	}

	public class charl : expr
	{
		public char value;

		public override string ToString ()
		{
			return string.Format ("{0}",value);
		}
	}

	public class getVar : expr
	{
		public symbol Symbol;

		public override string ToString ()
		{
			return string.Format ("[getVar] {0}",Symbol.Name);
		}
	}

	public class deref_pointer : expr
	{
		public symbol Symbol;

		public override string ToString ()
		{
			return string.Format ("[deref_pointer] *{0}",Symbol.Name);
		}
	}

	public class get_address : expr
	{
		public symbol Symbol;

		public override string ToString ()
		{
			return string.Format ("[get_address] &{0}",Symbol.Name);
		}
	}
}

