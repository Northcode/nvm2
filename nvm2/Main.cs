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

			int p1 = pager.CreatePageEntry(Pager.PAGE_KERNEL_MODE);
			int p2 = pager.CreatePageEntry(Pager.PAGE_USER_MODE);
			int p3 = pager.CreatePageEntry(Pager.PAGE_USER_MODE);
			pager.AddPage(pager.getEntry(p2));
			print (p1 + ": " + pager.getEntry(p1).PTAddress);
			pager.DumpPageTable(pager.getEntry(p1));
			print (p2 + ": " + pager.getEntry(p2).PTAddress);
			pager.DumpPageTable(pager.getEntry(p2));
			print (p3 + ": " + pager.getEntry(p3).PTAddress);
			pager.DumpPageTable(pager.getEntry(p3));
		}
	}
}
