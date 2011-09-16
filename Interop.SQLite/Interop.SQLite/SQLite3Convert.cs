
using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Interop.SQLite
{
	internal static class SQLite3Convert
	{

		#region Text Converters

		/// <summary>
		///  Convert a string to a UTF-8 encoded byte  wiarrayth a trailing zero byte.
		/// </summary>
		/// <param name="value">
		///  The string to convert.
		/// </param>
		/// <returns>
		///  The array of bytes.
		/// </returns>
		public static byte[] StringToUtf8(string value)
		{
			int length = Encoding.UTF8.GetByteCount(value);
			byte[] rawString = new byte[length + 1];
			length = Encoding.UTF8.GetBytes(value, 0, value.Length, rawString, 0);
			rawString[length] = 0;

			return rawString;
		}

		/// <summary>
		///  Convert a raw zero-terminated UTF-8 pointer to a string.
		/// </summary>
		/// <param name="pointer">
		///  The pointer to read data from.
		/// </param>
		/// <returns>
		///  The string value.
		/// </returns>
		public static string Utf8ToString(IntPtr pointer)
		{
			if (pointer == IntPtr.Zero)
				throw new ArgumentNullException("pointer");

			int length = 0;
			while (Marshal.ReadByte(pointer, length) != 0)
				length++;

			return Utf8ToString(pointer, length);
		}

		/// <summary>
		///  Converts an UTF-8 string given by a pointer, and a specified length to a <see cref="System.String"/>. 
		/// </summary>
		/// <param name="pointer">
		///  A <see cref="IntPtr"/> pointing to the string in memory.
		/// </param>
		/// <param name="length">
		///  A <see cref="System.Int32"/> specifying the length.
		/// </param>
		/// <returns>
		///  A <see cref="System.String"/>.
		/// </returns>
		public static string Utf8ToString(IntPtr pointer, int length)
		{
			if (pointer == IntPtr.Zero)
				throw new ArgumentNullException("pointer");

			if (length < 0)
				throw new ArgumentOutOfRangeException("length");

			byte[] raw = new byte[length];
			Marshal.Copy(pointer, raw, 0, length);

			return Encoding.UTF8.GetString(raw);
		}

		#endregion

		#region Type Converters

		internal static readonly SQLite3ColumnType[] TypeCodeAffinities =
        {
            SQLite3ColumnType.Null,    //  0 TypeCode.Empty
            SQLite3ColumnType.Blob,    //  1 TypeCode.Object
            SQLite3ColumnType.Null,    //  2 TypeCode.DbNull
            SQLite3ColumnType.Integer, //  3 TypeCode.Boolean
            SQLite3ColumnType.Integer, //  4 TypeCode.Char
            SQLite3ColumnType.Integer, //  5 TypeCode.SByte
            SQLite3ColumnType.Integer, //  6 TypeCode.Byte
            SQLite3ColumnType.Integer, //  7 TypeCode.Int16
            SQLite3ColumnType.Integer, //  8 TypeCode.UInt16
            SQLite3ColumnType.Integer, //  9 TypeCode.Int32
            SQLite3ColumnType.Integer, // 10 TypeCode.UInt32
            SQLite3ColumnType.Integer, // 11 TypeCode.Int64
            SQLite3ColumnType.Integer, // 12 TypeCode.UInt64
            SQLite3ColumnType.Float,   // 13 TypeCode.Single
            SQLite3ColumnType.Float,   // 14 TypeCode.Double
            SQLite3ColumnType.Float,   // 15 TypeCode.Decimal
            SQLite3ColumnType.Integer, // 16 TypeCode.DateTime
            SQLite3ColumnType.Null,    // 17 --
            SQLite3ColumnType.Text,    // 18 TypeCode.String
        };

		internal static readonly Type[] ColumnTypeTypes =
        {
            null,           // 0 --
            typeof(long),   // 1 SQLite3ColumnType.Integer
            typeof(double), // 2 SQLite3ColumnType.Float
            typeof(string), // 3 SQLite3ColumnType.Text
            typeof(byte[]), // 4 SQLite3ColumnType.Blob
            typeof(DBNull)  // 5 SQLite3ColumnType.Null
        };

		public static Func<object, T> GetConverter<T>(SQLite3ColumnType colType)
		{
			Type retType = typeof(T);

			// 1: Blob checking is carried out seperately
			if (colType == SQLite3ColumnType.Blob)
			{
				if (retType == typeof(byte[]))
					return obj => (T)obj;
				else throw new ArgumentException("Invalid type for the specified column.");
			}

			// 2: Check for nullable types.
			bool isNullable = false;

			if (retType.IsGenericType &&
				retType.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				retType = retType.GetGenericArguments()[0];
				isNullable = true;
			}

			// 3: Return the correct conversion method.
			TypeCode retCode = Type.GetTypeCode(retType);

			if (colType == TypeCodeAffinities[(int)retCode])
			{
				// 3a. The conversion from nullable types is a little complicated.
				if (isNullable)
				{
					return obj =>
					{
						if (obj is DBNull)
							return default(T);

						return (T)Convert.ChangeType(obj, typeof(T));
					};
				}

				// 3b. If the types match exactly, just unbox.
				if (retType == ColumnTypeTypes[(int)colType])
					return obj => (T)obj;

				// 3c. Else use the IConvertitble interface.
				return obj => (T)Convert.ChangeType(obj, retType);
			}
			else throw new SQLite3Exception("Invalid type for the specified column.");
		}

		#endregion

	}
}
