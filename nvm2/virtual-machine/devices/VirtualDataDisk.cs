using System;
using System.IO;

namespace nvm2
{
	public class VirtualDataDisk : StorageDevice
	{
		string filename;
		FileStream stream;

		public VirtualDataDisk (string filename)
		{
			this.filename = filename;
		}

		public override byte[] GetData ()
		{
			return File.ReadAllBytes(filename);
		}
	}
}

