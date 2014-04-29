using System;
using ncc.AST;
using System.Collections.Generic;

namespace ncc
{
	[Serializable]
	public class ParseException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:ParseException"/> class
		/// </summary>
		public ParseException ()
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="T:ParseException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		public ParseException (string message) : base (message)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="T:ParseException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		/// <param name="inner">The exception that is the cause of the current exception. </param>
		public ParseException (string message, Exception inner) : base (message, inner)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="T:ParseException"/> class
		/// </summary>
		/// <param name="context">The contextual information about the source or destination.</param>
		/// <param name="info">The object that holds the serialized object data.</param>
		protected ParseException (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base (info, context)
		{
		}
	}

	public class TypeTable
	{
		List<type> typeDefs;

		public TypeTable()
		{
			typeDefs = new List<type> ();
		}

		public bool IsType(string name) {
			return typeDefs.Exists(p => p.Name == name);
		}

		public type get(string name) {
			return typeDefs.Find (p => p.Name == name);
		}

		public void AddType(type t) {
			typeDefs.Add (t);
		}
	}

	public class SymbolTable
	{
		List<symbol> symbolTable;

		public SymbolTable()
		{
			symbolTable = new List<symbol> ();
		}

		public bool IsSymbol(string name) {
			return symbolTable.Exists(p => p.Name == name);
		}

		public symbol get(string name) {
			return symbolTable.Find (p => p.Name == name);
		}

		public void Add(symbol Symbol) {
			symbolTable.Add (Symbol);
		}
	}

	public class Parser
	{
		Token[] tokens;
		int index;

		TypeTable typeTable;
		SymbolTable symbolTable;

		public Token CurrentToken
		{
			get {
				return tokens [index];
			}
		}

		public Token NextToken {
			get {
				if (index + 1 < tokens.Length)
					return tokens [index + 1];
				else
					return CurrentToken;
			}
		}

		public bool EOF
		{
			get {
				return index >= tokens.Length - 1;
			}
		}

		public void Next()
		{
			if (index + 1 < tokens.Length)
				index++;
		}

		public Parser (Token[] Tokens)
		{
			tokens = Tokens;
			index = 0;
			symbolTable = new SymbolTable();
			typeTable = new TypeTable ();
			typeTable.AddType (new type () { Name = "void", Value = "void" });
			typeTable.AddType (new type () { Name = "int", Value = "int" });
			typeTable.AddType (new type () { Name = "float", Value = "float" });
			typeTable.AddType (new type () { Name = "bool", Value = "bool" });
			typeTable.AddType (new type () { Name = "char", Value = "char" });
			typeTable.AddType (new type () { Name = "string", Value = "string" });
		}

		public stmt[] Parse()
		{
			List<stmt> Stmts = new List<stmt> ();
			while (!EOF)
				Stmts.Add (ParseStmt ());
			return Stmts.ToArray ();
		}

		public stmt ParseStmt()
		{
			switch (CurrentToken.Type) {
			case TokenType.word:
				return ParseWord ();
			default:
				return null;
			}
		}

		public block ParseBlock()
		{
			List<stmt> block = new List<stmt> ();
			while (!(CurrentToken.Type == TokenType.word && (CurrentToken.Value as string).Equals("end")))
				block.Add (ParseStmt ());
			Next ();
			return new block () { list = block.ToArray() };
		}

		public stmt ParseWord()
		{
			string word = CurrentToken.Value as string;
			if (typeTable.IsType (word)) {
				type Type = typeTable.get (word);
				Next ();
				if (CurrentToken.Type == TokenType.star) {
					Next ();
					string name = CurrentToken.Value as string;
					symbol sym = new symbol () { Name = name, Type = Type, isPointer = true };
					Next ();
					Next ();
					assign_pointer a = new assign_pointer ();
					a.Name = sym;
					a.Value = ParseExpr ();
					symbolTable.Add (sym);
					return a;
				} else if (CurrentToken.Type == TokenType.word) {
					string Name = CurrentToken.Value as string;
					symbol sym = new symbol () { Type = Type, Name = Name };
					Next ();
					if (CurrentToken.Type == TokenType.openparan) {
						function_definition fdef = new function_definition ();
						fdef.Symbol = sym;
						Next ();
						fdef.Args = ParseArgs ();
						Next ();
						fdef.Body = ParseBlock ();
						sym.isFunc = true;
						symbolTable.Add (sym);
						return fdef;
					} else if (CurrentToken.Type == TokenType.symbol && ((char)CurrentToken.Value).Equals ('=')) {
						Next ();
						assign a = new assign ();
						a.Name = sym;
						a.Value = ParseExpr ();
						symbolTable.Add (sym);
						return a;
					}
				}
			} else if (symbolTable.IsSymbol (word)) {
				symbol sym = symbolTable.get (word);
				Next ();
				if (CurrentToken.Type == TokenType.symbol && ((char)CurrentToken.Value).Equals('=')) {
					Next ();
					assign a = new assign ();
					a.Name = sym;
					a.Value = ParseExpr ();
					return a;
				}
			} else if (word.Equals ("ret")) {
				Next ();
				ret r = new ret ();
				r.Value = ParseExpr ();
				return r;
			}
			throw new ParseException ("Unknown word: " + CurrentToken.ToString());
		}

