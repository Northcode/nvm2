using System;

namespace nvm2
{
	public class VirtualDataDisk : StorageDevice
	{
		byte[] data;

		public VirtualDataDisk ()
		{
		}

		public override byte[] GetData ()
		{
			return data;
		}

		public void Load (byte[] Data)
		{
			data = Data;
		}
	}
}

