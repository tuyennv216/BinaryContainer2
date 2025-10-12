namespace BinaryContainer2.Tests.ChallengeTests
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Immutable;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Drawing;
	using System.Net;
	using System.Reflection;
	using System.Text;

	// 1.1. Tham chiếu Vòng tròn
	public class Node
	{
		public string Name { get; set; } = string.Empty;
		public Node? Parent { get; set; }
		public Node? Child { get; set; }

		public override bool Equals(object? obj)
		{
			if (obj is not Node other) return false;
			// Kiểm tra đệ quy: quan trọng là tham chiếu phải giống nhau
			return Name == other.Name &&
				   (ReferenceEquals(Parent, other.Parent) || (Parent?.Name == other.Parent?.Name)) &&
				   (ReferenceEquals(Child, other.Child) || (Child?.Name == other.Child?.Name));
		}
	}

	// 2.1 & 2.2. Kiểu Dữ liệu Đặc biệt
	public class DataHolder
	{
		// Giá trị rỗng
		public List<int>? NullList { get; set; } = null;
		public string? NullString { get; set; } = null;
		public int? NullableInt { get; set; } = null;

		// Giá trị rỗng nhưng đối tượng không null
		public List<int> EmptyList { get; set; } = new List<int>();

		// Ngày giờ chính xác và độ chính xác thập phân
		public DateTime UtcTime { get; set; }
		public Decimal PreciseAmount { get; set; }
		public Guid UniqueId { get; set; }

		public override bool Equals(object? obj)
		{
			if (obj is not DataHolder other) return false;
			return NullList == other.NullList && // So sánh null
				   NullString == other.NullString &&
				   NullableInt == other.NullableInt &&
				   EmptyList.Count == other.EmptyList.Count &&
				   UtcTime.Equals(other.UtcTime) && // Quan trọng: kiểm tra cả Kind
				   PreciseAmount == other.PreciseAmount &&
				   UniqueId == other.UniqueId;
		}

		public override int GetHashCode() => HashCode.Combine(NullList, NullString, NullableInt, EmptyList, UtcTime, PreciseAmount, UniqueId);
	}

	// 3.1. Thuộc tính Chỉ Đọc (Read-Only)
	public class ReadOnlyProps
	{
		public string DataFromConstructor { get; }

		// Private setter (Nếu thư viện dựa vào public setter, nó sẽ thất bại)
		public int CalculatedValue { get; private set; }

		public ReadOnlyProps(string data)
		{
			DataFromConstructor = data;
			CalculatedValue = data.Length;
		}

		// Cần có setter ẩn để thư viện khôi phục (nếu nó thông minh)
		private void SetCalculatedValue(int value) => CalculatedValue = value;

		public override bool Equals(object? obj)
		{
			if (obj is not ReadOnlyProps other) return false;
			return DataFromConstructor == other.DataFromConstructor &&
				   CalculatedValue == other.CalculatedValue;
		}

		public override int GetHashCode() => HashCode.Combine(DataFromConstructor, CalculatedValue);
	}

	// 3.2. Kế thừa và Đa hình
	public abstract class Animal
	{
		public string Name { get; set; } = string.Empty;
		public int Age { get; set; }
	}

	public class Dog : Animal
	{
		public string Breed { get; set; } = string.Empty;

		public override bool Equals(object? obj)
		{
			if (obj is not Dog other) return false;
			return Name == other.Name && Age == other.Age && Breed == other.Breed;
		}
		public override int GetHashCode() => HashCode.Combine(Name, Age, Breed);
	}

	public class Zoo
	{
		public Animal Pet { get; set; } = new Dog(); // Thuộc tính có kiểu cơ sở
	}

	// 5.1. Dictionary với Key là một Class (Non-primitive Key)
	public class Coordinate
	{
		public int X { get; set; }
		public int Y { get; set; }

		public override bool Equals(object? obj)
		{
			if (obj is not Coordinate other) return false;
			return X == other.X && Y == other.Y;
		}

		// Yêu cầu GetHashCode() để hoạt động trong Dictionary
		public override int GetHashCode() => HashCode.Combine(X, Y);
	}

	public class MapData
	{
		// Dictionary với Key là kiểu phức tạp
		public Dictionary<Coordinate, string> Locations { get; set; } = new Dictionary<Coordinate, string>();
	}

	// 5.2. Constructor không mặc định và Struct
	public struct Measurement
	{
		public double Value { get; }
		public string Unit { get; }

		// Constructor duy nhất, không có public setter. Thư viện phải dùng constructor này
		public Measurement(double value, string unit)
		{
			Value = value;
			Unit = unit;
		}

		public override bool Equals(object? obj)
		{
			if (obj is not Measurement other) return false;
			return Value == other.Value && Unit == other.Unit;
		}

		public override int GetHashCode() => HashCode.Combine(Value, Unit);
	}

	// 5.3. Serialization qua Interface
	public interface IItem
	{
		string Name { get; set; }
		int Price { get; }
	}

	public class Book : IItem
	{
		public string Name { get; set; } = string.Empty;
		public int Price { get; set; }
		public string Author { get; set; } = string.Empty; // Thuộc tính riêng của lớp con

		int IItem.Price => Price; // Explicit interface implementation (tùy chọn)

		public override bool Equals(object? obj)
		{
			if (obj is not Book other) return false;
			return Name == other.Name && Price == other.Price && Author == other.Author;
		}

		public override int GetHashCode() => HashCode.Combine(Name, Price, Author);
	}

	public class Inventory
	{
		// Thử thách: Thuộc tính là kiểu Interface, nhưng đối tượng thực tế là Class.
		public IItem ItemA { get; set; } = new Book();

		// Thử thách: List chứa các Interface.
		public List<IItem> Items { get; set; } = new List<IItem>();
	}

	public class EdgeCaseData
	{
		// Array rỗng
		public byte[] EmptyBytes { get; set; } = new byte[0];

		// TimeSpan bất thường (Microseconds)
		public TimeSpan Duration { get; set; } = TimeSpan.FromMicroseconds(123456789.0);

		// Tuple (sử dụng Tuple<T1, T2> để kiểm tra serialization/deserialization)
		public Tuple<int, string> DataTuple { get; set; } = Tuple.Create(100, "Metadata");

		// Chuỗi chứa ký tự Unicode 4-byte (Surrogate Pair)
		public string SpecialString { get; set; } = "Test: 💩 End";

		// Thêm một kiểu dữ liệu khó khác: HashSet (kiểm tra tính duy nhất và không theo thứ tự)
		public HashSet<string> UniqueItems { get; set; } = new HashSet<string> { "Apple", "Banana", "Apple" };

		public override bool Equals(object? obj)
		{
			if (obj is not EdgeCaseData other) return false;

			// So sánh byte array
			bool bytesEqual = EmptyBytes.SequenceEqual(other.EmptyBytes);

			// So sánh HashSet (đảm bảo số lượng và nội dung giống nhau, không quan tâm thứ tự)
			bool hashSetEqual = UniqueItems.SetEquals(other.UniqueItems);

			return bytesEqual &&
				   hashSetEqual &&
				   Duration.Equals(other.Duration) &&
				   DataTuple.Item1 == other.DataTuple.Item1 &&
				   DataTuple.Item2 == other.DataTuple.Item2 &&
				   SpecialString == other.SpecialString;
		}

		public override int GetHashCode() => HashCode.Combine(EmptyBytes, Duration, DataTuple, SpecialString, UniqueItems);
	}

	// T13 & T14: Kiểm tra Tham chiếu Đa cấp và Collection dùng chung tham chiếu
	public class SharedItem
	{
		public int Value { get; set; }

		public override bool Equals(object? obj)
		{
			if (obj is not SharedItem other) return false;
			return Value == other.Value;
		}
		public override int GetHashCode() => Value.GetHashCode();
	}

	public class ReferenceHolder
	{
		// Tham chiếu 1
		public SharedItem RefA { get; set; } = new SharedItem();

		// Tham chiếu 2 - Cùng một đối tượng với RefA
		public SharedItem RefB { get; set; } = new SharedItem();

		// Collection sử dụng tham chiếu đó
		public List<SharedItem> Items { get; set; } = new List<SharedItem>();
	}

	// T15: Collection Bất biến (Immutable Collection)
	public class ImmutableContainer
	{
		public ImmutableList<int> Data { get; set; }

		public ImmutableContainer()
		{
			Data = ImmutableList.Create<int>();
		}
	}


	// T17: Thuộc tính Private và Fields Công khai/Riêng tư
	public class FieldClass
	{
		// ⚠️ THỬ THÁCH CHÍNH: Fields KHÔNG phải Properties
		public int PublicField = 10;
		private string PrivateField = "Secret";

		// Thuộc tính chỉ đọc dựa trên Field
		public string SecretData => PrivateField;

		// Constructor mặc định cần thiết cho serialization (nếu thư viện yêu cầu)
		public FieldClass() { }

		// Phương thức để kiểm tra dữ liệu sau Deserialization
		public override bool Equals(object? obj)
		{
			if (obj is not FieldClass other) return false;

			// Dùng Reflection để so sánh PrivateField sau khi deserialization
			var privateField = typeof(FieldClass).GetField("PrivateField", BindingFlags.NonPublic | BindingFlags.Instance);
			string? otherPrivateValue = privateField?.GetValue(other) as string;

			return PublicField == other.PublicField &&
				   PrivateField == otherPrivateValue;
		}

		public override int GetHashCode() => HashCode.Combine(PublicField, PrivateField);
	}

	// T18: Đa hình với Thuộc tính Collection
	public class Document
	{
		public string Title { get; set; } = string.Empty;
	}

	public class Report : Document
	{
		public DateTime ReportDate { get; set; }
	}

	public class Library
	{
		// List chứa các kiểu cơ sở (polymorphism)
		public List<Document> Items { get; set; } = new List<Document>();

		// Constructor mặc định
		public Library() { }
	}

	// T19: Kiểu dữ liệu đặc biệt (Dictionary of Structs và Interface)
	public interface IConfigValue
	{
		string DisplayValue { get; }
	}

	public struct ConfigStruct : IConfigValue
	{
		public int KeyCode { get; set; }

		public string DisplayValue => $"Key {KeyCode}";

		// Constructor mặc định
		public ConfigStruct() { }
	}

	public class Config
	{
		// Dictionary có Value là Struct
		public Dictionary<string, ConfigStruct> Settings { get; set; } = new Dictionary<string, ConfigStruct>();

		// Constructor mặc định
		public Config() { }
	}

	// T20: Thuộc tính là Object (Polymorphism sâu)
	public class DataWrapper
	{
		// Thử thách: Kiểu dữ liệu là object, thư viện phải ghi nhớ kiểu thực tế (Report)
		public object? Payload { get; set; }

		// Constructor mặc định
		public DataWrapper() { }
	}

	// T23: Kiểu dữ liệu không có giá trị (Zero Value) dễ bị nhầm lẫn
	public class ZeroValueData
	{
		// Thử thách 2: Array có kích thước 0 (khác với null)
		public int[]? EmptyArray { get; set; } = new int[0];

		// Thử thách 3: Sử dụng DefaultValue (thư viện có thể bỏ qua giá trị này)
		[DefaultValue(100)]
		public int MaxCount { get; set; } = 100;

		public ZeroValueData() { }
	}

	// T24: Kế thừa sâu với các đối tượng trùng lặp
	public class BaseMessage
	{
		public Guid MessageId { get; set; } = Guid.NewGuid();
	}

	public class Command : BaseMessage
	{
		public string CommandName { get; set; } = string.Empty;
	}

	public class CommandGroup : Command
	{
		public List<Command> Commands { get; set; } = new List<Command>();
		// Lớp này kế thừa Command và chứa List<Command> -> Lồng ghép đa hình

		public CommandGroup() { }
	}

	// T25: Thuộc tính có giá trị là Chính nó (Self-Referencing Property)
	public class SelfRef
	{
		// Đây KHÔNG phải tham chiếu vòng tròn, mà là một thuộc tính trỏ đến chính nó
		public SelfRef? Self { get; set; }
		public string Data { get; set; } = string.Empty;

		public SelfRef() { }
	}

	// T28: Kiểu dữ liệu đặc biệt (Uri và Char Array)
	public class EndpointData
	{
		// Thử thách 1: Uri (Là một Class phức tạp, giá trị null và rỗng có ý nghĩa khác nhau)
		public Uri? BaseUri { get; set; }

		// Thử thách 2: Array of Char (Thường bị xử lý khác so với String)
		public char[] NameChars { get; set; } = Array.Empty<char>();

		// Constructor mặc định
		public EndpointData() { }
	}

	// T29: Mảng Mảng Mảng (Deep Array Nesting)
	public class DeepArray
	{
		// Thử thách: Mảng 3 chiều (3D array) chứa kiểu Decimal (chính xác)
		public Decimal[,,]? DataGrid { get; set; }

		// Constructor mặc định
		public DeepArray() { }
	}

	// T31: DateTimeOffset và TimeSpan với giá trị góc
	public class TimeData
	{
		// Thử thách 1: DateTimeOffset (kết hợp DateTime và Offset)
		public DateTimeOffset StandardOffset { get; set; }

		// Thử thách 2: TimeSpan rất lớn hoặc rất nhỏ (chính xác)
		public TimeSpan LargeTimeSpan { get; set; }

		// Constructor mặc định
		public TimeData() { }
	}

	// T32: Class Generic Nâng cao
	public class GenericWrapper<TKey, TValue>
	{
		// Thuộc tính là các kiểu generic
		public TKey Key { get; set; } = default!;
		public TValue Value { get; set; } = default!;

		// Constructor mặc định
		public GenericWrapper() { }
	}

	// T33: Kiểu dữ liệu Enum với giá trị 0 và giá trị ngoài phạm vi
	public enum Status
	{
		Pending = 0,
		Active = 1,
		Deleted = 255 // Giá trị lớn
	}

	public class EnumData
	{
		public Status CurrentStatus { get; set; } = Status.Pending;
		public Status? NullableStatus { get; set; } = null;

		// Constructor mặc định
		public EnumData() { }
	}

	// T34: ReadOnlyCollection và Mảng Chuỗi Dữ liệu lớn
	public class LargeDataContainer
	{
		// Thử thách 1: Array of large strings (Public Get/Set)
		public string[] LargeStrings { get; set; } = Array.Empty<string>();

		// Thử thách 2: Dictionary với key và value đều là chuỗi lớn (Gây áp lực lên bộ nhớ và hash code)
		public Dictionary<string, string> LargeMap { get; set; } = new Dictionary<string, string>();

		// Thử thách 3: List<Guid> rỗng
		public List<Guid> EmptyGuidList { get; set; } = new List<Guid>();

		// Constructor mặc định
		public LargeDataContainer() { }

		public override bool Equals(object? obj)
		{
			if (obj is not LargeDataContainer other) return false;

			// So sánh LargeStrings
			bool stringsEqual = LargeStrings.SequenceEqual(other.LargeStrings);

			// So sánh LargeMap
			bool mapEqual = LargeMap.OrderBy(kv => kv.Key).SequenceEqual(other.LargeMap.OrderBy(kv => kv.Key));

			// So sánh EmptyGuidList
			bool listEqual = EmptyGuidList.Count == other.EmptyGuidList.Count;

			return stringsEqual && mapEqual && listEqual;
		}

		public override int GetHashCode() => HashCode.Combine(LargeStrings, LargeMap, EmptyGuidList);
	}

	// T35: Char Array 2D
	public class CodeSnippet
	{
		// Thử thách 2: Mảng 2 chiều (2D Array)
		public char[,]? CharMatrix { get; set; }

		// Constructor mặc định
		public CodeSnippet() { }
	}

	// T37: So sánh Mảng và Danh sách (List vs Array)
	public class ListArrayChallenge
	{
		// Thử thách 1: List rỗng lồng List rỗng
		public List<List<int>> DeeplyEmptyList { get; set; } = new List<List<int>> { new List<int>() };

		// Thử thách 2: Array rỗng lồng Array rỗng
		public int[][] DeeplyEmptyArray { get; set; } = new int[][] { Array.Empty<int>() };

		// Thử thách 3: List chứa mảng (List<int[]>)
		public List<int[]?> ListOfArrays { get; set; } = new List<int[]?>();

		public ListArrayChallenge() { }
	}

	// T38: Kiểu dữ liệu Hệ thống Nâng cao
	public class SystemData
	{
		// Thử thách 1: Version (Kiểu class phức tạp, thường bị xử lý sai)
		public Version CurrentVersion { get; set; } = new Version(1, 2, 3, 4);

		// Thử thách 2: DateTime mặc định (MinValue)
		public DateTime MinDate { get; set; } = DateTime.MinValue;

		// Constructor mặc định
		public SystemData() { }
	}

	// T40: Mảng Mặc định và Mảng Kích thước Lớn
	public class ArrayBoundary
	{
		// Thử thách 1: Mảng (Reference Type) được khai báo nhưng KHÔNG khởi tạo (NULL)
		public int[]? NullArray { get; set; } = null;

		// Thử thách 2: Mảng có kích thước rất lớn (kiểm tra giới hạn 32-bit integer cho độ dài)
		// Lưu ý: Dùng kích thước lớn HƠN mức 64KB tiêu chuẩn để kiểm tra
		public byte[] LargeByteArray { get; set; } = new byte[100000];

		// Constructor mặc định
		public ArrayBoundary() { }
	}

	// T41: Thuộc tính Boolean/Byte với giá trị mặc định
	public class LogicPrimitives
	{
		// Thử thách 1: Giá trị mặc định là FALSE (kiểm tra khả năng phân biệt với thiếu dữ liệu)
		public bool IsEnabled { get; set; } = false;

		// Thử thách 2: Giá trị Byte (0, 255)
		public byte MaxByte { get; set; } = 255;
		public byte MinByte { get; set; } = 0;

		// Constructor mặc định
		public LogicPrimitives() { }
	}

	// T43: Struct Lồng ghép và Kiểu Byte/Boolean Array
	public struct ColorPoint
	{
		// Struct có constructor mặc định
		public ColorPoint() { }

		// Thử thách 1: Struct phức tạp (Size)
		public Size Dimensions { get; set; }

		// Thử thách 2: Mảng Boolean
		public bool[] Flags { get; set; } = Array.Empty<bool>();

		public override bool Equals(object? obj)
		{
			if (obj is not ColorPoint other) return false;
			return Dimensions.Equals(other.Dimensions) && Flags.SequenceEqual(other.Flags);
		}
		public override int GetHashCode() => HashCode.Combine(Dimensions, Flags);
	}

	public class ItemContainer
	{
		// List chứa Struct có thể null
		public List<ColorPoint?> Points { get; set; } = new List<ColorPoint?>();

		// Constructor mặc định
		public ItemContainer() { }
	}

	// T44: Chuỗi Byte Đặc biệt (chỉ chứa các giá trị 0xFF và 0x00)
	public class BinaryBlock
	{
		// Thử thách: Byte Array chứa giá trị 0xFF (thường được dùng làm padding hoặc giá trị đặc biệt)
		public byte[] Data { get; set; } = Array.Empty<byte>();

		// Constructor mặc định
		public BinaryBlock() { }
	}

	// T46: SortedList (Collection có thứ tự)
	public class SortedCollectionData
	{
		// Thử thách: SortedList (Phải khôi phục đúng thứ tự KEY và giá trị)
		public SortedList<string, int> RankedData { get; set; } = new SortedList<string, int>();

		// Constructor mặc định
		public SortedCollectionData() { }
	}

	// T47: Kiểu Dữ liệu Nâng cao Lồng ghép Sâu
	public struct InnerData
	{
		public Guid? Id { get; set; } // Nullable Struct
		public int? Value { get; set; } // Nullable Struct

		public InnerData() { }
	}

	public class DeeplyNullableContainer
	{
		// Thử thách: List chứa các Struct có thể null, mà Struct đó lại chứa các Struct có thể null khác.
		public List<InnerData?> NestedList { get; set; } = new List<InnerData?>();

		// Constructor mặc định
		public DeeplyNullableContainer() { }
	}

	// T48: Kết hợp Tham chiếu Vòng tròn và Collection (Trường hợp Góc Hiếm)
	public class CollectionRef
	{
		public string Name { get; set; } = string.Empty;

		// List chứa đối tượng cha (tham chiếu vòng tròn gián tiếp qua Collection)
		public List<CollectionRef>? Children { get; set; } = new List<CollectionRef>();

		// Constructor mặc định
		public CollectionRef() { }
	}

	// T49: Kiểu dữ liệu Type và List chứa nhiều kiểu dữ liệu khác nhau
	public class TypeReferenceData
	{
		// Thử thách 2: List chứa các kiểu dữ liệu khác nhau (List<object>)
		public List<object?> MixedList { get; set; } = new List<object?>();

		// Constructor mặc định
		public TypeReferenceData() { }
	}

	// T50: List<T> với T là Kiểu Base Class không có Dữ liệu
	public class SimpleBase
	{
		// Class cơ sở KHÔNG có thuộc tính nào ngoài những thuộc tính kế thừa từ Object.
		// Thử thách: Đảm bảo thư viện vẫn serialize thành công một đối tượng KHÔNG có dữ tính Public Properties nào của riêng nó.
	}

	public class EmptyBaseContainer
	{
		public List<SimpleBase> Items { get; set; } = new List<SimpleBase>();

		// Constructor mặc định
		public EmptyBaseContainer() { }
	}

	// T52: KeyValuePair (Struct Generic Lồng ghép)
	public class KeyValuePairContainer
	{
		// Thử thách: KeyValuePair<TKey, TValue> là một Struct Generic. Serialization thường gặp khó khăn khi xử lý Struct Generic.
		public KeyValuePair<string, int> Item { get; set; }

		// List chứa KeyValuePair (Lồng ghép và Struct)
		public List<KeyValuePair<Guid, string>> ListItems { get; set; } = new List<KeyValuePair<Guid, string>>();

		// Constructor mặc định (cần thiết cho Class)
		public KeyValuePairContainer()
		{
			// Khởi tạo Item bằng giá trị mặc định (default(KeyValuePair<...>)
			Item = default;
		}
	}

	// T53: Object Array chứa Kiểu Enum
	public class EnumArrayContainer
	{
		// Thử thách: Mảng object chứa kiểu Struct (Enum), đòi hỏi Boxing/Unboxing chính xác
		public object[] MixedEnums { get; set; } = Array.Empty<object>();

		// Constructor mặc định
		public EnumArrayContainer() { }
	}

	// T54: Cấu trúc Rỗng Sâu với List<Array> và các giá trị mặc định của Struct
	public class FinalDeepEmptiness
	{
		// Thử thách 1: List lồng ghép List rỗng
		public List<List<int>> OuterList { get; set; } = new List<List<int>> { new List<int>() };

		// Thử thách 2: Tuple với các giá trị mặc định của Struct
		public Tuple<int, DateTime, bool> DefaultTuple { get; set; } = Tuple.Create(0, default(DateTime), false);

		// Constructor mặc định
		public FinalDeepEmptiness() { }
	}
}
