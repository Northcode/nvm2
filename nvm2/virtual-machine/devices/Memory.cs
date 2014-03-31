using System;
using System.Text;
using System.Collections.Generic;

namespace nvm2
{
	public class Frame
	{
		public const int FRAME_SIZE = 4 * 1024;

		Memory ram;

		public Frame (uint Address,Memory RAM)
		{
			this.Address = Address;
			this.ram = RAM;
		}

		public uint Address {get;set;}
		public bool IsFree { get; set; }
	}

	public class Memory : StorageDevice
	{
		byte[] data;
		uint m_pointer;
		Frame[] frames;

		public Memory (int size)
		{
			data = new byte[size];
			m_pointer = 0;
			frames = new Frame[size/Frame.FRAME_SIZE];
			for (int i = 0; i < frames.Length; i++) {
				frames[i] = new Frame((uint)(i * Frame.FRAME_SIZE),this) { IsFree = true };
			}
		}

		// ------ FRAME MANAGEMENT ------

		public Frame getFrame (int i)
		{
			return frames[i];
		}

		public Frame findFreeFrame ()
		{
			for (int i = 0; i < frames.Length; i++) {
				if(frames[i].IsFree)
					return frames[i];
			}
			throw new Exception("No free frame found!");
		}

		// ------ READ / WRITE FUNCTIONS ----

		// Overloads using memory pointer
		public void Write (byte value)	{ Write (m_pointer,value); m_pointer++; }
		public void Write (int value)	{ Write (m_pointer,value); m_pointer += 4; }
		public void Write (uint value)	{ Write (m_pointer,value); m_pointer += 4; }
		public void Write (float value)	{ Write (m_pointer,value); m_pointer += 4; }
		public void Write (string value)	{ Write (m_pointer,value); m_pointer += (uint)(value.Length + 1); }

		public byte Read () { byte b = Read (m_pointer); m_pointer++; return b; }
		public int ReadInt () { int i = ReadInt (m_pointer); m_pointer += 4; return i; }
		public uint ReadUInt () { uint u = ReadUInt (m_pointer); m_pointer += 4; return u; }
		public float ReadFloat () { float f = ReadFloat (m_pointer); m_pointer += 4; return f; }
		public string ReadString () { string s = ReadString (m_pointer); m_pointer += (uint)s.Length; return s; }

		 //Write methods
        public void Write(uint address, byte value)
        {
            data[address] = value;
        }

        public void Write(uint address, int value)
        {
            byte[] convertedValues = BitConverter.GetBytes(value);
            data[address + 0] = convertedValues[0];
            data[address + 1] = convertedValues[1];
            data[address + 2] = convertedValues[2];
            data[address + 3] = convertedValues[3];
        }

        public void Write(uint address, uint value)
        {
            byte[] convertedValues = BitConverter.GetBytes(value);
            data[address + 0] = convertedValues[0];
            data[address + 1] = convertedValues[1];
            data[address + 2] = convertedValues[2];
            data[address + 3] = convertedValues[3];
        }

        public void Write(uint address, float value)
        {
            byte[] convertedValues = BitConverter.GetBytes(value);
            data[address + 0] = convertedValues[0];
            data[address + 1] = convertedValues[1];
            data[address + 2] = convertedValues[2];
            data[address + 3] = convertedValues[3];
        }

        public void Write(uint address, string value)
        {
            Write((uint)(address), (uint)value.Length);
			address += 4;
            for (int i = 0; i < value.Length; i++)
            {
                Write((uint)(address + i), ((byte)value[i]));
            }
        }

        public byte Read(uint address)
        {
            return data[address];
        }

        internal ushort ReadUInt16(uint address)
        {
            return BitConverter.ToUInt16(data, (int)address);
        }

        public int ReadInt(uint address)
        {
            return BitConverter.ToInt32(data, (int)address);
        }

        public uint ReadUInt(uint address)
        {
            return BitConverter.ToUInt32(data, (int)address);
        }

        public float ReadFloat(uint address)
        {
            return BitConverter.ToSingle(data, (int)address);
        }

        public string ReadString(uint address)
        {
			int len = ReadInt(address);
			address += 4;
			char[] chr = new char[len];
			for (int i = 0; i < len; i++) {
				chr[i] = (char)Read ((uint)(address + i));
			}
			return new String(chr);
        }
	}
}

