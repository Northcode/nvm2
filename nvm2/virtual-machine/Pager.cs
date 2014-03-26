using System;
using System.Collections.Generic;

namespace nvm2
{
	public struct PageDirectoryEntry
	{
		public uint PTAddress;
		public bool AccessLevel;
		public bool InUse;
		public uint stack_pointer;
		public uint heap_pointer;
	}

	public class Pager
	{
		public const int PAGE_TABLE_SIZE = 512;

		public const bool PAGE_KERNEL_MODE = true;
		public const bool PAGE_USER_MODE = false;

		//vm machine;
		Memory ram;

		PageDirectoryEntry[] PageDirectory;

		public Pager (Memory RAM, int PDSize)
		{
			//machine = Machine;
			ram = RAM;

			PageDirectory = new PageDirectoryEntry[PDSize];
			for (int i = 0; i < PDSize; i++) {
				PageDirectory[i] = new PageDirectoryEntry() { PTAddress = 0, AccessLevel = false, InUse = false };
			}
		}

		public PageDirectoryEntry getEntry (int i)
		{
			return PageDirectory[i];
		}

		public int CreatePageEntry (bool mode)
		{
			//Declare stuff
			int index = 0;
			//Find free entry in page directory
			PageDirectoryEntry entry = new PageDirectoryEntry();
			for (int i = 0; i < PageDirectory.Length; i++) {
				if (!PageDirectory[i].InUse) {
					entry = PageDirectory[i];
					index = i;
					break;
				}
			}
			//Find a free ram frame for the first page
			Frame mainFrame = ram.findFreeFrame();
			mainFrame.IsFree = false;
			entry.PTAddress = mainFrame.Address;
			entry.AccessLevel = mode;
			entry.InUse = true;
			//Write page table to ram for entry
			WritePageTable(entry);
			PageDirectory[index] = entry;
			return index;
		}

		public void FreePageEntry (int index)
		{
			//Get entry
			PageDirectoryEntry entry = PageDirectory[index];

			FreePageTable(entry);
			entry.InUse = false;
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
			ram.Write(addr,addr + PAGE_TABLE_SIZE + 4);
		}

		public uint GetPageDirectoryEntrySize(PageDirectoryEntry entry) {
			uint addr = entry.PTAddress;
			uint count = 0;
			for(int i = 0; i < PAGE_TABLE_SIZE; i++) {
				if(ram.ReadUInt((uint)(addr + i*4)) != 0) {
					count++;
				}
			}
			return count * Frame.FRAME_SIZE;
		}

		public uint TranslateVitrualAddress(uint address, PageDirectoryEntry entry) {
			// Get address
			uint ptaddr = entry.PTAddress;
			// Get Page id
			int page = (int)(address / Frame.FRAME_SIZE);
			int offset = (int)(address - page * Frame.FRAME_SIZE);

			// Get page table entry address
			uint pageaddr = ram.ReadUInt((uint)(ptaddr + (page * 4)));

			// Get address
			return (uint)(pageaddr + offset);
		}

		public void SetupMemory(PageDirectoryEntry entry, uint stack_pointer, uint heap_pointer) {
			entry.stack_pointer = stack_pointer;
			entry.heap_pointer = heap_pointer;
		}

		public void SetupMemoryAllocation(PageDirectoryEntry entry) {
			// Get address
			//uint addr = entry.PTAddress + PAGE_TABLE_SIZE;
			// Write free pointer
			ram.Write(entry.heap_pointer,entry.heap_pointer);
			ram.Write(entry.heap_pointer + 4,GetPageDirectoryEntrySize(entry));
		}

		public void AddPage (PageDirectoryEntry entry)
		{
			//Find free frame
			Frame freeFrame = ram.findFreeFrame();
			freeFrame.IsFree = false;
			//Add frame to PT
			for (int i = 0; i < Pager.PAGE_TABLE_SIZE; i++) {
				if (ram.ReadUInt((uint)(entry.PTAddress + i * 4)) == 0) {
					ram.Write((uint)(entry.PTAddress + i * 4),freeFrame.Address);
					break;
				}
			}
		}		

		public void DumpPageTable (PageDirectoryEntry entry)
		{
			for (int i = 0; i < PAGE_TABLE_SIZE; i++) {
				uint addr = ram.ReadUInt((uint)(entry.PTAddress + i * 4));
				if (addr != 0) {
					Console.WriteLine(addr);
				}
			}
		}

	}
}

