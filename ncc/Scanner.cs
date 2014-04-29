using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace ncc
{
	public enum TokenType
	{
		word,
		charl,
		intl,
		stringl,
		decimall,
		booll,
		symbol,
		openparan,
		closeparan,
		openbracket,
		closebracket,
		comma,
		semicolon,
		plus,
		minus,
		star,
		divide,
		and
	}

	public class Token
	{
		public TokenType Type;
		public object Value;

		public Token(TokenType Type, object Value) {
			this.Type = Type;
			this.Value = Value;
		}

		public override string ToString ()
		{
			return string.Format ("[{0}] {1}", Type, Value);
		}
	}

	public class Scanner
	{
		StringReader codeReader;
		char current;

		List<string> includedfiles = new List<string>();

		List<Token> tokens;

		public List<Token> Tokens
		{
			get {
				return tokens;
			}
		}

		public Scanner (StringReader codeReader)
		{
			this.codeReader = codeReader;
			tokens = new List<Token> ();
		}

		public void Scan()
		{
			ReadNext ();
			while (codeReader.Peek() != -1)
				ScanCurrent ();
		}

		public void ScanCurrent()
		{
			if (char.IsDigit (current))
				ScanNumber ();
			else if (char.IsWhiteSpace (current))
				ReadNext ();
			else if (current == '"')
				ScanString ();
			else if (char.IsLetter (current))
				ScanWord ();
			else
				ParseSymbol ();
		}

		void ParseSymbol ()
		{
			switch (current) {
			case '(':
				tokens.Add (new Token (TokenType.openparan, current));
				break;
			case ')':
				tokens.Add (new Token (TokenType.closeparan, current));
				break;
			case '[':
				tokens.Add (new Token (TokenType.openbracket, current));
				break;
			case ']':
				tokens.Add (new Token (TokenType.closebracket, current));
				break;
			case ',':
				tokens.Add (new Token (TokenType.comma, current));
				break;
			case ';':
				tokens.Add (new Token (TokenType.semicolon, current));
				break;
			case '+':
				tokens.Add (new Token (TokenType.plus, current));
				break;
			case '-':
				tokens.Add (new Token (TokenType.minus, current));
				break;
			case '*':
				tokens.Add (new Token (TokenType.star, current));
				break;
			case '/':
				tokens.Add (new Token (TokenType.divide, current));
				break;
			case '&':
				tokens.Add (new Token (TokenType.and, current));
				break;
			case '#':
				PreProccess ();
				break;
			default:
				tokens.Add (new Token (TokenType.symbol, current));
				break;
			}
			ReadNext ();
		}

		public void ReadNext() {
			current = (char)codeReader.Read ();
		}

		public void ScanNumber()
		{
			StringBuilder sb = new StringBuilder ();
			while (char.IsDigit(current)) {
				sb.Append (current);
				ReadNext ();
			}
			if (current != '.') {
				string s = sb.ToString ();
				int i = Convert.ToInt32 (s);
				tokens.Add (new Token (TokenType.intl, i));
			} else {
				sb.Append ('.');
				ReadNext ();
				while (char.IsDigit(current)) {
					sb.Append (current);
					ReadNext ();
				}
				string s = sb.ToString ();
				double d = Convert.ToDouble (s);
				tokens.Add (new Token (TokenType.decimall, d));
			}
		}

		public void ScanString() 
		{
			StringBuilder sb = new StringBuilder ();
			ReadNext ();
			while (current != '"') {
				sb.Append (current);
				ReadNext ();
			}
			ReadNext ();
			string s = sb.ToString ();
			tokens.Add (new Token (TokenType.stringl, s));
		}

		public void ScanWord()
		{
			StringBuilder sb = new StringBuilder ();
			while (char.IsLetterOrDigit(current) || current == '_') {
				sb.Append (current);
				ReadNext ();
			}
			string s = sb.ToString ();
			switch (s) {
			case "true":
				tokens.Add (new Token (TokenType.booll, true));
				break;
			case "false":
				tokens.Add (new Token (TokenType.booll, false));
				break;
			default:
				tokens.Add (new Token (TokenType.word, s));
				break;
			}
		}

		void PreProccess ()
		{
			string preprocessorline = "";
			StringBuilder sb = new StringBuilder();
			while(current != '\n') {
				sb.Append(current);
				ReadNext ();
			}
			preprocessorline = sb.ToString();
			if (preprocessorline.StartsWith("#include ")) {
				if (preprocessorline.Contains("<")) {
					string file = preprocessorline.Substring(preprocessorline.IndexOf('<') + 1,preprocessorline.LastIndexOf('>') - preprocessorline.IndexOf('<') - 1);
					if(!includedfiles.Contains(file)) {
						includedfiles.Add(file);
						string Code = File.ReadAllText(file);
						Scanner subscan = new Scanner (new StringReader (Code));
						subscan.Scan ();
						tokens.AddRange (subscan.tokens);
					}
				} else if (preprocessorline.Contains("\"")) {
					string file = preprocessorline.Substring(preprocessorline.IndexOf('"') + 1,preprocessorline.LastIndexOf('"') - preprocessorline.IndexOf('"') - 1);
					if(!includedfiles.Contains(file)) {
						includedfiles.Add(file);
						string Code = File.ReadAllText(file);
						Scanner subscan = new Scanner (new StringReader (Code));
						subscan.Scan ();
						tokens.AddRange (subscan.tokens);
					}
				}
			}
		}
	}
}

