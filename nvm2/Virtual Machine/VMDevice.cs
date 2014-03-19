using System;

namespace nvm2
{
	public enum DeviceType
	{
		computation,
		storage,
		other
	}

	public interface VMDevice
	{
		DeviceType GetDeviceType();


	}
}

