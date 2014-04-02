using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace nvm2
{
	class MainClass
	{
		public static void print (object c)
		{
			Console.WriteLine(c);
		}

		public static void Main (string[] args)
		{
			/*byte[] bios = new byte[] {
				OpCodes.LD, BaseTypes.BYTE, Registers.B, (byte)'A', 	//Load 'A' into register B
				OpCodes.LD, BaseTypes.BYTE, Registers.A, 0, 			//Load 0 into register A
				OpCodes.INT, 1, 										//Print char
				OpCodes.LD, BaseTypes.BYTE, Registers.B, (byte)'B',
				OpCodes.INT, 1,
				OpCodes.LD, BaseTypes.BYTE, Registers.B, (byte)'C',
				OpCodes.INT, 1,
				OpCodes.LD, BaseTypes.BYTE, Registers.B, (byte)'D',
				OpCodes.INT, 1,
				
				OpCodes.INT, 0 											//Terminate program
			};
			*/

			List<VMDevice> extradevices = new List<VMDevice>();

			string bioscode = "";

			for(int i = 0; i < args.Length; i++) {
				if(args[i] == "-rd") {
					i++;
					extradevices.Add(new VirtualROMDisk(args[i]));
				}
				else if(args[i] == "-b") {
					i++;
					bioscode = File.ReadAllText(args[i]);
				}
			}

			if(bioscode == "") {
				StringBuilder stb = new StringBuilder();
				string c = "";
				while((c = Console.ReadLine()) != "q") {
					stb.AppendLine(c);
				}
				bioscode = stb.ToString();
			}

			Assembler asm = new Assembler(bioscode);
			asm.Scan();
			asm.Assemble();

			byte[] bios = asm.GetProgram();
			/*
			for(int i = 0; i < bios.Length; i++) {
				Console.WriteLine(i + ": " + bios[i]);
			}
			*/

			vm machine = new vm();
			machine.LoadBios(bios);
			machine.Start();

			/*
			 * MEMORY TEST INTERPRETER
			Console.Write("Size of ram (num. of frames (4k blocks)): ");
			int rsize = Convert.ToInt32(Console.ReadLine());
			Memory test = new Memory(Frame.FRAME_SIZE * rsize);
			Pager pager = new Pager(test, 10);

			string cmd = "";
			int selected = -1;
			while ((cmd = Console.ReadLine()) != "") {
				string[] sargs = cmd.Split(' ');
				try {
					if(sargs[0] == "mkpt") {
						Console.WriteLine("Created page table at: " + pager.CreatePageEntry(Pager.PAGE_USER_MODE));
					} else if (sargs[0] == "rmpt") {
						int pt = selected;
						selected = -1;
						pager.FreePageEntry(pager.getEntry(pt));
						Console.WriteLine("Removed page: " + sargs[1]);
					} else if(sargs[0] == "select") {
						selected = Convert.ToInt32(sargs[1]);
						Console.WriteLine("selected page table " + selected);
					} else if (sargs[0] == "addpage") {
						int pt = selected; 
						pager.AddPage(pager.getEntry(pt));
						Console.WriteLine("Added page");
					} else if (sargs[0] == "setmem") {
						int pt = selected; 
						uint stk = Convert.ToUInt32(sargs[1]);
						uint hep = Convert.ToUInt32(sargs[2]);
						pager.SetupMemory(pager.getEntry(pt),stk,hep);
						pager.SetupMemoryAllocation(pager.getEntry(pt));
					} else if (sargs[0] == "malloc") {
						int pt = selected; 
						uint size = Convert.ToUInt32(sargs[1]);
						uint addr = pager.Malloc(size,pager.getEntry(pt));
						Console.WriteLine("Allocated memory at: " + addr);
					} else if (sargs[0] == "free") {
						int pt = selected; 
						uint addr = Convert.ToUInt32(sargs[1]);
						uint size = Convert.ToUInt32(sargs[2]);
						pager.free(addr,size,pager.getEntry(pt));
						Console.WriteLine("Freed up space");
					} else if (sargs[0] == "dmppt") {
						int pt = selected; 
						Console.WriteLine("Page table " + pt + " at: " + pager.getEntry(pt).PTAddress);
						pager.DumpPageTable(pager.getEntry(pt));
					} else if (sargs[0] == "dmpfl") {
						int pt = selected; 
						pager.DumpFreeList(pager.getEntry(pt));
					} else if (sargs[0] == "pushb") {
						int pt = selected; 
						byte val = Convert.ToByte(sargs[1]);
						pager.Push(val,pager.getEntry(pt));
					} else if (sargs[0] == "pushi") {
						int pt = selected; 
						int val = Convert.ToInt32(sargs[1]);
						pager.Push(val,pager.getEntry(pt));
					} else if (sargs[0] == "pushui") {
						int pt = selected;
						uint val = Convert.ToUInt32(sargs[1]);
						pager.Push(val,pager.getEntry(pt));
					} else if (sargs[0] == "pushf") {
						int pt = selected;
						float val = (float)Convert.ToDecimal(sargs[1]);
						pager.Push(val,pager.getEntry(pt));
					} else if (sargs[0] == "pushs") {
						int pt = selected;
						string val = String.Join(" ",sargs);
						val = val.Substring(val.IndexOf('\'') + 1,val.LastIndexOf('\'') - (val.IndexOf('\'') + 1));
						pager.Push(val,pager.getEntry(pt));
					} else if (sargs[0] == "popb") {
						int pt = selected;
						Console.WriteLine(pager.PopByte(pager.getEntry(pt)));
					} else if (sargs[0] == "popi") {
						int pt = selected;
						Console.WriteLine(pager.PopInt(pager.getEntry(pt)));
					} else if (sargs[0] == "popui") {
						int pt = selected;
						Console.WriteLine(pager.PopUInt(pager.getEntry(pt)));
					} else if (sargs[0] == "popf") {
						int pt = selected;
						Console.WriteLine(pager.PopFloat(pager.getEntry(pt)));
					} else if (sargs[0] == "pops") {
						int pt = selected;
						Console.WriteLine(pager.PopString(pager.getEntry(pt)));
					} else if(sargs[0] == "vat") {
						uint val = Convert.ToUInt32(sargs[1]);
						Console.WriteLine(pager.getVAT(val,pager.getEntry(selected)));
					} else if(sargs[0] == "rvat") {
						uint val = Convert.ToUInt32(sargs[1]);
						Console.WriteLine(pager.reverseVAT(val));
					} else if (sargs[0] == "writeb") {
						uint addr = Convert.ToUInt32(sargs[1]);
						byte val = Convert.ToByte(sargs[2]);
						test.Write(addr,val);
					} else if (sargs[0] == "writei") {
						uint addr = Convert.ToUInt32(sargs[1]);
						int val = Convert.ToInt32(sargs[2]);
						test.Write(addr,val);
					} else if (sargs[0] == "writeui") {
						uint addr = Convert.ToUInt32(sargs[1]);
						uint val = Convert.ToUInt32(sargs[2]);
						test.Write(addr,val);
					} else if (sargs[0] == "writef") {
						uint addr = Convert.ToUInt32(sargs[1]);
						float val = (float)Convert.ToDecimal(sargs[2]);
						test.Write(addr,val);
					} else if (sargs[0] == "writes") {
						uint addr = Convert.ToUInt32(sargs[1]);
						string val = sargs[2];
						test.Write(addr,val);
					} else if (sargs[0] == "exit") {
						break;
					}
				} catch (IndexOutOfRangeException) {
					Console.WriteLine("Please select a page table!");
				} catch (Exception ex) {
					Console.WriteLine("Error: " + ex.Message);
				}
			}
			*/
		}
	}
}
