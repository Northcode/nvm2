using System;

namespace nvm2
{
	public class vm
	{
		public const int CALL_STACK_SIZE = 2048; // Size of callstack in bytes;
		public const int PAGE_DIRECTORY_SIZE = 16; //Number of allowed page tables;

		VMDevice[] devices;

		//Device shortcuts
		internal CPU cpu;
		internal Memory ram;
		internal HDI hdi;
		internal VirtualDataDisk disk0;
		
		//Registers
		public byte A; // "action" register, used in interupts
		public byte B; // "char" register, used to hold byte sized data
		public byte E; // result register, stored to by math functions
		public uint DA; // Data address register A
		public uint DB; // Data address register B
		public int AX; // Storage register A, used to hold ints
		public int BX; // Storage register B, used to hold ints
		public int EX; // result register
		public float EAX; // Extended storage register A, used for floats
		public float EBX; // Extended storage register B, used for floats
		public float EEX; // Extended result register

		//Special register
		public uint IP; // Instruction pointer register
		public bool CP; // Comparator register
		public uint SP;	// Stack pointer
		public uint HP; // Heap pointer
		public uint BP; // Base stack pointer
		public uint FP; // Frame pointer
		public PageDirectoryEntry CR3; // Current page table register
		public int CR3I; //Current page table index register
		public int BPG; //Base page table pointer
		public uint CSP; //Callstack pointer
		public uint CBP; //Callstack base pointer
		public bool RN; //Run register (true for running/false for terminated)
		public bool BRK; //Break register (halt until user presses ENTER)

		// Helper classes
		internal CallStack callstack;
		internal Pager pager;

		public vm ()
		{
			//Initialize devices;
			cpu = new CPU(this); // Initializes the cpu
			ram = new Memory(PAGE_DIRECTORY_SIZE * Frame.FRAME_SIZE); //Allocates enough ram to fit number of page tables allowed
			hdi = new HDI("disk0"); //Maps a folder to the hard disk interface
			disk0 = new VirtualDataDisk(); // Virtual data disk

			devices = new VMDevice[] {
				cpu, // Processer, device 0
				ram, // RAM, device 1
				hdi, // Hard drive Interface, device 3
				disk0, // Virtual Data Disk, device 4
			};

			callstack = new CallStack(this,ram);
			pager = new Pager(ram, PAGE_DIRECTORY_SIZE);

			//setup premade page for callstack and bios
			BPG = pager.CreatePageEntry(Pager.PAGE_KERNEL_MODE);
			CSP = pager.getVAT(1024,pager.getEntry(BPG)); //bios will be loaded at 0 to 1024
			CBP = CSP;
			CR3I = BPG;
			CR3 = pager.getEntry(CR3I);
		}

		/// <summary>
		/// Loads a new device to the device list
		/// </summary>
		/// <param name='device'>
		/// Device to load.
		/// </param>
		public void LoadDevice (VMDevice device) 
		{
			VMDevice[] ndevices = new VMDevice[devices.Length + 1];
			devices.CopyTo(ndevices,0);
			ndevices[ndevices.Length - 1] = device;
			devices = ndevices;
		}

		public VMDevice GetDevice(int device) {
			if (device > 0 && device < devices.Length) {
				return devices[device];
			}
			throw new ArgumentOutOfRangeException("Device does not exist: " + device.ToString());
		}

		public void LoadProgram (byte[] data)
		{
			CR3I = pager.CreatePageEntry(Pager.PAGE_USER_MODE);
			CR3 = pager.getEntry(CR3I);
			pager.LoadProgram(data,CR3);
		}

		public void SwitchPage (int newpage)
		{
			CR3I = newpage;
			CR3 = pager.getEntry(CR3I);
			SP = CR3.stack_pointer;
			HP = CR3.heap_pointer;
			BP = CR3.base_pointer;
		}
	}
}

