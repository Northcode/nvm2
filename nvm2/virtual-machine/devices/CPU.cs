using System;

namespace nvm2
{
	static class OpCodes
	{
		//Register opcodes
		public const byte LD = 0;
		public const byte MV = 1;
		//Stack opcodes
		public const byte PUSHB = 2;
		public const byte PUSHI = 3;
		public const byte PUSHUI = 4;
		public const byte PUSHF = 5;
		public const byte PUSHR = 6;
		public const byte POPB = 7;
		public const byte POPI = 8;
		public const byte POPUI = 9;
		public const byte POPF = 10;
		public const byte LODS = 11;
		public const byte POPS = 12;
		//RAM opcodes
		public const byte READB = 13;
		public const byte READI = 14;
		public const byte READUI = 15;
		public const byte READF = 16;
		public const byte WRITEB = 17;
		public const byte WRITEI = 18;
		public const byte WRITEUI = 19;
		public const byte WRITEF = 20;
		//JUMP opcodes
		public const byte JMP = 21;
		public const byte CALL = 22;
		public const byte RET = 23;
		public const byte JMPR = 24;
		public const byte CALLR = 25;
		//Math opcodes
		public const byte ADD = 26;
		public const byte SUB = 27;
		public const byte MUL = 28;
		public const byte DIV = 29;
		public const byte MOD = 30;
		public const byte POW = 31;
		public const byte SQRT = 32;
		//Memory opcodes
		public const byte MALLOC = 33;
		public const byte FREE = 34;
		//Paging opcodes
		public const byte ALLOC_PAGETABLE = 35;
		public const byte ALLOC_PAGE = 36;
		public const byte CALL_PAGE = 37;
		public const byte RET_PAGE = 38;
		public const byte FREE_PAGETABLE = 39;
		public const byte SET_PAGE_STACK = 40;
		public const byte SET_PAGE_HEAP = 41;
		public const byte GET_PAGE_TABLE_SIZE = 42;
		public const byte GET_PAGE_ID = 43;
		public const byte PAGE_VAT = 44;
		public const byte REVERSE_VAT = 45;
		public const byte PAGE_INIT_MEM = 46;
		//Interupts!
		public const byte INT = 47; //hardware interupt
		public const byte SWI = 48; //software interupt
		public const byte RSWI = 49; //register software interupt (DA)
		//debug commands
		public const byte HALT = 50;
		public const byte DMPPT = 51;
		public const byte DMPFL = 52;
		public const byte OCDSM = 53;
		//conditional opcodes
		public const byte JE = 54;
		public const byte JN = 55;
		public const byte LT = 56;
		public const byte LE = 57;
		public const byte EQ = 58;
		public const byte GE = 59;
		public const byte GT = 60;
		public const byte NE = 61;
		public const byte JER = 62;
		public const byte JNR = 63;
		//string opcodes
		public const byte STRLEN = 64;
		public const byte STRCMP = 65;
		public const byte STRSUB = 66;
	}

	static class Registers
	{
		public const byte A = 0;
		public const byte B = 1;
		public const byte E = 2;
		public const byte DA = 3;
		public const byte DB = 4;
		public const byte AX = 5;
		public const byte BX = 6;
		public const byte EX = 7;
		public const byte EAX = 8;
		public const byte EBX = 9;
		public const byte EEX = 10;
		public const byte IP = 11;
		public const byte CP = 12;
	}

	static class BaseTypes
	{
		public const byte BYTE = 0;
		public const byte INT = 1;
		public const byte UINT = 2;
		public const byte FLOAT = 3;
		public const byte STRING = 4;
	}

	interface HardwareInterupt
	{
		void Run(vm machine);
	}

	class CoreInterupt : HardwareInterupt
	{
		//operation modes
		public const byte TERMINATE = 0;
		public const byte BREAK = 1;
		public const byte LOAD_DEVICE = 2;
		public const byte UNLOAD_DEVICE = 3;
		public const byte LOAD_PROGRAM = 4;
		public const byte GET_ROM_DEVICE = 5;

		public void Run (vm machine)
		{
			if (machine.A == TERMINATE) {
				machine.RN = false;
			} else if (machine.A == BREAK) {
				machine.BRK = true;
			} else if (machine.A == UNLOAD_DEVICE) {
				int device = machine.AX;
				machine.UnloadDevice(device);
			} else if (machine.A == LOAD_PROGRAM) {
				int device = machine.AX;
				StorageDevice disk = (StorageDevice)machine.GetDevice(device);
				machine.LoadProgram(disk.GetData());
			} else if (machine.A == GET_ROM_DEVICE) {
				for (int i = 0; i < machine.NumberOfDevices(); i++) {
					VMDevice device = machine.GetDevice(i);
					if(device is VirtualROMDisk) {
						machine.EX = i;
						if(machine.AX <= 0) {
							return;
						}
						machine.AX--;
					}
				}
				throw new Exception("No ROM devices found!");
			}
		}
	}

