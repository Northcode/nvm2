using System;
using System.Collections.Generic;
using ncc.AST;

namespace ncc
{
	public class Scope
	{
		public string scopename;
		public Scope Parent { get; private set; }

		public Scope (string ScopeStr, Scope parent)
		{
			scopename = ScopeStr;
			Parent = parent;
		}

		public Scope SubScope (string name)
		{
			return new Scope(name, this);
		}

		public string GetFullName ()
		{
			if (Parent != null) {
				return Parent.GetFullName () + "_" + scopename;
			} else {
				return scopename;
			}
		}

	}

	public class SymbolTableEntry
	{
		public string name;
		public TypeDef type;
		public Scope scope;
		public bool isfunc;

		public string GetFullName() {
			return "{0}_{1}".f (scope.GetFullName(),name);
		}
	}

	public class TypeDef
	{
		public string name;
		public string value;

		public string GetDefine ()
		{
			switch (value) {
			case "int":
				return "DI";
			case "uint":
				return "DUI";
			case "float":
				return "DF";
			case "string":
				return "DS";
			case null:
				return "";
			default:
				throw new Exception("Unknown typedef: {0}".f (value));
				break;
			}
		}		

		public string Blank ()
		{
			switch (value) {
			case "int":
				return "0";
			case "uint":
				return "0";
			case "float":
				return "0.0";
			case "string":
				return "\"\"";
			default:
				throw new Exception("Unknown typedef: {0}".f (value));
				break;
			}
		}
	}

	public class Parser
	{
		public List<TypeDef> typeTable;
		public List<SymbolTableEntry> symbolTable;

		Token[] tokens;

		int i;

		public Parser (Token[] Tokens)
		{
			tokens = Tokens;
			i = 0;
			typeTable = new List<TypeDef>();
			typeTable.Add(new TypeDef() { name = "void", value = null });
			typeTable.Add(new TypeDef() { name = "int", value = "int" });
			typeTable.Add(new TypeDef() { name = "uint", value = "uint" });
			typeTable.Add(new TypeDef() { name = "byte", value = "char" });
			typeTable.Add(new TypeDef() { name = "char", value = "char" });
			typeTable.Add(new TypeDef() { name = "float", value = "float" });
			typeTable.Add(new TypeDef() { name = "string", value = "string" });

			symbolTable = new List<SymbolTableEntry>();
		}

		public STMT[] Parse ()
		{
			List<STMT> body = new List<STMT> ();
			Scope rootscope = new Scope("",null);
			while (i < tokens.Length) {
				body.Add(ParseStmt(rootscope));
			}
			return body.ToArray();
		}

		public STMT ParseStmt (Scope currentscope)
		{
			try {
				if (tokens [i].type == Token.KEYWORD) {
					if (tokens [i].value as String == "if") {
						throw new NotImplementedException();
					} else if (tokens[i].value as String == "asm") {
						ASM_CALL call = new ASM_CALL();
						i++;
						if(tokens[i].type != Token.STRING_LIT) {
							throw new Exception("Expected string after asm keyword, got: " + tokens[i].ToString());
						}
						string asm = tokens[i].value as String;
						call.asm = asm;
						i++;
						return call;
					} else if (tokens[i].value as String == "return") {
						throw new NotImplementedException();
					} else {
						throw new Exception(String.Format("Unexpected keyword: {0}",tokens[i].ToString()));
					}
				} else if (tokens [i].type == Token.WORD) {
					return ParseWord (currentscope);
				} else if (tokens[i].type == Token.OPENBLOCK) {
					BLOCK b = new BLOCK();
					List<STMT> statements = new List<STMT>();
					i++;
					while(tokens[i].type != Token.CLOSEBLOCK) {
						statements.Add(ParseStmt(currentscope));
					}
					b.statements = statements.ToArray(); i++;
					return b;
				} else {
					throw new Exception(String.Format("Unexpected token: {0}",tokens[i].ToString()));
				}
			} catch (IndexOutOfRangeException) {
				Console.WriteLine("Unexpected EOF");
				return null;
			}
		}

