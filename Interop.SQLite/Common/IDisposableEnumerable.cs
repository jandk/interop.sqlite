
using System;
using System.Collections.Generic;

namespace Common
{
	public interface IDisposableEnumerable<T>
		: IDisposable, IEnumerable<T>
	{
	}
}