	class TerminalInterupt : HardwareInterupt
	{
		public const byte PRINTCH = 0;
		public const byte PRINTSTR = 1;
		public const byte PRINTINT = 2;
		public const byte PRINTFLT = 3;
		public const byte READCH = 4;
		public const byte READSTR = 5;
		public const byte READINT = 6;
		public const byte READFLT = 7;
		public const byte PRINTB = 8;
		public const byte READB = 9;
		public const byte CLEAR = 10;
		public const byte CHFGCOLOR = 11;
		public const byte CHBGCOLOR = 12;

		public void Run (vm machine)
		{
			if (machine.A == PRINTCH) {
				Console.Write ((char)machine.B);
			} else if (machine.A == PRINTSTR) {
				string str = machine.pager.PopString (machine.CR3);
				Console.Write (str);
			} else if (machine.A == PRINTINT) {
				Console.Write (machine.AX);
			} else if (machine.A == PRINTFLT) {
				Console.Write (machine.EAX);
			} else if (machine.A == READCH) {
				machine.E = (byte)Console.Read ();
			} else if (machine.A == READSTR) {
				string str = Console.ReadLine ();
				machine.pager.Push (str, machine.CR3);
			} else if (machine.A == READINT) {
				machine.AX = Convert.ToInt32 (Console.ReadLine ());
			} else if (machine.A == READFLT) {
				machine.EAX = (float)Convert.ToInt32 (Console.ReadLine ());
			} else if (machine.A == PRINTB) {
				Console.Write (machine.B);
			} else if (machine.A == READB) {
				machine.E = Convert.ToByte (Console.ReadLine ());
			} else if (machine.A == CLEAR) {
				Console.Clear ();
			} else if (machine.A == CHFGCOLOR) {

			}
		}
	}

	class HDIInterupt : HardwareInterupt
	{
		public const byte GETDIR = 0;
		public const byte CHANGEDIR = 1;
		public const byte CREATEDIR = 2;
		public const byte CREATEFILE = 3;
		public const byte OPENFILE = 4;
		public const byte CLOSEFILE = 5;
		public const byte READBYTES = 6;
		public const byte WRITEBYTES = 7;
		public const byte READALLBYTES = 8;
		public const byte LOADPROGRAM = 9;
		public const byte DIREXISTS = 10;
		public const byte FILEEXISTS = 11;

		public HDIInterupt() {
		}

		public void Run(vm machine) {
			if(machine.A == GETDIR)	{
				machine.pager.Push(machine.hdi.GetWorkingDirectory(),machine.CR3);
			} else if (machine.A == CHANGEDIR) {
				string path = machine.pager.PopString(machine.CR3);
				machine.hdi.ChangeDirectory(path);
			} else if (machine.A == CREATEDIR) {
				string dir = machine.pager.PopString(machine.CR3);
				machine.hdi.CreateDirectory(dir);
			} else if (machine.A == CREATEFILE) {
				string filename = machine.pager.PopString(machine.CR3);
				machine.hdi.CreateFile(filename);
			} else if (machine.A == OPENFILE) {
				string filename = machine.pager.PopString(machine.CR3);
				machine.hdi.OpenFile(filename);
			} else if (machine.A == CLOSEFILE) {
				machine.hdi.CloseFile();
			} else if (machine.A == READBYTES) {
				int len = machine.AX;
				uint addr = machine.DA;
				byte[] data = machine.hdi.ReadBytes(len);
				if(machine.pager.checkVAT(addr,(uint)len,machine.CR3)) {
				} else {
				}
			} else if (machine.A == WRITEBYTES) {
			} else if (machine.A == LOADPROGRAM) {
				byte[] program = machine.hdi.ReadAllBytes();
				machine.LoadProgram(program);
			} else if (machine.A == DIREXISTS) {
				string name = machine.pager.PopString(machine.CR3);
				machine.CP = machine.hdi.DirectoryExists(name);
			} else if (machine.A == FILEEXISTS) {
				string name = machine.pager.PopString(machine.CR3);
				machine.CP = machine.hdi.FileExists(name);
			}
		}
	}

	class CPU : ComputeDevice
	{
		vm machine;

		HardwareInterupt[] hardwareinterupts;

		public CPU (vm Machine)
		{
			this.machine = Machine;

			hardwareinterupts = new HardwareInterupt[] {
				new CoreInterupt(),
				new TerminalInterupt(),
				new HDIInterupt(),
			};
		}

