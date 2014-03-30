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
			Memory test = new Memory(Frame.FRAME_SIZE * 16);
			Pager pager = new Pager(test, 10);

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

			//test allocate an integer
			uint testalloc = pager.Malloc(4u,p1);
			test.Write(pager.TranslateVitrualAddress(testalloc,p1),5);
			print("int 5 at: " + testalloc);
			uint testalloc2 = pager.Malloc(4u,p1);
			test.Write(pager.TranslateVitrualAddress(testalloc2,p1),10);
			print("int 10 at:" + testalloc2);
		}
	}
}
