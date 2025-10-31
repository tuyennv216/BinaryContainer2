using BinaryContainer2.Container;
using BinaryContainer2.Operators;
using BinaryContainer2.Others;

namespace BinaryContainer2.Tests.OperatorsTests.ArrayTests
{
	[TestClass]
	public class TypeArray_UInt64_Tests // Test cho mảng ulong[] (UInt64)
	{
		private TypeArray _typeArrayOperator;
		private RefPool _refPool;
		private Random _random;

		[TestInitialize]
		public void Setup()
		{
			// Khởi tạo với kiểu phần tử ulong[]
			_typeArrayOperator = new TypeArray(typeof(ulong[]));
			_typeArrayOperator.Build();
			_refPool = new RefPool();
			// Sử dụng seed 42
			_random = new Random(42);
		}

		// --- Phương thức tiện ích để so sánh mảng ---
		private void AssertArrayEqual(ulong[] expected, Array actual, string message)
		{
			Assert.IsNotNull(actual, $"Mảng đọc ra không được là NULL: {message}");
			Assert.AreEqual(expected.Length, actual.Length, $"Độ dài mảng không khớp: {message}");
			Assert.IsInstanceOfType(actual, typeof(ulong[]), "Kiểu đọc ra phải là ulong[].");
			for (int i = 0; i < expected.Length; i++)
			{
				Assert.AreEqual(expected[i], actual.GetValue(i), $"Phần tử thứ {i} không khớp: {message}");
			}
		}

		/// <summary>
		/// Phương thức tiện ích để tạo ulong ngẫu nhiên trong dải đầy đủ.
		/// </summary>
		private ulong GetRandomUInt64()
		{
			// Kết hợp 2 giá trị Next(int) để tạo ulong 64-bit đầy đủ
			// Cần phải tạo 64 bit ngẫu nhiên, không thể dựa vào Random.Next()
			byte[] buf = new byte[8];
			_random.NextBytes(buf);
			// Vì Random.NextBytes trả về byte[] nên ta dùng BitConverter
			ulong randomValue = BitConverter.ToUInt64(buf, 0);

			// Ta có thể giới hạn dải bằng cách dùng Random.NextDouble cho khoảng (0, 1) 
			// và nhân với ulong.MaxValue, nhưng cách dùng BitConverter an toàn và bao quát hơn.
			return randomValue;
		}

		// --- I. Các Test Case Cơ Bản ---

		/// <summary>
		/// Test case cho một mảng ulong[] điển hình.
		/// </summary>
		[TestMethod]
		public void WriteRead_TypicalUInt64Array_ShouldReturnCorrectValue()
		{
			// 1. Chuẩn bị data: ulong[]
			ulong[] originalArray = new ulong[] { 10ul, 4294967296ul, 90000000000000ul, ulong.MaxValue / 2 };
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 3. Read
			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);

			// 4. Kiểm tra
			AssertArrayEqual(originalArray, readArray!, "Mảng ulong[] điển hình.");
		}

		/// <summary>
		/// Test case cho giá trị NULL (Mảng là NULL).
		/// </summary>
		[TestMethod]
		public void WriteRead_NullArray_ShouldReturnNull()
		{
			object? originalData = null;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			object? readData = _typeArrayOperator.Read(newContainer, _refPool);
			Assert.IsNull(readData, "Dữ liệu đọc ra phải là NULL.");
		}

		// --- II. Giá Trị Biên (Edge Cases) ---

		/// <summary>
		/// Giá trị biên: Mảng Rỗng (ulong[] có 0 phần tử).
		/// </summary>
		[TestMethod]
		public void WriteRead_EmptyArray_ShouldReturnEmptyArray()
		{
			ulong[] originalArray = new ulong[0];
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);

			Assert.IsNotNull(readArray);
			Assert.AreEqual(0, readArray.Length, "Mảng rỗng phải được đọc ra là mảng có độ dài 0.");
		}

		/// <summary>
		/// Mảng chứa các giá trị biên của kiểu phần tử (Min: 0, Max: UInt64.MaxValue).
		/// </summary>
		[TestMethod]
		public void WriteRead_UInt64EdgeValuesArray_ShouldBePreserved()
		{
			ulong[] originalArray = new ulong[] { ulong.MinValue, ulong.MaxValue, 1ul, 9223372036854775808ul }; // 2^63
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);
			AssertArrayEqual(originalArray, readArray!, "Mảng chứa giá trị biên của UInt64.");
		}

		// --- III. Giá Trị Ngẫu Nhiên (Stress Testing) ---

		/// <summary>
		/// Stress Test: Write/Read một mảng lớn với các giá trị ngẫu nhiên.
		/// </summary>
		[TestMethod]
		public void WriteRead_LargeRandomUInt64Array_ShouldBeStable()
		{
			const int arrayLength = 1000;
			ulong[] originalArray = new ulong[arrayLength];

			// 1. Tạo mảng ngẫu nhiên
			for (int i = 0; i < arrayLength; i++)
			{
				originalArray[i] = GetRandomUInt64();
			}
			object originalData = originalArray;
			var originalContainer = new DataContainer();

			// 2. Write, Export, Import
			_typeArrayOperator.Write(originalContainer, originalData, _refPool);
			byte[] exportedBytes = originalContainer.Export();
			var newContainer = new DataContainer();
			newContainer.Import(exportedBytes);

			// 3. Read
			Array? readArray = (Array?)_typeArrayOperator.Read(newContainer, _refPool);

			// 4. Kiểm tra
			AssertArrayEqual(originalArray, readArray!, $"Mảng ngẫu nhiên lớn ({arrayLength} phần tử) kiểu UInt64.");
		}
	}
}
