using System;

namespace Battle.Pachinko.BallBehaviours
{
	[Flags]
	public enum AssemballType
	{
		NONE = 0,
		RED = 2,
		YELLOW = 4,
		WHITE = 8,
		GREEN = 0x10,
		PINK = 0x20,
		ALL = RED | YELLOW | WHITE | GREEN | PINK
	}
}
