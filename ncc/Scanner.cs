using System;
using System.Collections.Generic;

namespace ncc
{
	class Token
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

		public object value;
		public byte type;
	}

	public class Scanner
	{
		string code;
		List<Token> tokens = new List<Token>();

		public Scanner ()
		{
		}

		public void Scan()
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
					while(code[i] != '\n') {
						i++;
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
                    if (strb.ToString() == "true")
                    {
                        tokens.Add(new Token() { type = Token.BOOL_LIT, value = true });
                    }
                    else if (strb.ToString() == "false")
                    {
                        tokens.Add(new Token() { type = Token.BYTE_LIT, value = false });
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
                else
                {
                    tokens.Add(new Token() { type = Token.SYMBOL, value = code[i] });
                }
                i++;
            }
        }
	}
}

