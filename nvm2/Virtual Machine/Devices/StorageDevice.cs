using System;

namespace nvm2
{
	public class StorageDevice : IIODevice
	{
		public StorageDevice ()
		{
		}

		public virtual byte[] GetData() { return new byte[0]; }

		public DeviceType GetDeviceType ()
		{
			return DeviceType.storage;
		}
	}
}