		public void Run()
		{
			while (machine.RN) {
				if(machine.BRK) {
					Console.Write("[INFO] CPU HALTED, PRESS ENTER TO RESUME");
					Console.ReadLine();
					machine.BRK = false;
				}
				byte opcode = NextByte();
				machine.IP++;
				ExecuteOpcode(opcode);
			}
		}

		public void ExecuteOpcode(byte opcode)
		{
			if(machine.DSM)
				Console.WriteLine("[" + machine.IP + "] : " + DASM(opcode));
			switch (opcode){
			case OpCodes.LD:
				ExecuteLD();
				break;
			case OpCodes.MV:
				ExecuteMV();
				break;
			case OpCodes.PUSHB:
				PushByte();
				break;
			case OpCodes.PUSHI:
				PushInt();
				break;
			case OpCodes.PUSHUI:
				PushUInt();
				break;
			case OpCodes.PUSHF:
				PushFloat();
				break;
			case OpCodes.PUSHR:
				PushReg();
				break;
			case OpCodes.POPB:
				PopByte();
				break;
			case OpCodes.POPI:
				PopInt();
				break;
			case OpCodes.POPUI:
				PopUInt();
				break;
			case OpCodes.POPF:
				PopFloat();
				break;
			case OpCodes.LODS:
				LoadString();
				break;
			case OpCodes.POPS:
				PopString();
				break;
			case OpCodes.READB:
				ReadB();
				break;
			case OpCodes.READI:
				ReadI();
				break;
			case OpCodes.READUI:
				ReadUI();
				break;
			case OpCodes.READF:
				ReadF();
				break;
			case OpCodes.WRITEB:
				WriteB();
				break;
			case OpCodes.WRITEI:
				WriteI();
				break;
			case OpCodes.WRITEUI:
				WriteUI();
				break;
			case OpCodes.WRITEF:
				WriteF();
				break;
			case OpCodes.ADD:
				Add();
				break;
			case OpCodes.SUB:
				Sub();
				break;
			case OpCodes.MUL:
				Mul();
				break;
			case OpCodes.DIV:
				Div();
				break;
			case OpCodes.MOD:
				Mod();
				break;
			case OpCodes.POW:
				Pow();
				break;
			case OpCodes.SQRT:
				Sqrt();
				break;
			case OpCodes.JMP:
				Jmp();
				break;
			case OpCodes.CALL:
				Call();
				break;
			case OpCodes.RET:
				Ret();
				break;
			case OpCodes.JMPR:
				JmpR();
				break;
			case OpCodes.CALLR:
				CallR();
				break;
			case OpCodes.MALLOC:
				Malloc();
				break;
			case OpCodes.FREE:
				Free();
				break;
			case OpCodes.INT:
				Int();
				break;
			case OpCodes.ALLOC_PAGETABLE:
				AllocPageTable();
				break;
			case OpCodes.ALLOC_PAGE:
				AllocPage();
				break;
			case OpCodes.CALL_PAGE:
				CallPage();
				break;
			case OpCodes.RET_PAGE:
				RetPage();
				break;
			case OpCodes.FREE_PAGETABLE:
				FreePageTable();
				break;
			case OpCodes.SET_PAGE_STACK:
				PageSetStack();
				break;
			case OpCodes.SET_PAGE_HEAP:
				PageSetHeap();
				break;
			case OpCodes.PAGE_INIT_MEM:
				PageInitMem();
				break;
			case OpCodes.GET_PAGE_TABLE_SIZE:
				GetPageTableSize();
				break;
			case OpCodes.GET_PAGE_ID:
				GetPageId();
				break;
			case OpCodes.PAGE_VAT:
				PageVAT();
				break;
			case OpCodes.REVERSE_VAT:
				ReverseVAT();
				break;
			case OpCodes.HALT:
				machine.BRK = true;
				break;
			case OpCodes.DMPPT:
				DmpPt();
				break;
			case OpCodes.DMPFL:
				DmpFl();
				break;
			case OpCodes.OCDSM:
				OcDsm();
				break;
			case OpCodes.JE:
				Je();
				break;
			case OpCodes.JN:
				Jn();
				break;
			case OpCodes.LT:
				Lt();
				break;
			case OpCodes.LE:
				Le();
				break;
			case OpCodes.EQ:
				Eq();
				break;
			case OpCodes.GE:
				Ge();
				break;
			case OpCodes.GT:
				Gt();
				break;
			case OpCodes.NE:
				Ne();
				break;
			case OpCodes.JER:
				JeR();
				break;
			case OpCodes.JNR:
				JnR();
				break;
			case OpCodes.STRLEN:
				StrLen();
				break;
			case OpCodes.STRCMP:
				StrCmp();
				break;
			case OpCodes.STRSUB:
				StrSub();
				break;
			default:
				Nop();
				break;
			}
		}

