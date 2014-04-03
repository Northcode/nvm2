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
			List<VMDevice> extradevices = new List<VMDevice> ();

			string codetocompile = "";
			bool output = false;
			string outputfile = "";
			bool compileflag = false;
			byte[] biosdata = null;


#if DEBUG
			string inpdata = Console.ReadLine();
			if(inpdata == "") {
				inpdata = "-cd ../../testing -i bios.txt -o bios -rd helloworld";
			}
			args = inpdata.Split(' ');
#endif

			for (int i = 0; i < args.Length; i++) {
				if (args [i] == "-rd") {
					i++;
					extradevices.Add (new VirtualROMDisk (args [i]));
				} else if (args [i] == "-i") {
					i++;
					codetocompile = File.ReadAllText (args [i]);
				} else if (args [i] == "-o") {
					output = true;
					i++;
					outputfile = args [i];
				} else if (args [i] == "-cd") {
					i++;
					Directory.SetCurrentDirectory(args[i]);
				} else if (args[i] == "-c") {
					compileflag = true;
				} else if (args[i] == "-b") {
					i++;
					biosdata = File.ReadAllBytes(args[i]);
				} else if (args[i] == "-h") {
					Console.WriteLine("Northcode Virtual Machine 2");
					Console.WriteLine("Usage: nvm [options]");
					Console.WriteLine("Options:");
					Console.WriteLine("-cd <directory>\t\t: change working directory, useful for loading bios from somewhere outside of the nvm2 folder");
					Console.WriteLine("-i <file>\t\t: input file, select and inputfile for the Assembler");
					Console.WriteLine("-c\t\t\t: compile flag, when selected, code from inputfile will be assembled into nasm");
					Console.WriteLine("-o <file>\t\t: output file, file for compiler to write assembly into");
					Console.WriteLine("-rd <file>\t\t: assembly file to be loaded as a virtual ROM disk that can be read by the vm");
					Console.WriteLine("-h\t\t\t: shows this screen of commands");
					Console.WriteLine("-b <file>\t\t: Loads a bios to the virtual machine, this is the first program that is run");
					Console.WriteLine();
				}
			}


			if (compileflag) {
				if (codetocompile == "") {
					StringBuilder stb = new StringBuilder ();
					string c = "";
					while ((c = Console.ReadLine()) != "q") {
						stb.AppendLine (c);
					}
					codetocompile = stb.ToString ();
				}

				Assembler asm = new Assembler(codetocompile);
				asm.Scan();
				asm.Assemble();

				byte[] program = asm.GetProgram();

				if (output) {
					File.WriteAllBytes (outputfile, program);
				}
			}
			/*
			for(int i = 0; i < program.Length; i++) {
				Console.WriteLine(i + ": " + program[i]);
			}
			*/

			vm machine = new vm();
			foreach (VMDevice device in extradevices) {
				machine.LoadDevice(device);
			}
			if(biosdata == null)
			{
				Console.WriteLine("No bios loaded!");
				Console.ReadLine();
				return;
			}
			machine.LoadBios(biosdata);
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
