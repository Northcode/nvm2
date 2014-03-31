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
			while ((cmd = Console.ReadLine()) != "") {
				string[] sargs = cmd.Split(' ');
				if(sargs[0] == "mkpt") {
					Console.WriteLine("Created page table at: " + pager.CreatePageEntry(Pager.PAGE_USER_MODE));
				} else if (sargs[0] == "rmpt") {
					int pt = Convert.ToInt32(sargs[1]);
					pager.FreePageEntry(pager.getEntry(pt));
					Console.WriteLine("Removed page: " + sargs[1]);
				} else if (sargs[0] == "addpage") {
					int pt = Convert.ToInt32(sargs[1]);
					pager.AddPage(pager.getEntry(pt));
					Console.WriteLine("Added page");
				} else if (sargs[0] == "setmem") {
					int pt = Convert.ToInt32(sargs[1]);
					uint stk = Convert.ToUInt32(sargs[2]);
					uint hep = Convert.ToUInt32(sargs[3]);
					pager.SetupMemory(pager.getEntry(pt),stk,hep);
					pager.SetupMemoryAllocation(pager.getEntry(pt));
				} else if (sargs[0] == "malloc") {
					int pt = Convert.ToInt32(sargs[1]);
					uint size = Convert.ToUInt32(sargs[2]);
					uint addr = pager.Malloc(size,pager.getEntry(pt));
					Console.WriteLine("Allocated memory at: " + addr);
				} else if (sargs[0] == "free") {
					int pt = Convert.ToInt32(sargs[1]);
					uint addr = Convert.ToUInt32(sargs[2]);
					uint size = Convert.ToUInt32(sargs[3]);
					pager.free(addr,size,pager.getEntry(pt));
					Console.WriteLine("Freed up space");
				} else if (sargs[0] == "dmppt") {
					int pt = Convert.ToInt32(sargs[1]);
					Console.WriteLine("Page table " + pt + " at: " + pager.getEntry(pt).PTAddress);
					pager.DumpPageTable(pager.getEntry(pt));
				} else if (sargs[0] == "dmpfl") {
					int pt = Convert.ToInt32(sargs[1]);
					pager.DumpFreeList(pager.getEntry(pt));
				} else if (sargs[0] == "pushi") {
					int pt = Convert.ToInt32(sargs[1]);
					int val = Convert.ToInt32(sargs[2]);
					pager.Push(val,pager.getEntry(pt));
				} else if (sargs[0] == "popi") {
					int pt = Convert.ToInt32(sargs[1]);
					Console.WriteLine(pager.PopInt(pager.getEntry(pt)));
				} else if (sargs[0] == "exit") {
					break;
				}
			}
		}
	}
}