		string DASM (byte opcode)
		{
			switch (opcode){
			case OpCodes.LD:
				return "LD";
				
			case OpCodes.MV:
				return "MV";
				
			case OpCodes.PUSHB:
				return "PUSHB";
				
			case OpCodes.PUSHI:
				return "PUSHI";
				
			case OpCodes.PUSHUI:
				return "PUSHUI";
				
			case OpCodes.PUSHF:
				return "PUSHF";
				
			case OpCodes.PUSHR:
				return "PUSHR";
				
			case OpCodes.POPB:
				return "POPB";
				
			case OpCodes.POPI:
				return "POPI";
				
			case OpCodes.POPUI:
				return "POPUI";
				
			case OpCodes.POPF:
				return "POPF";
				
			case OpCodes.LODS:
				return "LODS";
				
			case OpCodes.POPS:
				return "POPS";
				
			case OpCodes.READB:
				return "READB";
				
			case OpCodes.READI:
				return "READI";
				
			case OpCodes.READUI:
				return "READUI";
				
			case OpCodes.READF:
				return "READF";
				
			case OpCodes.WRITEB:
				return "WRITEB";
				
			case OpCodes.WRITEI:
				return "WRITEI";
				
			case OpCodes.WRITEUI:
				return "WRITEUI";
				
			case OpCodes.WRITEF:
				return "WRITEF";
			
			case OpCodes.ADD:
				return "ADD";
			case OpCodes.SUB:
				return "SUB";
			case OpCodes.MUL:
				return "MUL";
			case OpCodes.DIV:
				return "DIV";
			case OpCodes.MOD:
				return "MOD";
			case OpCodes.POW:
				return "POW";
			case OpCodes.SQRT:
				return "SQRT";
				
			case OpCodes.JMP:
				return "JMP";
				
			case OpCodes.CALL:
				return "CALL";
				
			case OpCodes.RET:
				return "RET";
				
			case OpCodes.JMPR:
				return "JMPR";
				
			case OpCodes.CALLR:
				return "CALLR";
				
			case OpCodes.MALLOC:
				return "MALLOC";
				
			case OpCodes.FREE:
				return "FREE";
				
			case OpCodes.INT:
				return "INT";
				
			case OpCodes.ALLOC_PAGETABLE:
				return "ALLOC_PAGETABLE";
				
			case OpCodes.ALLOC_PAGE:
				return "ALLOC_PAGE";
				
			case OpCodes.CALL_PAGE:
				return "CALL_PAGE";
				
			case OpCodes.RET_PAGE:
				return "RET_PAGE";
				
			case OpCodes.FREE_PAGETABLE:
				return "FREE_PAGETABLE";
				
			case OpCodes.SET_PAGE_STACK:
				return "SET_PAGE_STACK";
				
			case OpCodes.SET_PAGE_HEAP:
				return "SET_PAGE_HEAP";
				
			case OpCodes.PAGE_INIT_MEM:
				return "PAGE_INIT_MEM";
				
			case OpCodes.GET_PAGE_TABLE_SIZE:
				return "GET_PAGE_TABLE_SIZE";
				
			case OpCodes.GET_PAGE_ID:
				return "GET_PAGE_ID";
				
			case OpCodes.PAGE_VAT:
				return "PAGE_VAT";
				
			case OpCodes.REVERSE_VAT:
				return "REVERSE_VAT";
				
			case OpCodes.HALT:
				return "HALT";
				
			case OpCodes.DMPPT:
				return "DMPPT";
			
			case OpCodes.DMPFL:
				return "DMPFL";
			
			case OpCodes.JE:
				return "JE";
			case OpCodes.JN:
				return "JN";
			case OpCodes.LT:
				return "LT";
			case OpCodes.LE:
				return "LE";
			case OpCodes.EQ:
				return "EQ";
			case OpCodes.NE:
				return "NE";
			case OpCodes.GE:
				return "GE";
			case OpCodes.GT:
				return "GT";
			case OpCodes.JER:
				return "JER";
			case OpCodes.JNR:
				return "JNR";
			case OpCodes.STRLEN:
				return "STRLEN";
			case OpCodes.STRCMP:
				return "STRCMP";
			case OpCodes.STRSUB:
				return "STRSUB";

			default:
				return "";
			}
		}

		void Nop ()
		{
		}

		byte NextByte() {
			return machine.ram.Read(machine.pager.getVAT(machine.IP,machine.CR3));
		}
		int NextInt() {
			return machine.ram.ReadInt(machine.pager.getVAT(machine.IP,machine.CR3));
		}
		uint NextUInt() {
			return machine.ram.ReadUInt(machine.pager.getVAT(machine.IP,machine.CR3));
		}
		float NextFloat() {
			return machine.ram.ReadFloat(machine.pager.getVAT(machine.IP,machine.CR3));
		}
		string NextString() {
			return machine.ram.ReadString(machine.pager.getVAT(machine.IP,machine.CR3));
		}

