
using System;
using System.Collections.Generic;

namespace Interop.SQLite.Common
{
	public interface IDisposableEnumerable<T>
		: IDisposable, IEnumerable<T>
	{
	}
}
