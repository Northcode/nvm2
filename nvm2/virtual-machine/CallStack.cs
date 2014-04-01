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
			return ram.ReadUInt(machine.CSP);
		}

		public void Pop ()
		{
			if (machine.CSP - 4 > machine.CBP) {
				machine.CSP -= 4;
			}
		}

		public void Push ()
		{
			if (machine.CSP + 4 < machine.CBP + vm.CALL_STACK_SIZE) {
				machine.CSP += 4;
				ram.Write(machine.CSP,machine.IP);
			}
		}
	}
}

