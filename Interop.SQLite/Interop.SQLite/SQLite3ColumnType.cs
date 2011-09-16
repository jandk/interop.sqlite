
using System;

namespace Interop.SQLite
{
	internal enum SQLite3ColumnType
		: int
	{
		Integer = 1,
		Float = 2,
		Text = 3,
		Blob = 4,
		Null = 5,
	}
}