		public args ParseArgs()
		{
			List<symbol> symbols = new List<symbol>();
			while (CurrentToken.Type != TokenType.closeparan) {
				if (CurrentToken.Type == TokenType.comma)
					Next ();
				symbol s = new symbol ();
				string typename = CurrentToken.Value as string;
				if (typeTable.IsType (typename)) 
					s.Type = typeTable.get (typename); 
				else throw new Exception (typename + " is not a type");
				Next ();
				s.Name = CurrentToken.Value as string;
				Next ();
				symbols.Add (s);
			}
			args Args = new args () { symbols = symbols.ToArray() };
			return Args;
		}

		public expr ParseExpr(bool isFactor = false)
		{
			expr e = null;

			if (CurrentToken.Type == TokenType.intl)
				e = new intl () { value = (int)CurrentToken.Value };
			else if (CurrentToken.Type == TokenType.decimall)
				e = new decimall () { value = (double)CurrentToken.Value };
			else if (CurrentToken.Type == TokenType.charl)
				e = new charl () { value = (char)CurrentToken.Value };
			else if (CurrentToken.Type == TokenType.stringl)
				e = new stringl () { value = CurrentToken.Value as string };
			else if (CurrentToken.Type == TokenType.word) {
				if (symbolTable.IsSymbol (CurrentToken.Value as string)) {
					symbol s = symbolTable.get (CurrentToken.Value as string);
					if (NextToken.Type == TokenType.openparan) {
						Next ();
						Next ();
						expr Args = ParseExpr ();
						Next ();
						function_call fc = new function_call ();
						fc.Argument = Args;
						fc.Name = s;
						e = fc;
					} else {
						getVar g = new getVar () { Symbol = s };
						e = g;
					}
				}
			} else if (CurrentToken.Type == TokenType.star) {
				Next ();
				string name = CurrentToken.Value as string;
				if (symbolTable.IsSymbol (name)) {
					symbol s = symbolTable.get (name);
					deref_pointer dp = new deref_pointer ();
					dp.Symbol = s;
					e = dp;
				} else
					throw new ParseException ("Unknown symbol: " + name);
			} else if (CurrentToken.Type == TokenType.and) {
				Next ();
				string name = CurrentToken.Value as string;
				if (symbolTable.IsSymbol (name)) {
					symbol s = symbolTable.get (name);
					get_address dp = new get_address ();
					dp.Symbol = s;
					e = dp;
				} else
					throw new ParseException ("Unknown symbol: " + name);
			}
			else
				throw new ParseException ("Unknown expression: " + CurrentToken.ToString ());

			Next ();

			if (CurrentToken.Type == TokenType.star || CurrentToken.Type == TokenType.divide) {
				factor f = new factor ();
				f.Term1 = e;
				f.Op = (CurrentToken.Type == TokenType.star ? op.mul : op.div);
				Next ();
				f.Term2 = ParseExpr (true);
				e = f;
			}

			if ((CurrentToken.Type == TokenType.plus || CurrentToken.Type == TokenType.minus) && !isFactor) {
				arith a = new arith ();
				a.Factor1 = e;
				a.Op = (CurrentToken.Type == TokenType.plus ? op.plus : op.minus);
				Next ();
				a.Factor2 = ParseExpr ();
				e = a;
			}

			if (CurrentToken.Type == TokenType.comma) {
				expr_list l = new expr_list ();
				l.first = e;
				Next ();
				l.last = ParseExpr ();
				e = l;
			}

			return e;
		}
	}
}

