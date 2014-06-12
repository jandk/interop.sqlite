using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Interop.SQLite
{
    internal class SQLite3Enumerator<T>
        : IEnumerator<T>
        where T : new()
    {
        protected readonly int _columnCount;
        protected readonly IObjectHydrater<T> _hydrater;
        protected readonly SQLite3StatementHandle _statement;
        protected readonly SetterMethod<T>[] _setters;

        internal SQLite3Enumerator(SQLite3StatementHandle statement)
        {
            _statement = statement;
            _columnCount = NativeMethods.ColumnCount(_statement);
            _setters = new SetterMethod<T>[_columnCount];

            var propertyNames = typeof(T).GetProperties().Where(p => p.CanWrite).Select(pi => pi.Name.ToLowerInvariant()).ToList();

            for (int i = 0; i < _columnCount; i++)
            {
                string columnName = SQLite3Helper.ColumnName(_statement, i).ToLowerInvariant();
                if (!propertyNames.Contains(columnName))
                    throw new SQLite3Exception("The object does not contain this property");
            }

            for (int i = 0; i < _columnCount; i++)
            {
                string columnName = SQLite3Helper.ColumnName(_statement, i);
                if (!SetterMethods.ContainsKey(columnName))
                    throw new SQLite3Exception("The object does not contain this property");

                _setters[i] = SetterMethods[columnName];
            }
        }

        #region IEnumerator<T> Members

        public T Current
        {
            get
            {
                var value = new T();
                for (int i = 0; i < _columnCount; i++)
                    _setters[i](value, _statementHandle, i);

                return value;
            }
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            return SQLite3Helper.Step(_statement) == SQLite3Error.Row;
        }

        public void Reset()
        {
            SQLite3Helper.Reset(_statement);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _statement.Dispose();
        }

        #endregion
    }
}