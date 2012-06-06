
using System;
using System.Collections.Generic;

using Common;

namespace Interop.SQLite
{
	public class SQLite3
		: IDisposable
	{

		private SQLite3Handle _db;
		private List<SQLite3StatementHandle> _statementHandles;
		// private List<SQLite3Blob> _blobs;
		protected bool _disposed;


		public SQLite3(string filename)
		{
			_db = SQLite3Helper.Open(filename);
			_statementHandles = new List<SQLite3StatementHandle>();
		}


		public void Query(string sql)
		{
			if (String.IsNullOrEmpty(sql))
				throw new ArgumentNullException("sql");

			using (SQLite3StatementHandle statement = SQLite3Helper.Prepare(_db, sql))
				SQLite3Helper.Step(statement);
		}

		public SQLite3Result QueryReader(string sql)
		{
			if (String.IsNullOrEmpty(sql))
				throw new ArgumentNullException("sql");

			SQLite3StatementHandle statement = SQLite3Helper.Prepare(_db, sql);
			return new SQLite3Result(statement);
		}

		public IEnumerator<T> QueryEnumerator<T>(string sql)
			where T : new()
		{
			if (String.IsNullOrEmpty(sql))
				throw new ArgumentNullException("sql");

			SQLite3StatementHandle statement = SQLite3Helper.Prepare(_db, sql);
			return new SQLite3Enumerator<T>(statement);
		}
		
		public IDisposableEnumerable<T> QueryEnumerable<T>(string sql)
			where T: new()
		{
			return new DisposableEnumerable<T>(QueryEnumerator<T>(sql));
		}


		public void Close()
		{
			SQLite3Helper.Close(_db);
		}


		public void Dispose()
		{
			if (_db != null)
				_db.Dispose();

			_db = null;
		}
	}
}
