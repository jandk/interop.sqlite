using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Interop.SQLite
{
    internal delegate void SetterMethod<in T>(T value, IntPtr statementHandle, int columnIndex);

    internal interface IObjectHydrater<in T>
        where T : new()
    {
        void Hydrate(T value, IntPtr statementHandle);
    }

    internal abstract class ObjectHydrater<T>
        : IObjectHydrater<T> where T : new()
    {
        protected static readonly List<PropertyInfo> WritableProperties;
        protected static readonly Dictionary<string, SetterMethod<T>> SetterMethods;

        protected static readonly MethodInfo[] ColumnGetters = new[]
        {
            null, // 0 --
            typeof(NativeMethods).GetMethod("ColumnInt64"), // 1 SQLite3ColumnType.Integer
            typeof(NativeMethods).GetMethod("ColumnDouble"), // 2 SQLite3ColumnType.Float
            typeof(SQLite3Helper).GetMethod("ColumnTextPtr"), // 3 SQLite3ColumnType.Text
            typeof(SQLite3Helper).GetMethod("ColumnBlobPtr"), // 4 SQLite3ColumnType.Blob
            null // 5 SQLite3ColumnType.Null
        };


        static ObjectHydrater()
        {
            WritableProperties = typeof(T).GetProperties().Where(property => property.CanWrite).ToList();
            var writablePropertyNames = WritableProperties.Select(pi => pi.Name.ToLowerInvariant());

        }

        protected ObjectHydrater()
        {
            if (SetterMethods != null)
                return;

            foreach (PropertyInfo writableProperty in WritableProperties)
                SetterMethods.Add(writableProperty.Name, GenerateSetterMethod(writableProperty));
        }

        public void Hydrate(T value, IntPtr statementHandle)
        {
            foreach (var method in SetterMethods.Values)
                method(value, statementHandle, i);
        }

        public abstract SetterMethod<T> GenerateSetterMethod(PropertyInfo propertyInfo);
    }

    internal class ReflectionHydrater<T>
        : ObjectHydrater<T> where T : new()
    {
        public override SetterMethod<T> GenerateSetterMethod(PropertyInfo propertyInfo)
        {
            Type propertyType = propertyInfo.PropertyType;
            TypeCode propertyTypeCode = Type.GetTypeCode(propertyType);
            MethodInfo columnGetter = ColumnGetters[(int)SQLite3Convert.TypeCodeAffinities[(int)propertyTypeCode]];

            return (v, stmt, idx) =>
            {
                object result = columnGetter.Invoke(null, new object[] { stmt, idx });
                object changedType = Convert.ChangeType(result, propertyType);
                propertyInfo.SetValue(v, changedType, null);
            };
        }
    }

    internal class MethodGeneratorHydrater<T>
        : ObjectHydrater<T> where T : new()
    {
        internal static readonly OpCode[] CastOpCodeAffinities = new[]
        {
            OpCodes.Nop, //  0 TypeCode.Empty
            OpCodes.Nop, //  1 TypeCode.Object
            OpCodes.Nop, //  2 TypeCode.DbNull
            OpCodes.Nop, //  3 TypeCode.Boolean
            OpCodes.Conv_U2, //  4 TypeCode.Char
            OpCodes.Conv_I1, //  5 TypeCode.SByte
            OpCodes.Conv_U1, //  6 TypeCode.Byte
            OpCodes.Conv_I2, //  7 TypeCode.Int16
            OpCodes.Conv_U2, //  8 TypeCode.UInt16
            OpCodes.Conv_I4, //  9 TypeCode.Int32
            OpCodes.Conv_U4, // 10 TypeCode.UInt32
            OpCodes.Nop, // 11 TypeCode.Int64
            OpCodes.Conv_U8, // 12 TypeCode.UInt64
            OpCodes.Conv_R4, // 13 TypeCode.Single
            OpCodes.Nop, // 14 TypeCode.Double
            OpCodes.Nop, // 15 TypeCode.Decimal
            OpCodes.Nop, // 16 TypeCode.DateTime
            OpCodes.Nop, // 17 --
            OpCodes.Nop // 18 TypeCode.String
        };

        public override SetterMethod<T> GenerateSetterMethod(PropertyInfo propertyInfo)
        {
            var setter = new DynamicMethod("_set" + propertyInfo.Name, null, new[] { typeof(T), typeof(IntPtr), typeof(int) }, typeof(T), true);

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

            generator.Emit(OpCodes.Call, ColumnGetters[(int)SQLite3Convert.TypeCodeAffinities[(int)typeCode]]);
        }

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
    }
}