
using System;

namespace Interop.SQLite
{
	[Serializable]
	public class SQLite3Exception : Exception
	{
		private static readonly string[] ErrorMessages = 
		{
			"Successful result",
			"SQL error or missing database",
			"Internal logic error in SQLite",
			"Access permission denied",
			"Callback routine requested an abort",
			"The database file is locked",
			"A table in the database is locked",
			"A malloc() failed",
			"Attempt to write a readonly database",
			"Operation terminated by sqlite3_interrupt()",
			"Some kind of disk I/O error occurred",
			"The database disk image is malformed",
			"NOT USED. Table or record not found",
			"Insertion failed because database is full",
			"Unable to open the database file",
			"Database lock protocol error",
			"Database is empty",
			"The database schema changed",
			"String or BLOB exceeds size limit",
			"Abort due to constraint violation",
			"Data type mismatch",
			"Library used incorrectly",
			"Uses OS features not supported on host",
			"Authorization denied",
			"Auxiliary database format error",
			"2nd parameter to sqlite3_bind out of range",
			"File opened that is not a database file"
		};

		internal SQLite3Exception(string message)
			: base(message)
		{
		}

		internal SQLite3Exception(string message, Exception inner)
			: base(message, inner)
		{
		}

		internal SQLite3Exception(SQLite3Error error)
			: base(ErrorMessages[(int)error])
		{
		}

		internal SQLite3Exception(SQLite3Error error, Exception inner)
			: base(ErrorMessages[(int)error], inner)
		{
		}
	}
}
