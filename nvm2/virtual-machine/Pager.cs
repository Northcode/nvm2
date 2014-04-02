using System;
using System.Collections.Generic;

namespace nvm2
{
	public class PageDirectoryEntry
	{
		public uint PTAddress;
		public bool AccessLevel;
		public bool InUse;
		public uint stack_pointer;
		public uint base_pointer;
		public uint heap_pointer;
		public uint free_pointer;

		public int return_page;
		public uint return_addr;
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

		#region paging
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
			mainFrame.PageTable = index;
			entry.PTAddress = mainFrame.Address;
			entry.AccessLevel = mode;
			entry.InUse = true;
			//Write page table to ram for entry
			WritePageTable(entry);
			PageDirectory[index] = entry;
			return index;
		}

		public void FreePageEntry (PageDirectoryEntry entry)
		{
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

		///<summary>
		///Translates the page relative address to a 'physical' one
		///</summary>
		public uint getVAT(uint address, PageDirectoryEntry entry) {
			// Get address
			uint ptaddr = entry.PTAddress;

			//The 4 and 5 is for keeping space for integers stored to keep stuff from overflowing to different pages, strings need their own logic to do this.
			if(address < Frame.FRAME_SIZE - PAGE_TABLE_SIZE - 4) { //address is in the first frame
				return ptaddr + PAGE_TABLE_SIZE + address;
			} else { //address is after first frame
				int page = (int)(address / Frame.FRAME_SIZE);
				int offset = (int)(address - page * Frame.FRAME_SIZE + PAGE_TABLE_SIZE);
				if(offset > Frame.FRAME_SIZE - 5) { //fix for addresses over framesize - 512
					page++;
					offset -= Frame.FRAME_SIZE;
				}
				uint pageaddr = ram.ReadUInt((uint)(ptaddr + (page * 4)));
				return (uint)(pageaddr + offset);
			}
			/*
			// Get Page id
			int page = (int)(address / Frame.FRAME_SIZE);
			int offset = (int)(address - page * Frame.FRAME_SIZE);

			// Get page table entry address
			uint pageaddr = ram.ReadUInt((uint)(ptaddr + (page * 4)));

			// Get address
			return (uint)(pageaddr + offset);
			*/
		}

		///<summary>
		///Check if the address will overflow to the next page with the given size
		///</summary>
		///<returns>
		///False if the address overflows
		///</returns>
		public bool checkVAT(uint address, uint size, PageDirectoryEntry entry) {
			uint addr1 = getVAT(address,entry);
			uint addr2 = getVAT(address + size,entry);
			return (addr2 - addr1 == size);
		}

		public uint reverseVAT(uint address) {
			int frame = (int)address / Frame.FRAME_SIZE;
			int pagetable = ram.getFrame(frame).PageTable;
			uint offset = (uint)(address - frame * Frame.FRAME_SIZE);
			uint addr = (uint)(pagetable + offset);
			return addr;
		}

		public uint[] ReadPageTable(PageDirectoryEntry entry) {
			uint[] array = new uint[PAGE_TABLE_SIZE / 4];
			for (int i = 0; i < PAGE_TABLE_SIZE / 4; i++) {
				uint addr = ram.ReadUInt((uint)(entry.PTAddress + i * 4));
				array[i] = addr;
			}
			return array;
		}

		public void AddPage (PageDirectoryEntry entry)
		{
			//Find free frame
			Frame freeFrame = ram.findFreeFrame();
			freeFrame.IsFree = false;
			for(int i = 0; i < PageDirectory.Length; i++) {
				if(PageDirectory[i].PTAddress == entry.PTAddress) {
					freeFrame.PageTable = i;
					Console.WriteLine(i);
					break;
				}
			}
			//Add frame to PT
			for (int i = 0; i < Pager.PAGE_TABLE_SIZE / 4; i++) {
				if (ram.ReadUInt((uint)(entry.PTAddress + i * 4)) == 0) {
					ram.Write((uint)(entry.PTAddress + i * 4),freeFrame.Address);
					break;
				}
			}
		}

		public void DumpPageTable (PageDirectoryEntry entry)
		{
			for (int i = 0; i < PAGE_TABLE_SIZE / 4; i++) {
				uint addr = ram.ReadUInt((uint)(entry.PTAddress + i * 4));
				if (addr != 0) {
					Console.WriteLine(addr);
				}
			}
		}

		#endregion

		#region custom_ram
		// ------- CUSTOM OVERLOADS FOR RAM METHODS USING VAT (WARNING SLOW!)-------

		//Write methods
        public void Write(uint address, byte value, PageDirectoryEntry entry)
        {
            ram.Write(getVAT(address,entry),value);
        }

        public void Write(uint address, int value, PageDirectoryEntry entry)
        {
            byte[] convertedValues = BitConverter.GetBytes(value);
            ram.Write(getVAT(address + 0,entry),convertedValues[0]);
            ram.Write(getVAT(address + 1,entry),convertedValues[1]);
            ram.Write(getVAT(address + 2,entry),convertedValues[2]);
            ram.Write(getVAT(address + 3,entry),convertedValues[3]);
        }

        public void Write(uint address, uint value, PageDirectoryEntry entry)
        {
            byte[] convertedValues = BitConverter.GetBytes(value);
            ram.Write(getVAT(address + 0,entry),convertedValues[0]);
            ram.Write(getVAT(address + 1,entry),convertedValues[1]);
            ram.Write(getVAT(address + 2,entry),convertedValues[2]);
            ram.Write(getVAT(address + 3,entry),convertedValues[3]);
        }

        public void Write(uint address, float value, PageDirectoryEntry entry)
        {
            byte[] convertedValues = BitConverter.GetBytes(value);
            ram.Write(getVAT(address + 0,entry),convertedValues[0]);
            ram.Write(getVAT(address + 1,entry),convertedValues[1]);
            ram.Write(getVAT(address + 2,entry),convertedValues[2]);
            ram.Write(getVAT(address + 3,entry),convertedValues[3]);
        }

        public void Write(uint address, string value, PageDirectoryEntry entry)
        {
            Write((uint)(address), (uint)value.Length, entry);
			address += 4;
            for (int i = 0; i < value.Length; i++)
            {
                Write((uint)(address + i), ((byte)value[i]), entry);
            }
        }

        public byte Read(uint address, PageDirectoryEntry entry)
        {
            return ram.Read(getVAT(address,entry));
        }

        internal ushort ReadUInt16(uint address, PageDirectoryEntry entry)
        {
            return ram.ReadUInt16(getVAT(address,entry));
        }

        public int ReadInt(uint address, PageDirectoryEntry entry)
        {
            return ram.ReadInt(getVAT(address,entry));
        }

        public uint ReadUInt(uint address, PageDirectoryEntry entry)
        {
            return ram.ReadUInt(getVAT(address,entry));
        }

        public float ReadFloat(uint address, PageDirectoryEntry entry)
        {
            return ram.ReadFloat(getVAT(address,entry));
        }

        public string ReadString(uint address, PageDirectoryEntry entry)
        {
			int len = ReadInt(address, entry);
			address += 4;
			char[] chr = new char[len];
			for (int i = 0; i < len; i++) {
				chr[i] = (char)Read ((uint)(address + i), entry);
			}
			return new String(chr);
        }

		public void LoadProgram (byte[] data, PageDirectoryEntry entry)
		{
			uint origin = getVAT (0u, entry);
			if (checkVAT (0u, (uint)data.Length, entry)) { //does the program fit in the first page?
				for (int i = 0; i < data.Length; i++) {
					ram.Write ((uint)(origin + i), data[i]); //this should be safe
				}
			} else { //oooh boy

				//get length of program
				uint length = (uint)data.Length;
				uint writtenbytes = 0;
				//loop through pages
				for (int i = 0; i < GetPageDirectoryEntrySize(entry); i++) {
					//get page start
					uint origio = getVAT(writtenbytes,entry);
					//get length of current page
					uint curlen = (uint)(i == 0 ? Frame.FRAME_SIZE - PAGE_TABLE_SIZE : Frame.FRAME_SIZE);
					//write through each page
					for (int j = 0; j < curlen; j++) {
						ram.Write((uint)(origio + j), data[writtenbytes]);
						writtenbytes++;
					}
				}
			}
		}

		#endregion

		// ---- MEMORY ALLOC STUFF ---- all memory allocation is done with relative addresses
		#region memory_allocation

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
		public void SetupMemory(PageDirectoryEntry entry, uint stack_start, uint heap_start) {
			entry.stack_pointer = heap_start - 1;
			entry.base_pointer = stack_start;
			entry.heap_pointer = heap_start;
		}

		/// <summary>
		/// Setup the heap memory allocation for the page table
		/// </summary>
		/// <param name='entry'>
		/// Page table to setup
		/// </param>
		public void SetupMemoryAllocation(PageDirectoryEntry entry) {
			entry.free_pointer = entry.heap_pointer; //set free-list pointer to start of heap
			ram.Write(getVAT(entry.heap_pointer,entry),0); // write 0 because there is no next block
			ram.Write(getVAT(entry.heap_pointer,entry) + 4,(uint)(GetPageDirectoryEntrySize(entry) - entry.heap_pointer)); //Write size of free block (total size of heap)
		}

		public void DumpFreeList(PageDirectoryEntry entry) {
			uint addr = entry.free_pointer;
			while (addr != 0) {
				uint size = ram.ReadUInt(getVAT(addr + 4,entry));
				Console.WriteLine(addr + " - " + size);
				addr = ram.ReadUInt(getVAT(addr,entry));
			}
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
			bool isfirst = true;
			//loop through freelist , check for large enough chunk
			for (uint addr = entry.free_pointer; addr < GetPageDirectoryEntrySize(entry); ) {
				uint nextblock = ram.ReadUInt(getVAT(addr, entry)); //read address of next free block
				uint blocksize = ram.ReadUInt(getVAT(addr + 4, entry)); //read size of current free block
				if (blocksize >= size) {
					uint newsize = blocksize - size; //get new size of free block
					uint newaddr = addr + size; //get new address of free block
					if (newsize >= 8) { //if size < 8, block is gone cause we can't store the next block pointer
						ram.Write(getVAT(newaddr,entry),nextblock); // set next block pointer
						ram.Write(getVAT(newaddr + 4,entry),newsize); //write new size of new freeblock
						ram.Write(getVAT(lastaddr,entry),getVAT(newaddr,entry)); //set pointer of last block to point to this new one
						if (isfirst) {
							entry.free_pointer = newaddr; //update first freepointer
						}
					}
					else //set pointer to skip unexisting block
					{
						ram.Write(getVAT(lastaddr,entry),getVAT(nextblock,entry)); 
					}
					return addr; //return address of allocated space
				}
				if (nextblock == 0) { // there is no next block, break from loop
					break;
				}
				addr = nextblock;
				isfirst = false;
			}
			throw new OutOfMemoryException(String.Format("No more free blocks to allocate memory in page table: {0}",entry.PTAddress));
		}

		public void free(uint address,uint size, PageDirectoryEntry entry)
		{
			Console.WriteLine("freeing memory at address: " + address + " with size: " + size);
			uint lastaddr = entry.free_pointer;
			//bool isfirst = true;
			//check if block we are freeing is before the first in the freelist
			if (address < lastaddr) {
				Console.WriteLine("Address is less than " + lastaddr);
				if(address + 8 < lastaddr) {
					entry.free_pointer = address; //set first freelist pointer to be this block
					ram.Write(getVAT(address,entry),lastaddr); //write previous freelist pointer as the next one
					ram.Write(getVAT(address + 4, entry), size); //write size of new block
					MergeFreeBlocks(entry);
					return; //job done!
				} else { //merge these blocks because they collide otherwise
					Console.WriteLine("Free blocks adjecent, merging...");
					uint nextaddr = ram.ReadUInt(getVAT(lastaddr,entry)); //get the address of the next block
					uint lastsize = ram.ReadUInt(getVAT(lastaddr + 4,entry)); //read size of previous first block
					ram.Write(getVAT(address,entry),nextaddr); //write address of next block
					uint newsize = size + lastsize;
					ram.Write(getVAT(address,entry),newsize); //write new size of merged freeblock
					MergeFreeBlocks(entry);
					return; //done!
				}
			}
			//if not this is the first one, complicated shit needs to happen:
			uint finaladdr = entry.free_pointer;
			//loop through freelist to find where we need to put the pointers
			for (uint addr = entry.free_pointer; addr < GetPageDirectoryEntrySize(entry);) {
				uint nextblock = ram.ReadUInt(getVAT(addr, entry)); //read address of next free block
				if (address < nextblock && address > addr) { //block is between current and next block
					Console.WriteLine("address between " + lastaddr + " and " + nextblock);
					if(address + 8 < nextblock) {
						ram.Write(getVAT(addr,entry), address); //write new pointer to current block
						ram.Write(getVAT(address,entry), nextblock); //write pointer to next block
						ram.Write(getVAT(address + 4,entry), size); //write size of new block
						MergeFreeBlocks(entry);
						return; //done!
					} else { //need to merge again...
						Console.WriteLine("Free blocks adjecent, merging...");
						uint nextsize = ram.ReadUInt(getVAT(nextblock + 4,entry)); //read size of next block
						uint dablockafter = ram.ReadUInt(getVAT(nextblock,entry)); //read address of block after next block
						uint newsize = size + nextsize;
						ram.Write(getVAT(address,entry),dablockafter); //write pointer to block after next block
						ram.Write(getVAT(address + 4,entry),newsize); //write new size
						ram.Write(getVAT(addr,entry), address); //write pointer to new block
						MergeFreeBlocks(entry);
						return; //done!
					}
				}
				addr = nextblock;
				finaladdr = addr;
			}

			//address is after last block
			if (address > finaladdr) {
				if(address > finaladdr + 8) {
					ram.Write(getVAT(lastaddr,entry),address); //write pointer to new block
					ram.Write(getVAT(address,entry),0); //write null pointer
					ram.Write(getVAT(address + 4,entry),size); //write size of new pointer
					MergeFreeBlocks(entry);
				} else { //merge
					uint finalsize = ram.ReadUInt(getVAT(finaladdr + 4,entry));
					uint newsize = finalsize + size;
					ram.Write(getVAT(finaladdr + 4,entry),newsize); //expand previous block
					MergeFreeBlocks(entry);
				}
			}
		}

		public void MergeFreeBlocks(PageDirectoryEntry entry) {
			for (uint addr = entry.free_pointer; addr < GetPageDirectoryEntrySize(entry);) {
				uint nextblock = ram.ReadUInt(getVAT(addr, entry)); //read address of next free block
				if(nextblock == 0) {
					break; //end of list
				}
				uint blocksize = ram.ReadUInt(getVAT(addr + 4, entry)); //read size of current free block
				if(addr + blocksize >= nextblock) {
					uint nextsize = ram.ReadUInt(getVAT(nextblock + 4, entry)); //read size of next block
					uint newsize = blocksize + nextsize;
					uint nextpointer = ram.ReadUInt(getVAT(nextblock, entry));
					ram.Write(getVAT(addr,entry),nextpointer);
					ram.Write(getVAT(addr + 4,entry),newsize);
				}
				addr = nextblock;
			}
		}
		#endregion

		//-------- STACK OPERATIONS --------
		#region stack
		public void Push(byte data,PageDirectoryEntry entry) {
			if (entry.stack_pointer -1 < entry.base_pointer) {
				throw new Exception("No more space in stack!");
			}
			entry.stack_pointer--;
			ram.Write(getVAT(entry.stack_pointer,entry),data);
		}
		public void Push(int data,PageDirectoryEntry entry) {
			if (entry.stack_pointer - 4 < entry.base_pointer) {
				throw new Exception("No more space in stack!");
			}
			entry.stack_pointer -= 4;
			ram.Write(getVAT(entry.stack_pointer,entry),data);
		}
		public void Push(uint data,PageDirectoryEntry entry) {
			if (entry.stack_pointer - 4 < entry.base_pointer) {
				throw new Exception("No more space in stack!");
			}
			entry.stack_pointer -= 4;
			ram.Write(getVAT(entry.stack_pointer,entry),data);
		}
		public void Push(float data,PageDirectoryEntry entry) {
			if (entry.stack_pointer - 4 < entry.base_pointer) {
				throw new Exception("No more space in stack!");
			}
			entry.stack_pointer -= 4;
			ram.Write(getVAT(entry.stack_pointer,entry),data);
		}
		public void Push(string data,PageDirectoryEntry entry) {
			if (entry.stack_pointer - (data.Length + 4) < entry.base_pointer) {
				throw new Exception("No more space in stack!");
			}
			entry.stack_pointer -= (uint)(data.Length + 4);
			if(checkVAT(entry.stack_pointer,(uint)(data.Length + 4),entry)) { //check for overflow to next page
				ram.Write(getVAT(entry.stack_pointer,entry),data);
			} else {
				Write(entry.stack_pointer,data,entry); //use paging method to write, slower
			}
		}

		public byte PopByte(PageDirectoryEntry entry) {
			if (entry.stack_pointer + 1 >= entry.heap_pointer) {
				throw new Exception("No more items in stack!");
			}
			byte val = ram.Read(getVAT(entry.stack_pointer,entry));
			entry.stack_pointer++;
			return val;
		}
		public int PopInt(PageDirectoryEntry entry) {
			if (entry.stack_pointer + 4 >= entry.heap_pointer) {
				throw new Exception("No more items in stack!");
			}
			int val = ram.ReadInt(getVAT(entry.stack_pointer,entry));
			entry.stack_pointer += 4;
			return val;
		}
		public uint PopUInt(PageDirectoryEntry entry) {
			if (entry.stack_pointer + 4 >= entry.heap_pointer) {
				throw new Exception("No more items in stack!");
			}
			uint val = ram.ReadUInt(getVAT(entry.stack_pointer,entry));
			entry.stack_pointer += 4;
			return val;
		}
		public float PopFloat(PageDirectoryEntry entry) {
			if (entry.stack_pointer + 4 >= entry.heap_pointer) {
				throw new Exception("No more items in stack!");
			}
			float val = ram.ReadFloat(getVAT(entry.stack_pointer,entry));
			entry.stack_pointer += 4;
			return val;
		}
		public string PopString (PageDirectoryEntry entry) {
			uint size = ram.ReadUInt(getVAT(entry.stack_pointer,entry));
			if (entry.stack_pointer + size + 4 >= entry.heap_pointer) {
				throw new Exception("No more items in stack!");
			}
			string val = "";
			if(checkVAT(entry.stack_pointer,size,entry)) { //check for page overflow
				val = ram.ReadString(getVAT(entry.stack_pointer,entry)); 
			} else {
				val = ReadString(entry.stack_pointer,entry); //use method for safe writing under page overflow
			}
			entry.stack_pointer	+= size + 4;
			return val;
		}

		#endregion
	}
}