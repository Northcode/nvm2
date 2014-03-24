using System;
using System.Collections.Generic;

namespace nvm2
{
	public struct PageDirectoryEntry
	{
		public uint PTAddress;
		public bool AccessLevel;
		public bool InUse;
	}

	public class Pager
	{
		vm machine;
		Memory ram;

		PageDirectoryEntry[] PageDirectory;

		public Pager (vm Machine, Memory RAM, int PDSize)
		{
			machine = Machine;
			ram = RAM;

			PageDirectory = new PageDirectoryEntry[PDSize];
			for (int i = 0; i < PDSize; i++) {
				PageDirectory[i] = new PageDirectoryEntry() { PTAddress = 0, AccessLevel = false, InUse = false };
			}
		}
	}
}

