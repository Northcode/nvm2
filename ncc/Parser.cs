using System;
using System.Collections.Generic;
using ncc.AST;

namespace ncc
{
	public class Scope
	{
		public string scopestr;
		public Scope Parent { get; private set; }

		public Scope (string ScopeStr, Scope parent)
		{
			scopestr = ScopeStr;
			Parent = parent;
		}

		public Scope SubScope (string name)
		{
			return new Scope(scopestr + "." + name, this);
		}
	}

	public class SymbolTableEntry
	{
		public string name;
		public TypeDef type;
		public Scope scope;
		public bool isfunc;
	}

	public class TypeDef
	{
		public string name;
		public string value;
	}

	public class Parser
	{
		List<TypeDef> typetable;
		List<SymbolTableEntry> symbolTable;

		Token[] tokens;

		int i;

		public Parser (Token[] Tokens)
		{
			tokens = Tokens;
			i = 0;
			typetable = new List<TypeDef>();
			typetable.Add(new TypeDef() { name = "void", value = null });
			typetable.Add(new TypeDef() { name = "int", value = "int" });
			typetable.Add(new TypeDef() { name = "uint", value = "uint" });
			typetable.Add(new TypeDef() { name = "byte", value = "char" });
			typetable.Add(new TypeDef() { name = "char", value = "char" });
			typetable.Add(new TypeDef() { name = "float", value = "float" });

			symbolTable = new List<SymbolTableEntry>();
		}

		public STMT ParseStmt (Scope currentscope)
		{
			try {
				if (tokens [i].type == Token.KEYWORD) {
					if (tokens [i].value as String == "if") {
						throw new NotImplementedException();
					} else {
						throw new Exception(String.Format("Unexpected keyword: {0}",tokens[i].ToString()));
					}
				} else if (tokens [i].type == Token.WORD) {
					if(typetable.Count > 0 && typetable.Exists (p => p.name == tokens [i].value as string)) { //check if the current token is a type
						TypeDef type = typetable.Find(t => t.name == tokens[i].value as String);
						i++;
						if (tokens [i].type == Token.WORD) { //next token is word
							string name = tokens [i].value as String;
							i++;
							if (tokens [i].type == Token.SYMBOL && (char)tokens [i].value == '=') { //variable declaration, create new variable
								SymbolTableEntry variable = new SymbolTableEntry() { name = name, type = type, scope = currentscope, isfunc = false };
								symbolTable.Add(variable);
								return new Assign() { variable = variable, value = ParseExpr() };
							} else if (tokens[i].type == Token.OPENPARAN) { //create function
								SymbolTableEntry func = new SymbolTableEntry() { name = name, scope = currentscope, isfunc = true };
								symbolTable.Add(func);
								FunctionArg[] args = ParseFuncArgs(); i++;
								Scope scope = currentscope.SubScope(name);
								STMT body = ParseStmt(scope);
								return new Function() { symbol = func, args = args, body = body };
							} else {
								throw new Exception(String.Format("Unexpected token: {0} after name",tokens[i].ToString()));
							}
						} else {
							throw new Exception(String.Format("Unexpected token: {0} after word",tokens[i].ToString()));
						}
					}
					else if (symbolTable.Exists(s => s.name == tokens[i].value as String)) { //doing something with a variable
						SymbolTableEntry variable = symbolTable.Find(s => s.name == tokens[i].value as String);
						i++;
						if(tokens[i].type == Token.SYMBOL && (char)tokens[i].value == '=') {
							i++;
							EXPR value = ParseExpr();
							return new ncc.AST.Assign() { variable = variable, value = value };
						} else {
							throw new Exception(String.Format("Unexpected token: {0} after variable",tokens[i].ToString()));
						}
					} else {
						throw new Exception(String.Format("Unexpected token: {0} after word",tokens[i].ToString()));
					}
				} else if (tokens[i].type == Token.OPENBLOCK) {
					while(tokens[i].type != Token.CLOSEBLOCK) {
						
					}
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
				e = new INT_LIT () { value = (int)tokens[i].value };
			} else if (tokens [i].type == Token.CHAR_LIT) {
				e = new CHAR_LIT () { value = (char)tokens[i].value };
			} else if (tokens [i].type == Token.STRING_LIT) {
				e = new STRING_LIT () { value = tokens[i].value as String };
			} else if (tokens [i].type == Token.BOOL_LIT) {
				e = new BOOL_LIT () { value = (bool)tokens[i].value };
			}
			return e;
		}

		FunctionArg[] ParseFuncArgs ()
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
						args.Add(new FunctionArg() { name = argname, type = type });
						if(!(tokens[i].type == Token.SYMBOL && (char)tokens[i].value == ',')) {
							break;
						} else { i++; }
					}
				}
				return args.ToArray();
			} else {
				throw new Exception ("Expected open paran after in function definition, got: " + tokens[i].ToString());
			}
		}	

		bool IsType (string type)
		{
			return typetable.Exists(p => p.name == type);
		}	

		TypeDef getType (string type)
		{
			return typetable.Find(p => p.name == type);
		}
	}
}

