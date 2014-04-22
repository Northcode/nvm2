using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ncc
{
	public class Token
	{
		public const byte WORD = 0;
		public const byte BYTE_LIT = 1;
		public const byte INT_LIT = 2;
		public const byte UINT_LIT = 3;
		public const byte FLOAT_LIT = 4;
		public const byte STRING_LIT = 5;
		public const byte SYMBOL = 6;
		public const byte BOOL_LIT = 7;
        public const byte CHAR_LIT = 8;
		public const byte TYPE = 9;
		public const byte OPENPARAN = 10;
		public const byte CLOSEPARAN = 11;
		public const byte OPENBLOCK = 12;
		public const byte CLOSEBLOCK = 13;
		public const byte KEYWORD = 14;

		public object value;
		public byte type;

		public override string ToString ()
		{
			return string.Format ("[Token] {0} : {1} ",type,value);
		}
	}

	public class Scanner
	{
		List<Token> tokens = new List<Token>();

		List<string> includedfiles = new List<string>();

		public Scanner ()
		{
		}

		public List<Token> getTokens ()
		{
			return tokens;
		}

		public void Scan(string code)
        {
            tokens = new List<Token>();
            int i = 0;
            while (i < code.Length)
            {
                if (char.IsDigit(code[i]))
                {
                    StringBuilder strb = new StringBuilder();
                    while (char.IsDigit(code[i]))
                    {
                        strb.Append(code[i]);
                        i++;
                    }
                    i--;
                    if (code[i + 1] == '.')
                    {
                        strb.Append('.');
                        i++;
                        while (char.IsDigit(code[i]))
                        {
                            strb.Append(code[i]);
                            i++;
                        }
                        i--;
                        float f = (float)Convert.ToDecimal(strb.ToString());
                        tokens.Add(new Token() { type = Token.FLOAT_LIT, value = f });
                    }
                    else if (code[i + 1] == 'x')
                    {
                        i += 2;
                        strb.Clear();
                        strb.Append(code[i]);
                        i++;
                        strb.Append(code[i]);
                        byte b = byte.Parse(strb.ToString(), System.Globalization.NumberStyles.HexNumber);
                        tokens.Add(new Token() { type = Token.BYTE_LIT, value = b });
                    }
                    else
                    {
                        string str = strb.ToString();
                        int n = Convert.ToInt32(str);
                        tokens.Add(new Token() { type = Token.INT_LIT, value = n });
                    }
                }
				else if (code[i] == '#') {
					string preprocessorline = "";
					StringBuilder sb = new StringBuilder();
					while(code[i] != '\n') {
						sb.Append(code[i]);
						i++;
					}
					preprocessorline = sb.ToString();
					if (preprocessorline.StartsWith("#include ")) {
						if (preprocessorline.Contains("<")) {
							string file = preprocessorline.Substring(preprocessorline.IndexOf('<') + 1,preprocessorline.LastIndexOf('>') - preprocessorline.IndexOf('<') - 1);
							if(!includedfiles.Contains(file)) {
								includedfiles.Add(file);
								string Code = File.ReadAllText(file);
								Scan (Code);
							}
						} else if (preprocessorline.Contains("\"")) {
							string file = preprocessorline.Substring(preprocessorline.IndexOf('"') + 1,preprocessorline.LastIndexOf('"') - preprocessorline.IndexOf('"') - 1);
							if(!includedfiles.Contains(file)) {
								includedfiles.Add(file);
								string Code = File.ReadAllText(file);
								Scan (Code);
							}
						}
					}
				}
                else if (char.IsLetter(code[i]) || code[i] == '_')
                {
                    StringBuilder strb = new StringBuilder();
                    while (i < code.Length && (char.IsLetterOrDigit(code[i]) || code[i] == '@' || code[i] == '.' || code[i] == '_'))
                    {
                        strb.Append(code[i]);
                        i++;
                    }
					string word = strb.ToString();
                    if (word == "true")
                    {
                        tokens.Add(new Token() { type = Token.BOOL_LIT, value = true });
                    }
                    else if (word == "false")
                    {
                        tokens.Add(new Token() { type = Token.BYTE_LIT, value = false });
                    }
					else if (word == "if") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "elseif") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "else") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "for") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "while") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "switch") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "case") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "default") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "const") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "continue") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "do") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "enum") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "goto") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "register") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "return") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "sizeof") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "static") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "struct") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "typedef") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "union") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "volatile") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "auto") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "break") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
					else if (word == "asm") {
						tokens.Add (new Token() { type = Token.KEYWORD, value = word });
					}
                    else
                    {
                        tokens.Add(new Token() { type = Token.WORD, value = strb.ToString() });
                    }
                    i--;
                }
                else if (code[i] == '"')
                {
                    bool escaped = false;
					bool writtenescape = false;
                    bool r = true;
                    StringBuilder strb = new StringBuilder();
                    i++;
                    r = (code[i] != '"');
                    while (r)
                    {
                        if (escaped && code[i] == 'n')
                        {
                            strb.Append("\n");
							writtenescape = true;
                        }
                        escaped = false;
                        if (code[i] == '\\')
                        {
                            escaped = true;
                        }
                        else
                        {
							if(!writtenescape) {
                            	strb.Append(code[i]);
							}
							writtenescape = false;
                        }
                        i++;
                        if (!escaped)
                        {
                            r = (code[i] != '"');
                        }
                    }
                    tokens.Add(new Token() { type = Token.STRING_LIT, value = strb.ToString() });
                } else if (code[i] == '\'') {
                    i++;
                    char c = code[i];
                    i++; i++;
                    tokens.Add(new Token() {type = Token.CHAR_LIT, value = c});
                }
                else if (code[i] == '\n')
                {
                    //nop
                }
                else if (code[i] == ' ')
                {
                    //nop
                }
                else if (code[i] == '\t')
                {
					//nop
                }
                else if (code[i] == '\r')
                {
					//nop
                }
				else if (code[i] == ';')
                {
					//nop
                }
				else if (code[i] == '(') { tokens.Add(new Token() { type = Token.OPENPARAN, value = code[i] }); }
				else if (code[i] == ')') { tokens.Add(new Token() { type = Token.CLOSEPARAN, value = code[i] }); }
				else if (code[i] == '{') { tokens.Add(new Token() { type = Token.OPENBLOCK, value = code[i] }); }
				else if (code[i] == '}') { tokens.Add(new Token() { type = Token.CLOSEBLOCK, value = code[i] }); }
                else
                {
                    tokens.Add(new Token() { type = Token.SYMBOL, value = code[i] });
                }
                i++;
            }
        }
	}
}

