
using System;

namespace Interop.SQLite
{
	public class SQLite3Transaction
		: IDisposable
	{

		private readonly SQLite3 _connection;
		// TODO: Check for a better way
		private bool _isValid = true;

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

		/// <summary>
		///  A boolean indicating if the transaction is valid or not.
		/// </summary>
		public bool IsValid
		{
			get { return _isValid; }
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
			_isValid = false;
		}

		/// <summary>
		///  Rollback the changes made.
		/// </summary>
		public void RollBack()
		{
			if (!_connection.InTransaction)
				throw new SQLite3Exception("No transaction present");

			_connection.Query("ROLLBACK");
			_isValid = false;
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (_isValid)
				RollBack();
		}

		#endregion

	}
}
