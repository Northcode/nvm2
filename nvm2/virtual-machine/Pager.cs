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
		public const int PAGE_TABLE_SIZE = 512;

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

		public int CreatePageEntry (bool mode)
		{
			//Declare stuff
			int index = 0;
			PageDirectoryEntry entry = new PageDirectoryEntry();
			//Find a free ram frame for the first page
			Frame mainFrame = ram.findFreeFrame();
			mainFrame.IsFree = false;
			entry.PTAddress = mainFrame.Address;
			entry.AccessLevel = mode;
			entry.InUse = true;
			//Find free entry in page directory
			for (int i = 0; i < PageDirectory.Length; i++) {
				if (!PageDirectory[i].InUse) {
					PageDirectory[i] = entry;
					index = i;
					break;
				}
			}
			//Write page table to ram for entry
			WritePageTable(entry);
			return index;
		}

		public void FreePageEntry (int index)
		{
			//Get entry
			PageDirectoryEntry entry = PageDirectory[index];

			FreePageTable(entry);
		}

		public void FreePageTable (PageDirectoryEntry entry)
		{
			uint addr = entry.PTAddress;

			//Read pages
			for (int i = 0; i < PAGE_TABLE_SIZE; i++) {
				uint frameAddr = ram.ReadUInt((uint)(addr + (i*4)));
				int frameindex = (int)(frameAddr / Frame.FRAME_SIZE);
				Frame frame = ram.getFrame(frameindex);
				frame.IsFree = true; // Set frame to free, clears the frame
			}
		}

		public void WritePageTable (PageDirectoryEntry entry)
		{
			//Write page table
			uint addr = entry.PTAddress; // Get page table address
			// Write: Page addresses
			for (int i = 0; i < PAGE_TABLE_SIZE; i++) {
				ram.Write((uint)(addr + (i*4)), 0);
			}
			//Write first page address start
			ram.Write(addr,addr + 512);
		}

		public uint TranslateVitrualAddress(uint address, PageDirectoryEntry entry) {
			// Get address
			uint ptaddr = entry.PTAddress;
			// Get Page id
			int page = address / Frame.FRAME_SIZE;
			int offset = address - page * Frame.FRAME_SIZE;

			// Get page table entry address
			uint pageaddr = ram.ReadUInt((uint)(ptaddr + (page * 4)));

			// Get address
			return pageaddr + offset;
		}
	}
}

