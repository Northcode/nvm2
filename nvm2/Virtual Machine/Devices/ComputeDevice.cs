using System;

namespace nvm2
{
	public class ComputeDevice : VMDevice
	{
		public ComputeDevice ()
		{
		}

		public DeviceType GetDeviceType ()
		{
			return DeviceType.computation;
		}
	}
}

