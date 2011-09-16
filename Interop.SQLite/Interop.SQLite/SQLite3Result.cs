
using System;
using System.Collections.Generic;

namespace Interop.SQLite
{
	public class SQLite3Result
		: IDisposable
	{

		private SQLite3StatementHandle _statement;
		private Dictionary<string, int> _columns
			= new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

		internal SQLite3Result(SQLite3StatementHandle statement)
		{
			_statement = statement;

			CacheColumnNames();
		}

		public bool NextRow()
		{
			SQLite3Error state = SQLite3Helper.Step(_statement);

			return state == SQLite3Error.Row;
		}

		#region Methods - Private

		private void CacheColumnNames()
		{
			int columnCount = NativeMethods.ColumnCount(_statement);

			for (int i = 0; i < columnCount; i++)
				_columns.Add(SQLite3Helper.ColumnName(_statement, i), i);
		}

		#endregion

		#region GetValue

		public object GetValue(string name)
		{
			if (!_columns.ContainsKey(name))
				throw new SQLite3Exception("Invalid column name");

			return GetValue(_columns[name]);
		}

		public object GetValue(int index)
		{
			SQLite3ColumnType type = NativeMethods.ColumnType(_statement, index);

			switch (type)
			{
				case SQLite3ColumnType.Integer:
					return NativeMethods.ColumnInt64(_statement, index);
				case SQLite3ColumnType.Float:
					return NativeMethods.ColumnDouble(_statement, index);
				case SQLite3ColumnType.Text:
					return SQLite3Helper.ColumnText(_statement, index);
				case SQLite3ColumnType.Blob:
					return SQLite3Helper.ColumnBlob(_statement, index);
				case SQLite3ColumnType.Null:
					return DBNull.Value;
				default: throw new InvalidOperationException();
			}
		}

		public T GetValue<T>(string name)
		{
			if (!_columns.ContainsKey(name))
				throw new SQLite3Exception("Invalid column name");

			return GetValue<T>(_columns[name]);
		}

		public T GetValue<T>(int index)
		{
			if (index < 0 || index >= _columns.Count)
				throw new ArgumentOutOfRangeException("index");

			SQLite3ColumnType type = NativeMethods.ColumnType(_statement, index);

			return SQLite3Convert.GetConverter<T>(type)(GetValue(index));
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (_statement != null)
				_statement.Dispose();

			_statement = null;
		}

		#endregion

	}
}