		void ExecuteLD() {
			byte type = NextByte();
			machine.IP++;
			byte reg = NextByte();
			machine.IP++;
			if(type == BaseTypes.BYTE) {
				byte val = NextByte();
				machine.IP++;
				LDREG(reg,val);
			} else if (type == BaseTypes.INT) {
				int val = NextInt();
				machine.IP += 4;
				LDREG(reg,val);
			} else if (type == BaseTypes.UINT) {
				uint val = NextUInt();
				machine.IP += 4;
				LDREG(reg,val);
			} else if (type == BaseTypes.FLOAT) {
				float val = NextFloat();
				machine.IP += 4;
				LDREG(reg,val);
			}
		}

		#region register_loading

		public void LDREG(byte reg, byte val) {
			switch (reg) {
			case Registers.A:
				machine.A = val;
				break;
			case Registers.B:
				machine.B = val;
				break;
			case Registers.E:
				machine.E = val;
				break;
			case Registers.CP:
				machine.CP = (val > 0);
				break;
			default:
				throw new Exception("Invalid register for byte!");
			}
		}

		public void LDREG(byte reg, int val) {
			switch (reg) {
				case Registers.AX:
					machine.AX = val;
					break;
				case Registers.BX:
					machine.BX = val;
					break;
				case Registers.EX:
					machine.EX = val;
					break;
				default:
					throw new Exception("Invalid register for int!");
			}
		}

		public void LDREG(byte reg, float val) {
			switch (reg) {
				case Registers.EAX:
					machine.EAX = val;
					break;
				case Registers.EBX:
					machine.EBX = val;
					break;
				case Registers.EEX:
					machine.EEX = val;
					break;
				default:
					throw new Exception("Invalid register for float!");
			}
		}
		
		public void LDREG(byte reg, uint val) {
			switch (reg) {
				case Registers.DA:
					machine.DA = val;
					break;
				case Registers.DB:
					machine.DB = val;
					break;
				case Registers.IP:
					machine.IP = val;
					break;
				default:
					throw new Exception("Invalid register for uint!");
			}
		}

		#endregion

		void ExecuteMV() {
			byte regD = NextByte();
			machine.IP++;
			byte regF = NextByte();
			machine.IP++;
			switch (regF) {
				case Registers.A:
					LDREG(regD,machine.A);
					break;
				case Registers.B:
					LDREG(regD,machine.B);
					break;
				case Registers.E:
					LDREG(regD,machine.E);
					break;
				case Registers.DA:
					LDREG(regD,machine.DA);
					break;
				case Registers.DB:
					LDREG(regD,machine.DB);
					break;
				case Registers.AX:
					LDREG(regD,machine.AX);
					break;
				case Registers.BX:
					LDREG(regD,machine.BX);
					break;
				case Registers.EX:
					LDREG(regD,machine.EX);
					break;
				case Registers.EAX:
					LDREG(regD,machine.EAX);
					break;
				case Registers.EBX:
					LDREG(regD,machine.EBX);
					break;
				case Registers.EEX:
					LDREG(regD,machine.EEX);
					break;
				case Registers.IP:
					LDREG(regD,machine.IP);
					break;
				default:
					throw new Exception("Invalid register! Got source: " + regF + " and dest: " + regD);
			}
		}

