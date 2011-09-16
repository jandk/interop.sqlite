
using System.Collections;
using System.Collections.Generic;

namespace Common
{
	public class DisposableEnumerable<T>
		: IDisposableEnumerable<T>
	{
		private readonly IEnumerator<T> _enumerator;

		public DisposableEnumerable(IEnumerator<T> enumerator)
		{
			_enumerator = enumerator;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _enumerator;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Dispose()
		{
			_enumerator.Dispose();
		}
	}
}
