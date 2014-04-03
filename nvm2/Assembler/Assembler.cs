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
        public const byte CHAR_LIT = 8;

		public object value;
		public byte type;
	}

	public class Assembler
	{
		string code;
		List<Token> tokens = new List<Token>();
        List<byte> program = new List<byte>();
        int i = 0;

        Dictionary<string,uint> symbolTable = new Dictionary<string,uint>();
        List<Tuple<int,string>> callTable = new List<Tuple<int,string>>();

		public Assembler (string Code)
		{
			code = Code;
		}

        public byte[] GetProgram() {
            return program.ToArray();
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

        byte register_to_byte(string register) {
            switch (register) {
                case "A": return Registers.A;
                case "B": return Registers.B;
                case "E": return Registers.E;
                case "DA": return Registers.DA;
                case "DB": return Registers.DB;
                case "AX": return Registers.AX;
                case "BX": return Registers.BX;
                case "EX": return Registers.EX;
                case "EAX": return Registers.EAX;
                case "EBX": return Registers.EBX;
                case "EEX": return Registers.EEX;
                case "IP": return Registers.IP;
                default: return 255;
            }
        }

		public void Assemble() {
            for(i = 0; i < tokens.Count; i++) {
                if (tokens[i].type == Token.WORD) {
                    string word = tokens[i].value as string;
                    if (word == "LD") {
                        i++;
                        program.Add(OpCodes.LD);
                        if(tokens[i].type != Token.WORD) {
                            throw new Exception("Unexpected token, expected word, got: " + tokens[i].type);
                        }
                        string register = tokens[i].value as string;
                        byte regB = register_to_byte(register);
                        i++;
                        if (register == "A" || register == "B" || register == "E") {
                            program.Add(BaseTypes.BYTE);
                            program.Add(regB);
                            AssembleByte();
                        } else if (register == "AX" || register == "BX" || register == "EX") {
                            program.Add(BaseTypes.INT);
                            program.Add(regB);
                            AssembleInt();
                        } else if (register == "DA" || register == "DB") {
                            program.Add(BaseTypes.UINT);
                            program.Add(regB);
                            AssembleUInt();
                        } else if (register == "EAX" || register == "EBX" || register == "EEX") {
                            program.Add(BaseTypes.FLOAT);
                            program.Add(regB);
                            AssembleFloat();
                        }
                    } else if (word == "MV") {
                        i++;
                        program.Add(OpCodes.MV);
                        string registerA = tokens[i].value as string;
                        i++;
                        string registerB = tokens[i].value as string;
                        if (registerA == "A" || registerA == "B" || registerA == "E") {
                            if(registerB == "A" || registerB == "B" || registerB == "E") {
                                program.Add(register_to_byte(registerA));
                                program.Add(register_to_byte(registerB));
                            } else {
                                throw new Exception("Can only move 8 bit registers to 8 bit registers!");
                            }
                        } else if (registerA == "AX" || registerA == "BX" || registerA == "EX") {
                            if(registerB == "AX" || registerB == "BX" || registerB == "EX") {
                                program.Add(register_to_byte(registerA));
                                program.Add(register_to_byte(registerB));
                            } else {
                                throw new Exception("Can only move 32 bit registers to 32 bit registers!");
                            }
                        } else if (registerA == "DA" || registerA == "DB") {
                            if(registerB == "DA" || registerB == "DB") {
                                program.Add(register_to_byte(registerA));
                                program.Add(register_to_byte(registerB));
                            } else {
                                throw new Exception("Can only move unsigned 32 bit registers to unsigned 32 bit registers!");
                            }
                        } else if (registerA == "EAX" || registerA == "EBX" || registerA == "EEX") {
                            if(registerB == "EAX" || registerB == "EBX" || registerB == "EEX") {
                                program.Add(register_to_byte(registerA));
                                program.Add(register_to_byte(registerB));
                            } else {
                                throw new Exception("Can only move float registers to float registers!");
                            }
                        }
                    } else if (word == "INT") {
                        i++;
                        byte n = 0;
                        if(tokens[i].type == Token.BYTE_LIT) {
                            n = (byte)tokens[i].value;
                        } else if (tokens[i].type == Token.INT_LIT) {
                            n = (byte)(int)tokens[i].value;
                        }
                        program.Add(OpCodes.INT);
                        program.Add(n);
                    } 

					else if (tokens[i].value as string == "ALLOC_PAGE") {
                        program.Add(OpCodes.ALLOC_PAGE);
                    } else if (tokens[i].value as string == "PUSHB") {
						program.Add(OpCodes.PUSHB); i++;
						AssembleByte();
                    } else if (tokens[i].value as string == "PUSHI") {
						program.Add(OpCodes.PUSHI); i++;
						AssembleInt();
                    } else if (tokens[i].value as string == "PUSHUI") {
						program.Add(OpCodes.PUSHUI); i++;
						AssembleUInt();
                    } else if (tokens[i].value as string == "PUSHR") {
						program.Add(OpCodes.PUSHR); i++;
						AssembleFloat();
                    } else if (tokens[i].value as string == "LODS") {
                        program.Add(OpCodes.LODS);
					} else if (tokens[i].value as string == "POPB") {
                        program.Add(OpCodes.POPB);
                    } else if (tokens[i].value as string == "POPI") {
                        program.Add(OpCodes.POPI);
                    } else if (tokens[i].value as string == "POPUI") {
                        program.Add(OpCodes.POPUI);
                    } else if (tokens[i].value as string == "POPF") {
                        program.Add(OpCodes.POPF);
                    } else if (tokens[i].value as string == "POPS") {
                        program.Add(OpCodes.POPS);
                    } else if (tokens[i].value as string == "READB") {
                        program.Add(OpCodes.READB);
                    } else if (tokens[i].value as string == "READI") {
                        program.Add(OpCodes.READI);
                    } else if (tokens[i].value as string == "READUI") {
                        program.Add(OpCodes.READUI);
                    } else if (tokens[i].value as string == "READF") {
                        program.Add(OpCodes.READF);
                    } else if (tokens[i].value as string == "WRITEB") {
                        program.Add(OpCodes.WRITEB);
                    } else if (tokens[i].value as string == "WRITEI") {
                        program.Add(OpCodes.WRITEI);
                    } else if (tokens[i].value as string == "WRITEUI") {
                        program.Add(OpCodes.WRITEUI);
                    } else if (tokens[i].value as string == "WRITEF") {
                        program.Add(OpCodes.WRITEF);
                    } else if (tokens[i].value as string == "ADD") {
                        program.Add(OpCodes.ADD); i++;
                        AssembleType();
                    } else if (tokens[i].value as string == "SUB") {
                        program.Add(OpCodes.SUB); i++;
                        AssembleType();
                    } else if (tokens[i].value as string == "MUL") {
                        program.Add(OpCodes.MUL); i++;
                        AssembleType();
                    } else if (tokens[i].value as string == "DIV") {
                        program.Add(OpCodes.DIV); i++;
                        AssembleType();
                    } else if (tokens[i].value as string == "MOD") {
                        program.Add(OpCodes.MOD); i++;
                        AssembleType();
                    } else if (tokens[i].value as string == "POW") {
                        program.Add(OpCodes.POW); i++;
                        AssembleType();
                    } else if (tokens[i].value as string == "SQRT") {
                        program.Add(OpCodes.SQRT); i++;
                        AssembleType();
                    } else if (tokens[i].value as string == "JMP") {
						program.Add(OpCodes.JMP); i++;
						AssembleUInt();
                    } else if (tokens[i].value as string == "CALL") {
						program.Add(OpCodes.CALL); i++;
						AssembleUInt();
                    } else if (tokens[i].value as string == "RET") {
                        program.Add(OpCodes.RET);
                    } else if (tokens[i].value as string == "JMPR") {
                        program.Add(OpCodes.JMPR);
                    } else if (tokens[i].value as string == "CALLR") {
                        program.Add(OpCodes.CALLR);
                    } else if (tokens[i].value as string == "MALLOC") {
                        program.Add(OpCodes.MALLOC);
                    } else if (tokens[i].value as string == "FREE") {
                        program.Add(OpCodes.FREE);
                    } else if (tokens[i].value as string == "ALLOC_PAGETABLE") {
						program.Add(OpCodes.ALLOC_PAGETABLE);
					} else if (tokens[i].value as string == "ALLOC_PAGE") {
                        program.Add(OpCodes.ALLOC_PAGE);
                    } else if (tokens[i].value as string == "CALL_PAGE") {
                        program.Add(OpCodes.CALL_PAGE);
                    } else if (tokens[i].value as string == "RET_PAGE") {
                        program.Add(OpCodes.RET_PAGE);
                    } else if (tokens[i].value as string == "FREE_PAGETABLE") {
                        program.Add(OpCodes.FREE_PAGETABLE);
                    } else if (tokens[i].value as string == "GET_PAGE_TABLE_SIZE") {
                        program.Add(OpCodes.GET_PAGE_TABLE_SIZE);
                    } else if (tokens[i].value as string == "GET_PAGE_ID") {
                        program.Add(OpCodes.GET_PAGE_ID);
                    } else if (tokens[i].value as string == "SET_PAGE_STACK") {
                        program.Add(OpCodes.SET_PAGE_STACK);
                    } else if (tokens[i].value as string == "SET_PAGE_HEAP") {
                        program.Add(OpCodes.SET_PAGE_HEAP);
                    } else if (tokens[i].value as string == "PAGE_INIT_MEM") {
                        program.Add(OpCodes.PAGE_INIT_MEM);
                    } else if (tokens[i].value as string == "PAGE_VAT") {
                        program.Add(OpCodes.PAGE_VAT);
					} else if (tokens[i].value as string == "REVERSE_VAT") {
                        program.Add(OpCodes.REVERSE_VAT);
					} else if (tokens[i].value as string == "PAGE_INIT_MEM") {
                        program.Add(OpCodes.PAGE_INIT_MEM);
					} else if (tokens[i].value as string == "HALT") {
                        program.Add(OpCodes.HALT);
					} else if (tokens[i].value as string == "DMPPT") {
                        program.Add(OpCodes.DMPPT);
					} else if (tokens[i].value as string == "DMPFL") {
                        program.Add(OpCodes.DMPFL);
					} else if (tokens[i].value as string == "OCDSM") {
                        program.Add(OpCodes.OCDSM);
					} else if (tokens[i].value as string == "JE") {
                        program.Add(OpCodes.JE); i++;
                        AssembleUInt();
                    } else if (tokens[i].value as string == "JN") {
                        program.Add(OpCodes.JN); i++;
                        AssembleUInt();
                    } else if (tokens[i].value as string == "LT") {
                        program.Add(OpCodes.LT); i++;
                        AssembleType();
                    } else if (tokens[i].value as string == "LE") {
                        program.Add(OpCodes.LE); i++;
                        AssembleType();
                    } else if (tokens[i].value as string == "EQ") {
                        program.Add(OpCodes.EQ); i++;
                        AssembleType();
                    } else if (tokens[i].value as string == "NE") {
                        program.Add(OpCodes.NE); i++;
                        AssembleType();
                    } else if (tokens[i].value as string == "GE") {
                        program.Add(OpCodes.GE); i++;
                        AssembleType();
                    } else if (tokens[i].value as string == "GT") {
                        program.Add(OpCodes.GT); i++;
                        AssembleType();
                    } else if (tokens[i].value as string == "JER") {
                        program.Add(OpCodes.JER);
                    } else if (tokens[i].value as string == "JNR") {
                        program.Add(OpCodes.JNR);
                    }

					else if (tokens[i + 1].type == Token.SYMBOL && (char)tokens[i + 1].value == ':') {
                        symbolTable.Add(tokens[i].value as string, (uint)program.Count);
                    } else if (tokens[i].value as string == "DB") {
                         i++;
                         if (tokens[i].type != Token.BYTE_LIT) {
                            throw new Exception("DB must be succeded by a byte!");
                         }
                         program.Add((byte)tokens[i].value);
                    } else if (tokens[i].value as string == "DI") {
                         i++;
                         if (tokens[i].type != Token.INT_LIT) {
                            throw new Exception("DI must be succeded by an int!");
                         }
                         program.AddRange(BitConverter.GetBytes((int)tokens[i].value));
                    } else if (tokens[i].value as string == "DUI") {
                         i++;
                         if (tokens[i].type != Token.INT_LIT) {
                            throw new Exception("DUI must be succeded by an unsigned int!");
                         }
                         program.AddRange(BitConverter.GetBytes((uint)(int)tokens[i].value));
                    } else if (tokens[i].value as string == "DF") {
                         i++;
                         if (tokens[i].type != Token.FLOAT_LIT) {
                            throw new Exception("DF must be succeded by a flaot!");
                         }
                         program.AddRange(BitConverter.GetBytes((float)tokens[i].value));
                    } else if (tokens[i].value as string == "DS") {
                         i++;
                         if (tokens[i].type != Token.STRING_LIT) {
                            throw new Exception("DS must be succeded by a string!");
                         }
                         string str = tokens[i].value as string;
                         byte[] bytes = new byte[str.Length + 4];
                         for(int j = 0; j < str.Length; j++) {
                            bytes[j + 4] = (byte)str[j];
                         }
                         byte[] len = BitConverter.GetBytes(str.Length);
                         bytes[0] = len[0];
                         bytes[1] = len[1];
                         bytes[2] = len[2];
                         bytes[3] = len[3];
                         program.AddRange(bytes);
                    }
                }
            }
            InsertCalls();
		}

        public void AssembleByte() {
            if(tokens[i].type == Token.BYTE_LIT) {
                program.Add((byte)tokens[i].value);
            } else if (tokens[i].type == Token.INT_LIT) {
                program.Add((byte)(int)tokens[i].value);
            } else if (tokens[i].type == Token.CHAR_LIT) {
                program.Add((byte)(char)tokens[i].value);
            } else {
                throw new Exception("Unexpected token, expected byte, got: " + tokens[i].type);
            }
        }

        public void AssembleInt() {
            if  (tokens[i].type == Token.INT_LIT) {
                program.AddRange(BitConverter.GetBytes((int)tokens[i].value));
            } else if (tokens[i].type == Token.BYTE_LIT) {
                program.AddRange(BitConverter.GetBytes((int)(byte)tokens[i].value));
            } else {
                throw new Exception("Unexpected token, expected int, got: " + tokens[i].type);
            }
        }

        public void AssembleUInt() {
            if (tokens[i].type == Token.BYTE_LIT) {
                program.AddRange(BitConverter.GetBytes((uint)(byte)tokens[i].value));
            } else if (tokens[i].type == Token.INT_LIT) {
                program.AddRange(BitConverter.GetBytes((uint)(int)tokens[i].value));
            } else if (tokens[i].type == Token.SYMBOL && (char)tokens[i].value == '[') {
                i++;
                string name = tokens[i].value as string;
                if(symbolTable.ContainsKey(name)) {
                    program.AddRange(BitConverter.GetBytes(symbolTable[name]));
                } else {
                    callTable.Add(new Tuple<int,string>(program.Count,name));
                    program.AddRange(new byte[] { 0,0,0,0 });
                }
            } else {
                throw new Exception("Unexpected token, expected uint, got: " + tokens[i].type);
            }
        }

        public void AssembleFloat() {
            if (tokens[i].type == Token.BYTE_LIT || tokens[i].type == Token.INT_LIT || tokens[i].type == Token.FLOAT_LIT) {
                program.AddRange(BitConverter.GetBytes((float)tokens[i].value));
            } else {
                throw new Exception("Unexpected token, expected float, got: " + tokens[i].type);
            }
        }

        public void AssembleType() {
            if(tokens[i].type == Token.WORD) {
                string word = tokens[i].value as string;
                switch(word) {
                    case "BYTE":
                        program.Add(BaseTypes.BYTE);
                        break;
                    case "INT":
                        program.Add(BaseTypes.INT);
                        break;
                    case "UINT":
                        program.Add(BaseTypes.UINT);
                        break;
                    case "FLOAT":
                        program.Add(BaseTypes.FLOAT);
                        break;
                }
            }
        }

        public void InsertCalls() {
            foreach(Tuple<int,string> key in callTable) {
                if(symbolTable.ContainsKey(key.Item2)) {
                    byte[] address = BitConverter.GetBytes(symbolTable[key.Item2]);
                    program[key.Item1 + 0] = address[0];
                    program[key.Item1 + 1] = address[1];
                    program[key.Item1 + 2] = address[2];
                    program[key.Item1 + 3] = address[3];
                } else {
                    throw new Exception("Symbol: " + key.Item2 + " does not exist in program!");
                }
            }
        }
	}
}