		void PushByte() {
			byte val = NextByte();
			machine.IP++;
			machine.pager.Push(val,machine.CR3);
		}
		void PushInt() {
			int val = NextInt();
			machine.IP += 4;
			machine.pager.Push(val,machine.CR3);
		}
		void PushUInt() {
			uint val = NextUInt();
			machine.IP += 4;
			machine.pager.Push(val,machine.CR3);
		}
		void PushFloat() {
			float val = NextFloat();
			machine.IP += 4;
			machine.pager.Push(val,machine.CR3);
		}
		void PushReg() {
			byte reg = NextByte();
			machine.IP++;
			switch (reg) {
				case Registers.A:
					machine.pager.Push(machine.A,machine.CR3);
					break;
				case Registers.B:
					machine.pager.Push(machine.B,machine.CR3);
					break;
				case Registers.E:
					machine.pager.Push(machine.E,machine.CR3);
					break;
				case Registers.DA:
					machine.pager.Push(machine.DA,machine.CR3);
					break;
				case Registers.DB:
					machine.pager.Push(machine.DB,machine.CR3);
					break;
				case Registers.AX:
					machine.pager.Push(machine.AX,machine.CR3);
					break;
				case Registers.BX:
					machine.pager.Push(machine.BX,machine.CR3);
					break;
				case Registers.EX:
					machine.pager.Push(machine.EX,machine.CR3);
					break;
				case Registers.EAX:
					machine.pager.Push(machine.EAX,machine.CR3);
					break;
				case Registers.EBX:
					machine.pager.Push(machine.EBX,machine.CR3);
					break;
				case Registers.EEX:
					machine.pager.Push(machine.EEX,machine.CR3);
					break;
				case Registers.IP:
					machine.pager.Push(machine.IP,machine.CR3);
					break;
				default:
					throw new Exception("Invalid register!");
			}
		}
		void LoadString() {
			uint addr = machine.DA;
			string val = machine.ram.ReadString(machine.pager.getVAT(addr,machine.CR3));
			machine.pager.Push(val,machine.CR3);
		}
		void PopByte() {
			byte reg = NextByte();
			machine.IP++;
			byte val = machine.pager.PopByte(machine.CR3);
			LDREG(reg,val);
		}
		void PopInt() {
			byte reg = NextByte();
			machine.IP++;
			int val = machine.pager.PopInt(machine.CR3);
			LDREG(reg,val);
		}
		void PopUInt() {
			byte reg = NextByte();
			machine.IP++;
			uint val = machine.pager.PopUInt(machine.CR3);
			LDREG(reg,val);
		}
		void PopFloat() {
			byte reg = NextByte();
			machine.IP++;
			float val = machine.pager.PopFloat(machine.CR3);
			LDREG(reg,val);
		}
		void PopString() {
			uint addr = machine.DA;
			string val = machine.pager.PopString(machine.CR3);
			if(machine.pager.checkVAT(addr,(uint)val.Length + 1,machine.CR3)) {
				machine.ram.Write(machine.pager.getVAT(addr,machine.CR3),val);
			} else {
				machine.pager.Write(addr,val,machine.CR3);
			}
		}

		void ReadB() {
			byte val = machine.ram.Read(machine.pager.getVAT(machine.DA,machine.CR3));
			machine.E = val;
		}
		void ReadI() {
			int val = machine.ram.ReadInt(machine.pager.getVAT(machine.DA,machine.CR3));
			machine.EX = val;
		}
		void ReadUI() {
			uint val = machine.ram.ReadUInt(machine.pager.getVAT(machine.DA,machine.CR3));
			machine.DB = val;
		}
		void ReadF() {
			float val = machine.ram.ReadFloat(machine.pager.getVAT(machine.DA,machine.CR3));
			machine.EEX = val;
		}

		void WriteB() {
			machine.ram.Write(machine.pager.getVAT(machine.DA,machine.CR3),machine.A);
		}
		void WriteI() {
			machine.ram.Write(machine.pager.getVAT(machine.DA,machine.CR3),machine.AX);
		}
		void WriteUI() {
			machine.ram.Write(machine.pager.getVAT(machine.DA,machine.CR3),machine.DB);
		}
		void WriteF() {
			machine.ram.Write(machine.pager.getVAT(machine.DA,machine.CR3),machine.EAX);
		}

		void Jmp() {
			uint addr = NextUInt();
			machine.IP = addr;
		}
		void Call() {
			uint addr = NextUInt();
			machine.IP += 4;
			machine.callstack.Push();
			machine.IP = addr;
		}
		void Ret() {
			uint addr = machine.callstack.ReadReturnAddress();
			machine.callstack.Pop();
			machine.IP = addr;
		}
		void JmpR() {
			uint addr = machine.DA;
			machine.IP = addr;
		}
		void CallR() {
			uint addr = machine.DA;
			machine.callstack.Push();
			machine.IP = addr;
		}

