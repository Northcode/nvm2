using System;

namespace nvm2
{
	public class vm
	{
		public const int CALL_STACK_SIZE = 512; // Size of callstack in bytes;

		VMDevice[] devices;

		//Device shortcuts
		CPU cpu;
		Memory ram;
		HDI hdi;
		VirtualDataDisk disk0;

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
		public uint BP; // Base stack pointer (start of callstack)
		public uint FP; // Frame pointer
		public uint CR3; // Current page register

		// Helper classes
		CallStack callstack;

		public vm ()
		{
			//Initialize devices;
			cpu = new CPU(this); // Initializes the cpu
			ram = new Memory(4 * 4 * 1024); // Allocates 16kB of RAM
			hdi = new HDI("disk0"); //Maps a folder to the hard disk interface
			disk0 = new VirtualDataDisk(); // Virtual data disk

			devices = new VMDevice[] {
				cpu, // Processer, device 0
				ram, // RAM, device 1
				hdi, // Hard drive Interface, device 3
				disk0, // Virtual Data Disk, device 4
			};

			callstack = new CallStack(this,ram);
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
	}
}

