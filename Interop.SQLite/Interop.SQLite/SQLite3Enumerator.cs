
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Interop.SQLite
{
	delegate void SetterMethod<T>(T value, IntPtr statementHandle, int columnIndex);

	internal class SQLite3Enumerator<T>
		: IEnumerator<T>, IEnumerator, IDisposable
		where T : new()
	{

		#region Static

		private static readonly MethodInfo[] ColumnGetters = new MethodInfo[]
		{
			null,                                             // 0 --
			typeof(NativeMethods).GetMethod("ColumnInt64"),   // 1 SQLite3ColumnType.Integer
            typeof(NativeMethods).GetMethod("ColumnDouble"),  // 2 SQLite3ColumnType.Float
            typeof(SQLite3Helper).GetMethod("ColumnTextPtr"), // 3 SQLite3ColumnType.Text
            typeof(SQLite3Helper).GetMethod("ColumnBlobPtr"), // 4 SQLite3ColumnType.Blob
            null,                                             // 5 SQLite3ColumnType.Null
		};

		private static readonly Dictionary<string, SetterMethod<T>> Setters;

		static SQLite3Enumerator()
		{
			// 1. Get all object properties which can be set
			PropertyInfo[] propertyInfos = typeof(T)
				.GetProperties()
				.Where(prop => prop.CanWrite)
				.ToArray();

			// 2. Create the array with the object setters, and populate it
			Setters = new Dictionary<string, SetterMethod<T>>(StringComparer.InvariantCultureIgnoreCase);
			foreach (PropertyInfo propertyInfo in propertyInfos)
				Setters.Add(propertyInfo.Name, GenerateSetterMethod(propertyInfo));
		}

		private static SetterMethod<T> GenerateSetterMethod(PropertyInfo propertyInfo)
		{
			DynamicMethod setter = new DynamicMethod(
				"_set" + propertyInfo.Name,
				null,
				new[] { typeof(T), typeof(IntPtr), typeof(int) },
				typeof(T),
				true
			);

			ILGenerator generator = setter.GetILGenerator();
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Ldarg_2);
			EmitColumnMethod(generator, propertyInfo.PropertyType);
			EmitCastOpCode(generator, propertyInfo.PropertyType);
			generator.Emit(OpCodes.Call, typeof(T).GetProperty(propertyInfo.Name).GetSetMethod());
			generator.Emit(OpCodes.Ret);

			return (SetterMethod<T>)setter.CreateDelegate(typeof(SetterMethod<T>));
		}

		private static void EmitColumnMethod(ILGenerator generator, Type type)
		{
			TypeCode typeCode = Type.GetTypeCode(type);

			// If we have an object, check for an array
			if (typeCode == TypeCode.Object)
			{
				if (type != typeof(byte[]))
					throw new SQLite3Exception("Invalid property type: " + type.Name);

				generator.Emit(OpCodes.Call, ColumnGetters[(int)SQLite3ColumnType.Blob]);
				return;
			}

			// TODO: Support for Decimal and DateTime and Nullable<T>
			if (typeCode == TypeCode.Decimal || typeCode == TypeCode.DateTime)
				throw new SQLite3Exception("Invalid property type: " + type.Name);

			generator.Emit(
				OpCodes.Call,
				ColumnGetters[(int)SQLite3Convert.TypeCodeAffinities[(int)typeCode]]
			);
		}

		internal static readonly OpCode[] CastOpCodeAffinities = new[]
        {
            OpCodes.Nop,     //  0 TypeCode.Empty
            OpCodes.Nop,     //  1 TypeCode.Object
            OpCodes.Nop,     //  2 TypeCode.DbNull
            OpCodes.Nop,     //  3 TypeCode.Boolean
            OpCodes.Conv_U2, //  4 TypeCode.Char
            OpCodes.Conv_I1, //  5 TypeCode.SByte
            OpCodes.Conv_U1, //  6 TypeCode.Byte
            OpCodes.Conv_I2, //  7 TypeCode.Int16
            OpCodes.Conv_U2, //  8 TypeCode.UInt16
            OpCodes.Conv_I4, //  9 TypeCode.Int32
            OpCodes.Conv_U4, // 10 TypeCode.UInt32
            OpCodes.Nop,     // 11 TypeCode.Int64
            OpCodes.Conv_U8, // 12 TypeCode.UInt64
            OpCodes.Conv_R4, // 13 TypeCode.Single
            OpCodes.Nop,     // 14 TypeCode.Double
            OpCodes.Nop,     // 15 TypeCode.Decimal
            OpCodes.Nop,     // 16 TypeCode.DateTime
            OpCodes.Nop,     // 17 --
            OpCodes.Nop,     // 18 TypeCode.String
        };

		private static void EmitCastOpCode(ILGenerator generator, Type type)
		{
			if (type == typeof(byte[]))
			{
				generator.Emit(OpCodes.Unbox_Any, typeof(byte[]));
				return;
			}

			TypeCode typeCode = Type.GetTypeCode(type);
			OpCode opCode = CastOpCodeAffinities[(int)typeCode];

			if (opCode != OpCodes.Nop)
				generator.Emit(opCode);
		}

		#endregion

		protected readonly SQLite3StatementHandle _statement;
		protected readonly IntPtr _statementHandle;

		protected readonly SetterMethod<T>[] _setters;
		protected readonly int _columnCount;

		internal SQLite3Enumerator(SQLite3StatementHandle statement)
		{
			_statement = statement;
			_statementHandle = _statement;

			_columnCount = NativeMethods.ColumnCount(_statement);
			_setters = new SetterMethod<T>[_columnCount];

			for (int i = 0; i < _columnCount; i++)
			{
				string columnName = SQLite3Helper.ColumnName(_statement, i);
				if (!Setters.ContainsKey(columnName))
					throw new SQLite3Exception("The object does not contain this property");

				_setters[i] = Setters[columnName];
			}
		}

		#region Inherited Members

		#region IEnumerator<T> Members

		public T Current
		{
			get
			{
				T value = new T();
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
			throw new NotImplementedException();
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			_statement.Dispose();
		}

		#endregion

		#endregion

	}
}
