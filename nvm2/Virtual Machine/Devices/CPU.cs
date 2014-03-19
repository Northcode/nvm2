using System;

namespace nvm2
{
	class CPU : ComputeDevice
	{
		vm machine;

		public CPU (vm Machine)
		{
			this.machine = Machine;
		}
	}

}

