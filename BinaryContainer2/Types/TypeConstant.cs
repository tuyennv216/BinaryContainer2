namespace BinaryContainer2.Types
{
	// 1 byte = 8 bits định nghĩa kiểu dữ liệu
	// 3 bits đầu là type_category
	// 5 bits sau là type phụ trong danh sách category

	// int						0b000_(3)
	// list<int>				0b010_(3), và 0b00_(3)
	// list<list<int>>			0b010_(3), 0b10_(3), và 0b00_(3)
	// dictionary<string, int>	0b011_(13), và 0b11_(5)

	public enum TypeCategory : byte
	{
		Buildin = 0b000_00000,
		Enum = 0b001_00000,
		Array = 0b010_00000,
		Dictionary = 0b011_00000,
	}

	public enum TypeBuildinItems : byte
	{
		SBYTE = 1,
		BYTE = 2,
		INT16 = 3,
		UINT16 = 4,
		INT32 = 5,
		UINT32 = 6,
		INT64 = 7,
		UINT64 = 8,
		SINGLE = 9,
		DOUBLE = 10,
		DECIMAL = 11,
		CHAR = 12,
		STRING = 13,
		BOOLEAN = 14,

		DATETIME = 20,
		DATETIMEOFFSET = 21,
		TIMESPAN = 22,

		GUID = 25,

		CLASSES = 31,
		OBJECT = 32,
	}
	
	public enum TypeArrayItems : byte
	{
		IENUMERABLE = 1,
		ARRAY = 2,
		LIST = 3,
		QUEUE = 4,
		STACK = 5,
		HASHSET = 6,
		LINKEDLIST = 7,
	}

	public static class TypeNames
	{
		public const string Buildin_SByte = "System.SByte";
		public const string Buildin_Byte = "System.Byte";
		public const string Buildin_Int16 = "System.Int16";
		public const string Buildin_UInt16 = "System.UInt16";
		public const string Buildin_Int32 = "System.Int32";
		public const string Buildin_UInt32 = "System.UInt32";
		public const string Buildin_Int64 = "System.Int64";
		public const string Buildin_UInt64 = "System.UInt64";
		public const string Buildin_Single = "System.Single";
		public const string Buildin_Double = "System.Double";
		public const string Buildin_Decimal = "System.Decimal";
		public const string Buildin_Char = "System.Char";
		public const string Buildin_String = "System.String";
		public const string Buildin_Boolean = "System.Boolean";
		public const string Buildin_Object = "System.Object";
		public const string Buildin_DateTime = "System.DateTime";
		public const string Buildin_DateTimeOffset = "System.DateTimeOffset";
		public const string Buildin_TimeSpan = "System.TimeSpan";
		public const string Buildin_Struct = "System.TimeSpan";
		public const string Buildin_Guid = "System.Guid";

		public const string Array_IEnumerable = "System.Collections.IEnumerable";
		public const string Array_Array = "System.Array";
		public const string Array_List = "System.Collection.Generic.List<T>";
		public const string Array_Queue = "System.Collections.Generic.Queue<T>";
		public const string Array_Stack = "System.Collections.Generic.Stack<T>";
		public const string Array_HashSet = "System.Collections.Generic.HashSet<T>";
		public const string Array_LinkedList = "System.Collections.Generic.LinkedList<T>";

		public const string Enum = "System.Enum";
		public const string Dictionary = "System.Collections.Generic.Dictionary<TKey, TValue>";

		public const string TypeClass = "Custom.Class";
	}

	public static class TypeNamesGeneric
	{
		public const string List1 = "System.Collections.Generic.List`1";
		public const string Queue1 = "System.Collections.Generic.Queue`1";
		public const string Stack1 = "System.Collections.Generic.Stack`1";
		public const string HashSet1 = "System.Collections.Generic.HashSet`1";
		public const string LinkedList1 = "System.Collections.Generic.LinkedList`1";

		public const string KeyValuePair2 = "System.Collections.Generic.KeyValuePair`2";
		public const string Dictionary2 = "System.Collections.Generic.Dictionary`2";
		public const string SortedList2 = "System.Collections.Generic.SortedList`2";

		public const string Tuple1 = "System.Tuple`1";
		public const string Tuple2 = "System.Tuple`2";
		public const string Tuple3 = "System.Tuple`3";
		public const string Tuple4 = "System.Tuple`4";
		public const string Tuple5 = "System.Tuple`5";
		public const string Tuple6 = "System.Tuple`6";
		public const string Tuple7 = "System.Tuple`7";
		public const string Tuple8 = "System.Tuple`8";

		public const string Nullable1 = "System.Nullable`1";
	}
}