		void Add() {
			byte type = NextByte();
			machine.IP++;
			switch(type) {
				case BaseTypes.BYTE:
					machine.E = (byte)(machine.A + machine.B);
					break;
				case BaseTypes.INT:
					machine.EX = machine.AX + machine.BX;
					break;
				case BaseTypes.UINT:
					machine.DA = (uint)machine.DA + machine.DB;
					break;
				case BaseTypes.FLOAT:
					machine.EEX = (float)machine.EAX + machine.EBX;
					break;
			}
		}
		void Sub() {
			byte type = NextByte();
			machine.IP++;
			switch(type) {
				case BaseTypes.BYTE:
					machine.E = (byte)(machine.A - machine.B);
					break;
				case BaseTypes.INT:
					machine.EX = machine.AX - machine.BX;
					break;
				case BaseTypes.UINT:
					machine.DA = (uint)machine.DA - machine.DB;
					break;
				case BaseTypes.FLOAT:
					machine.EEX = (float)machine.EAX - machine.EBX;
					break;
			}
		}
		void Mul() {
			byte type = NextByte();
			machine.IP++;
			switch(type) {
				case BaseTypes.BYTE:
					machine.E = (byte)(machine.A * machine.B);
					break;
				case BaseTypes.INT:
					machine.EX = machine.AX * machine.BX;
					break;
				case BaseTypes.UINT:
					machine.DA = (uint)machine.DA * machine.DB;
					break;
				case BaseTypes.FLOAT:
					machine.EEX = (float)machine.EAX * machine.EBX;
					break;
			}
		}
		void Div() {
			byte type = NextByte();
			machine.IP++;
			switch(type) {
				case BaseTypes.BYTE:
					machine.E = (byte)(machine.A / machine.B);
					break;
				case BaseTypes.INT:
					machine.EX = (int)machine.AX / machine.BX;
					break;
				case BaseTypes.UINT:
					machine.DA = (uint)machine.DA / machine.DB;
					break;
				case BaseTypes.FLOAT:
					machine.EEX = (float)machine.EAX / machine.EBX;
					break;
			}
		}
		void Mod() {
			byte type = NextByte();
			machine.IP++;
			switch(type) {
				case BaseTypes.BYTE:
					machine.E = (byte)(machine.A % machine.B);
					break;
				case BaseTypes.INT:
					machine.EX = machine.AX % machine.BX;
					break;
				case BaseTypes.UINT:
					machine.DA = (uint)machine.DA % machine.DB;
					break;
				case BaseTypes.FLOAT:
					machine.EEX = (float)machine.EAX % machine.EBX;
					break;
			}
		}
		void Pow() {
			byte type = NextByte();
			machine.IP++;
			switch(type) {
				case BaseTypes.BYTE:
					machine.E = (byte)Math.Pow(machine.A,machine.B);
					break;
				case BaseTypes.INT:
					machine.EX = (int)Math.Pow(machine.AX,machine.BX);
					break;
				case BaseTypes.UINT:
					machine.DA = (uint)Math.Pow(machine.DA,machine.DB);
					break;
				case BaseTypes.FLOAT:
					machine.EEX = (float)Math.Pow(machine.EAX,machine.EBX);
					break;
			}
		}
		void Sqrt() {
			byte type = NextByte();
			machine.IP++;
			switch(type) {
				case BaseTypes.BYTE:
					machine.E = (byte)Math.Sqrt(machine.A);
					break;
				case BaseTypes.INT:
					machine.EX = (int)Math.Sqrt(machine.AX);
					break;
				case BaseTypes.UINT:
					machine.DA = (uint)Math.Sqrt(machine.DA);
					break;
				case BaseTypes.FLOAT:
					machine.EEX = (float)Math.Sqrt(machine.EAX);
					break;
			}
		}

		void Malloc () {
			int size = machine.AX;
			machine.DA = machine.pager.Malloc((uint)size, machine.CR3);
		}
		void Free () {
			uint addr = machine.DA;
			int size = machine.AX;
			machine.pager.free(addr,(uint)size, machine.CR3);
		}

		void Int () {
			byte interupt = NextByte();
			machine.IP++;
			if(interupt >= 0 && interupt < hardwareinterupts.Length)
				hardwareinterupts[interupt].Run(machine);
			else
				throw new Exception("No interupt at index: " + interupt);
		}

		void AllocPageTable ()
		{
			bool ptmode = (machine.A > 0);
			machine.EX = machine.pager.CreatePageEntry(ptmode);
		}

		void AllocPage ()
		{
			machine.pager.AddPage(machine.CR3);
		}

		void PageSetStack() {
			uint addr = machine.DA;
			machine.BP = addr;
		}

		void PageSetHeap() {
			uint addr = machine.DA;
			machine.HP = addr;
			machine.SP = addr - 1;
		}

		void PageInitMem() {
			machine.pager.SetupMemory(machine.CR3,machine.BP,machine.HP);
			machine.pager.SetupMemoryAllocation(machine.CR3);
		}

		void CallPage ()
		{
			int pt = machine.AX;
			PageDirectoryEntry page = machine.pager.getEntry(pt);
			page.return_addr = machine.IP;
			page.return_page = machine.CR3I;
			machine.SwitchPage(pt);
			machine.IP = 0;
		}

		void RetPage ()	{
			int retpg = machine.CR3.return_page;
			uint retaddr = machine.CR3.return_addr;
			machine.SwitchPage(retpg);
			machine.IP = retaddr;
		}

		void FreePageTable () {
			int table = machine.AX;
			machine.pager.FreePageEntry(machine.pager.getEntry(table));
		}

