using System;

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
			Console.Write("Size of ram (num. of frames (4k blocks)): ");
			int rsize = Convert.ToInt32(Console.ReadLine());
			Memory test = new Memory(Frame.FRAME_SIZE * rsize);
			Pager pager = new Pager(test, 10);

			/*
			PageDirectoryEntry p1 = pager.getEntry(pager.CreatePageEntry(Pager.PAGE_KERNEL_MODE));
			PageDirectoryEntry p2 = pager.getEntry(pager.CreatePageEntry(Pager.PAGE_USER_MODE));
			PageDirectoryEntry p3 = pager.getEntry(pager.CreatePageEntry(Pager.PAGE_USER_MODE));
			pager.AddPage(p2);

			//Setup stack and heap for pagetable1
			pager.SetupMemory(p1,100u,128u);
			pager.SetupMemoryAllocation(p1);

			print (p1 + ": " + p1.PTAddress);
			pager.DumpPageTable(p1);
			print (p2 + ": " + p2.PTAddress);
			pager.DumpPageTable(p2);
			print (p3 + ": " + p3.PTAddress);
			pager.DumpPageTable(p3);

			uint testalloc = pager.Malloc(4u,p1);
			test.Write(pager.TranslateVitrualAddress(testalloc,p1),5);
			print("int 5 at: " + testalloc + " @ " + pager.TranslateVitrualAddress(testalloc,p1));
			pager.DumpFreeList(p1);
			uint testalloc2 = pager.Malloc(4u,p1);
			test.Write(pager.TranslateVitrualAddress(testalloc2,p1),10);
			print("int 10 at:" + testalloc2 + " @ " + pager.TranslateVitrualAddress(testalloc2,p1));
			pager.DumpFreeList(p1);
			pager.free(testalloc2,4u,p1);
			pager.DumpFreeList(p1);
			uint testalloc3 = pager.Malloc(4u,p1);
			test.Write(pager.TranslateVitrualAddress(testalloc3,p1),15);
			print("int 15 at:" + testalloc3 + " @ " + pager.TranslateVitrualAddress(testalloc3,p1));
			pager.DumpFreeList(p1);
			*/

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
		}
	}
}
