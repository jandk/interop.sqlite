
using System;
using System.Runtime.InteropServices;

namespace Interop.SQLite
{
	internal static class NativeMethods
	{
		const string DllName = "sqlite3.dll";

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_close")]
		public static extern SQLite3Error Close(IntPtr db);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_blob")]
		public static extern IntPtr ColumnBlob(IntPtr statement, int n);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_bytes")]
		public static extern int ColumnBytes(IntPtr statement, int n);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_count")]
		public static extern int ColumnCount(IntPtr statement);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_double")]
		public static extern double ColumnDouble(IntPtr statement, int n);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_int")]
		public static extern int ColumnInt(IntPtr statement, int n);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_int64")]
		public static extern long ColumnInt64(IntPtr statement, int n);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_name")]
		public static extern IntPtr ColumnName(IntPtr statement, int n);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_text")]
		public static extern IntPtr ColumnText(IntPtr statement, int n);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_type")]
		public static extern SQLite3ColumnType ColumnType(IntPtr statement, int n);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_db_handle")]
		public static extern IntPtr DbHandle(IntPtr statement);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_errcode")]
		public static extern SQLite3Error ErrCode(IntPtr db);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_errmsg")]
		public static extern IntPtr ErrMsg(IntPtr db);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_finalize")]
		public static extern SQLite3Error Finalize(IntPtr statement);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_open")]
		public static extern SQLite3Error Open(byte[] utf8Filename, out IntPtr db);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_open_v2")]
		public static extern SQLite3Error OpenV2(byte[] utf8Filename, out IntPtr db, SQLite3OpenFlags flags, IntPtr vfs);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_prepare")]
		public static extern SQLite3Error Prepare(IntPtr db, byte[] utf8Sql, int sqlLength, out IntPtr statement, out IntPtr unusedSql);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_prepare_v2")]
		public static extern SQLite3Error PrepareV2(IntPtr db, byte[] utf8Sql, int sqlLength, out IntPtr statement, out IntPtr unusedSql);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_reset")]
		public static extern SQLite3Error Reset(IntPtr statement);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_step")]
		public static extern SQLite3Error Step(IntPtr statement);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_libversion")]
		public static extern IntPtr LibVersion();

		//[DllImport(DllName, CharSet = CharSet.Unicode, EntryPoint = "sqlite3_create_function16")]
		//public static extern int CreateFunction16(IntPtr db, string zFunctionName,int nArg,SQLite3TextEncoding eTextRep,IntPtr pApp,);
	}
}
