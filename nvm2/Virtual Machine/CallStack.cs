using System;

namespace nvm2
{
	public class CallStack
	{
		vm machine;
		Memory ram;

		public CallStack (vm Machine, Memory RAM)
		{
			machine = Machine;
			ram = RAM;
		}

		public uint ReadReturnAddress ()
		{
			return ram.ReadUInt(machine.SP);
		}

		public void Pop ()
		{
			if (machine.SP - 4 > machine.BP) {
				machine.SP -= 4;
			}
		}

		public void Push (uint addr)
		{
			if (machine.SP + 4 < machine.BP + vm.CALL_STACK_SIZE) {
				machine.SP += 4;
				ram.Write(addr,machine.SP);
			}
		}
	}
}