		public EXPR ParseExpr ()
		{
			EXPR e = null;

			if (tokens [i].type == Token.INT_LIT) {
				e = new INT_LIT () { value = (int)tokens[i].value }; i++;
			} else if (tokens [i].type == Token.CHAR_LIT) {
				e = new CHAR_LIT () { value = (char)tokens[i].value }; i++;
			} else if (tokens [i].type == Token.STRING_LIT) {
				e = new STRING_LIT () { value = tokens[i].value as String }; i++;
			} else if (tokens [i].type == Token.BOOL_LIT) {
				e = new BOOL_LIT () { value = (bool)tokens[i].value }; i++;
			}
			return e;
		}

		FunctionArg[] ParseFuncArgs (Scope currentscope)
		{
			if (tokens [i].type == Token.OPENPARAN) {
				List<FunctionArg> args = new List<FunctionArg>();
				i++;
				while(tokens[i].type != Token.CLOSEPARAN) {
					if(IsType(tokens[i].value as String)) {
						TypeDef type = getType(tokens[i].value as String); i++;
						if(tokens[i].type != Token.WORD) {
							throw new Exception("Expected word, got: " + tokens[i].ToString());
						}
						string argname = tokens[i].value as String; i++;
						SymbolTableEntry argSymbol = new SymbolTableEntry() { name = argname, type = type, isfunc = false, scope = currentscope };
						symbolTable.Add(argSymbol);
						args.Add(new FunctionArg() { symbol = argSymbol, type = type });
						if(!(tokens[i].type == Token.SYMBOL && (char)tokens[i].value == ',')) {
							break;
						} else { i++; }
					} else {
						throw new Exception("Unknown type: " + tokens[i].value as String);
					}
				}
				return args.ToArray();
			} else {
				throw new Exception ("Expected open paran after in function definition, got: " + tokens[i].ToString());
			}
		}	

		STMT ParseWord (Scope currentscope)
		{
			if (typeTable.Count > 0 && typeTable.Exists (p => p.name == tokens [i].value as string)) {
				//check if the current token is a type
				TypeDef type = typeTable.Find (t => t.name == tokens [i].value as String);
				i++;
				if (tokens [i].type == Token.WORD) {
					//next token is word
					string name = tokens [i].value as String;
					i++;
					if (tokens [i].type == Token.SYMBOL && (char)tokens [i].value == '=') {
						//variable declaration, create new variable
						SymbolTableEntry variable = new SymbolTableEntry () {
							name = name,
							type = type,
							scope = currentscope,
							isfunc = false
						};
						symbolTable.Add (variable);
						i++;
						return new Assign () {
							variable = variable,
							value = ParseExpr ()
						};
					}
					else if (tokens [i].type == Token.OPENPARAN) {
							//create function
							SymbolTableEntry func = new SymbolTableEntry () {
								name = name,
								scope = currentscope,
								isfunc = true,
								type = type
							};
							symbolTable.Add (func);
							Scope scope = currentscope.SubScope (name);
							FunctionArg[] args = ParseFuncArgs (scope);
							i++;
							STMT body = ParseStmt (scope);
							return new Function () {
								symbol = func,
								args = args,
								body = body
							};
						}
						else {
							throw new Exception (String.Format ("Unexpected token: {0} after name", tokens [i].ToString ()));
						}
				}
				else {
					throw new Exception (String.Format ("Unexpected token: {0} after word", tokens [i].ToString ()));
				}
			}
			else if (symbolTable.Exists (s => s.name == tokens [i].value as String)) {
					//doing something with a variable
					SymbolTableEntry variable = symbolTable.Find (s => s.name == tokens [i].value as String);
					i++;
				if (tokens [i].type == Token.SYMBOL && (char)tokens [i].value == '=') {
						i++;
						EXPR value = ParseExpr ();
						return new ncc.AST.Assign () {
							variable = variable,
							value = value
						};
					}
					else if (tokens[i].type == Token.OPENPARAN) {
						i++;
						EXPR args = ParseExpr();
						i++;
						return new FUNC_CALL() {
							name = variable,
							args = args
						};
					} else {
						throw new Exception (String.Format ("Unexpected token: {0} after variable", tokens [i].ToString ()));
					}
				}
				else {
					throw new Exception (String.Format ("Unexpected token: {0} after word", tokens [i].ToString ()));
				}
		}

		bool IsType (string type)
		{
			return typeTable.Exists(p => p.name == type);
		}	

		TypeDef getType (string type)
		{
			return typeTable.Find(p => p.name == type);
		}
	}
}

