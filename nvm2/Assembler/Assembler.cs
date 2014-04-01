using System;
using System.Text;
using System.Collections.Generic;

namespace nvm2
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

		public object value;
		public byte tokentype;
	}


	public class Assembler
	{
		string code;
		List<Token> tokens = new Token();

		public Assembler (string Code)
		{
			code = Code;
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
                        tokens.Add(new Token() { type = Token.FLOAT_LIT, val = f });
                    }
                    else if (code[i + 1] == 'x')
                    {
                        i += 2;
                        strb.Clear();
                        strb.Append(code[i]);
                        i++;
                        strb.Append(code[i]);
                        byte b = byte.Parse(strb.ToString(), System.Globalization.NumberStyles.HexNumber);
                        tokens.Add(new Token() { type = Token.BYTE_LIT, val = b });
                    }
                    else
                    {
                        string str = strb.ToString();
                        int n = Convert.ToInt32(str);
                        tokens.Add(new Token() { type = Token.INT_LIT, val = n });
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
                        tokens.Add(new Token() { type = Token.BOOL_LIT, val = true });
                    }
                    else if (strb.ToString() == "false")
                    {
                        tokens.Add(new Token() { type = Token.BYTE_LIT, val = false });
                    }
                    else
                    {
                        tokens.Add(new Token() { type = Token.WORD, val = strb.ToString() });
                    }
                    i--;
                }
                else if (code[i] == '"')
                {
                    bool escaped = false;
                    bool r = true;
                    StringBuilder strb = new StringBuilder();
                    i++;
                    r = (code[i] != '"');
                    while (r)
                    {
                        if (escaped && code[i] == 'n')
                        {
                            strb.Append("\\");
                        }
                        escaped = false;
                        if (code[i] == '\\')
                        {
                            escaped = true;
                        }
                        else
                        {
                            strb.Append(code[i]);
                        }
                        i++;
                        if (!escaped)
                        {
                            r = (code[i] != '"');
                        }
                    }
                    tokens.Add(new Token() { type = Token.STRING_LIT, val = strb.ToString() });
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
                    tokens.Add(new Token() { type = Token.SYMBOL, val = code[i] });
                }
                i++;
            }
        }

		public void Assemble() {
			foreach (Token token in tokens) {
				
			}
		}
	}
}

