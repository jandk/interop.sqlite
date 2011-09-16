
using System;
using System.Runtime.InteropServices;

namespace Interop.SQLite
{
	internal static class SQLite3Helper
	{

		#region Open & Close

		public static SQLite3Handle Open(string filename)
		{
			IntPtr tempHandle;

			SQLite3Error error = NativeMethods.Open(
				SQLite3Convert.StringToUtf8(filename),
				out tempHandle
			);

			CheckError(error);

			return tempHandle;
		}

		public static void Close(SQLite3Handle db)
		{
			// TODO: 1 - Close all prepared statements
			// TODO: 2 - Close all blob handles
			// TODO: 3 - Transactions...
			SQLite3Error error = NativeMethods.Close(db);

			CheckError(error);
		}

		#endregion

		#region Querying

		public static SQLite3StatementHandle Prepare(SQLite3Handle db, string sql)
		{
			byte[] utf8Sql = SQLite3Convert.StringToUtf8(sql);
			IntPtr statement, unused;

			SQLite3Error error = NativeMethods.PrepareV2(
				db,
				utf8Sql,
				utf8Sql.Length,
				out statement,
				out unused
			);

			CheckError(error, db);

			return statement;
		}

		public static SQLite3Error Step(SQLite3StatementHandle statement)
		{
			SQLite3Error error = NativeMethods.Step(statement);

			CheckError(error, statement);

			return error;
		}

		public static void Finalize(SQLite3StatementHandle statement)
		{
			SQLite3Error error = NativeMethods.Finalize(statement);

			CheckError(error, statement);
		}

		#endregion


		public static byte[] ColumnBlob(SQLite3StatementHandle statement, int index)
		{
			return ColumnBlobPtr((IntPtr)statement, index);
		}

		public static byte[] ColumnBlobPtr(IntPtr statement, int index)
		{
			int rawDataSize = NativeMethods.ColumnBytes(statement, index);
			byte[] rawData = new byte[rawDataSize];

			IntPtr rawDataPtr = NativeMethods.ColumnBlob(statement, index);
			Marshal.Copy(rawDataPtr, rawData, 0, rawDataSize);

			return rawData;
		}

		public static string ColumnText(SQLite3StatementHandle statement, int index)
		{
			return ColumnTextPtr((IntPtr)statement, index);
		}

		public static string ColumnTextPtr(IntPtr statement, int index)
		{
			int rawDataSize = NativeMethods.ColumnBytes(statement, index);
			IntPtr rawDataPtr = NativeMethods.ColumnText(statement, index);

			return SQLite3Convert.Utf8ToString(rawDataPtr, rawDataSize);
		}

		public static string ColumnName(SQLite3StatementHandle statement, int index)
		{
			return SQLite3Convert.Utf8ToString(
				NativeMethods.ColumnName(statement, index)
			);
		}

		public static string ErrorMessage(SQLite3Handle db)
		{
			return SQLite3Convert.Utf8ToString(
				NativeMethods.ErrMsg(db)
			);
		}


		#region Error Checking

		static void CheckError(SQLite3Error error)
		{
			CheckError(error, (SQLite3Handle)IntPtr.Zero);
		}

		static void CheckError(SQLite3Error error, SQLite3StatementHandle statement)
		{
			CheckError(error, (SQLite3Handle)NativeMethods.DbHandle(statement));
		}

		static void CheckError(SQLite3Error error, SQLite3Handle db)
		{
			if (error == SQLite3Error.OK ||
				error == SQLite3Error.Row ||
				error == SQLite3Error.Done
			) return;

			if (db != IntPtr.Zero)
				throw new SQLite3Exception(ErrorMessage(db));
			else
				throw new SQLite3Exception(error);
		}

		#endregion

	}
}
