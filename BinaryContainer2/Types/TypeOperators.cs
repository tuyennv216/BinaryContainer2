using BinaryContainer2.Abstraction;
using BinaryContainer2.Operators;
using System;
using System.Collections.Concurrent;

namespace BinaryContainer2.Types
{
	internal class TypeOperators
	{
		public static readonly TypeOperators Instance = new();

		private readonly ConcurrentDictionary<Type, ITypeOperator> operators = new();

		public ITypeOperator GetOperator<T>()
		{
			return GetOperator(typeof(T));
		}

		public ITypeOperator GetOperator(Type type)
		{
			var op = operators.GetOrAdd(type, key =>
			{
				// Kiểm tra các kiểu dẫn xuất từ BaseType
				switch (key.BaseType?.FullName)
				{
					case TypeNames.Array_Array:
						{
							if (key.GetArrayRank() == 1)
								return new TypeArray(key);
							else
								return new TypeArrayMultiDimension(key);
						}

					case TypeNames.Enum:
						{
							return new TypeEnum(key);
						}
				}

				// Kiểm tra Generic
				var genericTypeIndex = key.FullName.IndexOf('[');
				if (genericTypeIndex >= 0)
				{
					var baseTypeName = key.FullName.Substring(0, genericTypeIndex);
					switch (baseTypeName)
					{
						case TypeNamesGeneric.List1:
						case TypeNamesGeneric.Queue1:
						case TypeNamesGeneric.HashSet1:
						case TypeNamesGeneric.LinkedList1:
							{
								return new TypeArrayGeneric(key);
							}

						case TypeNamesGeneric.Stack1:
							{
								return new TypeArrayStack(key);
							}

						case TypeNamesGeneric.KeyValuePair2:
							{
								return new TypeKeyPairValue(key);
							}

						case TypeNamesGeneric.Dictionary2:
							{
								return new TypeDictionary(key);
							}
						case TypeNamesGeneric.SortedList2:
							{
								return new TypeShortedList(key);
							}

						case TypeNamesGeneric.Tuple1:
						case TypeNamesGeneric.Tuple2:
						case TypeNamesGeneric.Tuple3:
						case TypeNamesGeneric.Tuple4:
						case TypeNamesGeneric.Tuple5:
						case TypeNamesGeneric.Tuple6:
						case TypeNamesGeneric.Tuple7:
						case TypeNamesGeneric.Tuple8:
							{
								return new TypeTuple(key);
							}

						case TypeNamesGeneric.Nullable1:
							{
								return new TypeNullable(key);
							}
					}
				}

				// Kiểm tra các kiểu cơ bản và Collection đặc biệt
				switch (key.FullName)
				{
					case TypeNames.Buildin_SByte:
						{
							return new TypeSByte();
						}
					case TypeNames.Buildin_Byte:
						{
							return new TypeByte();
						}
					case TypeNames.Buildin_Int16:
						{
							return new TypeInt16();
						}
					case TypeNames.Buildin_UInt16:
						{
							return new TypeUInt16();
						}
					case TypeNames.Buildin_Int32:
						{
							return new TypeInt32();
						}
					case TypeNames.Buildin_UInt32:
						{
							return new TypeUInt32();
						}
					case TypeNames.Buildin_Int64:
						{
							return new TypeInt64();
						}
					case TypeNames.Buildin_UInt64:
						{
							return new TypeUInt64();
						}
					case TypeNames.Buildin_Single:
						{
							return new TypeSingle();
						}
					case TypeNames.Buildin_Double:
						{
							return new TypeDouble();
						}
					case TypeNames.Buildin_Decimal:
						{
							return new TypeDecimal();
						}
					case TypeNames.Buildin_Char:
						{
							return new TypeChar();
						}
					case TypeNames.Buildin_String:
						{
							return new TypeString();
						}
					case TypeNames.Buildin_Boolean:
						{
							return new TypeBoolean();
						}
					case TypeNames.Buildin_DateTime:
						{
							return new TypeDateTime();
						}
					case TypeNames.Buildin_DateTimeOffset:
						{
							return new TypeDateTimeOffset();
						}
					case TypeNames.Buildin_TimeSpan:
						{
							return new TypeTimeSpan();
						}
					case TypeNames.Buildin_Guid:
						{
							return new TypeGuid();
						}

					// Xử lý mảng (kiểu Array) nếu không bị bắt ở BaseType
					case TypeNames.Array_Array:
						{
							return new TypeArray(key);
						}

					// Các kiểu Collection giống mảng (List, Queue,...)
					case TypeNames.Array_List:
					case TypeNames.Array_Queue:
					case TypeNames.Array_Stack:
					case TypeNames.Array_HashSet:
					case TypeNames.Array_LinkedList:
						{
							return new TypeArrayGeneric(key);
						}

					// Kiểu Dictionary không generic (thường là IDictionary)
					case TypeNames.Dictionary:
						{
							return new TypeDictionary(key);
						}

					case TypeNames.Buildin_Object:
						{
							return new TypeObject();
						}

					default:
						// Mặc định là TypeClass (Class/Struct thông thường)
						{
							return new TypeClass(key);
						}
				}
			});

			lock(op)
			{
				op.Build();
			}

			return op;
		}
	}
}
