
using System;
using System.Collections.Generic;

using Common;

namespace Interop.SQLite
{
	public class SQLite3
		: IDisposable
	{

		private SQLite3Handle _db;
		private SQLite3Transaction _transaction;


		public SQLite3(string filename)
		{
			_db = SQLite3Helper.Open(filename);
		}

		#region Query Methods

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
			where T : new()
		{
			return new DisposableEnumerable<T>(QueryEnumerator<T>(sql));
		}

		#endregion

		public SQLite3Transaction BeginTransaction()
		{
			return _transaction = new SQLite3Transaction(this);
		}

		public void Close()
		{
			if (InTransaction)
			{
				_transaction.Dispose();
				_transaction = null;
			}

			SQLite3Helper.Close(_db);
		}

		#region Internal Members

		internal bool InTransaction
		{
			get
			{
				return
					_transaction != null &&
					_transaction.IsValid;
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (_db == null)
				return;

			Close();
			_db.Dispose();
			_db = null;
		}

		#endregion

	}
}
