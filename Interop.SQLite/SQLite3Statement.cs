
using System;

namespace Interop.SQLite
{
	public class SQLite3Statement
		: IDisposable
	{
		private bool _disposed;
		private SQLite3StatementHandle _statement;

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);

			// Use SupressFinalize in case a subclass
			// of this type implements a finalizer.
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			// If you need thread safety, use a lock around these 
			// operations, as well as in your methods that use the resource.
			if (!_disposed)
			{
				if (disposing)
				{
					if (_statement != null)
						_statement.Dispose();
				}

				// Indicate that the instance has been disposed.
				_statement = null;
				_disposed = true;
			}
		}

		#endregion

	}
}
