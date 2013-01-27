
using System;
using System.Runtime.InteropServices;

namespace Interop.SQLite
{
	internal class SQLite3Handle
		: SafeHandle
	{
		public override bool IsInvalid
		{
			get { return handle == IntPtr.Zero; }
		}

		internal SQLite3Handle()
			: base(IntPtr.Zero, true)
		{
		}

		private SQLite3Handle(IntPtr db)
			: this()
		{
			SetHandle(db);
		}

		protected override bool ReleaseHandle()
		{
			try
			{
				SQLite3Helper.Close(this);
			}
			catch (SQLite3Exception)
			{
			}
			return true;
		}

		public static implicit operator IntPtr(SQLite3Handle db)
		{
			return db.handle;
		}

		public static implicit operator SQLite3Handle(IntPtr db)
		{
			return new SQLite3Handle(db);
		}
	}
}
