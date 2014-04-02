using System;
using System.IO;

namespace nvm2
{
	class VirtualROMDisk : StorageDevice
	{
		string filename;

		public VirtualROMDisk(string file) {
			filename = file;
		}

		public override byte[] GetData() {
			return File.ReadAllBytes(filename);
		}
	}
}