		void GetPageTableSize() {
			int page = machine.AX;
			machine.EX = (int)machine.pager.GetPageDirectoryEntrySize(machine.pager.getEntry(page));
		}

		void GetPageId ()
		{
			machine.EX = machine.CR3I;
		}

		void PageVAT() {
			machine.DB = machine.pager.getVAT(machine.DA, machine.CR3);
		}

		void ReverseVAT() {
			machine.DB = machine.pager.reverseVAT(machine.DA);
		}

		void DmpPt ()
		{
			machine.pager.DumpPageTable(machine.CR3);
		}

		void DmpFl ()
		{
			machine.pager.DumpFreeList(machine.CR3);
		}

		void OcDsm ()
		{
			machine.DSM = ! machine.DSM;
		}

		void Je() {
			if(machine.CP) {
				machine.IP = NextUInt();
			} else {
				machine.IP += 4;
			}
		}

		void Jn ()
		{
			if (!machine.CP) {
				machine.IP = NextUInt ();
			} else {
				machine.IP += 4;
			}
		}

		void Lt() {
			byte type = NextByte();
			machine.IP++;
			if(type == BaseTypes.BYTE) {
				machine.CP = (machine.A < machine.B);
			} else if (type == BaseTypes.INT) {
				machine.CP = (machine.AX < machine.BX);
			} else if (type == BaseTypes.UINT) {
				machine.CP = (machine.DA < machine.DB);
			} else if (type == BaseTypes.FLOAT) {
				machine.CP = (machine.EAX < machine.EBX);
			}
		}
		void Le() {
			byte type = NextByte();
			machine.IP++;
			if(type == BaseTypes.BYTE) {
				machine.CP = (machine.A <= machine.B);
			} else if (type == BaseTypes.INT) {
				machine.CP = (machine.AX <= machine.BX);
			} else if (type == BaseTypes.UINT) {
				machine.CP = (machine.DA <= machine.DB);
			} else if (type == BaseTypes.FLOAT) {
				machine.CP = (machine.EAX <= machine.EBX);
			}
		}
		void Eq() {
			byte type = NextByte();
			machine.IP++;
			if(type == BaseTypes.BYTE) {
				machine.CP = (machine.A == machine.B);
			} else if (type == BaseTypes.INT) {
				machine.CP = (machine.AX == machine.BX);
			} else if (type == BaseTypes.UINT) {
				machine.CP = (machine.DA == machine.DB);
			} else if (type == BaseTypes.FLOAT) {
				machine.CP = (machine.EAX == machine.EBX);
			}
		}
		void Ne() {
			byte type = NextByte();
			machine.IP++;
			if(type == BaseTypes.BYTE) {
				machine.CP = (machine.A != machine.B);
			} else if (type == BaseTypes.INT) {
				machine.CP = (machine.AX != machine.BX);
			} else if (type == BaseTypes.UINT) {
				machine.CP = (machine.DA != machine.DB);
			} else if (type == BaseTypes.FLOAT) {
				machine.CP = (machine.EAX != machine.EBX);
			}
		}
		void Ge() {
			byte type = NextByte();
			machine.IP++;
			if(type == BaseTypes.BYTE) {
				machine.CP = (machine.A >= machine.B);
			} else if (type == BaseTypes.INT) {
				machine.CP = (machine.AX >= machine.BX);
			} else if (type == BaseTypes.UINT) {
				machine.CP = (machine.DA >= machine.DB);
			} else if (type == BaseTypes.FLOAT) {
				machine.CP = (machine.EAX >= machine.EBX);
			}
		}
		void Gt() {
			byte type = NextByte();
			machine.IP++;
			if(type == BaseTypes.BYTE) {
				machine.CP = (machine.A > machine.B);
			} else if (type == BaseTypes.INT) {
				machine.CP = (machine.AX > machine.BX);
			} else if (type == BaseTypes.UINT) {
				machine.CP = (machine.DA > machine.DB);
			} else if (type == BaseTypes.FLOAT) {
				machine.CP = (machine.EAX > machine.EBX);
			}
		}

		void JeR() {
			if(machine.CP) {
				machine.IP = machine.DA;
			}
		}

		void JnR() {
			if(!machine.CP) {
				machine.IP = machine.DA;
			}	
		}
		void StrLen ()
		{
			string str = machine.pager.PopString(machine.CR3);
			machine.EX = str.Length;
		}

		void StrCmp ()
		{
			string a = machine.pager.PopString(machine.CR3);
			string b = machine.pager.PopString(machine.CR3);
			machine.CP = a == b;
		}

		void StrSub()
		{
			string a = machine.pager.PopString(machine.CR3);
			int index = machine.AX;
			int length = machine.BX;
			machine.pager.Push(a.Substring(index,length),machine.CR3);
		}
	}
}