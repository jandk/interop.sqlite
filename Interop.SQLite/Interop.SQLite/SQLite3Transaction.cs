
using System;

namespace Interop.SQLite
{
	public class SQLite3Transaction
		: IDisposable
	{

		private SQLite3 _connection;

		#region Constructor

		/// <summary>
		///  Create a new <see cref="SQLite3Transaction"/>
		/// </summary>
		/// <param name="connection">
		///  The <see cref="SQLite3"/> connection on which to create the transaction
		/// </param>
		internal SQLite3Transaction(SQLite3 connection)
		{
			if (connection == null)
				throw new ArgumentNullException("connection");

			if (connection.InTransaction)
				throw new SQLite3Exception("There is a transaction present");

			_connection = connection;
			_connection.Query("BEGIN");
		}

		#endregion

		#region Properties

		/// <summary>
		///  The <see cref="SQLite3"/> connection on which the transaction was made.
		/// </summary>
		public SQLite3 Connection
		{
			get { return _connection; }
		}

		#endregion

		#region Methods

		/// <summary>
		///  Commit the changes to the database.
		/// </summary>
		public void Commit()
		{
			if (!_connection.InTransaction)
				throw new SQLite3Exception("No transaction present");

			_connection.Query("COMMIT");
		}

		/// <summary>
		///  Rollback the changes made.
		/// </summary>
		public void RollBack()
		{
			if (!_connection.InTransaction)
				throw new SQLite3Exception("No transaction present");

			_connection.Query("ROLLBACK");
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			RollBack();
		}

		#endregion

	}
}
