
using System;

namespace Interop.SQLite
{
	[Flags]
	public enum SQLite3OpenFlags
	{
		None = 0,
		ReadOnly = 1 << 0,
		ReadWrite = 1 << 1,
		Create = 1 << 2,
	}
}
