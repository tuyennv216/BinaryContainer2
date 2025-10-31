using System;

namespace BinaryContainer2.Types
{
	public static class TypeUtils
	{
		public const byte Mask_Category = 0b_111_00000;
		public const byte Mask_Type = 0b_000_11111;

		public static byte GetSign(TypeCategory category, TypeArrayItems subCategory)
		{
			return GetSign(((byte)category), ((byte)subCategory));
		}

		public static byte GetSign(TypeCategory category, TypeBuildinItems subCategory)
		{
			return GetSign(((byte)category), ((byte)subCategory));
		}

		public static byte GetSign(TypeCategory category, byte subCategory)
		{
			return GetSign(((byte)category), ((byte)subCategory));
		}

		public static byte GetSign(byte category, byte subCategory)
		{
			var result = (byte)(
				((byte)category & Mask_Category) | ((byte)subCategory & Mask_Type));

			return result;
		}

		public static bool IsType(TypeCategory category, TypeArrayItems subCategory, byte sign)
		{
			return IsType(((byte)category), ((byte)subCategory), sign);
		}

		public static bool IsType(TypeCategory category, TypeBuildinItems subCategory, byte sign)
		{
			return IsType(((byte)category), ((byte)subCategory), sign);
		}

		public static bool IsType(TypeCategory category, byte subCategory, byte sign)
		{
			return IsType(((byte)category), ((byte)subCategory), sign);
		}

		public static bool IsType(byte category, byte subCategory, byte sign)
		{
			return (sign & Mask_Category) == category &&
				(sign & Mask_Type) == subCategory;
		}


		public static byte GetBuildinSign<T>()
		{
			return GetBuildinSign(typeof(T));
		}

		public static byte GetBuildinSign(Type type)
		{
			switch (type.FullName)
			{
				// 1
				case TypeNames.Buildin_SByte:
					return TypeBuildinSign.SByte;
				case TypeNames.Buildin_Byte:
					return TypeBuildinSign.Byte;
				case TypeNames.Buildin_Int16:
					return TypeBuildinSign.Int16;
				case TypeNames.Buildin_UInt16:
					return TypeBuildinSign.UInt16;
				case TypeNames.Buildin_Int32:
					return TypeBuildinSign.Int32;
				case TypeNames.Buildin_UInt32:
					return TypeBuildinSign.UInt32;
				case TypeNames.Buildin_Int64:
					return TypeBuildinSign.Int64;
				case TypeNames.Buildin_UInt64:
					return TypeBuildinSign.UInt64;
				case TypeNames.Buildin_Single:
					return TypeBuildinSign.Single;
				case TypeNames.Buildin_Double:
					return TypeBuildinSign.Double;
				case TypeNames.Buildin_Decimal:
					return TypeBuildinSign.Decimal;
				case TypeNames.Buildin_Char:
					return TypeBuildinSign.Char;
				case TypeNames.Buildin_String:
					return TypeBuildinSign.String;
				case TypeNames.Buildin_Boolean:
					return TypeBuildinSign.Boolean;

				default:
					return TypeBuildinSign.Classes;
			}
		}

		public static class TypeBuildinSign
		{
			public const byte SByte = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.SBYTE & Mask_Type);
			public const byte Byte = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.BYTE & Mask_Type);
			public const byte Int16 = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.INT16 & Mask_Type);
			public const byte UInt16 = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.UINT16 & Mask_Type);
			public const byte Int32 = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.INT32 & Mask_Type);
			public const byte UInt32 = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.UINT32 & Mask_Type);
			public const byte Int64 = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.INT64 & Mask_Type);
			public const byte UInt64 = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.UINT64 & Mask_Type);
			public const byte Single = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.SINGLE & Mask_Type);
			public const byte Double = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.DOUBLE & Mask_Type);
			public const byte Decimal = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.DECIMAL & Mask_Type);
			public const byte Char = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.CHAR & Mask_Type);
			public const byte String = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.STRING & Mask_Type);
			public const byte Boolean = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.BOOLEAN & Mask_Type);
			public const byte DateTime = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.DATETIME & Mask_Type);
			public const byte DateTimeOffset = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.DATETIMEOFFSET & Mask_Type);
			public const byte TimeSpan = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.TIMESPAN & Mask_Type);
			public const byte Guid = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.GUID & Mask_Type);
			public const byte Classes = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.CLASSES & Mask_Type);
			public const byte Object = ((byte)TypeCategory.Buildin & Mask_Category) | ((byte)TypeBuildinItems.OBJECT & Mask_Type);
		}
	}
}
