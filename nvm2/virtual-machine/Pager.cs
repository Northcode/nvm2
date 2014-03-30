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
		public uint free_pointer;
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



		// ---- MEMORY ALLOC STUFF ---- all memory allocation is done with relative addresses

		/// <summary>
		/// Setup the heap and stack for the page table
		/// </summary>
		/// <param name='entry'>
		/// Page directory entry
		/// </param>
		/// <param name='stack_pointer'>
		/// New stack_pointer, relative address.
		/// </param>
		/// <param name='heap_pointer'>
		/// New heap_pointer, relative address
		/// </param>
		public void SetupMemory(PageDirectoryEntry entry, uint stack_pointer, uint heap_pointer) {
			entry.stack_pointer = stack_pointer;
			entry.heap_pointer = heap_pointer;
		}

		/// <summary>
		/// Setup the heap memory allocation for the page table
		/// </summary>
		/// <param name='entry'>
		/// Page table to setup
		/// </param>
		public void SetupMemoryAllocation(PageDirectoryEntry entry) {
			entry.free_pointer = entry.heap_pointer; //set free-list pointer to start of heap
			ram.Write(TranslateVitrualAddress(entry.heap_pointer,entry),0); // write 0 because there is no next block
			ram.Write(TranslateVitrualAddress(entry.heap_pointer,entry) + 4,(uint)(GetPageDirectoryEntrySize(entry) - entry.heap_pointer)); //Write size of free block (total size of heap)
		}

		/// <summary>
		/// Allocate a new block of memory in the page table with the specified size
		/// </summary>
		/// <param name='size'>
		/// Size of memory to allocate
		/// </param>
		/// <param name='entry'>
		/// Page table to use
		/// </param>
		/// <returns>
		/// The address of the allocated block
		/// </returns>
		public uint Malloc(uint size,PageDirectoryEntry entry) {
			uint lastaddr = entry.free_pointer; //last free block pointer
			//loop through freelist , check for large enough chunk
			for (uint addr = entry.free_pointer; addr < GetPageDirectoryEntrySize(entry); ) {
				uint nextblock = ram.ReadUInt(TranslateVitrualAddress(addr, entry)); //read address of next free block
				uint blocksize = ram.ReadUInt(TranslateVitrualAddress(addr + 4, entry)); //read size of current free block
				if (blocksize > size) {
					uint newsize = blocksize - size; //get new size of free block
					uint newaddr = addr + size; //get new address of free block
					if (newsize > 0) { //if size == 0, block is gone
						ram.Write(TranslateVitrualAddress(newaddr,entry),nextblock); // set next block pointer
						ram.Write(TranslateVitrualAddress(newaddr + 4,entry),newsize); //write new size of new freeblock
						ram.Write(TranslateVitrualAddress(lastaddr,entry),TranslateVitrualAddress(newaddr,entry)); //set pointer of last block to point to this new one
					}
					else
					{
						ram.Write(TranslateVitrualAddress(lastaddr,entry),TranslateVitrualAddress(nextblock,entry)); //set pointer to skip unexisting block
					}
					return addr; //return address of allocated space
				}
			}
		}
	}
}

