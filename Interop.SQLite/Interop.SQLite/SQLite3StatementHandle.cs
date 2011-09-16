
using System;
using System.Runtime.InteropServices;

namespace Interop.SQLite
{
	internal class SQLite3StatementHandle
		: SafeHandle
	{
		public override bool IsInvalid
		{
			get { return handle == IntPtr.Zero; }
		}

		internal SQLite3StatementHandle()
			: base(IntPtr.Zero, true)
		{
		}

		private SQLite3StatementHandle(IntPtr stmt)
			: this()
		{
			SetHandle(stmt);
		}

		public static implicit operator IntPtr(SQLite3StatementHandle stmt)
		{
			return stmt.handle;
		}

		public static implicit operator SQLite3StatementHandle(IntPtr stmt)
		{
			return new SQLite3StatementHandle(stmt);
		}

		protected override bool ReleaseHandle()
		{
			try
			{
				SQLite3Helper.Finalize(this);
			}
			catch (SQLite3Exception) { }

			return true;
		}
	}
}
